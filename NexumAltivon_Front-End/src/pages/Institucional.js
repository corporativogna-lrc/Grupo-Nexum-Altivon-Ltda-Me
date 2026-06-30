/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { Building2, CheckCircle2, Store, Users } from 'lucide-react';

export default function Institucional() {
  return (
    <main className="min-h-screen bg-[#050505] text-white">
      <section className="relative overflow-hidden px-4 py-16 sm:px-6 lg:px-8">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,#C9A22724,transparent_42%)]" />
        <div className="relative mx-auto max-w-5xl">
          <p className="text-sm font-black uppercase tracking-[0.24em] text-[#E8D5A3]">Institucional</p>
          <h1 className="mt-4 font-serif text-5xl font-bold text-[#C9A227]">Grupo Nexum Altivon</h1>
          <p className="mt-6 max-w-3xl text-lg leading-8 text-zinc-300">
            Grupo empresarial estruturado para atuação em e-commerce, parcerias comerciais, compras, estoque, atendimento e expansão de marcas próprias.
          </p>
        </div>
      </section>

      <section className="mx-auto grid max-w-6xl gap-6 px-4 pb-16 sm:px-6 md:grid-cols-3 lg:px-8">
        {[
          { icon: Store, title: 'Marcas em expansão', text: 'Operação integrada para seis frentes comerciais com curadoria, cadastro e venda assistida.' },
          { icon: Building2, title: 'Gestão centralizada', text: 'Processos conectados ao GenesisGest.Net para compras, vendas, financeiro, fiscal e logística.' },
          { icon: Users, title: 'Relacionamento', text: 'Atendimento comercial e área do cliente para histórico, pedidos, documentos e acompanhamento.' },
        ].map((item) => {
          const Icon = item.icon;
          return (
            <article key={item.title} className="rounded-[28px] border border-white/10 bg-[#111111] p-6">
              <Icon className="text-[#C9A227]" size={30} />
              <h2 className="mt-5 text-xl font-black text-white">{item.title}</h2>
              <p className="mt-3 text-sm leading-7 text-zinc-400">{item.text}</p>
            </article>
          );
        })}
      </section>

      <section className="mx-auto max-w-5xl px-4 pb-20 sm:px-6 lg:px-8">
        <div className="rounded-[32px] border border-[#C9A227]/20 bg-[#111111] p-8">
          <h2 className="font-serif text-3xl font-bold text-[#C9A227]">Compromissos operacionais</h2>
          <div className="mt-6 grid gap-4">
            {[
              'Organizar dados reais de produtos, clientes, pedidos e fornecedores.',
              'Evitar exposição de informações técnicas ao cliente final.',
              'Manter políticas públicas claras para privacidade, reembolso e atendimento.',
              'Evoluir os módulos do ERP sem mascarar pendências operacionais.',
            ].map((item) => (
              <div key={item} className="flex items-start gap-3 rounded-2xl border border-white/10 bg-black/25 p-4 text-sm text-zinc-200">
                <CheckCircle2 className="mt-0.5 shrink-0 text-[#C9A227]" size={18} />
                {item}
              </div>
            ))}
          </div>
        </div>
      </section>
    </main>
  );
}
