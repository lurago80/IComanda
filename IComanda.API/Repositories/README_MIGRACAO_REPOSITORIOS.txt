# MIGRAÇÃO DE REPOSITÓRIOS - PADRÃO IMPLEMENTATIONS/INTERFACES

Todos os repositórios do projeto foram padronizados para o modelo:
- `Repositories/Interfaces/INomeRepository.cs` (interface)
- `Repositories/Implementations/NomeRepository.cs` (implementação)

Os arquivos duplicados/antigos em `Repositories` raiz (ex: `MesaRepository.cs`, `RecebimentoRepository.cs`, etc) devem ser removidos.

Os controllers e services devem referenciar apenas as interfaces e implementações dos diretórios `Interfaces` e `Implementations`.

**Atenção:**
- Se precisar criar um novo repositório, siga sempre esse padrão.
- Se encontrar ambiguidade de tipos (ex: FormaPagamento), use o namespace explícito.

---

## Próximos passos
- Remover arquivos antigos do diretório raiz de `Repositories`.
- Garantir que todos os DI/Controllers usam as implementações corretas.
- Rodar `dotnet build` para garantir sucesso.
