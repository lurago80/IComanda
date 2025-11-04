import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import GruposList from './components/GruposList';
import HistoryModal from './components/HistoryModal';
import ProdutosList from './components/ProdutosList';
import SearchResults from './components/SearchResults';
import ToastContainer from './components/ToastContainer';
import CartDrawer from './components/cart/CartDrawer';
import CartFAB from './components/cart/CartFAB';
import AppHeader from './components/ui/AppHeader';
import { useToast } from './hooks/useToast';
import Login from './pages/Login';
import { Grupo } from './types/api';

const queryClient = new QueryClient();

function App() {
  const [grupoSelecionado, setGrupoSelecionado] = useState<Grupo | null>(null);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [showHistory, setShowHistory] = useState<boolean>(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const { toasts, removeToast } = useToast();

  useEffect(() => {
    const usuario = localStorage.getItem('usuario_logado');
    setIsLoggedIn(!!usuario);
  }, []);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    if (query.trim()) {
      setGrupoSelecionado(null);
    }
  };

  const handleOpenHistory = () => {
    setShowHistory(true);
  };

  const handleCloseHistory = () => {
    setShowHistory(false);
  };

  const handleBackToCategories = () => {
    setGrupoSelecionado(null);
    setSearchQuery('');
  };

  if (!isLoggedIn) {
    return <Login />;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-bg">
        <AppHeader 
          onSearch={handleSearch}
          onOpenHistory={handleOpenHistory}
        />
        
        <main className="pb-20">
          {searchQuery ? (
            <SearchResults 
              query={searchQuery}
              onBack={handleBackToCategories}
            />
          ) : !grupoSelecionado ? (
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
        <HistoryModal 
          isOpen={showHistory}
          onClose={handleCloseHistory}
        />
        <ToastContainer 
          toasts={toasts}
          onRemove={removeToast}
        />
      </div>
    </QueryClientProvider>
  );
}

export default App;
