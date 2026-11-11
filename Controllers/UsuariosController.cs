using Binova.Api.Contratos;
using Binova.Api.Dados;
using Binova.Api.Modelos;
using Binova.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(BinovaDbContext banco) : ControllerBase
{
    private static object Resposta(Usuario u) => new { u.Id, u.Nome, u.Email, u.Papel, u.CriadoEm };
    private static bool PapelValido(string papel) => papel is "CIDADAO" or "ADMIN";

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await banco.Usuarios.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        return Ok(usuarios.Select(Resposta));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var usuario = await banco.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return usuario is null ? NotFound(new { erro = "Usuario nao encontrado." }) : Ok(Resposta(usuario));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] UsuarioEntrada entrada)
    {
        var papel = entrada.Papel?.Trim().ToUpperInvariant() ?? "";
        var email = entrada.Email?.Trim().ToLowerInvariant() ?? "";
        if (string.IsNullOrWhiteSpace(entrada.Nome) || !PapelValido(papel))
            return BadRequest(new { erro = "Nome ou papel invalido." });
        if (await banco.Usuarios.AnyAsync(u => u.Email == email))
            return BadRequest(new { erro = "Email ja cadastrado." });

        var usuario = new Usuario
        {
            Id = await SequenciaService.ProximoIdAsync(banco, "USUARIO"),
            Nome = entrada.Nome.Trim(), Email = email, Papel = papel,
            SenhaHash = SenhaService.CriarHash(entrada.Senha)
        };
        banco.Usuarios.Add(usuario);
        await banco.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = usuario.Id }, Resposta(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UsuarioAtualizacao entrada)
    {
        var usuario = await banco.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { erro = "Usuario nao encontrado." });
        var papel = entrada.Papel?.Trim().ToUpperInvariant() ?? "";
        var email = entrada.Email?.Trim().ToLowerInvariant() ?? "";
        if (string.IsNullOrWhiteSpace(entrada.Nome) || !PapelValido(papel))
            return BadRequest(new { erro = "Nome ou papel invalido." });
        if (await banco.Usuarios.AnyAsync(u => u.Email == email && u.Id != id))
            return BadRequest(new { erro = "Email ja utilizado." });
        usuario.Nome = entrada.Nome.Trim(); usuario.Email = email; usuario.Papel = papel;
        await banco.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var usuario = await banco.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { erro = "Usuario nao encontrado." });
        if (await banco.Ocorrencias.AnyAsync(o => o.UsuarioId == id))
            return BadRequest(new { erro = "Usuario possui ocorrencias. Exclua os testes vinculados primeiro." });
        banco.Usuarios.Remove(usuario);
        await banco.SaveChangesAsync();
        return NoContent();
    }
}
