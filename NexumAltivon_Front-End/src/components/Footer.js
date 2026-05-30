import { Link } from 'react-router-dom';
import { Mail, MapPin, ShieldCheck } from 'lucide-react';

const links = [
  { to: '/produtos', label: 'Catalogo' },
  { to: '/lojas', label: 'Lojas' },
  { to: '/contato', label: 'Contato' },
];

export default function Footer() {
  return (
    <footer className="border-t border-slate-200 bg-white">
      <div className="mx-auto grid max-w-7xl gap-8 px-4 py-10 sm:px-6 md:grid-cols-[1.5fr_1fr_1fr] lg:px-8">
        <div className="space-y-4">
          <Link to="/" className="inline-flex items-center gap-3" aria-label="Nexum Altivon">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-950 text-xs font-black tracking-wide text-amber-300">
              NA
            </div>
            <div className="leading-tight">
              <p className="text-base font-black text-slate-950">Nexum Altivon</p>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Commerce</p>
            </div>
          </Link>
          <p className="max-w-md text-sm leading-6 text-slate-600">
            Operacao digital do Grupo Nexum Altivon para produtos premium, lojas integradas e atendimento comercial.
          </p>
        </div>

        <div>
          <p className="mb-3 text-sm font-black uppercase tracking-[0.14em] text-slate-500">Navegacao</p>
          <div className="space-y-2">
            {links.map((item) => (
              <Link key={item.to} to={item.to} className="block text-sm font-semibold text-slate-700 transition hover:text-slate-950">
                {item.label}
              </Link>
            ))}
          </div>
        </div>

        <div>
          <p className="mb-3 text-sm font-black uppercase tracking-[0.14em] text-slate-500">Contato</p>
          <div className="space-y-3 text-sm font-semibold text-slate-700">
            <a className="flex items-center gap-2 transition hover:text-slate-950" href="mailto:contato@nexumaltivon.com">
              <Mail size={16} />
              contato@nexumaltivon.com
            </a>
            <p className="flex items-center gap-2">
              <MapPin size={16} />
              Brasil
            </p>
            <p className="flex items-center gap-2">
              <ShieldCheck size={16} />
              Compra segura
            </p>
          </div>
        </div>
      </div>
      <div className="border-t border-slate-200 px-4 py-4 text-center text-xs font-semibold text-slate-500">
        © 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.
      </div>
    </footer>
  );
}
