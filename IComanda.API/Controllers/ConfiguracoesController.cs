using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.Dtos;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracoesRepository _repo;
    private readonly ILogger<ConfiguracoesController> _logger;
    private readonly IConfiguration _configuration;

    public ConfiguracoesController(
        IConfiguracoesRepository repo,
        ILogger<ConfiguracoesController> logger,
        IConfiguration configuration)
    {
        _repo = repo;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>Retorna as configurações gerais do sistema.</summary>
    [HttpGet("sistema")]
    [ProducesResponseType(typeof(ConfiguracaoSistemaDto), 200)]
    public async Task<ActionResult<ConfiguracaoSistemaDto>> GetSistema()
    {
        try
        {
            var usarDelivery     = await _repo.GetUsarDeliveryAsync();
            var usarForcaVendas  = await _repo.GetUsarForcaVendasAsync();
            var usarComanda      = await _repo.GetUsarComandaAsync();
            var habilitarImprimirDuasVias = await _repo.GetHabilitarImprimirDuasViasAsync();
            return Ok(new ConfiguracaoSistemaDto
            {
                UsarDelivery    = usarDelivery,
                UsarForcaVendas = usarForcaVendas,
                UsarComanda     = usarComanda,
                HabilitarImprimirDuasVias = habilitarImprimirDuasVias
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ler configurações do sistema");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Salva as configurações gerais do sistema.</summary>
    [HttpPut("sistema")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> PutSistema([FromBody] ConfiguracaoSistemaDto dto)
    {
        try
        {
            await _repo.SetUsarDeliveryAsync(dto.UsarDelivery);
            await _repo.SetUsarForcaVendasAsync(dto.UsarForcaVendas);
            await _repo.SetUsarComandaAsync(dto.UsarComanda);
            await _repo.SetHabilitarImprimirDuasViasAsync(dto.HabilitarImprimirDuasVias);
            _logger.LogInformation("[Configuracoes] UsarDelivery={D} UsarForcaVendas={F} UsarComanda={C} HabilitarImprimirDuasVias={V}",
                dto.UsarDelivery, dto.UsarForcaVendas, dto.UsarComanda, dto.HabilitarImprimirDuasVias);
            return Ok(new { mensagem = "Configurações salvas com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configurações do sistema");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Realiza backup do banco de dados para C:\IComanda\Backup.</summary>
    [HttpPost("backup")]
    [ProducesResponseType(200)]
    public IActionResult FazerBackup()
    {
        try
        {
            // Extrair caminho do banco da connection string
            var connStr = _configuration.GetConnectionString("Firebird") ?? "";
            var dbPath = connStr
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Substring("Database=".Length).Trim())
                .FirstOrDefault();

            if (string.IsNullOrEmpty(dbPath) || !System.IO.File.Exists(dbPath))
            {
                _logger.LogWarning("[Backup] Arquivo do banco não encontrado. Path='{P}'", dbPath);
                return BadRequest(new { error = $"Arquivo do banco de dados não encontrado: {dbPath}" });
            }

            var backupDir = @"C:\IComanda\Backup";
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dbFileName = Path.GetFileNameWithoutExtension(dbPath);
            var backupFileName = $"{dbFileName}_backup_{timestamp}.FDB";
            var backupPath = Path.Combine(backupDir, backupFileName);

            System.IO.File.Copy(dbPath, backupPath, overwrite: false);

            var tamanhoMb = Math.Round(new FileInfo(backupPath).Length / 1048576.0, 2);
            _logger.LogInformation("[Backup] Backup criado: {F} ({T} MB)", backupFileName, tamanhoMb);

            return Ok(new
            {
                mensagem = "Backup realizado com sucesso!",
                arquivo  = backupFileName,
                caminho  = backupPath,
                tamanhoMb
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar backup do banco de dados");
            return StatusCode(500, new { error = $"Erro ao realizar backup: {ex.Message}" });
        }
    }
}
