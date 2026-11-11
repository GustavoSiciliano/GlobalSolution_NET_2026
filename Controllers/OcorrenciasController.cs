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
    private static readonly string[] Tipos =
        ["LIXEIRA_CHEIA", "DESCARTE_IRREGULAR", "ESTRUTURA_DANIFICADA", "OUTRO"];
    private static readonly string[] Situacoes = ["ABERTA", "EM_ATENDIMENTO", "RESOLVIDA"];
    private static object Resposta(Ocorrencia o) => new
    { o.Id, o.PontoColetaId, o.UsuarioId, o.Tipo, o.Descricao, o.Status, o.CriadoEm, o.AtualizadoEm };

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? pontoColetaId, [FromQuery] string? status)
    {
        var consulta = banco.Ocorrencias.AsNoTracking().AsQueryable();
        if (pontoColetaId.HasValue) consulta = consulta.Where(o => o.PontoColetaId == pontoColetaId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var filtro = status.Trim().ToUpperInvariant();
            consulta = consulta.Where(o => o.Status == filtro);
        }
        var ocorrencias = await consulta.OrderBy(o => o.Id).ToListAsync();
        return Ok(ocorrencias.Select(Resposta));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var ocorrencia = await banco.Ocorrencias.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return ocorrencia is null ? NotFound(new { erro = "Ocorrencia nao encontrada." }) : Ok(Resposta(ocorrencia));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] OcorrenciaEntrada entrada)
    {
        var tipo = entrada.Tipo?.Trim().ToUpperInvariant() ?? "";
        if (!Tipos.Contains(tipo) || string.IsNullOrWhiteSpace(entrada.Descricao))
            return BadRequest(new { erro = "Tipo ou descricao invalida." });
        if (!await banco.PontosColeta.AnyAsync(p => p.Id == entrada.PontoColetaId && p.Ativo))
            return BadRequest(new { erro = "Ponto inexistente ou inativo." });
        if (!await banco.Usuarios.AnyAsync(u => u.Id == entrada.UsuarioId))
            return BadRequest(new { erro = "Usuario inexistente." });
        var ocorrencia = new Ocorrencia
        {
            Id = await SequenciaService.ProximoIdAsync(banco, "OCORRENCIA"),
            PontoColetaId = entrada.PontoColetaId, UsuarioId = entrada.UsuarioId,
            Tipo = tipo, Descricao = entrada.Descricao.Trim(), Status = "ABERTA"
        };
        banco.Ocorrencias.Add(ocorrencia);
        await banco.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = ocorrencia.Id }, Resposta(ocorrencia));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] OcorrenciaAtualizacao entrada)
    {
        var ocorrencia = await banco.Ocorrencias.FindAsync(id);
        if (ocorrencia is null) return NotFound(new { erro = "Ocorrencia nao encontrada." });
        var tipo = entrada.Tipo?.Trim().ToUpperInvariant() ?? "";
        var status = entrada.Status?.Trim().ToUpperInvariant() ?? "";
        if (!Tipos.Contains(tipo) || !Situacoes.Contains(status) || string.IsNullOrWhiteSpace(entrada.Descricao))
            return BadRequest(new { erro = "Tipo, status ou descricao invalida." });
        ocorrencia.Tipo = tipo;
        ocorrencia.Descricao = entrada.Descricao.Trim();
        ocorrencia.Status = status;
        ocorrencia.AtualizadoEm = DateTimeOffset.UtcNow;
        await banco.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var ocorrencia = await banco.Ocorrencias.FindAsync(id);
        if (ocorrencia is null) return NotFound(new { erro = "Ocorrencia nao encontrada." });
        banco.Ocorrencias.Remove(ocorrencia);
        await banco.SaveChangesAsync();
        return NoContent();
    }
}
