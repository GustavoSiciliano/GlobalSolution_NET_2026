
using System.ComponentModel.DataAnnotations;

namespace Binova.Api.Contratos;

// Dados para cadastro e atualização de pontos de coleta
public class PontoEntrada
{
    [Required(ErrorMessage = "O nome do ponto de coleta é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome deve ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "A localização é obrigatória.")]
    [MaxLength(255, ErrorMessage = "A localização deve ter no máximo 255 caracteres.")]
    public string DescricaoLocalizacao { get; set; } = "";

    [Range(typeof(decimal), "0.01", "999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "A capacidade deve ser maior que zero.")]
    public decimal CapacidadeLitros { get; set; }

    public bool Ativo { get; set; } = true;
}

// Dados necessários para cadastrar um usuário
public class UsuarioEntrada
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome deve ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [MaxLength(180, ErrorMessage = "O e-mail deve ter no máximo 180 caracteres.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = "";

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    [RegularExpression(@"^(CIDADAO|ADMIN)$",
        ErrorMessage = "O papel deve ser CIDADAO ou ADMIN.")]
    public string Papel { get; set; } = "CIDADAO";
}

// Dados permitidos na atualização de um usuário
public class UsuarioAtualizacao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome deve ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [MaxLength(180, ErrorMessage = "O e-mail deve ter no máximo 180 caracteres.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    [RegularExpression(@"^(CIDADAO|ADMIN)$",
        ErrorMessage = "O papel deve ser CIDADAO ou ADMIN.")]
    public string Papel { get; set; } = "CIDADAO";
}

// Dados para registrar uma ocorrência em um ponto de coleta
public class OcorrenciaEntrada
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Informe um ponto de coleta válido.")]
    public int PontoColetaId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Informe um usuário válido.")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "O tipo da ocorrência é obrigatório.")]
    [MaxLength(32, ErrorMessage = "O tipo deve ter no máximo 32 caracteres.")]
    [RegularExpression(
        @"^(LIXEIRA_CHEIA|DESCARTE_IRREGULAR|ESTRUTURA_DANIFICADA|OUTRO)$",
        ErrorMessage = "Informe um tipo de ocorrência válido.")]
    public string Tipo { get; set; } = "";

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string Descricao { get; set; } = "";
}

// Dados para atualizar uma ocorrência existente
public class OcorrenciaAtualizacao
{
    [Required(ErrorMessage = "O tipo da ocorrência é obrigatório.")]
    [MaxLength(32, ErrorMessage = "O tipo deve ter no máximo 32 caracteres.")]
    [RegularExpression(
        @"^(LIXEIRA_CHEIA|DESCARTE_IRREGULAR|ESTRUTURA_DANIFICADA|OUTRO)$",
        ErrorMessage = "Informe um tipo de ocorrência válido.")]
    public string Tipo { get; set; } = "";

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "O status é obrigatório.")]
    [MaxLength(20, ErrorMessage = "O status deve ter no máximo 20 caracteres.")]
    [RegularExpression(@"^(ABERTA|EM_ATENDIMENTO|RESOLVIDA)$",
        ErrorMessage = "O status deve ser ABERTA, EM_ATENDIMENTO ou RESOLVIDA.")]
    public string Status { get; set; } = "";
}

// Dados recebidos de uma leitura realizada pelo sensor
public class LeituraEntrada
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Informe um ponto de coleta válido.")]
    public int PontoColetaId { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "A distância deve ser maior que zero.")]
    public decimal DistanciaCm { get; set; }

    [Range(typeof(decimal), "0", "100",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "O nível de preenchimento deve estar entre 0 e 100.")]
    public decimal NivelPercentual { get; set; }
}
