using Binova.Api.Dados;
using Binova.Api.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Binova.Api.Migrations;

[DbContext(typeof(BinovaDbContext))]
public class BinovaDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("USUARIO");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();
            e.Property(x => x.Nome).HasColumnName("NOME").HasColumnType("VARCHAR2(120)").IsRequired();
            e.Property(x => x.Email).HasColumnName("EMAIL").HasColumnType("VARCHAR2(180)").IsRequired();
            e.HasIndex(x => x.Email).HasDatabaseName("BINOVA_USUARIO_EMAIL_UK").IsUnique();
            e.Property(x => x.SenhaHash).HasColumnName("SENHA_HASH").HasColumnType("VARCHAR2(255)").IsRequired();
            e.Property(x => x.Papel).HasColumnName("PAPEL").HasColumnType("VARCHAR2(20)").IsRequired();
            e.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM").HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
        modelBuilder.Entity<PontoColeta>(e =>
        {
            e.ToTable("PONTO_COLETA");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();
            e.Property(x => x.Nome).HasColumnName("NOME").HasColumnType("VARCHAR2(120)").IsRequired();
            e.Property(x => x.DescricaoLocalizacao).HasColumnName("DESCRICAO_LOCALIZACAO")
                .HasColumnType("VARCHAR2(255)").IsRequired();
            e.Property(x => x.CapacidadeLitros).HasColumnName("CAPACIDADE_LITROS").HasPrecision(8, 2);
            e.Property(x => x.Ativo).HasColumnName("ATIVO").HasConversion<int>().HasColumnType("NUMBER(1)");
            e.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM").HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
        modelBuilder.Entity<LeituraSensor>(e =>
        {
            e.ToTable("LEITURA_SENSOR");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();
            e.Property(x => x.PontoColetaId).HasColumnName("PONTO_COLETA_ID");
            e.Property(x => x.DistanciaCm).HasColumnName("DISTANCIA_CM").HasPrecision(8, 2);
            e.Property(x => x.NivelPercentual).HasColumnName("NIVEL_PERCENTUAL").HasPrecision(5, 2);
            e.Property(x => x.RegistradoEm).HasColumnName("REGISTRADO_EM").HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
            e.HasOne(x => x.PontoColeta).WithMany(x => x.Leituras)
                .HasForeignKey(x => x.PontoColetaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.PontoColetaId).HasDatabaseName("BINOVA_LEITURA_PONTO_IX");
        });
        modelBuilder.Entity<Ocorrencia>(e =>
        {
            e.ToTable("OCORRENCIA");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();
            e.Property(x => x.PontoColetaId).HasColumnName("PONTO_COLETA_ID");
            e.Property(x => x.UsuarioId).HasColumnName("USUARIO_ID");
            e.Property(x => x.Tipo).HasColumnName("TIPO").HasColumnType("VARCHAR2(32)").IsRequired();
            e.Property(x => x.Descricao).HasColumnName("DESCRICAO").HasColumnType("VARCHAR2(1000)").IsRequired();
            e.Property(x => x.Status).HasColumnName("STATUS").HasColumnType("VARCHAR2(20)").IsRequired();
            e.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM").HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.AtualizadoEm).HasColumnName("ATUALIZADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.HasOne(x => x.PontoColeta).WithMany(x => x.Ocorrencias)
                .HasForeignKey(x => x.PontoColetaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario).WithMany(x => x.Ocorrencias)
                .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.PontoColetaId).HasDatabaseName("OCORRENCIA_PONTO_IX");
            e.HasIndex(x => x.UsuarioId).HasDatabaseName("OCORRENCIA_USUARIO_IX");
        });
    }
}
