# 🥐 IComanda - Sistema de Pedidos para Padaria

Sistema moderno de comandas e vendas desenvolvido em .NET 8 + React + TypeScript, integrado com sistema legado Delphi via Firebird 2.5.

## 📋 Sobre o Projeto

O IComanda é uma modernização progressiva do sistema legado de comandas desenvolvido em Delphi. O sistema funciona de forma híbrida:

- **Frontend Web/Tablet (React)**: Garçons fazem pedidos, adicionam produtos, selecionam clientes
- **Backend API (.NET 8)**: Grava vendas temporárias no Firebird
- **Desktop Delphi**: Caixa finaliza vendas definitivas e emite cupom fiscal

## 🏗️ Arquitetura

### Sistema Híbrido Web + Desktop

```
┌─────────────────────────────┐
│  GARÇOM (React - Tablet)   │
│  - Login simples            │
│  - Adiciona produtos        │
│  - Seleciona cliente        │
│  - Finaliza pré-venda       │
└──────────┬──────────────────┘
           │ HTTP/REST
           ▼
┌─────────────────────────────┐
│  API .NET 8 (Backend)       │
│  - Valida pedido            │
│  - Grava VENDAS (ABERTO)    │
│  - Grava frente_tmpitvendas │
└──────────┬──────────────────┘
           │ Firebird 2.5
           ▼
┌─────────────────────────────┐
│  Banco Firebird 2.5         │
│  - VENDAS (status: ABERTO)  │
│  - frente_tmpitvendas       │
└──────────┬──────────────────┘
           │ Acesso Direto
           ▼
┌─────────────────────────────┐
│  CAIXA (Delphi Desktop)     │
│  - Busca vendas abertas     │
│  - Fecha venda definitiva   │
│  - Grava ITVENDAS           │
│  - Emite cupom fiscal       │
│  - STATUS = EFETIVADO       │
└─────────────────────────────┘
```

## 🛠️ Tecnologias

### Backend
- **.NET 8** - Framework principal
- **C#** - Linguagem
- **Dapper** - Micro ORM para Firebird
- **FirebirdSql.Data.FirebirdClient** - Driver Firebird
- **AutoMapper** - Mapeamento de objetos
- **Serilog** - Logging
- **Swagger/OpenAPI** - Documentação da API

### Frontend
- **React 18** - Biblioteca UI
- **TypeScript** - Type safety
- **Vite** - Build tool
- **TailwindCSS** - Estilização
- **React Query** - Cache e estado servidor
- **Zustand** - Gerenciamento de estado local
- **Axios** - Cliente HTTP
- **Framer Motion** - Animações

### Banco de Dados
- **Firebird 2.5** - Banco de dados legado

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Node.js 18+
- Firebird 2.5 instalado
- Banco de dados DADOSG5.FDB configurado

### Backend (API)

```bash
cd IComanda.API

# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar (HTTP only - desenvolvimento)
set ASPNETCORE_URLS=http://localhost:65375
dotnet run --no-launch-profile
```

**API estará em:** http://localhost:65375  
**Swagger:** http://localhost:65375/index.html

### Frontend (React)

```bash
cd IComanda.API/icomanda-frontend

# Instalar dependências
npm install

# Executar em desenvolvimento
npm start
```

**Frontend estará em:** http://localhost:3000

## 📁 Estrutura do Projeto

```
icomanda/
├── IComanda.API/                    # Backend .NET 8
│   ├── Controllers/                 # Endpoints da API
│   ├── Services/                    # Lógica de negócio
│   ├── Repositories/                # Acesso a dados (Dapper)
│   ├── Models/                      # Entidades e DTOs
│   │   ├── Entities/                # Entidades do banco
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Requests/                # Request models
│   ├── Mappings/                    # Perfis do AutoMapper
│   ├── Data/                        # Configuração do banco
│   ├── Extensions/                  # Extension methods
│   └── icomanda-frontend/           # Frontend React
│       ├── src/
│       │   ├── components/          # Componentes React
│       │   ├── pages/               # Páginas
│       │   ├── services/            # Serviços HTTP
│       │   ├── store/               # Zustand stores
│       │   ├── types/               # TypeScript types
│       │   └── hooks/               # Custom hooks
│       └── public/
└── dados/
    └── DADOSG5.FDB                  # Banco Firebird
```

## 🎨 Tema Visual - Padaria

O sistema foi estilizado com cores quentes e acolhedoras para ambiente de padaria:

- **Primary**: Laranja dourado (#f59e0b) - cor de pão assado
- **Accent**: Amarelo suave (#fbbf24) - manteiga
- **Background**: Tons de bege (#fef9f3) - pão fresco
- **Cards**: Branco com bordas âmbar
- **Ícone**: 🥐 Croissant

## 📊 Fluxo de Dados

### 1. Garçom Adiciona Pedido (Web)

```typescript
POST /api/vendas
{
  "cliente": 1,
  "operador": 1,        // ID do garçom logado
  "vendedor": 1,
  "itens": [
    { "codigo": 123, "qtd": 2, "preco": 5.50 }
  ]
}
```

**Resultado:**
- ✅ Grava na tabela `VENDAS` com `LANCADO = 'ABERTO'`
- ✅ Grava itens na tabela `frente_tmpitvendas`
- ❌ NÃO grava em `ITVENDAS` (aguarda finalização do caixa)

### 2. Caixa Finaliza Venda (Delphi)

O Delphi Desktop:
- Busca vendas com `LANCADO = 'ABERTO'`
- Busca itens em `frente_tmpitvendas`
- Finaliza venda
- Grava em `ITVENDAS` (definitivo)
- Atualiza `LANCADO = 'EFETIVADO'`
- Emite cupom fiscal

## 🔐 Autenticação

Sistema de login simplificado (sem JWT) adequado para apresentação:

- **Login:** Apenas nome do usuário
- **Armazenamento:** localStorage do navegador
- **Vinculação:** ID do operador é gravado nas vendas
- **Logout:** Botão no header

## 📝 Endpoints Principais

### Produtos
- `GET /api/produtos` - Listar produtos
- `GET /api/produtos/{id}` - Buscar produto
- `GET /api/produtos/buscar?q=termo` - Buscar produtos

### Grupos
- `GET /api/grupos` - Listar grupos/categorias

### Clientes
- `GET /api/clientes/buscar` - Buscar clientes
- `GET /api/clientes/{id}` - Buscar cliente

### Vendas
- `POST /api/vendas` - Criar venda (ABERTA)
- `GET /api/vendas/{nota}` - Buscar venda
- `GET /api/vendas/hoje` - Vendas do dia
- `GET /api/vendas/comanda/{numero}` - Vendas por comanda
- `GET /api/vendas/mesa/{numero}` - Vendas por mesa

## 🗄️ Tabelas Utilizadas

### Tabela VENDAS
```sql
-- Campos principais
nota          VARCHAR(6)      -- Número da venda
operador      INTEGER         -- ID do garçom (usuário logado)
vendedor      INTEGER         -- ID do vendedor
cliente       INTEGER         -- ID do cliente
lancado       VARCHAR(20)     -- 'ABERTO' ou 'EFETIVADO'
total         DECIMAL(15,2)   -- Valor total
comanda       INTEGER         -- Número da comanda
mesa          INTEGER         -- Número da mesa
```

### Tabela frente_tmpitvendas (Temporária)
```sql
-- Itens temporários da venda
cupom         VARCHAR(6)      -- Número da venda
operador      INTEGER         -- ID do operador
item          INTEGER         -- Sequencial do item
codigo        INTEGER         -- Código do produto
qtd           DECIMAL(15,3)   -- Quantidade
preco         DECIMAL(15,2)   -- Preço unitário
total         DECIMAL(15,2)   -- Total do item
tipo          INTEGER         -- Tipo (1 = item normal)
```

### Tabela ITVENDAS (Definitiva - gravada pelo Delphi)
```sql
-- Itens finalizados da venda
nota          VARCHAR(6)
item          INTEGER
codigo        INTEGER
qtd           DECIMAL(15,3)
preco         DECIMAL(15,2)
total         DECIMAL(15,2)
```

## 🎯 Funcionalidades Implementadas

### ✅ Backend
- [x] CRUD de Produtos
- [x] CRUD de Grupos/Categorias  
- [x] CRUD de Clientes
- [x] Criação de vendas temporárias
- [x] Gravação em frente_tmpitvendas
- [x] Busca de vendas por comanda/mesa
- [x] Histórico de vendas
- [x] Health Check do banco
- [x] Logs estruturados (Serilog)
- [x] CORS configurado
- [x] Swagger/OpenAPI

### ✅ Frontend
- [x] Login simples
- [x] Exibição de nome do usuário
- [x] Logout
- [x] Listagem de grupos/categorias
- [x] Listagem de produtos por grupo
- [x] Busca de produtos
- [x] Carrinho de compras
- [x] Busca de clientes
- [x] Finalização de pedido
- [x] Histórico de vendas
- [x] Tema visual Padaria (cores quentes)
- [x] Responsivo (mobile-first)
- [x] Animações suaves

## 👥 Autores

- **David** - Desenvolvimento Full Stack (.NET + React)

## 📄 Licença

Este projeto é proprietário e confidencial.

## 📞 Suporte

Para dúvidas ou problemas, entre em contato com a equipe de desenvolvimento.

---

**Desenvolvido com ❤️ para modernizar o sistema de comandas** 🥐

