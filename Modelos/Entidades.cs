namespace Binova.Api.Modelos;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Papel { get; set; } = "CIDADAO";
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
}

public class PontoColeta
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string DescricaoLocalizacao { get; set; } = "";
    public decimal CapacidadeLitros { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    public ICollection<LeituraSensor> Leituras { get; set; } = new List<LeituraSensor>();
}

public class LeituraSensor
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public PontoColeta PontoColeta { get; set; } = null!;
    public decimal DistanciaCm { get; set; }
    public decimal NivelPercentual { get; set; }
    public DateTimeOffset RegistradoEm { get; set; } = DateTimeOffset.UtcNow;
}

public class Ocorrencia
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public PontoColeta PontoColeta { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Tipo { get; set; } = "OUTRO";
    public string Descricao { get; set; } = "";
    public string Status { get; set; } = "ABERTA";
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
