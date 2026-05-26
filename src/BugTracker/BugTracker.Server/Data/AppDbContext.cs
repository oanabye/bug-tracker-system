using BugTracker.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BugTracker.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Bug> Bugs => Set<Bug>();

    public DbSet<BugShareRequest> BugShareRequests => Set<BugShareRequest>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Avoid cascade delete conflict
        modelBuilder.Entity<Bug>()
            .HasOne(b => b.ReportedBy)
            .WithMany(u => u.ReportedBugs)
            .HasForeignKey(b => b.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bug>()
            .HasOne(b => b.AssignedTo)
            .WithMany(u => u.AssignedBugs)
            .HasForeignKey(b => b.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BugShareRequest>()
            .HasOne(r => r.Bug)
            .WithMany()
            .HasForeignKey(r => r.BugId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BugShareRequest>()
            .HasOne(r => r.FromDeveloper)
            .WithMany()
            .HasForeignKey(r => r.FromDeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BugShareRequest>()
            .HasOne(r => r.ToDeveloper)
            .WithMany()
            .HasForeignKey(r => r.ToDeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}