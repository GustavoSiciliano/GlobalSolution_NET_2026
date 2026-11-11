
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
    // Organiza os dados da leitura retornados pela API
    private static object MontarResposta(LeituraSensor leitura)
    {
        return new
        {
            leitura.Id,
            leitura.PontoColetaId,
            leitura.DistanciaCm,
            leitura.NivelPercentual,
            leitura.RegistradoEm
        };
    }

    // Lista as leituras, permitindo filtrar por ponto de coleta
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? pontoColetaId)
    {
        var consulta = banco.LeiturasSensor.AsNoTracking();

        if (pontoColetaId.HasValue)
        {
            if (pontoColetaId.Value <= 0)
            {
                return BadRequest(new
                {
                    erro = "Informe um identificador de ponto de coleta válido."
                });
            }

            consulta = consulta.Where(leitura =>
                leitura.PontoColetaId == pontoColetaId.Value);
        }

        var leituras = await consulta
            .OrderByDescending(leitura => leitura.RegistradoEm)
            .ToListAsync();

        return Ok(leituras.Select(MontarResposta));
    }

    // Consulta uma leitura pelo identificador
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var leitura = await banco.LeiturasSensor
            .AsNoTracking()
            .FirstOrDefaultAsync(leitura => leitura.Id == id);

        if (leitura is null)
        {
            return NotFound(new
            {
                erro = "Leitura de sensor não encontrada."
            });
        }

        return Ok(MontarResposta(leitura));
    }

    // Registra uma leitura vinculada a um ponto de coleta ativo
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] LeituraEntrada entrada)
    {
        bool pontoAtivo = await banco.PontosColeta
            .AnyAsync(ponto =>
                ponto.Id == entrada.PontoColetaId && ponto.Ativo);

        if (!pontoAtivo)
        {
            return BadRequest(new
            {
                erro = "O ponto de coleta não existe ou está inativo."
            });
        }

        var leitura = new LeituraSensor
        {
            Id = await SequenciaService.ProximoIdAsync(
                banco, "LEITURA_SENSOR"),

            PontoColetaId = entrada.PontoColetaId,
            DistanciaCm = entrada.DistanciaCm,
            NivelPercentual = entrada.NivelPercentual
        };

        banco.LeiturasSensor.Add(leitura);
        await banco.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Buscar),
            new { id = leitura.Id },
            MontarResposta(leitura)
        );
    }

    // Atualiza os dados de uma leitura existente
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] LeituraEntrada entrada)
    {
        var leitura = await banco.LeiturasSensor.FindAsync(id);

        if (leitura is null)
        {
            return NotFound(new
            {
                erro = "Leitura de sensor não encontrada."
            });
        }

        bool pontoAtivo = await banco.PontosColeta
            .AnyAsync(ponto =>
                ponto.Id == entrada.PontoColetaId && ponto.Ativo);

        if (!pontoAtivo)
        {
            return BadRequest(new
            {
                erro = "O ponto de coleta não existe ou está inativo."
            });
        }

        leitura.PontoColetaId = entrada.PontoColetaId;
        leitura.DistanciaCm = entrada.DistanciaCm;
        leitura.NivelPercentual = entrada.NivelPercentual;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Exclui uma leitura sem remover o ponto de coleta
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var leitura = await banco.LeiturasSensor.FindAsync(id);

        if (leitura is null)
        {
            return NotFound(new
            {
                erro = "Leitura de sensor não encontrada."
            });
        }

        banco.LeiturasSensor.Remove(leitura);
        await banco.SaveChangesAsync();

        return NoContent();
    }
}
