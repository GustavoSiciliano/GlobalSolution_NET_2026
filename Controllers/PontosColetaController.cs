
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
    // Monta a resposta com os dados do ponto de coleta
    private static object MontarResposta(PontoColeta ponto)
    {
        return new
        {
            ponto.Id,
            ponto.Nome,
            ponto.DescricaoLocalizacao,
            ponto.CapacidadeLitros,
            ponto.Ativo,
            ponto.CriadoEm
        };
    }

    // Lista os pontos de coleta, permitindo incluir os inativos
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] bool incluirInativos = false)
    {
        var consulta = banco.PontosColeta.AsNoTracking();

        if (!incluirInativos)
        {
            consulta = consulta.Where(ponto => ponto.Ativo);
        }

        var pontos = await consulta
            .OrderBy(ponto => ponto.Id)
            .ToListAsync();

        return Ok(pontos.Select(MontarResposta));
    }

    // Consulta um ponto de coleta pelo identificador
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var ponto = await banco.PontosColeta
            .AsNoTracking()
            .FirstOrDefaultAsync(ponto => ponto.Id == id);

        if (ponto is null)
        {
            return NotFound(new
            {
                erro = "Ponto de coleta não encontrado."
            });
        }

        return Ok(MontarResposta(ponto));
    }

    // Cadastra um novo ponto de coleta
    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] PontoEntrada entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Nome) ||
            string.IsNullOrWhiteSpace(entrada.DescricaoLocalizacao))
        {
            return BadRequest(new
            {
                erro = "O nome e a localização são obrigatórios."
            });
        }

        var ponto = new PontoColeta
        {
            Id = await SequenciaService.ProximoIdAsync(
                banco, "PONTO_COLETA"),

            Nome = entrada.Nome.Trim(),
            DescricaoLocalizacao = entrada.DescricaoLocalizacao.Trim(),
            CapacidadeLitros = entrada.CapacidadeLitros,
            Ativo = entrada.Ativo
        };

        banco.PontosColeta.Add(ponto);
        await banco.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Buscar),
            new { id = ponto.Id },
            MontarResposta(ponto)
        );
    }

    // Atualiza os dados de um ponto de coleta existente
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] PontoEntrada entrada)
    {
        var ponto = await banco.PontosColeta.FindAsync(id);

        if (ponto is null)
        {
            return NotFound(new
            {
                erro = "Ponto de coleta não encontrado."
            });
        }

        if (string.IsNullOrWhiteSpace(entrada.Nome) ||
            string.IsNullOrWhiteSpace(entrada.DescricaoLocalizacao))
        {
            return BadRequest(new
            {
                erro = "O nome e a localização são obrigatórios."
            });
        }

        ponto.Nome = entrada.Nome.Trim();
        ponto.DescricaoLocalizacao = entrada.DescricaoLocalizacao.Trim();
        ponto.CapacidadeLitros = entrada.CapacidadeLitros;
        ponto.Ativo = entrada.Ativo;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Desativa um ponto sem excluir seu histórico
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var ponto = await banco.PontosColeta.FindAsync(id);

        if (ponto is null || !ponto.Ativo)
        {
            return NotFound(new
            {
                erro = "Ponto de coleta não encontrado ou já inativo."
            });
        }

        ponto.Ativo = false;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Consulta a situação do ponto com base na última leitura
    [HttpGet("{id:int}/situacao")]
    public async Task<IActionResult> Situacao(int id)
    {
        var ponto = await banco.PontosColeta
            .AsNoTracking()
            .FirstOrDefaultAsync(ponto => ponto.Id == id);

        if (ponto is null)
        {
            return NotFound(new
            {
                erro = "Ponto de coleta não encontrado."
            });
        }

        var ultimaLeitura = await banco.LeiturasSensor
            .AsNoTracking()
            .Where(leitura => leitura.PontoColetaId == id)
            .OrderByDescending(leitura => leitura.RegistradoEm)
            .FirstOrDefaultAsync();

        string situacao;

        if (ultimaLeitura is null ||
            ultimaLeitura.RegistradoEm < DateTimeOffset.UtcNow.AddMinutes(-15))
        {
            situacao = "SEM_DADOS_ATUALIZADOS";
        }
        else if (ultimaLeitura.NivelPercentual >= 80)
        {
            situacao = "PROXIMA_DO_LIMITE";
        }
        else if (ultimaLeitura.NivelPercentual >= 50)
        {
            situacao = "ATENCAO";
        }
        else
        {
            situacao = "DISPONIVEL";
        }

        return Ok(new
        {
            pontoId = ponto.Id,
            ponto = ponto.Nome,
            situacao,
            nivelPercentual = ultimaLeitura?.NivelPercentual,
            ultimaLeitura = ultimaLeitura?.RegistradoEm
        });
    }
}
