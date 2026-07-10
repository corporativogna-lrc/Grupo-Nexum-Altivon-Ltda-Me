/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { Link, NavLink } from 'react-router-dom';
import { BarChart3, ChevronDown, LayoutDashboard, LogOut, Menu, Search, ShoppingBag, User, X } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { siteAPI, unwrapApiData } from '../services/api';

const fallbackLogo = '/imagens/homepage/Logo-2.png';
const resolveLogo = (logo) => {
  const value = String(logo || '').trim();
  return value && !value.includes('logo-grupo-nexum-altivon.svg') ? value : fallbackLogo;
};

const navItems = [
  { to: '/', label: 'Início' },
  { to: '/produtos', label: 'Catálogo' },
  { to: '/lojas', label: 'Lojas' },
  { to: '/acompanhar-pedido', label: 'Pedido' },
  { to: '/contato', label: 'Contato' },
];

export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const { logout, isAuthenticated, isAdmin, user } = useAuth();
  const { getItemCount } = useCart();
  const itemCount = getItemCount();
  const [branding, setBranding] = useState({
    siteName: 'Grupo Nexum Altivon',
    subtitle: 'Participações societárias',
    logo: fallbackLogo,
  });
  const displayName = useMemo(() => {
    const rawName = String(user?.nome || user?.name || user?.email || '').trim();
    return rawName ? rawName.split(' ')[0] : 'Conta';
  }, [user]);
  const portalLink = isAdmin ? '/dashboard' : '/area-cliente';
  const portalLabel = isAdmin ? 'GenesisGest.Net' : 'Área do cliente';
  const navClass = ({ isActive }) =>
    `rounded-full px-4 py-2 text-sm font-semibold transition ${
      isActive ? 'bg-[#C9A227] text-black' : 'text-zinc-200 hover:bg-white/10 hover:text-[#E8D5A3]'
    }`;

  useEffect(() => {
    let active = true;

    siteAPI
      .getPublicConfig()
      .then((response) => {
        const config = unwrapApiData(response.data) || {};
        if (!active) return;
        setBranding({
          siteName: config.siteNome || config.siteName || 'Grupo Nexum Altivon',
          subtitle: config.siteSubtitulo || config.siteSubtitle || 'Participações societárias',
          logo: resolveLogo(config.siteLogo),
        });
      })
      .catch(() => {});

    return () => {
      active = false;
    };
  }, []);

  return (
    <header className="sticky top-0 z-50 border-b border-[#2A2A2A] bg-[#0A0A0A]/95 text-white backdrop-blur-xl">
      <div className="mx-auto flex h-[72px] max-w-7xl items-center justify-between gap-3 px-3 sm:px-6 lg:px-8">
        <Link to="/" className="flex min-w-0 items-center gap-2 sm:gap-3" aria-label="Grupo Nexum Altivon">
          <img
            src={branding.logo}
            alt="Logotipo Grupo Nexum Altivon"
            className="h-12 w-12 shrink-0 rounded-xl bg-[#C9A227] object-contain p-1 shadow-sm sm:h-14 sm:w-14"
            onError={(event) => {
              event.currentTarget.src = fallbackLogo;
            }}
          />
          <div className="min-w-0 leading-tight">
            <p className="max-w-[132px] truncate text-sm font-black tracking-wide text-[#C9A227] sm:max-w-none sm:text-base">{branding.siteName}</p>
            <p className="max-w-[132px] truncate text-[10px] font-semibold uppercase tracking-[0.16em] text-zinc-400 sm:max-w-none sm:text-xs">{branding.subtitle}</p>
          </div>
        </Link>

        <nav className="hidden items-center gap-1 rounded-full border border-[#2A2A2A] bg-black/30 p-1 shadow-sm lg:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navClass} data-testid={`nav-${item.label.toLowerCase()}`}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="hidden items-center gap-2 lg:flex">
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
            className="inline-flex items-center gap-2 rounded-full border border-[#2A2A2A] px-4 py-2 text-sm font-black text-zinc-200 transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
            aria-label={`Carrinho (${itemCount} itens)`}
            title={`Carrinho (${itemCount} itens)`}
            data-testid="nav-cart"
          >
            <ShoppingBag size={19} />
            <span>Carrinho</span>
            <span
              className="inline-flex min-w-6 items-center justify-center rounded-full bg-rose-600 px-2 py-0.5 text-xs font-black text-white"
              data-testid="nav-cart-count"
            >
              {itemCount}
            </span>
          </Link>

          {isAuthenticated ? (
            <div className="relative">
              <button
                type="button"
                onClick={() => setIsProfileOpen((current) => !current)}
                className="inline-flex items-center gap-3 rounded-full border border-[#2A2A2A] bg-black/30 px-4 py-2 text-sm font-bold text-zinc-100 transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
              >
                <span className="inline-flex h-10 w-10 items-center justify-center rounded-full bg-[#C9A227] text-sm font-black text-black">
                  {displayName.slice(0, 2).toUpperCase()}
                </span>
                <span className="text-left leading-tight">
                  <span className="block text-xs uppercase tracking-[0.18em] text-zinc-500">Logado como</span>
                  <span className="block">{displayName}</span>
                </span>
                <ChevronDown size={16} />
              </button>

              {isProfileOpen && (
                <div className="absolute right-0 top-14 w-64 rounded-3xl border border-[#2A2A2A] bg-[#111111] p-3 shadow-2xl">
                  <div className="rounded-2xl border border-white/5 bg-white/5 p-4">
                    <p className="text-sm font-black text-white">{user?.nome || user?.email}</p>
                    <p className="mt-1 text-xs font-semibold uppercase tracking-[0.18em] text-zinc-500">{user?.role || user?.perfil}</p>
                  </div>
                  <div className="mt-3 grid gap-2">
                    <Link
                      to={portalLink}
                      onClick={() => setIsProfileOpen(false)}
                      className="inline-flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-bold text-zinc-100 transition hover:bg-white/10"
                    >
                      {isAdmin ? <BarChart3 size={17} /> : <LayoutDashboard size={17} />}
                      {portalLabel}
                    </Link>
                    <Link
                      to="/carrinho"
                      onClick={() => setIsProfileOpen(false)}
                      className="inline-flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-bold text-zinc-100 transition hover:bg-white/10"
                    >
                      <ShoppingBag size={17} />
                      Minhas compras
                    </Link>
                    <button
                      type="button"
                      onClick={() => {
                        setIsProfileOpen(false);
                        logout();
                      }}
                      className="inline-flex items-center gap-3 rounded-2xl px-4 py-3 text-left text-sm font-bold text-rose-200 transition hover:bg-rose-500/10"
                    >
                      <LogOut size={17} />
                      Sair
                    </button>
                  </div>
                </div>
              )}
            </div>
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

        <div className="flex shrink-0 items-center gap-2 lg:hidden">
          <Link
            to="/carrinho"
            className="inline-flex items-center gap-2 rounded-full border border-[#2A2A2A] px-3 py-2 text-xs font-black text-zinc-200 transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
            aria-label={`Carrinho (${itemCount} itens)`}
            title={`Carrinho (${itemCount} itens)`}
            data-testid="nav-cart-mobile"
          >
            <ShoppingBag size={17} />
            <span>Carrinho</span>
            <span
              className="inline-flex min-w-6 items-center justify-center rounded-full bg-rose-600 px-2 py-0.5 text-[10px] font-black text-white"
              data-testid="nav-cart-count-mobile"
            >
              {itemCount}
            </span>
          </Link>

          <button
            onClick={() => setIsOpen((value) => !value)}
            className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#2A2A2A] text-[#C9A227]"
            aria-label="Abrir menu"
          >
            {isOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </div>

      {isOpen && (
        <div className="border-t border-[#2A2A2A] bg-[#0A0A0A] px-4 py-4 shadow-xl lg:hidden">
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
            {isAuthenticated ? (
              <>
                <Link to={portalLink} className="block rounded-lg bg-[#C9A227] px-3 py-3 text-sm font-bold text-black" onClick={() => setIsOpen(false)}>
                  {portalLabel}
                </Link>
                <button
                  type="button"
                  onClick={() => {
                    setIsOpen(false);
                    logout();
                  }}
                  className="block w-full rounded-lg border border-[#2A2A2A] px-3 py-3 text-left text-sm font-bold text-zinc-200 hover:bg-white/10"
                >
                  Sair
                </button>
              </>
            ) : (
              <Link to="/login" className="block rounded-lg bg-[#C9A227] px-3 py-3 text-sm font-bold text-black" onClick={() => setIsOpen(false)}>
                Entrar
              </Link>
            )}
          </div>
        </div>
      )}
    </header>
  );
}
