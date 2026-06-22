import { Link, useLocation } from 'react-router-dom';
import { MessageCircleMore, LayoutDashboard, ShoppingBag, Home, UserRound, FileText, Truck, Search } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const supportConfigs = {
  public: {
    href: 'mailto:corporativo.gna@gmail.com?subject=Yara%20-%20Atendimento%20de%20vendas&body=Ol%C3%A1%20Yara%2C%20preciso%20de%20ajuda%20com%20assuntos%20da%20empresa%2C%20produtos%20ou%20d%C3%BAvidas%20sobre%20a%20compra.',
    title: 'Yara online',
    subtitle: 'Ajuda de vendas',
  },
  backoffice: {
    href: 'mailto:corporativo.gna@gmail.com?subject=Sophia%20-%20Apoio%20ERP&body=Ol%C3%A1%20Sophia%2C%20preciso%20de%20apoio%20interno%20na%20opera%C3%A7%C3%A3o%20do%20GenesisGest.Net.',
    title: 'Sophia online',
    subtitle: 'Mensagem instantânea',
  },
};

function ActionLink({ to, label, icon: Icon }) {
  return (
    <Link
      to={to}
      className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-black/85 px-4 py-2 text-xs font-black uppercase tracking-[0.12em] text-zinc-200 shadow-2xl shadow-black/35 backdrop-blur transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
    >
      <Icon size={14} />
      <span>{label}</span>
    </Link>
  );
}

export default function GlobalActions() {
  const location = useLocation();
  const { isAuthenticated, isAdmin } = useAuth();
  const backoffice = location.pathname.startsWith('/dashboard');
  const support = backoffice ? supportConfigs.backoffice : supportConfigs.public;

  const actions = backoffice
    ? [
        { to: '/dashboard', label: 'Painel', icon: LayoutDashboard },
        { to: '/dashboard/erp', label: 'ERP', icon: FileText },
        { to: '/dashboard/erp-fiscal', label: 'Fiscal', icon: FileText },
        { to: '/dashboard/erp-logistica', label: 'Logística', icon: Truck },
      ]
    : [
        { to: '/', label: 'Início', icon: Home },
        { to: '/produtos', label: 'Catálogo', icon: Search },
        { to: '/carrinho', label: 'Carrinho', icon: ShoppingBag },
        { to: isAuthenticated && !isAdmin ? '/area-cliente' : '/login', label: 'Cliente', icon: UserRound },
      ];

  return (
    <>
      <a
        href={support.href}
        className="fixed bottom-4 right-4 z-50 inline-flex items-center gap-3 rounded-full border border-[#C9A227]/40 bg-[#111111]/95 px-4 py-3 text-sm font-black text-[#E8D5A3] shadow-2xl shadow-black/40 backdrop-blur transition hover:border-[#E8D5A3] hover:text-white sm:bottom-5 sm:right-5"
      >
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-[#C9A227] text-black">
          <MessageCircleMore size={18} />
        </span>
        <span className="flex flex-col text-left leading-tight">
          <span>{support.title}</span>
          <span className="text-[11px] font-semibold uppercase tracking-[0.14em] text-zinc-400">{support.subtitle}</span>
        </span>
      </a>

      <div className="fixed bottom-4 left-4 right-4 z-50 sm:left-1/2 sm:right-auto sm:w-max sm:-translate-x-1/2">
        <div className="mx-auto flex max-w-full gap-2 overflow-x-auto rounded-full border border-white/10 bg-[#0B0B0B]/95 px-2 py-2 shadow-2xl shadow-black/35 backdrop-blur sm:justify-center">
          {actions.map((item) => (
            <ActionLink key={item.to} to={item.to} label={item.label} icon={item.icon} />
          ))}
        </div>
      </div>
    </>
  );
}
