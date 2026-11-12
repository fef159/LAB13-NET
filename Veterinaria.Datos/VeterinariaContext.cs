using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos;

public partial class VeterinariaContext : DbContext
{
    public VeterinariaContext(DbContextOptions<VeterinariaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<Propietario> Propietarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mascotas__3214EC07EF76213D");

            entity.Property(e => e.Especie).HasMaxLength(40);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Nombre).HasMaxLength(80);
            entity.Property(e => e.Peso).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Raza).HasMaxLength(60);

            entity.HasOne(d => d.Propietario).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.PropietarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Propietarios");
        });

        modelBuilder.Entity<Propietario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Propieta__3214EC07C751BB40");

            entity.HasIndex(e => e.Documento, "UQ__Propieta__AF73706D6358241A").IsUnique();

            entity.Property(e => e.Documento).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
