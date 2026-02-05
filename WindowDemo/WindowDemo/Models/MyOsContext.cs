using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WindowDemo.Models;

public partial class MyOsContext : DbContext
{
    public MyOsContext()
    {
    }

    public MyOsContext(DbContextOptions<MyOsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BoardGame> BoardGames { get; set; }
    public virtual DbSet<BoardGameInfo> BoardGameInfo { get; set; }
    public virtual DbSet<Customer> Customer { get; set; }

    public virtual DbSet<ProduktCategory> ProduktCategory { get; set; }
    public virtual DbSet<ProduktSales> ProduktSales { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(ADOShopChecker.SetServWithPass());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Genre).HasMaxLength(50);
            entity.Property(e => e.Publisher).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
        });
        modelBuilder.Entity<BoardGameInfo>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Information).HasMaxLength(50);
        });
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.username).HasMaxLength(255);
        });
        modelBuilder.Entity<ProduktCategory>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Kategori).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
