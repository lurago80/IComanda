using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Configuration;
using System.IO;

class TestarQuery
{
    static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("TESTE DIRETO DA QUERY SQL");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Carregar configuração
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = builder.Build();
        var connectionString = configuration.GetConnectionString("Firebird");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ Connection string não encontrada!");
            return;
        }

        Console.WriteLine($"📋 Conectando ao banco...");
        Console.WriteLine($"   Connection: {connectionString.Substring(0, Math.Min(80, connectionString.Length))}...");
        Console.WriteLine();

        try
        {
            using var connection = new FbConnection(connectionString);
            connection.Open();
            Console.WriteLine("✅ Conexão estabelecida com sucesso!");
            Console.WriteLine();

            // Query que está falhando
            var query = @"
                SELECT FIRST 5 SKIP 0
                    p3.id, p1.codigobarra, p1.padraobarra, p1.imagem, p1.classificacao, p1.diasvalidade,
                    p1.pesoliquido, p1.pesobruto, p1.tipovalidade, p1.pesavel, p1.composicaoid,
                    p1.marcaid, p1.medicamentoid, p1.combustivelid, p1.impressao, p1.codigobarras1,
                    p2.quantidade, p2.quantidademinima, p2.quantidademaxima, p2.localizacao,
                    p2.dataultimavenda, p2.dataultimacompra, p2.chavepaf, p2.produtotributacaoestadualid,
                    p2.fabricante, p2.ultimo_reajuste, p2.status_sazional,
                    p4.precovenda, p4.vendavel, p4.precocusto, p4.precocustomedio, p4.limitedesconto,
                    p4.numerofci, p4.chavepaf as chavepafempresa, p4.produtoempresaid, p4.servicoempresaid,
                    p4.pessoaid, p4.indicecalcularprecovendaid, p4.indicecalcularprecocustoid,
                    p4.produtoeservicoid, p4.precodolar, p4.percentual, p4.atacado, p4.preco3,
                    p4.data_alteracao as data_alteracao_preco, p4.margem_seguranca,
                    p4.custo_seguranca,
                    p3.descricao, p3.caracteristica, p3.codigointerno, p3.datainclusao, p3.ativo,
                    p3.observacao, p3.iat, p3.ippt, p3.codigocontabil, p3.percentualcomissao,
                    p3.chavepafp2, p3.chavepafe2, p3.subgrupoid, p3.generoitemid, p3.tipoitemid,
                    p3.unidademedidaid, p3.produtoid, p3.servicoid, p3.tributacaofederalid, p3.ncmid,
                    p3.naturezareceitaid, p3.un_medida, p3.grupo, p3.grade, p3.customedio, p3.ultimacompra,
                    p3.dtcadastro, p3.cfop, p3.csosn, p3.cest, p3.despesas, p3.tot_custo, p3.cf,
                    p3.frete, p3.icms_st, p3.ipi, p3.ref_cor, p3.cor, p3.tamanho, p3.composicao,
                    p3.titulo, p3.partida, p3.tipo, p3.categoria, p3.ntam, p3.marca, p3.sacokg,
                    p3.codrastreavel, p3.tipo_produto, p3.cst_origem, p3.cst, p3.ncm, p3.icms, p3.margem,
                    p3.codestacao, p3.cst_pis, p3.aliq_pis, p3.cst_cofins, p3.aliq_cofins, p3.cst_ipi,
                    p3.aliq_ipi, p3.enquadra_ipi, p3.fcp, p3.mva, p3.data_alteracao as data_alteracao_produto,
                    p3.naturezareceita, p3.cfop_inter, p3.red_basecalculo, p3.fracaoml, p3.monofasico,
                    CAST(NULL AS SMALLINT) as situacao_rpl_produto, p3.caixam2, CAST(NULL AS VARCHAR(40)) as grupofiscal, p3.pesobobina,
                    p3.corte, p3.defensivo
                FROM PRODUTOESERVICO p3
                LEFT JOIN PRODUTO p1 ON p1.id = p3.id
                LEFT JOIN PRODUTOEMPRESA p2 ON p2.id = p3.id
                LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.id = p3.id
                WHERE (p3.ativo = 1 OR p3.ativo IS NULL)
                ORDER BY p3.descricao";

            Console.WriteLine("🔍 Executando query...");
            Console.WriteLine();

            using var command = new FbCommand(query, connection);
            using var reader = command.ExecuteReader();

            int count = 0;
            while (reader.Read())
            {
                count++;
                if (count <= 3)
                {
                    var id = reader["id"]?.ToString() ?? "NULL";
                    var descricao = reader["descricao"]?.ToString() ?? "NULL";
                    Console.WriteLine($"   ✅ Produto {count}: ID={id}, Descrição={descricao.Substring(0, Math.Min(50, descricao.Length))}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"✅ Query executada com sucesso! Total de produtos: {count}");
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("TESTE CONCLUÍDO COM SUCESSO!");
            Console.WriteLine("========================================");
        }
        catch (FbException ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ ERRO SQL DETECTADO:");
            Console.WriteLine($"   Código: {ex.ErrorCode}");
            Console.WriteLine($"   Mensagem: {ex.Message}");
            Console.WriteLine();
            
            if (ex.Message.Contains("Column unknown"))
            {
                Console.WriteLine("🔍 ANÁLISE DO ERRO:");
                Console.WriteLine("   A coluna mencionada no erro NÃO EXISTE na tabela!");
                Console.WriteLine("   Verifique se a coluna existe ou precisa ser removida da query.");
            }
            
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("ERRO IDENTIFICADO!");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ ERRO GERAL:");
            Console.WriteLine($"   Tipo: {ex.GetType().Name}");
            Console.WriteLine($"   Mensagem: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine();
        }
    }
}
