﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SimpleCloudStorage.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FileSystemObject> FileSystemObjects { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Share> Shares { get; set; }
        public virtual DbSet<PublicFile> PublicFiles { get; set; }
        public virtual DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileSystemObject>(entity =>
            {
                entity.HasIndex(e => e.ParentId);

                entity.HasIndex(e => new { e.ParentId, e.Name })
                    .HasName("uniq_fso_name")
                    .IsUnique();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.IsFolder).HasColumnName("isFolder");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.HomeDirId);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.HomeDir)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.HomeDirId);
            });

            modelBuilder.Entity<Share>(entity =>
            {
                entity.HasKey(e => new { e.FromUserId, e.ToUserId, e.FsoId });

                entity.Property(e => e.SharedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PublicFile>(entity =>
            {
                entity.HasKey(e => new { e.FromUserId, e.FsoId });

                entity.Property(e => e.PublicId).IsRequired();

                entity.Property(e => e.SharedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
