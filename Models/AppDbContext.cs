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
        public virtual DbSet<File> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=SimpleCloudStorageDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileSystemObject>(entity =>
            {
                entity.HasIndex(e => e.ParentId);

                entity.Property(e => e.IsFolder).HasColumnName("isFolder");

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

            modelBuilder.Entity<File>(entity =>
            {
                entity.Property(e => e.FileName).IsRequired();

                entity.Property(e => e.UploadDate).HasColumnType("datetime");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}