using Dragonwright.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Database;

/// <summary>
/// The Dragonswright application database context allows interaction with the underlying PostgreSQL database.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    #region Content

    public DbSet<Class> Classes => Set<Class>();
    
    public DbSet<Background> Backgrounds => Set<Background>();
    
    public DbSet<Feat> Feats => Set<Feat>();
    
    public DbSet<Race> Races => Set<Race>();
    
    public DbSet<Spell> Spells => Set<Spell>();

    #endregion

    #region Character

    public DbSet<Character> Characters => Set<Character>();
    
    public DbSet<CharacterClass> CharacterClasses => Set<CharacterClass>();
    
    public DbSet<CharacterAbility> CharacterAbilities => Set<CharacterAbility>();
    
    public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();
    
    public DbSet<CharacterRace> CharacterRaces => Set<CharacterRace>();
    
    public DbSet<CharacterBackground> CharacterBackgrounds => Set<CharacterBackground>();
    
    public DbSet<CharacterFeat> CharacterFeats => Set<CharacterFeat>();
    
    public DbSet<CharacterSpell> CharacterSpells => Set<CharacterSpell>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.RegisterEntity<User>();
        modelBuilder.RegisterEntity<RefreshToken>();
        modelBuilder.RegisterEntity<StoredFile>();
        modelBuilder.RegisterEntity<Class>();
        modelBuilder.RegisterEntity<Character>();
        modelBuilder.RegisterEntity<Background>();
        modelBuilder.RegisterEntity<Feat>();
        modelBuilder.RegisterEntity<Race>();
        modelBuilder.RegisterEntity<Spell>();
        modelBuilder.RegisterEntity<CharacterClass>();
        modelBuilder.RegisterEntity<CharacterAbility>();
        modelBuilder.RegisterEntity<CharacterSkill>();
        modelBuilder.RegisterEntity<CharacterRace>();
        modelBuilder.RegisterEntity<CharacterBackground>();
        modelBuilder.RegisterEntity<CharacterFeat>();
        modelBuilder.RegisterEntity<CharacterSpell>();
    }
}

internal static class EfCoreExtensions
{
    public static void RegisterEntity<T>(this ModelBuilder modelBuilder) where T : class, IEntity<T>, new()
    {
        var entity = new T();
        var builder = modelBuilder.Entity<T>();
        entity.Configure(builder);
    }
}