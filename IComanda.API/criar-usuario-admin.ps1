$connectionString = "User=SYSDBA;Password=masterkey;Database=C:\iComanda\Dados\DADOSG5.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;"

Write-Host "🔧 Criando usuário admin..." -ForegroundColor Yellow

# Caminho do isql.exe do Firebird
$isqlPath = "C:\Program Files (x86)\Firebird\Firebird_2_5\bin\isql.exe"

if (-not (Test-Path $isqlPath)) {
    $isqlPath = "C:\Program Files\Firebird\Firebird_2_5\bin\isql.exe"
}

if (-not (Test-Path $isqlPath)) {
    Write-Host "❌ isql.exe não encontrado!" -ForegroundColor Red
    exit 1
}

# SQL para criar/atualizar usuário admin
$sql = @"
SET SQL DIALECT 3;
CONNECT 'C:\iComanda\Dados\DADOSG5.FDB' USER 'SYSDBA' PASSWORD 'masterkey';

DELETE FROM USUARIO WHERE UPPER(TRIM(NOME)) = 'ADMIN';

INSERT INTO USUARIO (
    ID,
    NOME,
    SENHA,
    ATIVO,
    BLOQUEIO,
    VISUALIZAR,
    TOTAL,
    TIPO,
    CANCELAR
) VALUES (
    GEN_ID(USUARIO_ID_GEN, 1),
    'admin',
    '123456',
    '1',
    '0',
    '1',
    '1',
    '0',
    '1'
);

COMMIT;

SELECT ID, NOME, SENHA, ATIVO, BLOQUEIO FROM USUARIO WHERE UPPER(TRIM(NOME)) = 'ADMIN';

EXIT;
"@

# Salvar SQL em arquivo temporário
$sqlFile = "$env:TEMP\criar-admin.sql"
$sql | Out-File -FilePath $sqlFile -Encoding ASCII

# Executar isql
Write-Host "📝 Execut

ando SQL..." -ForegroundColor Cyan
& $isqlPath -i $sqlFile

Remove-Item $sqlFile -Force

Write-Host "`n✅ Usuário admin criado/atualizado!" -ForegroundColor Green
Write-Host "Credenciais: admin / 123456" -ForegroundColor Cyan
