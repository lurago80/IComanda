#!/bin/bash
# Script de publicação para IComanda API
# Publica o backend .NET e faz build do frontend React

echo "🚀 Iniciando publicação do IComanda..."
echo ""

# 1. Publicar Backend (.NET)
echo "📦 Publicando Backend (.NET)..."
cd IComanda.API

# Criar diretório de publicação se não existir
PUBLISH_DIR="./publish"
if [ -d "$PUBLISH_DIR" ]; then
    rm -rf "$PUBLISH_DIR"
fi
mkdir -p "$PUBLISH_DIR"

# Publicar backend
dotnet publish -c Release -o "$PUBLISH_DIR" --self-contained false

if [ $? -ne 0 ]; then
    echo "❌ Erro ao publicar backend!"
    exit 1
fi

echo "✅ Backend publicado com sucesso em: $PUBLISH_DIR"
echo ""

# 2. Build Frontend (React)
echo "📦 Fazendo build do Frontend (React)..."
cd icomanda-frontend

# Instalar dependências se necessário
if [ ! -d "node_modules" ]; then
    echo "📥 Instalando dependências do frontend..."
    npm install
fi

# Build do frontend
npm run build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao fazer build do frontend!"
    exit 1
fi

echo "✅ Frontend buildado com sucesso em: build"
echo ""

# Voltar para o diretório raiz
cd ../..

echo "🎉 Publicação concluída com sucesso!"
echo ""
echo "📁 Arquivos publicados:"
echo "   Backend: IComanda.API/publish/"
echo "   Frontend: IComanda.API/icomanda-frontend/build/"
echo ""
echo "💡 Para executar o backend publicado:"
echo "   cd IComanda.API/publish"
echo "   dotnet IComanda.API.dll"
echo ""

