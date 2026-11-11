using Binova.Api.Contratos;
using Binova.Api.Dados;
using Binova.Api.Modelos;
using Binova.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Controllers;

[ApiController]
[Route("api/pontos-coleta")]
public class PontosColetaController(BinovaDbContext banco) : ControllerBase
{
    private static object Resposta(PontoColeta p) => new
    {
        p.Id, p.Nome, p.DescricaoLocalizacao, p.CapacidadeLitros, p.Ativo, p.CriadoEm
    };

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInativos = false)
    {
        var consulta = banco.PontosColeta.AsNoTracking();
        if (!incluirInativos) consulta = consulta.Where(p => p.Ativo);
        var pontos = await consulta.OrderBy(p => p.Id).ToListAsync();
        return Ok(pontos.Select(Resposta));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var ponto = await banco.PontosColeta.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return ponto is null ? NotFound(new { erro = "Ponto de coleta nao encontrado." }) : Ok(Resposta(ponto));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PontoEntrada entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Nome) || string.IsNullOrWhiteSpace(entrada.DescricaoLocalizacao))
            return BadRequest(new { erro = "Nome e localizacao sao obrigatorios." });

        var ponto = new PontoColeta
        {
            Id = await SequenciaService.ProximoIdAsync(banco, "PONTO_COLETA"),
            Nome = entrada.Nome.Trim(), DescricaoLocalizacao = entrada.DescricaoLocalizacao.Trim(),
            CapacidadeLitros = entrada.CapacidadeLitros, Ativo = entrada.Ativo
        };
        banco.PontosColeta.Add(ponto);
        await banco.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = ponto.Id }, Resposta(ponto));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PontoEntrada entrada)
    {
        var ponto = await banco.PontosColeta.FindAsync(id);
        if (ponto is null) return NotFound(new { erro = "Ponto de coleta nao encontrado." });
        if (string.IsNullOrWhiteSpace(entrada.Nome) || string.IsNullOrWhiteSpace(entrada.DescricaoLocalizacao))
            return BadRequest(new { erro = "Nome e localizacao sao obrigatorios." });
        ponto.Nome = entrada.Nome.Trim();
        ponto.DescricaoLocalizacao = entrada.DescricaoLocalizacao.Trim();
        ponto.CapacidadeLitros = entrada.CapacidadeLitros;
        ponto.Ativo = entrada.Ativo;
        await banco.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var ponto = await banco.PontosColeta.FindAsync(id);
        if (ponto is null || !ponto.Ativo) return NotFound(new { erro = "Ponto nao encontrado ou ja inativo." });
        ponto.Ativo = false;
        await banco.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/situacao")]
    public async Task<IActionResult> Situacao(int id)
    {
        var ponto = await banco.PontosColeta.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (ponto is null) return NotFound(new { erro = "Ponto de coleta nao encontrado." });

        var ultima = await banco.LeiturasSensor.AsNoTracking()
            .Where(l => l.PontoColetaId == id)
            .OrderByDescending(l => l.RegistradoEm).FirstOrDefaultAsync();

        string situacao;
        if (ultima is null || ultima.RegistradoEm < DateTimeOffset.UtcNow.AddMinutes(-15))
            situacao = "SEM_DADOS_ATUALIZADOS";
        else if (ultima.NivelPercentual >= 80) situacao = "PROXIMA_DO_LIMITE";
        else if (ultima.NivelPercentual >= 50) situacao = "ATENCAO";
        else situacao = "DISPONIVEL";

        return Ok(new { pontoId = ponto.Id, ponto = ponto.Nome, situacao,
            nivelPercentual = ultima?.NivelPercentual, ultimaLeitura = ultima?.RegistradoEm });
    }
}
