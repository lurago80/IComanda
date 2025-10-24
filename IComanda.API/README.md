# IComanda API

API moderna para sistema de comandas e vendas, desenvolvida em .NET 8 para modernizar o sistema legado Delphi.

## 🚀 Tecnologias

- **.NET 8** - Framework principal
- **Dapper** - ORM leve para Firebird
- **Firebird 2.5** - Banco de dados legado
- **Swagger** - Documentação da API
- **AutoMapper** - Mapeamento de objetos
- **Serilog** - Logging estruturado

## 📋 Funcionalidades

### Produtos

- ✅ Busca de produtos por múltiplos critérios
- ✅ Busca por código de barras
- ✅ Busca por código interno
- ✅ Paginação de resultados
- ✅ Filtros por grupo e status ativo

### Carrinho (Em desenvolvimento)

- 🔄 Adicionar itens ao carrinho temporário
- 🔄 Remover itens do carrinho
- 🔄 Atualizar quantidades
- 🔄 Limpar carrinho

### Vendas (Em desenvolvimento)

- 🔄 Finalizar vendas
- 🔄 Consultar vendas
- 🔄 Relatórios de vendas

### Comandas (Em desenvolvimento)

- 🔄 Gerenciar comandas
- 🔄 Gerenciar mesas
- 🔄 Status de comandas

## 🏗️ Estrutura do Projeto

```
IComanda.API/
├── Controllers/          # Controllers da API
├── Models/              # DTOs, Entities e Requests
├── Services/            # Lógica de negócio
├── Repositories/        # Acesso a dados
├── Data/               # Configuração de conexão
├── Mappings/           # AutoMapper profiles
├── Middleware/         # Middlewares customizados
├── Extensions/         # Extensões de configuração
└── HealthChecks/       # Health checks
```

## 🔧 Configuração

### Connection String

```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=D:\\fontes\\icomanda\\dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
  }
}
```

### Executar a API

```bash
cd IComanda.API
dotnet run
```

### Swagger

Acesse: `http://localhost:5000` (Swagger na raiz)

## 📊 Endpoints Principais

### Produtos

- `GET /api/produtos/buscar` - Buscar produtos
- `GET /api/produtos/buscar-paginado` - Busca com paginação
- `GET /api/produtos/{id}` - Produto por ID
- `GET /api/produtos/codigo-barras/{codigo}` - Produto por código de barras
- `GET /api/produtos/codigo-interno/{codigo}` - Produto por código interno

### Health Check

- `GET /health` - Status da aplicação

## 🗄️ Banco de Dados

A API utiliza as mesmas tabelas do sistema legado:

- **PRODUTO** - Dados básicos do produto
- **PRODUTOEMPRESA** - Estoque e localização
- **PRODUTOESERVICO** - Descrição e características
- **PRODUTOESERVICOEMPRESA** - Preços e custos
- **FRENTE_TMPITVENDAS** - Carrinho temporário
- **VENDAS** - Vendas finalizadas
- **ITEVENDAS** - Itens das vendas

## 🔄 Próximos Passos

1. ✅ Estrutura base da API
2. ✅ Controller de produtos
3. 🔄 Controller de carrinho
4. 🔄 Controller de vendas
5. 🔄 Controller de comandas
6. 🔄 Frontend React
7. 🔄 Testes unitários
8. 🔄 Deploy e documentação

## 📝 Logs

Os logs são salvos em:

- Console (desenvolvimento)
- Arquivo: `logs/icomanda-{data}.txt`

## 🤝 Contribuição

Este projeto moderniza o sistema legado Delphi mantendo compatibilidade total com o banco de dados existente.
