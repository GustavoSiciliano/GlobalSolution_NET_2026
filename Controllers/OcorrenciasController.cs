
using Binova.Api.Contratos;
using Binova.Api.Dados;
using Binova.Api.Modelos;
using Binova.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Controllers;

[ApiController]
[Route("api/ocorrencias")]
public class OcorrenciasController(BinovaDbContext banco) : ControllerBase
{
    private static readonly string[] TiposPermitidos =
    [
        "LIXEIRA_CHEIA",
        "DESCARTE_IRREGULAR",
        "ESTRUTURA_DANIFICADA",
        "OUTRO"
    ];

    private static readonly string[] StatusPermitidos =
    [
        "ABERTA",
        "EM_ATENDIMENTO",
        "RESOLVIDA"
    ];

    // Organiza os dados retornados pela API
    private static object MontarResposta(Ocorrencia ocorrencia)
    {
        return new
        {
            ocorrencia.Id,
            ocorrencia.PontoColetaId,
            ocorrencia.UsuarioId,
            ocorrencia.Tipo,
            ocorrencia.Descricao,
            ocorrencia.Status,
            ocorrencia.CriadoEm,
            ocorrencia.AtualizadoEm
        };
    }

    // Lista ocorrências com filtros opcionais
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? pontoColetaId,
        [FromQuery] string? status)
    {
        var consulta = banco.Ocorrencias.AsNoTracking();

        if (pontoColetaId.HasValue)
        {
            consulta = consulta.Where(ocorrencia =>
                ocorrencia.PontoColetaId == pontoColetaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            string filtro = status.Trim().ToUpperInvariant();

            if (!StatusPermitidos.Contains(filtro))
            {
                return BadRequest(new
                {
                    erro = "O status informado é inválido."
                });
            }

            consulta = consulta.Where(ocorrencia =>
                ocorrencia.Status == filtro);
        }

        var ocorrencias = await consulta
            .OrderBy(ocorrencia => ocorrencia.Id)
            .ToListAsync();

        return Ok(ocorrencias.Select(MontarResposta));
    }

    // Consulta uma ocorrência pelo identificador
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var ocorrencia = await banco.Ocorrencias
            .AsNoTracking()
            .FirstOrDefaultAsync(ocorrencia => ocorrencia.Id == id);

        if (ocorrencia is null)
        {
            return NotFound(new
            {
                erro = "Ocorrência não encontrada."
            });
        }

        return Ok(MontarResposta(ocorrencia));
    }

    // Registra uma ocorrência vinculada a um usuário e ponto ativo
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] OcorrenciaEntrada entrada)
    {
        string tipo = entrada.Tipo.Trim().ToUpperInvariant();
        string descricao = entrada.Descricao.Trim();

        if (!TiposPermitidos.Contains(tipo) ||
            string.IsNullOrWhiteSpace(descricao))
        {
            return BadRequest(new
            {
                erro = "O tipo ou a descrição da ocorrência é inválido."
            });
        }

        bool pontoValido = (await banco.PontosColeta
            .CountAsync(ponto =>
                ponto.Id == entrada.PontoColetaId && ponto.Ativo)) > 0;

        if (!pontoValido)
        {
            return BadRequest(new
            {
                erro = "O ponto de coleta não existe ou está inativo."
            });
        }

        bool usuarioExiste = (await banco.Usuarios
            .CountAsync(usuario => usuario.Id == entrada.UsuarioId)) > 0;

        if (!usuarioExiste)
        {
            return BadRequest(new
            {
                erro = "O usuário informado não existe."
            });
        }

        var ocorrencia = new Ocorrencia
        {
            Id = await SequenciaService.ProximoIdAsync(
                banco, "OCORRENCIA"),

            PontoColetaId = entrada.PontoColetaId,
            UsuarioId = entrada.UsuarioId,
            Tipo = tipo,
            Descricao = descricao,
            Status = "ABERTA"
        };

        banco.Ocorrencias.Add(ocorrencia);
        await banco.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Buscar),
            new { id = ocorrencia.Id },
            MontarResposta(ocorrencia)
        );
    }

    // Atualiza o tipo, a descrição e o status de uma ocorrência
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] OcorrenciaAtualizacao entrada)
    {
        var ocorrencia = await banco.Ocorrencias.FindAsync(id);

        if (ocorrencia is null)
        {
            return NotFound(new
            {
                erro = "Ocorrência não encontrada."
            });
        }

        string tipo = entrada.Tipo.Trim().ToUpperInvariant();
        string status = entrada.Status.Trim().ToUpperInvariant();
        string descricao = entrada.Descricao.Trim();

        if (!TiposPermitidos.Contains(tipo) ||
            !StatusPermitidos.Contains(status) ||
            string.IsNullOrWhiteSpace(descricao))
        {
            return BadRequest(new
            {
                erro = "O tipo, o status ou a descrição é inválido."
            });
        }

        ocorrencia.Tipo = tipo;
        ocorrencia.Descricao = descricao;
        ocorrencia.Status = status;
        ocorrencia.AtualizadoEm = DateTimeOffset.UtcNow;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Exclui uma ocorrência cadastrada
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var ocorrencia = await banco.Ocorrencias.FindAsync(id);

        if (ocorrencia is null)
        {
            return NotFound(new
            {
                erro = "Ocorrência não encontrada."
            });
        }

        banco.Ocorrencias.Remove(ocorrencia);
        await banco.SaveChangesAsync();

        return NoContent();
    }
}
