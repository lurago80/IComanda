# 🍔 IComanda Frontend

Frontend React inspirado no iFood para sistema de comandas eletrônicas.

## 🎨 Design

- **Inspirado no iFood** com interface limpa e intuitiva
- **Cor principal: Azul** (ao invés de vermelho)
- **Mobile-first** design responsivo
- **Componentes modernos** com Tailwind CSS

## 🚀 Como executar

### Pré-requisitos

- Node.js 16+ instalado
- API backend rodando em http://localhost:5000

### Instalação e execução

```bash
# Instalar dependências
npm install

# Executar em modo desenvolvimento
npm start
```

O app estará disponível em: http://localhost:3000

## 📱 Funcionalidades

### ✅ Implementadas

- **Lista de Grupos** - Categorias de produtos com ícones
- **Lista de Produtos** - Produtos por categoria
- **Carrinho de Compras** - Adicionar/remover produtos
- **Interface Responsiva** - Otimizada para mobile
- **Integração com API** - Conectado ao backend C# .NET

### 🔄 Fluxo do App

1. **Tela Inicial** → Lista de categorias (grupos)
2. **Clicar na Categoria** → Lista de produtos
3. **Adicionar Produtos** → Carrinho
4. **Finalizar Pedido** → (Próxima implementação)

## 🎯 Componentes

- **Header** - Logo, título e carrinho
- **GruposList** - Lista de categorias com ícones
- **ProdutosList** - Lista de produtos por categoria
- **Carrinho** - Modal com itens e total

## 🛠️ Tecnologias

- **React 18** com TypeScript
- **Tailwind CSS** para estilização
- **React Query** para gerenciamento de estado
- **Axios** para requisições HTTP
- **Heroicons** para ícones

## 📊 Endpoints Utilizados

- `GET /api/grupos/todos-com-quantidade` - Lista grupos
- `GET /api/produtos/grupo/{id}` - Produtos por grupo
- `GET /api/produtos/buscar` - Busca de produtos

## 🎨 Paleta de Cores

- **Primary**: Azul (#3b82f6)
- **Secondary**: Cinza (#64748b)
- **Background**: Cinza claro (#f8fafc)
- **Cards**: Branco (#ffffff)

## 📱 Interface

```
┌─────────────────────────────┐
│  🍔 IComanda - Sistema      │
├─────────────────────────────┤
│  📂 Categorias              │
│                             │
│  🔧 ADITIVOS (5 produtos)   │
│  ❄️ AR CONDICIONADO (3)     │
│  🧽 LIMPEZA (8 produtos)    │
│  💡 LAMPADA (2 produtos)    │
│                             │
│  [🛒 Carrinho - 3 itens]    │
└─────────────────────────────┘
```

## 🔗 Integração

O frontend está totalmente integrado com a API C# .NET:

- **CORS configurado** para localhost:3000
- **Endpoints mapeados** corretamente
- **Tratamento de erros** implementado
- **Loading states** para melhor UX

## 🚀 Próximos Passos

1. **Finalizar Pedido** - Integração com endpoint de vendas
2. **Busca de Produtos** - Campo de pesquisa
3. **Histórico de Pedidos** - Lista de vendas
4. **PWA** - Instalação como app mobile
5. **Offline Support** - Cache de dados

---

**Desenvolvido com ❤️ para modernizar o sistema legado Delphi**
