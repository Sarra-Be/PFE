using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using stage_api.configuration;
using stage_api.Models;

public class DbContext : IdentityDbContext<ApplicationUser>
{
    // pour acceder aux données disponibles dans la base,
    // on ajoute un attribut de type DbSet<classe_model>
    public DbContext(DbContextOptions<DbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UploadedFile>()
            .ToTable("UploadedFiles");
    }
    public virtual DbSet<UploadedFile> Files { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ValidationRequest> ValidationRequests { get; set; }
    public  DbSet<ActionLog> ActionLogs { get; set; }

}

