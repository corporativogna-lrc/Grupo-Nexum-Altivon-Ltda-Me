import { BrowserRouter, Navigate, Route, Routes, useLocation } from 'react-router-dom';
import '@/App.css';
import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './context/CartContext';

import Navbar from './components/Navbar';
import Footer from './components/Footer';
import Home from './pages/Home';
import Produtos from './pages/Produtos';
import ProdutoDetalhe from './pages/ProdutoDetalhe';
import Lojas from './pages/Lojas';
import Contato from './pages/Contato';
import Carrinho from './pages/Carrinho';
import Checkout from './pages/Checkout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import AreaCliente from './pages/AreaCliente';

function getRouterBasename() {
  const publicUrl = process.env.PUBLIC_URL || '';

  if (publicUrl.startsWith('http')) {
    try {
      return new URL(publicUrl).pathname.replace(/\/$/, '');
    } catch {
      return '';
    }
  }

  return publicUrl.startsWith('/') ? publicUrl.replace(/\/$/, '') : '';
}

function AppShell() {
  const location = useLocation();
  const isBackoffice = location.pathname.startsWith('/dashboard');
  const showStoreShell = !isBackoffice;

  return (
    <div className="min-h-screen bg-[#f5f7fb] text-slate-950">
      {showStoreShell && <Navbar />}
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/produtos" element={<Produtos />} />
        <Route path="/produto/:id" element={<ProdutoDetalhe />} />
        <Route path="/lojas" element={<Lojas />} />
        <Route path="/contato" element={<Contato />} />
        <Route path="/carrinho" element={<Carrinho />} />
        <Route path="/checkout" element={<Checkout />} />
        <Route path="/login" element={<Login />} />
        <Route path="/area-cliente" element={<AreaCliente />} />
        <Route path="/dashboard/*" element={<Dashboard />} />
        <Route path="/admin" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      {showStoreShell && <Footer />}
    </div>
  );
}

function App() {
  const routerBasename = getRouterBasename();

  return (
    <AuthProvider>
      <CartProvider>
        <BrowserRouter basename={routerBasename || undefined}>
          <AppShell />
        </BrowserRouter>
      </CartProvider>
    </AuthProvider>
  );
}

export default App;
