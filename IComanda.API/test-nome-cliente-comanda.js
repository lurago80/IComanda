/**
 * Teste E2E: criar comanda com nome do cliente "CURSOR" (não cadastrado),
 * adicionar 1 produto, fechar comanda, buscar venda e verificar se nomeCliente aparece.
 * Uso: node test-nome-cliente-comanda.js
 * Requer: API rodando em http://localhost:65375
 */
const API = 'http://localhost:65375/api';

async function request(method, path, body = null) {
  const url = path.startsWith('http') ? path : `${API}${path}`;
  const opts = { method, headers: { 'Content-Type': 'application/json' } };
  if (body != null) opts.body = JSON.stringify(body);
  const res = await fetch(url, opts);
  const text = await res.text();
  if (!res.ok) throw new Error(`${res.status} ${path}: ${text}`);
  return text ? JSON.parse(text) : null;
}

async function main() {
  console.log('=== Teste: nome_cliente na comanda (CURSOR) ===\n');

  // 1) Próximo número de comanda
  const proximoNumero = await request('GET', '/vendas/comanda/proximo-numero');
  console.log('1. Próximo número comanda:', proximoNumero);

  // 2) Um produto (primeiro grupo, primeiro produto)
  const grupos = await request('GET', '/grupos');
  if (!grupos?.length) throw new Error('Nenhum grupo encontrado');
  const produtos = await request('GET', `/produtos/grupo/${grupos[0].id}`);
  if (!produtos?.length) throw new Error('Nenhum produto no grupo');
  const produto = produtos[0];
  const preco = produto.precoVenda ?? produto.preco ?? 1;
  console.log('2. Produto:', produto.descricao, '| Preço:', preco);

  // 3) Criar venda: cliente 0, nomeCliente "CURSOR", 1 item (camelCase como o frontend)
  const criarBody = {
    cliente: 0,
    nomeCliente: 'CURSOR',
    comanda: proximoNumero,
    operador: 1,
    caixa: 1,
    vendedor: 1,
    totProdutos: preco,
    total: preco,
    desconto: 0,
    acrescimo: 0,
    itens: [{ codigo: produto.id, qtd: 1, preco: preco }]
  };
  const vendaCriada = await request('POST', '/vendas', criarBody);
  const nota = vendaCriada?.nota ?? vendaCriada?.Nota;
  if (!nota) throw new Error('Venda criada sem nota: ' + JSON.stringify(vendaCriada));
  console.log('3. Venda criada. Nota:', nota, '| nomeCliente na resposta:', vendaCriada.nomeCliente ?? vendaCriada.NomeCliente ?? '(não veio)');

  // 4) Formas de pagamento e fechar comanda
  const formas = await request('GET', '/recebimentos/formas-pagamento');
  const idForma = formas?.[0]?.id ?? formas?.[0]?.Id ?? 1;
  await request('POST', '/recebimentos/fechar-comanda', {
    comanda: proximoNumero,
    recebimentos: [{ idFormaPagamento: idForma, valor: preco }]
  });
  console.log('4. Comanda fechada.');

  // 5) Buscar venda por nota (como na tela "Editar") e verificar nomeCliente
  const vendaGet = await request('GET', `/vendas/${nota}`);
  const nomeExibido = vendaGet?.nomeCliente ?? vendaGet?.NomeCliente ?? '';
  const ok = (nomeExibido || '').trim().toUpperCase() === 'CURSOR';

  console.log('\n--- Resultado ---');
  console.log('GET /vendas/' + nota, '-> nomeCliente:', nomeExibido || '(vazio)');
  if (!ok && vendaGet) {
    const keys = Object.keys(vendaGet).filter(k => k.toLowerCase().includes('nome'));
    console.log('Chaves com "nome" na resposta:', keys);
  }
  if (ok) {
    console.log('OK: O nome "CURSOR" aparece na venda. Ao editar, o nome deve aparecer na tela.');
  } else {
    console.log('FALHA: nomeCliente esperado "CURSOR", obtido:', JSON.stringify(nomeExibido));
    process.exit(1);
  }
}

main().catch((err) => {
  if (err.cause?.code === 'ECONNREFUSED' || err.message === 'fetch failed') {
    console.error('Erro: API não está rodando. Inicie com: dotnet run');
    console.error('       Depois execute novamente: node test-nome-cliente-comanda.js');
  } else {
    console.error('Erro:', err.message);
  }
  process.exit(1);
});
