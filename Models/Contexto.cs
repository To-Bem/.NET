using Microsoft.EntityFrameworkCore;
using TOBem.TelemetriaApi.Models;

namespace TOBem.TelemetriaApi.Models;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Evento> Eventos => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Nomes de tabela e coluna em MAIÚSCULAS para que o PL/SQL do grupo leia sem aspas.
        modelBuilder.Entity<Dispositivo>(e =>
        {
            e.ToTable("DISPOSITIVO");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.IdosoId).HasColumnName("IDOSO_ID").IsRequired();
            e.Property(x => x.Tipo).HasColumnName("TIPO").HasMaxLength(10).IsRequired();
            e.Property(x => x.Comodo).HasColumnName("COMODO").HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(10).IsRequired();
            e.Property(x => x.UltimoHeartbeat).HasColumnName("ULTIMO_HEARTBEAT");
        });

        modelBuilder.Entity<Evento>(e =>
        {
            e.ToTable("EVENTO");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.DispositivoId).HasColumnName("DISPOSITIVO_ID").IsRequired();
            e.Property(x => x.Tipo).HasColumnName("TIPO").HasMaxLength(30).IsRequired();
            e.Property(x => x.Valor).HasColumnName("VALOR").IsRequired();
            e.Property(x => x.DataHora).HasColumnName("DATA_HORA").IsRequired();

            // Dispositivo 1:N Evento
            e.HasOne(x => x.Dispositivo)
             .WithMany(d => d.Eventos)
             .HasForeignKey(x => x.DispositivoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.DispositivoId, x.DataHora });
        });
    }
}
