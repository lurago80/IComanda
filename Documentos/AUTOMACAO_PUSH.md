# Script de commit e push automático

Este script (`auto-commit-push.bat`) monitora alterações no projeto e faz commit/push automático para o GitHub a cada 10 segundos.

## Como usar

1. Execute o script `auto-commit-push.bat` para ativar a automação.
2. Toda alteração salva será enviada automaticamente para o repositório remoto.
3. Para interromper, basta fechar o terminal onde o script está rodando.

---

**Atenção:**
- Certifique-se de que as credenciais do GitHub estejam configuradas para push automático.
- O script faz commit com a mensagem padrão: `Atualização automática: alterações detectadas`.
- Edite o script para customizar o intervalo ou mensagens, se necessário.
