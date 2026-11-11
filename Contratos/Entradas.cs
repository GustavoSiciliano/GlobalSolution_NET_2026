using System.ComponentModel.DataAnnotations;

namespace Binova.Api.Contratos;

public class PontoEntrada
{
    [Required, MaxLength(120)] public string Nome { get; set; } = "";
    [Required, MaxLength(255)] public string DescricaoLocalizacao { get; set; } = "";
    [Range(typeof(decimal), "0.01", "999999.99")] public decimal CapacidadeLitros { get; set; }
    public bool Ativo { get; set; } = true;
}

public class UsuarioEntrada
{
    [Required, MaxLength(120)] public string Nome { get; set; } = "";
    [Required, EmailAddress, MaxLength(180)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Senha { get; set; } = "";
    [Required] public string Papel { get; set; } = "CIDADAO";
}

public class UsuarioAtualizacao
{
    [Required, MaxLength(120)] public string Nome { get; set; } = "";
    [Required, EmailAddress, MaxLength(180)] public string Email { get; set; } = "";
    [Required] public string Papel { get; set; } = "CIDADAO";
}

public class OcorrenciaEntrada
{
    [Range(1, int.MaxValue)] public int PontoColetaId { get; set; }
    [Range(1, int.MaxValue)] public int UsuarioId { get; set; }
    [Required, MaxLength(32)] public string Tipo { get; set; } = "";
    [Required, MaxLength(1000)] public string Descricao { get; set; } = "";
}

public class OcorrenciaAtualizacao
{
    [Required, MaxLength(32)] public string Tipo { get; set; } = "";
    [Required, MaxLength(1000)] public string Descricao { get; set; } = "";
    [Required, MaxLength(20)] public string Status { get; set; } = "";
}

public class LeituraEntrada
{
    [Range(1, int.MaxValue)] public int PontoColetaId { get; set; }
    [Range(typeof(decimal), "0.01", "999999.99")] public decimal DistanciaCm { get; set; }
    [Range(typeof(decimal), "0", "100")] public decimal NivelPercentual { get; set; }
}
