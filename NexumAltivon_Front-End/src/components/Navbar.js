import { Link, NavLink } from 'react-router-dom';
import { BarChart3, Menu, Search, ShoppingBag, User, X } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

const navItems = [
  { to: '/', label: 'Inicio' },
  { to: '/produtos', label: 'Catalogo' },
  { to: '/lojas', label: 'Lojas' },
  { to: '/contato', label: 'Contato' },
];

export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);
  const { logout, isAuthenticated } = useAuth();
  const { getItemCount } = useCart();
  const itemCount = getItemCount();
  const navClass = ({ isActive }) =>
    `rounded-full px-4 py-2 text-sm font-semibold transition ${
      isActive ? 'bg-slate-950 text-white' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-950'
    }`;

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200/80 bg-white/92 backdrop-blur-xl">
      <div className="mx-auto flex h-[72px] max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link to="/" className="flex items-center gap-3" aria-label="Nexum Altivon">
          <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-slate-950 text-sm font-black tracking-wide text-amber-300 shadow-sm">
            NA
          </div>
          <div className="leading-tight">
            <p className="text-base font-black tracking-wide text-slate-950">Nexum Altivon</p>
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Store</p>
          </div>
        </Link>

        <nav className="hidden items-center gap-1 rounded-full border border-slate-200 bg-white p-1 shadow-sm md:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navClass} data-testid={`nav-${item.label.toLowerCase()}`}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="hidden items-center gap-2 md:flex">
          <Link
            to="/produtos"
            className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-slate-200 text-slate-700 transition hover:border-slate-950 hover:text-slate-950"
            aria-label="Buscar produtos"
            title="Buscar produtos"
          >
            <Search size={19} />
          </Link>
          <Link
            to="/carrinho"
            className="relative inline-flex h-10 w-10 items-center justify-center rounded-full border border-slate-200 text-slate-700 transition hover:border-slate-950 hover:text-slate-950"
            aria-label="Carrinho"
            title="Carrinho"
            data-testid="nav-cart"
          >
            <ShoppingBag size={19} />
            {itemCount > 0 && (
              <span className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-rose-600 px-1 text-xs font-bold text-white">
                {itemCount}
              </span>
            )}
          </Link>

          {isAuthenticated ? (
            <>
              <Link
                to="/dashboard"
                className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-slate-200 text-slate-700 transition hover:border-slate-950 hover:text-slate-950"
                aria-label="Painel"
                title="Painel"
                data-testid="nav-dashboard"
              >
                <BarChart3 size={19} />
              </Link>
              <button onClick={logout} className="rounded-full px-4 py-2 text-sm font-bold text-slate-600 transition hover:bg-slate-100">
                Sair
              </button>
            </>
          ) : (
            <Link
              to="/login"
              className="inline-flex items-center gap-2 rounded-full bg-slate-950 px-5 py-2.5 text-sm font-bold text-white shadow-sm transition hover:bg-slate-800"
              data-testid="nav-login"
            >
              <User size={17} />
              Entrar
            </Link>
          )}
        </div>

        <button
          onClick={() => setIsOpen((value) => !value)}
          className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-slate-200 text-slate-800 md:hidden"
          aria-label="Abrir menu"
        >
          {isOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>

      {isOpen && (
        <div className="border-t border-slate-200 bg-white px-4 py-4 shadow-xl md:hidden">
          <div className="space-y-1">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className="block rounded-lg px-3 py-3 text-sm font-bold text-slate-700 hover:bg-slate-100"
                onClick={() => setIsOpen(false)}
              >
                {item.label}
              </NavLink>
            ))}
            <Link to="/carrinho" className="block rounded-lg px-3 py-3 text-sm font-bold text-slate-700 hover:bg-slate-100" onClick={() => setIsOpen(false)}>
              Carrinho ({itemCount})
            </Link>
            <Link to={isAuthenticated ? '/dashboard' : '/login'} className="block rounded-lg bg-slate-950 px-3 py-3 text-sm font-bold text-white" onClick={() => setIsOpen(false)}>
              {isAuthenticated ? 'Painel' : 'Entrar'}
            </Link>
          </div>
        </div>
      )}
    </header>
  );
}
