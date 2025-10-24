import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import GruposList from './components/GruposList';
import ProdutosList from './components/ProdutosList';
import CartDrawer from './components/cart/CartDrawer';
import CartFAB from './components/cart/CartFAB';
import AppHeader from './components/ui/AppHeader';
import { Grupo } from './types/api';

const queryClient = new QueryClient();

function App() {
  const [grupoSelecionado, setGrupoSelecionado] = useState<Grupo | null>(null);

  const handleSearch = (query: string) => {
    // TODO: Implementar busca de produtos
    console.log('Buscar:', query);
  };

  const handleOpenHistory = () => {
    // TODO: Implementar histórico de pedidos
    console.log('Abrir histórico');
  };

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-bg">
        <AppHeader 
          onSearch={handleSearch}
          onOpenHistory={handleOpenHistory}
        />
        
        <main className="pb-20">
          {!grupoSelecionado ? (
            <GruposList onSelecionarGrupo={setGrupoSelecionado} />
          ) : (
            <ProdutosList 
              grupo={grupoSelecionado}
              onVoltar={() => setGrupoSelecionado(null)}
            />
          )}
        </main>

        <CartFAB />
        <CartDrawer />
      </div>
    </QueryClientProvider>
  );
}

export default App;
