# CLAUDE.md

## Instruções para integração automática com GitHub

Este projeto está configurado para:

1. Criar um repositório no GitHub com o código do projeto.
2. A cada alteração no projeto, atualizar automaticamente o repositório no GitHub.
3. Este arquivo CLAUDE.md será atualizado sempre que as instruções mudarem.

### Como funciona
- Toda alteração feita no projeto será automaticamente commitada e enviada para o repositório remoto no GitHub.
- Caso o repositório ainda não exista, ele será criado automaticamente.
- Este processo garante que o repositório esteja sempre sincronizado com as últimas alterações do projeto.

### Observações
- Certifique-se de que as credenciais do GitHub estejam configuradas na máquina para permitir push automático.
- Caso precise alterar o comportamento, edite este arquivo e as automações correspondentes.
