using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Extensions;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User,
    Role,
    long,
    UserClaim,
    UserRole,
    UserLogin,
    RoleClaim,
    UserToken>
{

  private readonly IDomainEventDispatcher? _dispatcher;

  public AppDbContext(
    DbContextOptions<AppDbContext> options) : base(options)
  {

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
  }

  public AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher

    )
      : base(options)
  {
    _dispatcher = dispatcher;
  }
  #region Identity
  public DbSet<User> User => Set<User>();
  public DbSet<UserRole> UserRole => Set<UserRole>();
  public DbSet<ApplicationClaim> ApplicationClaims => Set<ApplicationClaim>();
  public DbSet<ClaimGroup> ClaimGroup => Set<ClaimGroup>();
  public DbSet<RoleClaim> RoleClaim => Set<RoleClaim>();
  public DbSet<Document> Document => Set<Document>();
  public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();
  public DbSet<Order> Order => Set<Order>();

  #endregion
  public DbSet<PMSErrorLog> PMSErrorLogs => Set<PMSErrorLog>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    var roleEntity = builder.Entity<Role>().ToTable("Role");

    builder.Entity<User>().ToTable("User");

    builder.Entity<UserRole>().ToTable("UserRole");
    builder.Entity<RoleClaim>().ToTable("RoleClaim");
    builder.Entity<UserClaim>().ToTable("UserClaim");

    roleEntity.HasIndex(r => r.NormalizedName).IsUnique(false);

    var userEntity = builder.Entity<User>();

    userEntity.HasIndex(r => r.Email).IsUnique(false);
    userEntity.HasIndex(r => r.NormalizedUserName).IsUnique(false);
    userEntity.HasIndex(r => r.UserName).IsUnique(false);
    userEntity.HasIndex(r => r.NormalizedEmail).IsUnique(false);


    builder.Entity<Document>()
        .HasMany(d => d.Metadata)
        .WithOne(dm => dm.Document)
        .HasForeignKey(dm => dm.DocumentId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Document>()
       .HasIndex(d => d.Status);

    builder.Entity<Document>()
        .HasIndex(d => new { d.Status, d.CreatedDate });

    builder.Entity<DocumentMetadata>()
        .HasIndex(dm => new { dm.Key, dm.Value });

    builder.Entity<DocumentMetadata>()
        .HasIndex(dm => dm.CreatedDate);

    DataSeeds.DataSeeder.SeedData(builder);
    builder.AddQueryFilterToAllEntitiesAssignableFrom<ISoftDeleteEntity>(_ => !_.IsDeleted);
  }
  private void SetSoftDelete()
  {
    foreach (var entry in ChangeTracker.Entries<ISoftDeleteEntity>())
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.IsDeleted = false;
          break;
        case EntityState.Deleted:
          entry.State = EntityState.Modified;
          entry.Entity.IsDeleted = true;
          break;
      }
    }
  }
  private void SetAuditData()
  {
    foreach (var entry in ChangeTracker.Entries<IAuditEntity>())
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.CreatedDate = DateTime.UtcNow;
          break;

        case EntityState.Modified:
          entry.Entity.ModifiedDate = DateTime.UtcNow;
          break;
      }
    }
  }
  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
    ChangeTracker.DetectChanges();
    SetSoftDelete();
    SetAuditData();
    int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return result;
  }

  public override int SaveChanges()
  {
    return SaveChangesAsync().GetAwaiter().GetResult();
  }
}
