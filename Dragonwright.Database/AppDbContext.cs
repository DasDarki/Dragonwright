using Dragonwright.Database.Entities;

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
    
    public DbSet<Subclass> Subclasses => Set<Subclass>();
    
    public DbSet<ClassFeature> ClassFeatures => Set<ClassFeature>();
    
    public DbSet<Background> Backgrounds => Set<Background>();
    
    public DbSet<Feat> Feats => Set<Feat>();
    
    public DbSet<Modifier> Modifiers => Set<Modifier>();
    
    public DbSet<Race> Races => Set<Race>();
    
    public DbSet<RaceVariant> RaceVariants => Set<RaceVariant>();
    
    public DbSet<RaceTrait> RaceTraits => Set<RaceTrait>();
    
    public DbSet<RaceTraitOption> RaceTraitOptions => Set<RaceTraitOption>();
    
    public DbSet<RaceTraitAction> RaceTraitActions => Set<RaceTraitAction>();
    
    public DbSet<RaceTraitCreature> RaceTraitCreatures => Set<RaceTraitCreature>();
    
    public DbSet<RaceTraitSpell> RaceTraitSpells => Set<RaceTraitSpell>();
    
    public DbSet<Spell> Spells => Set<Spell>();
    
    public DbSet<Item> Items => Set<Item>();
    
    public DbSet<Creature> Creatures => Set<Creature>();

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
    
    public DbSet<CharacterItem> CharacterItems => Set<CharacterItem>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.RegisterEntity<User>();
        modelBuilder.RegisterEntity<RefreshToken>();
        modelBuilder.RegisterEntity<StoredFile>();
        modelBuilder.RegisterEntity<Class>();
        modelBuilder.RegisterEntity<Subclass>();
        modelBuilder.RegisterEntity<ClassFeature>();
        modelBuilder.RegisterEntity<Character>();
        modelBuilder.RegisterEntity<Background>();
        modelBuilder.RegisterEntity<Feat>();
        modelBuilder.RegisterEntity<Modifier>();
        modelBuilder.RegisterEntity<Race>();
        modelBuilder.RegisterEntity<RaceVariant>();
        modelBuilder.RegisterEntity<RaceTrait>();
        modelBuilder.RegisterEntity<RaceTraitOption>();
        modelBuilder.RegisterEntity<RaceTraitAction>();
        modelBuilder.RegisterEntity<RaceTraitCreature>();
        modelBuilder.RegisterEntity<RaceTraitSpell>();
        modelBuilder.RegisterEntity<Spell>();
        modelBuilder.RegisterEntity<Item>();
        modelBuilder.RegisterEntity<Creature>();
        modelBuilder.RegisterEntity<CharacterClass>();
        modelBuilder.RegisterEntity<CharacterAbility>();
        modelBuilder.RegisterEntity<CharacterSkill>();
        modelBuilder.RegisterEntity<CharacterRace>();
        modelBuilder.RegisterEntity<CharacterBackground>();
        modelBuilder.RegisterEntity<CharacterFeat>();
        modelBuilder.RegisterEntity<CharacterSpell>();
        modelBuilder.RegisterEntity<CharacterItem>();
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