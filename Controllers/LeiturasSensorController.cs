using Binova.Api.Contratos;
using Binova.Api.Dados;
using Binova.Api.Modelos;
using Binova.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Controllers;

[ApiController]
[Route("api/leituras-sensor")]
public class LeiturasSensorController(BinovaDbContext banco) : ControllerBase
{
    private static object Resposta(LeituraSensor l) => new
    { l.Id, l.PontoColetaId, l.DistanciaCm, l.NivelPercentual, l.RegistradoEm };

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? pontoColetaId)
    {
        var consulta = banco.LeiturasSensor.AsNoTracking().AsQueryable();
        if (pontoColetaId.HasValue) consulta = consulta.Where(l => l.PontoColetaId == pontoColetaId.Value);
        var leituras = await consulta.OrderByDescending(l => l.RegistradoEm).ToListAsync();
        return Ok(leituras.Select(Resposta));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var leitura = await banco.LeiturasSensor.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        return leitura is null ? NotFound(new { erro = "Leitura nao encontrada." }) : Ok(Resposta(leitura));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LeituraEntrada entrada)
    {
        if (!await banco.PontosColeta.AnyAsync(p => p.Id == entrada.PontoColetaId && p.Ativo))
            return BadRequest(new { erro = "Ponto inexistente ou inativo." });
        var leitura = new LeituraSensor
        {
            Id = await SequenciaService.ProximoIdAsync(banco, "LEITURA_SENSOR"),
            PontoColetaId = entrada.PontoColetaId,
            DistanciaCm = entrada.DistanciaCm,
            NivelPercentual = entrada.NivelPercentual
        };
        banco.LeiturasSensor.Add(leitura);
        await banco.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = leitura.Id }, Resposta(leitura));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] LeituraEntrada entrada)
    {
        var leitura = await banco.LeiturasSensor.FindAsync(id);
        if (leitura is null) return NotFound(new { erro = "Leitura nao encontrada." });
        if (!await banco.PontosColeta.AnyAsync(p => p.Id == entrada.PontoColetaId && p.Ativo))
            return BadRequest(new { erro = "Ponto inexistente ou inativo." });
        leitura.PontoColetaId = entrada.PontoColetaId;
        leitura.DistanciaCm = entrada.DistanciaCm;
        leitura.NivelPercentual = entrada.NivelPercentual;
        await banco.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var leitura = await banco.LeiturasSensor.FindAsync(id);
        if (leitura is null) return NotFound(new { erro = "Leitura nao encontrada." });
        banco.LeiturasSensor.Remove(leitura);
        await banco.SaveChangesAsync();
        return NoContent();
    }
}
