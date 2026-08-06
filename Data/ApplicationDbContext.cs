using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DictionaryAPI.Models.Entities;
using DictionaryAPI.Interfaces;

namespace DictionaryAPI.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.Property(u => u.UserName).HasMaxLength(30);
        });

        builder.Entity<Term>(entity =>
        {
            entity.Property(w => w.Word).HasMaxLength(100).UseCollation("Modern_Spanish_CI_AI");
            entity.Property(w => w.Definition).HasMaxLength(2000).UseCollation("Modern_Spanish_CI_AI");
            entity.Property(w => w.ExtraInformation).HasMaxLength(500);
            entity.Property(w => w.Example).HasMaxLength(500);
            entity.Property(w => w.Etymology).HasMaxLength(500);
        });

        builder.Entity<TermTag>()
            .HasKey(tt => new { tt.WordId, tt.TagId });

        builder.Entity<TermTag>()
            .HasOne(wt => wt.Word)
            .WithMany(w => w.TermTags)
            .HasForeignKey(wt => wt.WordId);

        builder.Entity<TermTag>()
            .HasOne(wt => wt.Tag)
            .WithMany(t => t.TermTags)
            .HasForeignKey(wt => wt.TagId);

        builder.Entity<TermRelation>()
            .HasOne(wr => wr.Word)
            .WithMany(w => w.OutgoingTermRelations)
            .HasForeignKey(wr => wr.WordId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<TermRelation>()
            .HasOne(wr => wr.RelatedWord)
            .WithMany(w => w.IncomingTermRelations)
            .HasForeignKey(wr => wr.RelatedWordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<List>(entity =>
        {
            entity.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .IsRequired();
            entity.Property(l => l.Name).HasMaxLength(200);
        });

        builder.Entity<TermList>().HasKey(wl => new { wl.WordId, wl.ListId });

        builder.Entity<Submission>(entity =>
        {
            entity.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired();
            entity.Property(s => s.Word).HasMaxLength(100);
            entity.Property(s => s.Definition).HasMaxLength(2000);
            entity.Property(s => s.ExtraInformation).HasMaxLength(500);
            entity.Property(s => s.Example).HasMaxLength(500);
            entity.Property(s => s.Etymology).HasMaxLength(500);
        });

        builder.Entity<SubmissionStatus>(entity =>
        {
            entity.Property(ss => ss.Name).HasMaxLength(30);
            entity.Property(ss => ss.Description).HasMaxLength(500);
        });

        builder.Entity<FeaturedList>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.StartDate).IsRequired();
            entity.Property(f => f.EndDate).IsRequired();
            entity.HasOne(f => f.List)
            .WithMany()
            .HasForeignKey(f => f.ListId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public DbSet<Term> Terms { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<TermList> TermLists { get; set; }
    public DbSet<SubmissionStatus> SubmissionStatus { get; set; }
    public DbSet<FeaturedList> FeaturedLists { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Term &&
            (e.State == EntityState.Added || e.State == EntityState.Modified));
        
        foreach (var entry in entries)
        {
            var trackable = (ITrackableEntity)entry.Entity;

            if (entry.State == EntityState.Modified)
            {
                trackable.UpdatedAt = DateTime.Now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
