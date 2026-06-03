import { Link, NavLink } from 'react-router-dom';
import { BarChart3, Menu, Search, ShoppingBag, User, X } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

const navItems = [
  { to: '/', label: 'Início' },
  { to: '/produtos', label: 'Catálogo' },
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
      isActive ? 'bg-[#C9A227] text-black' : 'text-zinc-200 hover:bg-white/10 hover:text-[#E8D5A3]'
    }`;

  return (
    <header className="sticky top-0 z-50 border-b border-[#2A2A2A] bg-[#0A0A0A]/95 text-white backdrop-blur-xl">
      <div className="mx-auto flex h-[72px] max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link to="/" className="flex items-center gap-3" aria-label="Nexum Altivon">
          <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-[#C9A227] text-sm font-black tracking-wide text-black shadow-sm">
            NA
          </div>
          <div className="leading-tight">
            <p className="text-base font-black tracking-wide text-[#C9A227]">Nexum Altivon</p>
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-zinc-400">Grupo Commerce</p>
          </div>
        </Link>

        <nav className="hidden items-center gap-1 rounded-full border border-[#2A2A2A] bg-black/30 p-1 shadow-sm md:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navClass} data-testid={`nav-${item.label.toLowerCase()}`}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="hidden items-center gap-2 md:flex">
          <Link
            to="/produtos"
            className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#2A2A2A] text-zinc-300 transition hover:border-[#C9A227] hover:text-[#C9A227]"
            aria-label="Buscar produtos"
            title="Buscar produtos"
          >
            <Search size={19} />
          </Link>
          <Link
            to="/carrinho"
            className="relative inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#2A2A2A] text-zinc-300 transition hover:border-[#C9A227] hover:text-[#C9A227]"
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
                className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#2A2A2A] text-zinc-300 transition hover:border-[#C9A227] hover:text-[#C9A227]"
                aria-label="Painel"
                title="Painel"
                data-testid="nav-dashboard"
              >
                <BarChart3 size={19} />
              </Link>
              <button onClick={logout} className="rounded-full px-4 py-2 text-sm font-bold text-zinc-300 transition hover:bg-white/10">
                Sair
              </button>
            </>
          ) : (
            <Link
              to="/login"
              className="inline-flex items-center gap-2 rounded-full bg-[#C9A227] px-5 py-2.5 text-sm font-bold text-black shadow-sm transition hover:bg-[#E8D5A3]"
              data-testid="nav-login"
            >
              <User size={17} />
              Entrar
            </Link>
          )}
        </div>

        <button
          onClick={() => setIsOpen((value) => !value)}
          className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#2A2A2A] text-[#C9A227] md:hidden"
          aria-label="Abrir menu"
        >
          {isOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>

      {isOpen && (
        <div className="border-t border-[#2A2A2A] bg-[#0A0A0A] px-4 py-4 shadow-xl md:hidden">
          <div className="space-y-1">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className="block rounded-lg px-3 py-3 text-sm font-bold text-zinc-200 hover:bg-white/10"
                onClick={() => setIsOpen(false)}
              >
                {item.label}
              </NavLink>
            ))}
            <Link to="/carrinho" className="block rounded-lg px-3 py-3 text-sm font-bold text-zinc-200 hover:bg-white/10" onClick={() => setIsOpen(false)}>
              Carrinho ({itemCount})
            </Link>
            <Link to={isAuthenticated ? '/dashboard' : '/login'} className="block rounded-lg bg-[#C9A227] px-3 py-3 text-sm font-bold text-black" onClick={() => setIsOpen(false)}>
              {isAuthenticated ? 'Painel' : 'Entrar'}
            </Link>
          </div>
        </div>
      )}
    </header>
  );
}
