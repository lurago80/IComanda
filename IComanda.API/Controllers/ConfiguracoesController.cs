using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.Dtos;
using IComanda.API.Models.DTOs;
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
    private readonly IWebHostEnvironment _env;

    public ConfiguracoesController(
        IConfiguracoesRepository repo,
        ILogger<ConfiguracoesController> logger,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _repo = repo;
        _logger = logger;
        _configuration = configuration;
        _env = env;
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
            var usarCozinha      = await _repo.GetUsarCozinhaAsync();
            var usarCardapio     = await _repo.GetUsarCardapioAsync();
            return Ok(new ConfiguracaoSistemaDto
            {
                UsarDelivery    = usarDelivery,
                UsarForcaVendas = usarForcaVendas,
                UsarComanda     = usarComanda,
                HabilitarImprimirDuasVias = habilitarImprimirDuasVias,
                UsarCozinha     = usarCozinha,
                UsarCardapio    = usarCardapio
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
            await _repo.SetUsarCozinhaAsync(dto.UsarCozinha);
            await _repo.SetUsarCardapioAsync(dto.UsarCardapio);
            _logger.LogInformation("[Configuracoes] UsarDelivery={D} UsarForcaVendas={F} UsarComanda={C} HabilitarImprimirDuasVias={V} UsarCozinha={K} UsarCardapio={A}",
                dto.UsarDelivery, dto.UsarForcaVendas, dto.UsarComanda, dto.HabilitarImprimirDuasVias, dto.UsarCozinha, dto.UsarCardapio);
            return Ok(new { mensagem = "Configurações salvas com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configurações do sistema");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Realiza backup do banco de dados para C:\IComanda\Backup, compacta em ZIP e envia por email se configurado.</summary>
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

            // Copiar arquivo do banco
            System.IO.File.Copy(dbPath, backupPath, overwrite: false);

            // Compactar em ZIP
            var zipFileName = $"{dbFileName}_backup_{timestamp}.zip";
            var zipPath = Path.Combine(backupDir, zipFileName);
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(backupPath, backupFileName);

            // Remover cópia descompactada
            System.IO.File.Delete(backupPath);

            var tamanhoMb = Math.Round(new FileInfo(zipPath).Length / 1048576.0, 2);
            _logger.LogInformation("[Backup] Backup criado: {F} ({T} MB)", zipFileName, tamanhoMb);

            // Enviar por email se configurado
            var smtpHost    = _configuration["EmailBackup:SmtpHost"] ?? "";
            var destinatario = _configuration["EmailBackup:Destinatario"] ?? "";
            bool emailEnviado = false;
            string? emailErro = null;

            if (!string.IsNullOrEmpty(smtpHost) && !string.IsNullOrEmpty(destinatario))
            {
                try
                {
                    var smtpPort = int.TryParse(_configuration["EmailBackup:SmtpPort"], out var p) ? p : 587;
                    var smtpSsl  = !(_configuration["EmailBackup:SmtpUseSsl"]?.Equals("false", StringComparison.OrdinalIgnoreCase) ?? false);
                    var smtpUser = _configuration["EmailBackup:Usuario"] ?? "";
                    var smtpPass = _configuration["EmailBackup:Senha"] ?? "";
                    var remetente    = _configuration["EmailBackup:Remetente"] ?? smtpUser;
                    var nomeRemetente = _configuration["EmailBackup:NomeRemetente"] ?? "IComanda Backup";

                    // Ignora validação de nome do certificado SSL (comum em provedores compartilhados
                    // cujo cert é emitido para o domínio do host, não para o alias do cliente)
                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;

                    using var smtp = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                    {
                        EnableSsl   = smtpSsl,
                        Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass)
                    };

                    using var mail = new System.Net.Mail.MailMessage
                    {
                        From        = new System.Net.Mail.MailAddress(remetente, nomeRemetente),
                        Subject     = $"[IComanda] Backup - {DateTime.Now:dd/MM/yyyy HH:mm}",
                        Body        = $"Backup do banco de dados realizado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}.\n\nArquivo: {zipFileName}\nTamanho: {tamanhoMb} MB\n\nEste é um envio automático do sistema IComanda.",
                        IsBodyHtml  = false
                    };
                    mail.To.Add(destinatario);
                    mail.Attachments.Add(new System.Net.Mail.Attachment(zipPath));

                    smtp.Send(mail);
                    emailEnviado = true;
                    _logger.LogInformation("[Backup] Email enviado para {D}", destinatario);
                }
                catch (Exception emailEx)
                {
                    emailErro = emailEx.Message;
                    _logger.LogWarning(emailEx, "[Backup] Falha ao enviar email de backup");
                }
            }

            var mensagem = emailEnviado
                ? "Backup realizado e enviado por email com sucesso!"
                : "Backup realizado com sucesso!";

            return Ok(new
            {
                mensagem,
                arquivo      = zipFileName,
                caminho      = zipPath,
                tamanhoMb,
                emailEnviado,
                emailDestino = emailEnviado ? destinatario : (string?)null,
                emailErro
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar backup do banco de dados");
            return StatusCode(500, new { error = $"Erro ao realizar backup: {ex.Message}" });
        }
    }

    /// <summary>Retorna as configurações de email para backup (sem expor a senha).</summary>
    [HttpGet("backup-email")]
    [ProducesResponseType(200)]
    public IActionResult GetBackupEmail()
    {
        return Ok(new
        {
            smtpHost       = _configuration["EmailBackup:SmtpHost"] ?? "",
            smtpPort       = int.TryParse(_configuration["EmailBackup:SmtpPort"], out var p) ? p : 587,
            smtpUseSsl     = !(_configuration["EmailBackup:SmtpUseSsl"]?.Equals("false", StringComparison.OrdinalIgnoreCase) ?? false),
            usuario        = _configuration["EmailBackup:Usuario"] ?? "",
            senhaCadastrada = !string.IsNullOrEmpty(_configuration["EmailBackup:Senha"]),
            remetente      = _configuration["EmailBackup:Remetente"] ?? "",
            nomeRemetente  = _configuration["EmailBackup:NomeRemetente"] ?? "IComanda Backup",
            destinatario   = _configuration["EmailBackup:Destinatario"] ?? ""
        });
    }

    /// Encontra o diretório raiz do projeto onde appsettings.json está localizado,
    /// independente de onde o processo foi iniciado.
    private string FindProjectRoot()
    {
        // 1. Verifica o ContentRootPath (correto quando iniciado pelo bat do projeto)
        if (System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "appsettings.json")))
            return _env.ContentRootPath;

        // 2. Sobe até 5 níveis a partir do diretório do executável (bin/Release/net8.0/)
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 5; i++)
        {
            if (System.IO.File.Exists(Path.Combine(dir, "appsettings.json")))
                return dir;
            var parent = Path.GetDirectoryName(dir);
            if (parent == null) break;
            dir = parent;
        }

        // 3. Fallback: ContentRootPath
        return _env.ContentRootPath;
    }

    /// <summary>Salva as configurações de email para backup no appsettings.local.json.</summary>
    [HttpPut("backup-email")]
    [ProducesResponseType(200)]
    public IActionResult PutBackupEmail([FromBody] BackupEmailDto dto)
    {
        try
        {
            var localPath = Path.Combine(FindProjectRoot(), "appsettings.local.json");

            JsonNode root;
            if (System.IO.File.Exists(localPath))
            {
                var text = System.IO.File.ReadAllText(localPath);
                root = JsonNode.Parse(text) ?? new JsonObject();
            }
            else
            {
                root = new JsonObject();
            }

            var emailSection = new JsonObject
            {
                ["SmtpHost"]     = dto.SmtpHost,
                ["SmtpPort"]     = dto.SmtpPort,
                ["SmtpUseSsl"]   = dto.SmtpUseSsl,
                ["Usuario"]      = dto.Usuario,
                ["Remetente"]    = dto.Remetente,
                ["NomeRemetente"] = dto.NomeRemetente,
                ["Destinatario"] = dto.Destinatario
            };

            // Manter senha existente se nova não foi informada
            if (!string.IsNullOrEmpty(dto.Senha))
                emailSection["Senha"] = dto.Senha;
            else
            {
                var existingSenha = root["EmailBackup"]?["Senha"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(existingSenha))
                    emailSection["Senha"] = existingSenha;
                else
                    emailSection["Senha"] = "";
            }

            root["EmailBackup"] = emailSection;

            System.IO.File.WriteAllText(
                localPath,
                root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            if (_configuration is IConfigurationRoot configRoot)
                configRoot.Reload();

            _logger.LogInformation("[Backup] Configurações de email salvas");
            return Ok(new { mensagem = "Configurações de email salvas com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configurações de email");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
