using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DictionaryAPI.Models;

namespace DictionaryAPI.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Term>(entity =>
        {
            entity.Property(w => w.Word).HasMaxLength(100);
            entity.Property(w => w.Definition).HasMaxLength(2000);
            entity.Property(w => w.ExtraInformation).HasMaxLength(500);
        });

        builder.Entity<List>(entity =>
        {
            entity.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .IsRequired();
            entity.Property(l => l.Name).HasMaxLength(200);
        });

        builder.Entity<WordList>().HasKey(wl => new { wl.WordId, wl.ListId });

        builder.Entity<Submission>(entity =>
        {
            entity.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired();
            entity.Property(s => s.Word).HasMaxLength(100);
            entity.Property(s => s.Definition).HasMaxLength(2000);
            entity.Property(s => s.ExtraInformation).HasMaxLength(500);
        });

        builder.Entity<SubmissionState>(entity =>
        {
            entity.Property(ss => ss.Name).HasMaxLength(30);
            entity.Property(ss => ss.Description).HasMaxLength(500);
        });
    }

    public DbSet<Term> Words { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<WordList> WordLists { get; set; }
    public DbSet<SubmissionState> SubmissionStates { get; set; }
}
