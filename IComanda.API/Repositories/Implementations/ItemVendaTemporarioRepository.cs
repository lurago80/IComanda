using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class ItemVendaTemporarioRepository : IItemVendaTemporarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ItemVendaTemporarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CriarItensTemporariosAsync(List<ItemVendaTemporario> itens, System.Data.IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            const string sql = @"
                INSERT INTO frente_tmpitvendas (
                    cupom, n_caixa, data, hora, operador, item, codigo, barras,
                    descricao, qtd, preco, tributacao, icms, iss, und,
                    desconto, acrescimo, total, serial, tipo
                ) VALUES (
                    @Cupom, @NCaixa, @Data, @Hora, @Operador, @Item, @Codigo, @Barras,
                    @Descricao, @Qtd, @Preco, @Tributacao, @Icms, @Iss, @Und,
                    @Desconto, @Acrescimo, @Total, @Serial, @Tipo
                )";

            // Desmembrar itens com quantidade inteira em linhas individuais (qty=1 cada)
            // para que a verificação de lançamentos exiba horário por unidade.
            var linhasExpandidas = ExpandirItens(itens);
            var linhasAfetadas = await connection.ExecuteAsync(sql, linhasExpandidas, transaction: transaction);
            return linhasAfetadas > 0;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Expande itens com quantidade inteira > 1 em N linhas de qty=1 cada.
    /// Quantidades fracionárias (ex: 0,5 kg) são mantidas em uma única linha.
    /// </summary>
    private static IEnumerable<ItemVendaTemporario> ExpandirItens(IEnumerable<ItemVendaTemporario> itens)
    {
        var resultado = new List<ItemVendaTemporario>();
        var numeroItem = 0;

        foreach (var item in itens)
        {
            var qtdInteira = (int)Math.Floor(item.Qtd);
            var qtdResto   = item.Qtd - qtdInteira;

            if (qtdInteira >= 1)
            {
                for (int u = 0; u < qtdInteira; u++)
                {
                    resultado.Add(new ItemVendaTemporario
                    {
                        Cupom      = item.Cupom,
                        NCaixa     = item.NCaixa,
                        Data       = item.Data,
                        Hora       = item.Hora,
                        Operador   = item.Operador,
                        Item       = ++numeroItem,
                        Codigo     = item.Codigo,
                        Barras     = item.Barras,
                        Descricao  = item.Descricao,
                        Qtd        = 1,
                        Preco      = item.Preco,
                        Tributacao = item.Tributacao,
                        Icms       = item.Icms,
                        Iss        = item.Iss,
                        Und        = item.Und,
                        Desconto   = item.Desconto,
                        Acrescimo  = item.Acrescimo,
                        Total      = item.Preco,
                        Serial     = item.Serial,
                        Tipo       = item.Tipo
                    });
                }
            }

            if (qtdResto > 0)
            {
                resultado.Add(new ItemVendaTemporario
                {
                    Cupom      = item.Cupom,
                    NCaixa     = item.NCaixa,
                    Data       = item.Data,
                    Hora       = item.Hora,
                    Operador   = item.Operador,
                    Item       = ++numeroItem,
                    Codigo     = item.Codigo,
                    Barras     = item.Barras,
                    Descricao  = item.Descricao,
                    Qtd        = qtdResto,
                    Preco      = item.Preco,
                    Tributacao = item.Tributacao,
                    Icms       = item.Icms,
                    Iss        = item.Iss,
                    Und        = item.Und,
                    Desconto   = item.Desconto,
                    Acrescimo  = item.Acrescimo,
                    Total      = item.Preco * qtdResto,
                    Serial     = item.Serial,
                    Tipo       = item.Tipo
                });
            }
        }

        return resultado;
    }

    public async Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom, int operador)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo, 
                   barras, descricao, qtd, preco, tributacao, icms, iss, und,
                   desconto, acrescimo, total, serial, tipo
            FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND operador = @Operador AND tipo = 1
            ORDER BY data, hora, item";

        return await connection.QueryAsync<ItemVendaTemporario>(sql, new { Cupom = cupom, Operador = operador });
    }

    public async Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Normalizar a nota (tentar com e sem zeros)
        var cupomNormalizado = cupom.TrimStart('0');
        if (string.IsNullOrEmpty(cupomNormalizado))
        {
            cupomNormalizado = "0";
        }
        var cupomComZeros = cupom.PadLeft(6, '0');

        var sql = @"
            SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo, 
                   barras, descricao, qtd, preco, tributacao, icms, iss, und,
                   desconto, acrescimo, total, serial, tipo
            FROM frente_tmpitvendas 
            WHERE (cupom = @Cupom OR cupom = @CupomComZeros OR cupom = @CupomSemZeros) AND tipo = 1
            ORDER BY data, hora, item";

        return await connection.QueryAsync<ItemVendaTemporario>(sql, new { 
            Cupom = cupom, 
            CupomComZeros = cupomComZeros,
            CupomSemZeros = cupomNormalizado
        });
    }

    public async Task<bool> LimparItensCupomAsync(string cupom, int operador)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Filtrar por tipo = 1 para garantir que só deleta itens temporários
        var sql = @"
            DELETE FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND operador = @Operador AND tipo = 1";

        await connection.ExecuteAsync(sql, new { Cupom = cupom, Operador = operador });
        return true;
    }

    public async Task<bool> LimparItensCupomAsync(string cupom, int operador, System.Data.IDbTransaction? transaction)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();

        var sql = @"
            DELETE FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND operador = @Operador AND tipo = 1";

        await connection.ExecuteAsync(sql, new { Cupom = cupom, Operador = operador }, transaction: transaction);
        return true;
    }

    public async Task<bool> AtualizarItensTemporariosAsync(string cupom, int operador, List<ItemVendaTemporario> novosItens, System.Data.IDbTransaction? transaction = null)
    {
        Console.WriteLine($"🔄 [REPO] Iniciando atualização de itens temporários - Cupom: '{cupom}', Operador: {operador}, Itens: {novosItens.Count}");

        // Validar e normalizar itens antes de processar
        foreach (var item in novosItens)
        {
            if (string.IsNullOrEmpty(item.Cupom)) item.Cupom = cupom;
            if (item.Codigo <= 0) throw new Exception($"Item com código inválido: {item.Codigo}");
            if (item.Qtd <= 0) throw new Exception($"Item com quantidade inválida: {item.Qtd}");
            if (string.IsNullOrEmpty(item.NCaixa)) item.NCaixa = "1";
            if (string.IsNullOrEmpty(item.Barras)) item.Barras = "";
            if (string.IsNullOrEmpty(item.Descricao)) item.Descricao = $"Produto {item.Codigo}";
            if (string.IsNullOrEmpty(item.Tributacao)) item.Tributacao = "";
            if (string.IsNullOrEmpty(item.Und)) item.Und = "UN";
            if (string.IsNullOrEmpty(item.Serial)) item.Serial = "";
        }

        // Agregar itens por código de produto para evitar que múltiplas entradas com o mesmo
        // código causem cálculos de delta incorretos no merge. O frontend pode enviar várias
        // linhas com o mesmo código (uma por CartItem), mas o merge precisa de uma entrada
        // por código com a quantidade total desejada.
        novosItens = novosItens
            .GroupBy(x => x.Codigo)
            .Select(g =>
            {
                var primeiro = g.First();
                var totalQtd = g.Sum(x => x.Qtd);
                primeiro.Qtd   = totalQtd;
                primeiro.Total = primeiro.Preco * totalQtd;
                return primeiro;
            })
            .ToList();
        Console.WriteLine($"🔄 [REPO] Após agregação por código: {novosItens.Count} produto(s) — {novosItens.Sum(x => x.Qtd)} unidade(s) total");

        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null) throw new Exception("Falha ao criar conexão Firebird");

        var shouldDispose = transaction == null;
        var wasOpen = connection.State == System.Data.ConnectionState.Open;

        try
        {
            if (!wasOpen && transaction == null)
                connection.Open();

            // ── Carregar estado atual do banco ───────────────────────────────────
            var sqlSelect = @"
                SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo,
                       barras, descricao, qtd, preco, tributacao, icms, iss, und,
                       desconto, acrescimo, total, serial, tipo
                FROM frente_tmpitvendas
                WHERE cupom = @Cupom AND tipo = 1
                ORDER BY data, hora, item";

            var existentes = (await connection.QueryAsync<ItemVendaTemporario>(
                sqlSelect, new { Cupom = cupom }, transaction: transaction)).ToList();

            var existentesPorCodigo = existentes
                .GroupBy(x => x.Codigo)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Data).ThenBy(x => x.Hora).ToList());

            var maxItem = existentes.Any() ? existentes.Max(x => x.Item) : 0;
            var codigosNovos = new HashSet<int>(novosItens.Select(x => x.Codigo));

            // ── 1. Remover produtos que foram excluídos do pedido ────────────────
            var codigosRemovidos = existentesPorCodigo.Keys.Where(c => !codigosNovos.Contains(c)).ToList();
            if (codigosRemovidos.Any())
            {
                await connection.ExecuteAsync(
                    "DELETE FROM frente_tmpitvendas WHERE cupom = @Cupom AND tipo = 1 AND codigo = @Codigo",
                    codigosRemovidos.Select(c => new { Cupom = cupom, Codigo = c }),
                    transaction: transaction);
                Console.WriteLine($"🗑️ [REPO] Removido(s) {codigosRemovidos.Count} produto(s) excluído(s) do pedido");
            }

            var sqlInserir = @"
                INSERT INTO frente_tmpitvendas (
                    cupom, n_caixa, data, hora, operador, item, codigo, barras,
                    descricao, qtd, preco, tributacao, icms, iss, und,
                    desconto, acrescimo, total, serial, tipo
                ) VALUES (
                    @Cupom, @NCaixa, @Data, @Hora, @Operador, @Item, @Codigo, @Barras,
                    @Descricao, @Qtd, @Preco, @Tributacao, @Icms, @Iss, @Und,
                    @Desconto, @Acrescimo, @Total, @Serial, @Tipo
                )";

            // ── 2. Para cada produto no novo estado do carrinho ──────────────────
            foreach (var novoItem in novosItens)
            {
                var qtdNova = novoItem.Qtd;

                if (!existentesPorCodigo.ContainsKey(novoItem.Codigo))
                {
                    // Produto completamente novo — inserir com timestamp atual
                    novoItem.Item = ++maxItem;
                    novoItem.Data = DateTime.Now.Date;
                    novoItem.Hora = DateTime.Now.TimeOfDay;
                    await connection.ExecuteAsync(sqlInserir, novoItem, transaction: transaction);
                    Console.WriteLine($"➕ [REPO] Inserido novo produto {novoItem.Codigo} (qty={qtdNova}) item#{novoItem.Item}");
                }
                else
                {
                    var linhasExistentes = existentesPorCodigo[novoItem.Codigo];
                    var qtdExistente = linhasExistentes.Sum(x => (decimal)x.Qtd);
                    var delta = qtdNova - qtdExistente;

                    if (delta > 0)
                    {
                        // Acréscimo: inserir uma linha por unidade com timestamp atual
                        var qtdAcrescimoInteira = (int)Math.Floor(delta);
                        var qtdAcrescimoResto   = delta - qtdAcrescimoInteira;

                        for (int u = 0; u < qtdAcrescimoInteira; u++)
                        {
                            var linhaAdicao = new ItemVendaTemporario
                            {
                                Cupom      = cupom,
                                NCaixa     = novoItem.NCaixa,
                                Data       = DateTime.Now.Date,
                                Hora       = DateTime.Now.TimeOfDay,
                                Operador   = operador,
                                Item       = ++maxItem,
                                Codigo     = novoItem.Codigo,
                                Barras     = novoItem.Barras,
                                Descricao  = novoItem.Descricao,
                                Qtd        = 1,
                                Preco      = novoItem.Preco,
                                Tributacao = novoItem.Tributacao,
                                Icms       = novoItem.Icms,
                                Iss        = novoItem.Iss,
                                Und        = novoItem.Und,
                                Desconto   = novoItem.Desconto,
                                Acrescimo  = novoItem.Acrescimo,
                                Total      = novoItem.Preco,
                                Serial     = novoItem.Serial,
                                Tipo       = 1
                            };
                            await connection.ExecuteAsync(sqlInserir, linhaAdicao, transaction: transaction);
                        }

                        if (qtdAcrescimoResto > 0)
                        {
                            var linhaResto = new ItemVendaTemporario
                            {
                                Cupom      = cupom,
                                NCaixa     = novoItem.NCaixa,
                                Data       = DateTime.Now.Date,
                                Hora       = DateTime.Now.TimeOfDay,
                                Operador   = operador,
                                Item       = ++maxItem,
                                Codigo     = novoItem.Codigo,
                                Barras     = novoItem.Barras,
                                Descricao  = novoItem.Descricao,
                                Qtd        = qtdAcrescimoResto,
                                Preco      = novoItem.Preco,
                                Tributacao = novoItem.Tributacao,
                                Icms       = novoItem.Icms,
                                Iss        = novoItem.Iss,
                                Und        = novoItem.Und,
                                Desconto   = novoItem.Desconto,
                                Acrescimo  = novoItem.Acrescimo,
                                Total      = novoItem.Preco * qtdAcrescimoResto,
                                Serial     = novoItem.Serial,
                                Tipo       = 1
                            };
                            await connection.ExecuteAsync(sqlInserir, linhaResto, transaction: transaction);
                        }

                        Console.WriteLine($"➕ [REPO] Acréscimo de {delta} unidade(s) do produto {novoItem.Codigo} em linhas individuais");
                    }
                    else if (delta < 0)
                    {
                        // Redução: remover das linhas mais recentes
                        var aReduzir = -delta;
                        var linhasOrdenadas = linhasExistentes.OrderByDescending(x => x.Data).ThenByDescending(x => x.Hora).ToList();
                        foreach (var linha in linhasOrdenadas)
                        {
                            if (aReduzir <= 0) break;
                            if (aReduzir >= (decimal)linha.Qtd)
                            {
                                // Remover esta linha inteira
                                await connection.ExecuteAsync(
                                    "DELETE FROM frente_tmpitvendas WHERE cupom = @Cupom AND item = @Item AND tipo = 1",
                                    new { Cupom = cupom, linha.Item },
                                    transaction: transaction);
                                aReduzir -= (decimal)linha.Qtd;
                            }
                            else
                            {
                                // Redução parcial nesta linha
                                var novaQtd = (decimal)linha.Qtd - aReduzir;
                                await connection.ExecuteAsync(
                                    "UPDATE frente_tmpitvendas SET qtd = @Qtd, total = @Total WHERE cupom = @Cupom AND item = @Item AND tipo = 1",
                                    new { Qtd = novaQtd, Total = linha.Preco * novaQtd, Cupom = cupom, linha.Item },
                                    transaction: transaction);
                                aReduzir = 0;
                            }
                        }
                        Console.WriteLine($"➖ [REPO] Reduzido produto {novoItem.Codigo}: delta={delta}");
                    }
                    // Se delta == 0: nenhuma alteração necessária (preserva linhas e timestamps)
                }
            }

            // ── 3. Re-numerar os itens sequencialmente para garantir consistência ─
            var itensFinal = (await connection.QueryAsync<ItemVendaTemporario>(
                sqlSelect, new { Cupom = cupom }, transaction: transaction)).ToList();

            // Usar números negativos temporários para evitar conflito de PK durante renumeração
            for (int idx = 0; idx < itensFinal.Count; idx++)
            {
                await connection.ExecuteAsync(
                    "UPDATE frente_tmpitvendas SET item = @NovoItem WHERE cupom = @Cupom AND item = @ItemAtual AND tipo = 1",
                    new { NovoItem = -(idx + 1), Cupom = cupom, ItemAtual = itensFinal[idx].Item },
                    transaction: transaction);
            }
            for (int idx = 0; idx < itensFinal.Count; idx++)
            {
                await connection.ExecuteAsync(
                    "UPDATE frente_tmpitvendas SET item = @NovoItem WHERE cupom = @Cupom AND item = @ItemAtual AND tipo = 1",
                    new { NovoItem = idx + 1, Cupom = cupom, ItemAtual = -(idx + 1) },
                    transaction: transaction);
            }

            Console.WriteLine($"✅ [REPO] Merge concluído para cupom '{cupom}' — {itensFinal.Count} linhas no banco");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [REPO] Erro ao atualizar itens temporários: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            Console.WriteLine($"   Cupom: '{cupom}', Operador: {operador}, Itens: {novosItens.Count}");
            Console.WriteLine($"   Estado da conexão no erro: {connection?.State}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"   Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            throw;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                try
                {
                    if (connection is FbConnection fbConnection && fbConnection.State == System.Data.ConnectionState.Open)
                    {
                        fbConnection.Close();
                        Console.WriteLine($"🔌 [REPO] Conexão fechada no finally");
                    }
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [REPO] Erro ao fechar conexão: {ex.Message}");
                }
            }
        }
    }

    public async Task<ItemVendaTemporario?> GetItemPorCupomEItemAsync(string cupom, int item, int operador)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo, 
                   barras, descricao, qtd, preco, tributacao, icms, iss, und,
                   desconto, acrescimo, total, serial, tipo
            FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND item = @Item AND operador = @Operador AND tipo = 1";

        return await connection.QueryFirstOrDefaultAsync<ItemVendaTemporario>(
            sql, 
            new { Cupom = cupom, Item = item, Operador = operador });
    }

    public async Task<bool> TransferirItemAsync(string notaOrigem, int itemOrigem, string notaDestino, int operador)
    {
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        try
        {
            connection.Open();

            // Iniciar transação para garantir atomicidade
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Buscar o item na venda origem
                var itemSql = @"
                    SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo, 
                           barras, descricao, qtd, preco, tributacao, icms, iss, und,
                           desconto, acrescimo, total, serial, tipo
                    FROM frente_tmpitvendas 
                    WHERE cupom = @Cupom AND item = @Item AND operador = @Operador AND tipo = 1";

                var item = await connection.QueryFirstOrDefaultAsync<ItemVendaTemporario>(
                    itemSql, 
                    new { Cupom = notaOrigem, Item = itemOrigem, Operador = operador },
                    transaction);

                if (item == null)
                {
                    throw new Exception($"Item {itemOrigem} não encontrado na venda {notaOrigem}");
                }

                // 2. Buscar o próximo número de item na venda destino
                var maxItemSql = @"
                    SELECT COALESCE(MAX(item), 0) 
                    FROM frente_tmpitvendas 
                    WHERE cupom = @Cupom AND operador = @Operador AND tipo = 1";

                var proximoItem = await connection.QueryFirstOrDefaultAsync<int>(
                    maxItemSql,
                    new { Cupom = notaDestino, Operador = operador },
                    transaction);

                var novoItemNumero = proximoItem + 1;

                // 3. Remover o item da venda origem
                var deleteSql = @"
                    DELETE FROM frente_tmpitvendas 
                    WHERE cupom = @Cupom AND item = @Item AND operador = @Operador AND tipo = 1";

                await connection.ExecuteAsync(
                    deleteSql,
                    new { Cupom = notaOrigem, Item = itemOrigem, Operador = operador },
                    transaction);

                // 4. Reordenar itens na venda origem (diminuir número dos itens seguintes)
                var reordenarSql = @"
                    UPDATE frente_tmpitvendas 
                    SET item = item - 1 
                    WHERE cupom = @Cupom AND item > @Item AND operador = @Operador AND tipo = 1";

                await connection.ExecuteAsync(
                    reordenarSql,
                    new { Cupom = notaOrigem, Item = itemOrigem, Operador = operador },
                    transaction);

                // 5. Inserir o item na venda destino com data/hora atual do lançamento
                // Garantir data e hora atual para o item transferido (novo lançamento)
                var dataHoraAtual = DateTime.Now;
                var horaAtual = dataHoraAtual.TimeOfDay;
                
                var insertSql = @"
                    INSERT INTO frente_tmpitvendas (
                        cupom, n_caixa, data, hora, operador, item, codigo, barras, 
                        descricao, qtd, preco, tributacao, icms, iss, und, 
                        desconto, acrescimo, total, serial, tipo
                    ) VALUES (
                        @Cupom, @NCaixa, @Data, @Hora, @Operador, @Item, @Codigo, @Barras,
                        @Descricao, @Qtd, @Preco, @Tributacao, @Icms, @Iss, @Und,
                        @Desconto, @Acrescimo, @Total, @Serial, @Tipo
                    )";
                
                var itemDestino = new ItemVendaTemporario
                {
                    Cupom = notaDestino,
                    NCaixa = item.NCaixa,
                    Data = dataHoraAtual.Date, // ✅ Sempre com data atual do lançamento
                    Hora = horaAtual, // ✅ Sempre com hora atual do lançamento
                    Operador = operador,
                    Item = novoItemNumero,
                    Codigo = item.Codigo,
                    Barras = item.Barras,
                    Descricao = item.Descricao,
                    Qtd = item.Qtd,
                    Preco = item.Preco,
                    Tributacao = item.Tributacao,
                    Icms = item.Icms,
                    Iss = item.Iss,
                    Und = item.Und,
                    Desconto = item.Desconto,
                    Acrescimo = item.Acrescimo,
                    Total = item.Total,
                    Serial = item.Serial,
                    Tipo = item.Tipo
                };
                
                // Validação: garantir que sempre tenha data e hora
                if (itemDestino.Data == default(DateTime))
                {
                    itemDestino.Data = DateTime.Now.Date;
                }
                if (itemDestino.Hora == default(TimeSpan))
                {
                    itemDestino.Hora = DateTime.Now.TimeOfDay;
                }

                await connection.ExecuteAsync(insertSql, itemDestino, transaction);

                // Commit da transação
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
            connection.Dispose();
        }
    }

    public async Task<bool> CancelarItemAsync(string cupom, int item, int operador, System.Data.IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Verificar se o item existe
            var itemExistente = await GetItemPorCupomEItemAsync(cupom, item, operador);
            if (itemExistente == null)
            {
                throw new Exception($"Item {item} não encontrado na venda {cupom}");
            }

            // Remover o item (cancelar = deletar da tabela temporária)
            var sql = @"
                DELETE FROM frente_tmpitvendas 
                WHERE cupom = @Cupom AND item = @Item AND operador = @Operador AND tipo = 1";

            Console.WriteLine($"🗑️ [REPO] Cancelando item {item} da venda {cupom}");

            var linhasAfetadas = await connection.ExecuteAsync(sql, new 
            { 
                Cupom = cupom, 
                Item = item, 
                Operador = operador 
            }, transaction: transaction);

            if (linhasAfetadas > 0)
            {
                Console.WriteLine($"✅ [REPO] Item {item} cancelado com sucesso da venda {cupom}");
                return true;
            }

            return false;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

