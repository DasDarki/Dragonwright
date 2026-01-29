using Dragonwright.Configuration;
using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Services.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dragonwright.Services;

/// <summary>
/// Service responsible for storing and retrieving files on the local filesystem with database-backed metadata tracking.
/// </summary>
public sealed class FileStorageService(
    ILogger logger,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<FileStorageConfiguration> fileStorageConfiguration
) : BaseService
{
    private readonly FileStorageConfiguration _config = fileStorageConfiguration.Value;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var basePath = GetAbsoluteBasePath();
        Directory.CreateDirectory(basePath);
        logger.Information("File storage initialized at {BasePath}", basePath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stores a file on disk and creates a corresponding database record.
    /// </summary>
    /// <param name="stream">The file content stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type of the file.</param>
    /// <returns>The created <see cref="StoredFile"/> entity.</returns>
    public async Task<StoredFile> StoreAsync(Stream stream, string fileName, string contentType)
    {
        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(fileName);
        var storagePath = Path.Combine(
            DateTime.UtcNow.ToString("yyyy"),
            DateTime.UtcNow.ToString("MM"),
            $"{fileId}{extension}");

        var fullPath = Path.Combine(GetAbsoluteBasePath(), storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await stream.CopyToAsync(fileStream);
        }

        var fileInfo = new FileInfo(fullPath);

        var storedFile = new StoredFile
        {
            Id = fileId,
            FileName = SanitizeFileName(fileName),
            ContentType = contentType,
            Size = fileInfo.Length,
            StoragePath = storagePath,
            CreatedAt = DateTime.UtcNow
        };

        await using var dbContext = CreateDbContext();
        dbContext.StoredFiles.Add(storedFile);
        await dbContext.SaveChangesAsync();

        logger.Information("Stored file {FileName} ({Size} bytes) as {FileId}", fileName, fileInfo.Length, fileId);

        return storedFile;
    }

    /// <summary>
    /// Retrieves the metadata for a stored file.
    /// </summary>
    /// <param name="fileId">The ID of the file.</param>
    /// <returns>The <see cref="StoredFile"/> entity, or null if not found.</returns>
    public async Task<StoredFile?> GetFileInfoAsync(Guid fileId)
    {
        await using var dbContext = CreateDbContext();
        return await dbContext.StoredFiles.FindAsync(fileId);
    }

    /// <summary>
    /// Opens a read stream for the specified stored file.
    /// </summary>
    /// <param name="storedFile">The stored file metadata.</param>
    /// <returns>A readable file stream, or null if the physical file is missing.</returns>
    public FileStream? OpenReadStream(StoredFile storedFile)
    {
        var fullPath = Path.Combine(GetAbsoluteBasePath(), storedFile.StoragePath);

        if (!File.Exists(fullPath))
        {
            logger.Warning("Physical file missing for StoredFile {FileId} at {Path}", storedFile.Id, fullPath);
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Deletes a stored file from both the database and the filesystem.
    /// </summary>
    /// <param name="fileId">The ID of the file to delete.</param>
    /// <returns>True if the file was found and deleted, false otherwise.</returns>
    public async Task<bool> DeleteAsync(Guid fileId)
    {
        await using var dbContext = CreateDbContext();

        var storedFile = await dbContext.StoredFiles.FindAsync(fileId);
        if (storedFile == null)
        {
            return false;
        }

        dbContext.StoredFiles.Remove(storedFile);
        await dbContext.SaveChangesAsync();

        DeletePhysicalFile(storedFile.StoragePath);

        logger.Information("Deleted stored file {FileId} ({FileName})", fileId, storedFile.FileName);

        return true;
    }

    /// <summary>
    /// Deletes the physical file at the given storage-relative path.
    /// </summary>
    /// <param name="storagePath">The storage-relative path of the file.</param>
    public void DeletePhysicalFile(string storagePath)
    {
        var fullPath = Path.Combine(GetAbsoluteBasePath(), storagePath);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to delete physical file at {Path}", fullPath);
        }
    }

    /// <summary>
    /// Returns all stored file IDs that are not referenced by any entity in the database.
    /// </summary>
    /// <returns>A list of orphaned <see cref="StoredFile"/> entries.</returns>
    public async Task<List<StoredFile>> FindOrphanedFilesAsync()
    {
        await using var dbContext = CreateDbContext();

        var referencedIds = await dbContext.Users
            .Where(u => u.AvatarId != null)
            .Select(u => u.AvatarId!.Value)
            .Distinct()
            .ToListAsync();

        return await dbContext.StoredFiles
            .Where(f => !referencedIds.Contains(f.Id))
            .ToListAsync();
    }

    private string GetAbsoluteBasePath()
    {
        return Path.IsPathRooted(_config.BasePath)
            ? _config.BasePath
            : Path.Combine(AppContext.BaseDirectory, _config.BasePath);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private AppDbContext CreateDbContext()
    {
        var scope = serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
