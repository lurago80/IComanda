-- ============================================
-- TESTE DA CONSULTA DE PRODUTOS COMPLETOS
-- ============================================

-- Teste 1: Verificar se a query básica funciona (sem filtros)
SELECT FIRST 5 SKIP 0
    p3.id, 
    p3.descricao, 
    p3.ativo, 
    p3.grupo,
    p1.codigobarra,
    p4.precovenda
FROM PRODUTOESERVICO p3
LEFT JOIN PRODUTO p1 ON p1.id = p3.id
LEFT JOIN PRODUTOEMPRESA p2 ON p2.id = p3.id
LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.id = p3.id
WHERE 1=1
ORDER BY p3.descricao;

-- Teste 2: Verificar se a query com filtro ATIVO funciona
SELECT FIRST 5 SKIP 0
    p3.id, 
    p3.descricao, 
    p3.ativo, 
    p3.grupo,
    p1.codigobarra,
    p4.precovenda
FROM PRODUTOESERVICO p3
LEFT JOIN PRODUTO p1 ON p1.id = p3.id
LEFT JOIN PRODUTOEMPRESA p2 ON p2.id = p3.id
LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.id = p3.id
WHERE (p3.ativo = 1 OR p3.ativo IS NULL)
ORDER BY p3.descricao;

-- Teste 3: Verificar se todas as colunas da query completa existem
-- (Esta query deve retornar erro se alguma coluna não existir)
SELECT FIRST 1 SKIP 0
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
ORDER BY p3.descricao;

-- Teste 4: Contar produtos ativos
SELECT COUNT(*) AS TOTAL_ATIVOS
FROM PRODUTOESERVICO
WHERE (ativo = 1 OR ativo IS NULL);

-- Teste 5: Verificar se há produtos sem dados nas tabelas relacionadas
SELECT 
    COUNT(*) AS TOTAL,
    COUNT(p1.ID) AS COM_PRODUTO,
    COUNT(p2.ID) AS COM_PRODUTOEMPRESA,
    COUNT(p4.ID) AS COM_PRODUTOESERVICOEMPRESA
FROM PRODUTOESERVICO p3
LEFT JOIN PRODUTO p1 ON p1.ID = p3.ID
LEFT JOIN PRODUTOEMPRESA p2 ON p2.ID = p3.ID
LEFT JOIN PRODUTOESERVICOEMPRESA p4 ON p4.ID = p3.ID
WHERE (p3.ativo = 1 OR p3.ativo IS NULL);
