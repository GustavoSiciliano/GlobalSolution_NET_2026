
using Binova.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Dados;

public class BinovaDbContext(DbContextOptions<BinovaDbContext> options)
    : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<PontoColeta> PontosColeta => Set<PontoColeta>();

    public DbSet<LeituraSensor> LeiturasSensor => Set<LeituraSensor>();

    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();

    // Aplica os mapeamentos das entidades ao banco Oracle
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarModelo(modelBuilder);
    }

    // Organiza a configuração das quatro entidades
    internal static void ConfigurarModelo(ModelBuilder modelBuilder)
    {
        ConfigurarUsuario(modelBuilder);
        ConfigurarPontoColeta(modelBuilder);
        ConfigurarLeituraSensor(modelBuilder);
        ConfigurarOcorrencia(modelBuilder);
    }

    // Configura a tabela de usuários
    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entidade =>
        {
            entidade.ToTable("USUARIO");

            entidade.HasKey(usuario => usuario.Id);

            entidade.Property(usuario => usuario.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();

            entidade.Property(usuario => usuario.Nome)
                .HasColumnName("NOME")
                .HasColumnType("VARCHAR2(120)")
                .IsRequired();

            entidade.Property(usuario => usuario.Email)
                .HasColumnName("EMAIL")
                .HasColumnType("VARCHAR2(180)")
                .IsRequired();

            entidade.HasIndex(usuario => usuario.Email)
                .HasDatabaseName("BINOVA_USUARIO_EMAIL_UK")
                .IsUnique();

            entidade.Property(usuario => usuario.SenhaHash)
                .HasColumnName("SENHA_HASH")
                .HasColumnType("VARCHAR2(255)")
                .IsRequired();

            entidade.Property(usuario => usuario.Papel)
                .HasColumnName("PAPEL")
                .HasColumnType("VARCHAR2(20)")
                .IsRequired();

            entidade.Property(usuario => usuario.CriadoEm)
                .HasColumnName("CRIADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
    }

    // Configura os dados dos pontos de coleta
    private static void ConfigurarPontoColeta(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PontoColeta>(entidade =>
        {
            entidade.ToTable("PONTO_COLETA");

            entidade.HasKey(ponto => ponto.Id);

            entidade.Property(ponto => ponto.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();

            entidade.Property(ponto => ponto.Nome)
                .HasColumnName("NOME")
                .HasColumnType("VARCHAR2(120)")
                .IsRequired();

            entidade.Property(ponto => ponto.DescricaoLocalizacao)
                .HasColumnName("DESCRICAO_LOCALIZACAO")
                .HasColumnType("VARCHAR2(255)")
                .IsRequired();

            entidade.Property(ponto => ponto.CapacidadeLitros)
                .HasColumnName("CAPACIDADE_LITROS")
                .HasPrecision(8, 2);

            entidade.Property(ponto => ponto.Ativo)
                .HasColumnName("ATIVO")
                .HasColumnType("NUMBER(1)");

            entidade.Property(ponto => ponto.CriadoEm)
                .HasColumnName("CRIADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
        });
    }

    // Configura as leituras e seu relacionamento com os pontos
    private static void ConfigurarLeituraSensor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeituraSensor>(entidade =>
        {
            entidade.ToTable("LEITURA_SENSOR");

            entidade.HasKey(leitura => leitura.Id);

            entidade.Property(leitura => leitura.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();

            entidade.Property(leitura => leitura.PontoColetaId)
                .HasColumnName("PONTO_COLETA_ID");

            entidade.Property(leitura => leitura.DistanciaCm)
                .HasColumnName("DISTANCIA_CM")
                .HasPrecision(8, 2);

            entidade.Property(leitura => leitura.NivelPercentual)
                .HasColumnName("NIVEL_PERCENTUAL")
                .HasPrecision(5, 2);

            entidade.Property(leitura => leitura.RegistradoEm)
                .HasColumnName("REGISTRADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entidade.HasOne(leitura => leitura.PontoColeta)
                .WithMany(ponto => ponto.Leituras)
                .HasForeignKey(leitura => leitura.PontoColetaId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(leitura => leitura.PontoColetaId)
                .HasDatabaseName("BINOVA_LEITURA_PONTO_IX");
        });
    }

    // Configura as ocorrências e seus relacionamentos
    private static void ConfigurarOcorrencia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ocorrencia>(entidade =>
        {
            entidade.ToTable("OCORRENCIA");

            entidade.HasKey(ocorrencia => ocorrencia.Id);

            entidade.Property(ocorrencia => ocorrencia.Id)
                .HasColumnName("ID")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedNever();

            entidade.Property(ocorrencia => ocorrencia.PontoColetaId)
                .HasColumnName("PONTO_COLETA_ID");

            entidade.Property(ocorrencia => ocorrencia.UsuarioId)
                .HasColumnName("USUARIO_ID");

            entidade.Property(ocorrencia => ocorrencia.Tipo)
                .HasColumnName("TIPO")
                .HasColumnType("VARCHAR2(32)")
                .IsRequired();

            entidade.Property(ocorrencia => ocorrencia.Descricao)
                .HasColumnName("DESCRICAO")
                .HasColumnType("VARCHAR2(1000)")
                .IsRequired();

            entidade.Property(ocorrencia => ocorrencia.Status)
                .HasColumnName("STATUS")
                .HasColumnType("VARCHAR2(20)")
                .IsRequired();

            entidade.Property(ocorrencia => ocorrencia.CriadoEm)
                .HasColumnName("CRIADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entidade.Property(ocorrencia => ocorrencia.AtualizadoEm)
                .HasColumnName("ATUALIZADO_EM")
                .HasColumnType("TIMESTAMP(6) WITH TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            entidade.HasOne(ocorrencia => ocorrencia.PontoColeta)
                .WithMany(ponto => ponto.Ocorrencias)
                .HasForeignKey(ocorrencia => ocorrencia.PontoColetaId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(ocorrencia => ocorrencia.Usuario)
                .WithMany(usuario => usuario.Ocorrencias)
                .HasForeignKey(ocorrencia => ocorrencia.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(ocorrencia => ocorrencia.PontoColetaId)
                .HasDatabaseName("OCORRENCIA_PONTO_IX");

            entidade.HasIndex(ocorrencia => ocorrencia.UsuarioId)
                .HasDatabaseName("OCORRENCIA_USUARIO_IX");
        });
    }
}
