/* 
 * Script para criar usuário de teste no sistema iComanda
 * Execute este script no Firebird para criar um usuário administrador
 */

-- Verificar se o usuário já existe
SELECT *
FROM USUARIO
WHERE NOME = 'admin';

-- Deletar usuário de teste se já existir (opcional)
-- DELETE FROM USUARIO WHERE NOME = 'admin';

-- Inserir usuário de teste
INSERT INTO USUARIO
    (
    ID,
    NOME,
    SENHA,
    ATIVO,
    BLOQUEIO,
    VISUALIZAR,
    TOTAL,
    TIPO,
    CANCELAR
    )
VALUES
    (
        GEN_ID(USUARIO_ID_GEN, 1), -- Gera novo ID usando o generator
        'admin', -- Nome de usuário
        '123456', -- Senha (texto plano - sistema legado)
        '1', -- Ativo
        '0', -- Não bloqueado
        '1', -- Pode visualizar
        '1', -- Pode ver total
        '0', -- Tipo padrão
        '1'                           -- Pode cancelar
);

-- Verificar se o usuário foi criado
SELECT *
FROM USUARIO
WHERE NOME = 'admin';

COMMIT;

/*
 * CREDENCIAIS DE TESTE:
 * Usuário: admin
 * Senha: 123456
 */

