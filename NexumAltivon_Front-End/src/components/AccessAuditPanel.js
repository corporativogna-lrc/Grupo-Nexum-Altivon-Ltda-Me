/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useCallback, useEffect, useState } from 'react';
import { FileSearch, Pencil, Save, ShieldCheck, UserCog, UserX, X } from 'lucide-react';
import { auditoriaAPI, usuarioAdminAPI } from '../services/api';

const emptyUser = { id: null, nome: '', email: '', telefone: '', perfil: 'Vendedor', senha: '', ativo: true };
const emptyFilters = { modulo: '', tabela: '', usuario: '', dataInicio: '', dataFim: '' };

const errorMessage = (error) =>
  error?.response?.data?.mensagem || error?.response?.data?.detail || error?.message || 'Operação administrativa não concluída.';

export default function AccessAuditPanel({ mode }) {
  return mode === 'usuarios' ? <UsersPanel /> : <AuditPanel />;
}

function UsersPanel() {
  const [usuarios, setUsuarios] = useState([]);
  const [perfis, setPerfis] = useState([]);
  const [form, setForm] = useState(emptyUser);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [feedback, setFeedback] = useState({ type: '', text: '' });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [usersResponse, rolesResponse] = await Promise.all([
        usuarioAdminAPI.listar(),
        usuarioAdminAPI.listarPerfis(),
      ]);
      setUsuarios(Array.isArray(usersResponse.data) ? usersResponse.data : []);
      setPerfis(Array.isArray(rolesResponse.data) ? rolesResponse.data : []);
    } catch (error) {
      setFeedback({ type: 'error', text: errorMessage(error) });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const edit = (usuario) => {
    setForm({
      id: usuario.id,
      nome: usuario.nome || '',
      email: usuario.email || '',
      telefone: usuario.telefone || '',
      perfil: usuario.perfil || 'Vendedor',
      senha: '',
      ativo: Boolean(usuario.ativo),
    });
    setFeedback({ type: '', text: '' });
  };

  const persist = async (event) => {
    event.preventDefault();
    setSaving(true);
    setFeedback({ type: '', text: '' });
    const payload = {
      nome: form.nome.trim(),
      email: form.email.trim().toLowerCase(),
      telefone: form.telefone.trim() || null,
      perfil: form.perfil,
      senha: form.senha || null,
      ativo: form.ativo,
    };
    try {
      const response = form.id
        ? await usuarioAdminAPI.atualizar(form.id, payload)
        : await usuarioAdminAPI.criar(payload);
      const persisted = response.data;
      if (!persisted?.id || persisted.email !== payload.email) throw new Error('A API não confirmou o usuário persistido.');
      setUsuarios((current) => current.some((item) => item.id === persisted.id)
        ? current.map((item) => item.id === persisted.id ? persisted : item)
        : [...current, persisted].sort((a, b) => a.nome.localeCompare(b.nome)));
      edit(persisted);
      setFeedback({ type: 'success', text: `Usuário ${persisted.email} confirmado no banco com ID ${persisted.id}.` });
    } catch (error) {
      setFeedback({ type: 'error', text: errorMessage(error) });
    } finally {
      setSaving(false);
    }
  };

  const deactivate = async (usuario) => {
    if (!window.confirm(`Desativar o acesso de ${usuario.nome}?`)) return;
    setFeedback({ type: '', text: '' });
    try {
      const response = await usuarioAdminAPI.atualizar(usuario.id, {
        nome: usuario.nome,
        email: usuario.email,
        telefone: usuario.telefone,
        perfil: usuario.perfil,
        senha: null,
        ativo: false,
      });
      const persisted = response.data;
      if (!persisted?.id || persisted.ativo !== false) throw new Error('A API não confirmou a desativação do acesso.');
      setUsuarios((current) => current.map((item) => item.id === persisted.id ? persisted : item));
      if (form.id === persisted.id) edit(persisted);
      setFeedback({ type: 'success', text: `Acesso ${persisted.email} desativado no banco.` });
    } catch (error) {
      setFeedback({ type: 'error', text: errorMessage(error) });
    }
  };

  return (
    <section className="space-y-6">
      <PanelHeader eyebrow="GRC e IAM" title="Usuários administrativos" description="Acessos do GenesisGest.Net com perfil, senha BCrypt e estado persistidos na API oficial." icon={UserCog} />
      <Feedback value={feedback} />
      <div className="grid gap-6 xl:grid-cols-[minmax(350px,0.75fr)_minmax(0,1.25fr)]">
        <form onSubmit={persist} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between border-b border-slate-100 pb-4">
            <div><p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">{form.id ? `Usuário ${form.id}` : 'Novo usuário'}</p><h3 className="mt-1 text-lg font-black text-slate-950">Identidade e permissão</h3></div>
            {form.id && <button type="button" title="Novo usuário" onClick={() => setForm(emptyUser)} className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200"><X size={18} /></button>}
          </div>
          <div className="mt-5 grid gap-4">
            <Input label="Nome" value={form.nome} onChange={(value) => setForm((current) => ({ ...current, nome: value }))} required />
            <Input label="E-mail" type="email" value={form.email} onChange={(value) => setForm((current) => ({ ...current, email: value }))} required />
            <Input label="Telefone" value={form.telefone} onChange={(value) => setForm((current) => ({ ...current, telefone: value }))} />
            <label className="text-sm font-bold text-slate-700">Perfil<select value={form.perfil} onChange={(event) => setForm((current) => ({ ...current, perfil: event.target.value }))} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 font-semibold">{perfis.map((perfil) => <option key={perfil} value={perfil}>{perfil}</option>)}</select></label>
            <Input label={form.id ? 'Nova senha (opcional)' : 'Senha inicial'} type="password" minLength="8" value={form.senha} onChange={(value) => setForm((current) => ({ ...current, senha: value }))} required={!form.id} />
            <label className="flex items-center gap-3 rounded-lg border border-slate-200 p-3 text-sm font-bold text-slate-700"><input type="checkbox" checked={form.ativo} onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))} className="h-5 w-5 accent-slate-950" />Acesso ativo</label>
          </div>
          <button disabled={saving} className="mt-6 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-60"><Save size={17} />{saving ? 'Gravando...' : form.id ? 'Atualizar usuário' : 'Criar usuário'}</button>
        </form>
        <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-5"><h3 className="text-lg font-black text-slate-950">Acessos persistidos</h3><p className="mt-1 text-sm text-slate-500">{usuarios.length} registros retornados por <code>/api/admin/usuarios</code>.</p></div>
          {loading ? <p className="p-6 text-sm font-semibold text-slate-500">Consultando usuários...</p> : <div className="divide-y divide-slate-100">{usuarios.map((usuario) => (
            <article key={usuario.id} className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:justify-between">
              <div><div className="flex flex-wrap items-center gap-2"><ShieldCheck size={18} className="text-amber-700" /><strong className="text-slate-950">{usuario.nome}</strong><span className={`rounded-full px-2.5 py-1 text-xs font-black ${usuario.ativo ? 'bg-emerald-50 text-emerald-800' : 'bg-slate-100 text-slate-600'}`}>{usuario.ativo ? 'Ativo' : 'Inativo'}</span></div><p className="mt-2 text-sm text-slate-600">{usuario.email} · {usuario.perfil}</p></div>
              <div className="flex gap-2"><button type="button" title="Editar usuário" onClick={() => edit(usuario)} className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200"><Pencil size={17} /></button><button type="button" title="Desativar usuário" disabled={!usuario.ativo} onClick={() => deactivate(usuario)} className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-red-200 text-red-700 disabled:opacity-35"><UserX size={17} /></button></div>
            </article>
          ))}</div>}
        </div>
      </div>
    </section>
  );
}

function AuditPanel() {
  const [filters, setFilters] = useState(emptyFilters);
  const [records, setRecords] = useState([]);
  const [detail, setDetail] = useState(null);
  const [loading, setLoading] = useState(true);
  const [feedback, setFeedback] = useState({ type: '', text: '' });

  const load = useCallback(async (activeFilters = emptyFilters) => {
    setLoading(true); setFeedback({ type: '', text: '' });
    try {
      const params = Object.fromEntries(Object.entries(activeFilters).filter(([, value]) => value !== ''));
      if (params.usuario) params.usuario = Number(params.usuario);
      const response = await auditoriaAPI.listar(params);
      setRecords(Array.isArray(response.data) ? response.data : []);
    } catch (error) { setFeedback({ type: 'error', text: errorMessage(error) }); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  const inspect = async (id) => {
    setFeedback({ type: '', text: '' });
    try {
      const response = await auditoriaAPI.obter(id);
      if (!response.data?.id) throw new Error('A API não confirmou o detalhe da auditoria.');
      setDetail(response.data);
    } catch (error) { setFeedback({ type: 'error', text: errorMessage(error) }); }
  };

  return (
    <section className="space-y-6">
      <PanelHeader eyebrow="GRC e IAM" title="Trilha de auditoria" description="Consulta rastreável de alterações, usuário, endpoint e dados anteriores/novos registrados no MySQL." icon={FileSearch} />
      <Feedback value={feedback} />
      <form onSubmit={(event) => { event.preventDefault(); load(filters); }} className="grid gap-4 rounded-lg border border-slate-200 bg-white p-5 shadow-sm md:grid-cols-3 xl:grid-cols-6">
        <Input label="Módulo / endpoint" value={filters.modulo} onChange={(value) => setFilters((current) => ({ ...current, modulo: value }))} />
        <Input label="Tabela" value={filters.tabela} onChange={(value) => setFilters((current) => ({ ...current, tabela: value }))} />
        <Input label="ID do usuário" type="number" min="1" value={filters.usuario} onChange={(value) => setFilters((current) => ({ ...current, usuario: value }))} />
        <Input label="Data inicial" type="date" value={filters.dataInicio} onChange={(value) => setFilters((current) => ({ ...current, dataInicio: value }))} />
        <Input label="Data final" type="date" value={filters.dataFim} onChange={(value) => setFilters((current) => ({ ...current, dataFim: value }))} />
        <button className="mt-auto inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white"><FileSearch size={17} />Consultar</button>
      </form>
      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.25fr)_minmax(340px,0.75fr)]">
        <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-5"><h3 className="text-lg font-black text-slate-950">Eventos registrados</h3><p className="mt-1 text-sm text-slate-500">{records.length} ocorrências retornadas.</p></div>
          {loading ? <p className="p-6 text-sm font-semibold text-slate-500">Consultando trilha...</p> : <div className="max-h-[620px] divide-y divide-slate-100 overflow-auto">{records.map((record) => (
            <button type="button" key={record.id} onClick={() => inspect(record.id)} className="grid w-full gap-2 p-4 text-left transition hover:bg-slate-50 sm:grid-cols-[90px_1fr_130px] sm:items-center"><span className="font-mono text-xs font-black text-slate-500">#{record.id}</span><span><strong className="block text-sm text-slate-950">{record.acao} · {record.tabela || 'Sem tabela'}</strong><span className="mt-1 block text-xs text-slate-500">{record.endpoint || 'Endpoint não informado'} · usuário {record.usuarioId ?? 'sistema'}</span></span><span className="text-xs font-semibold text-slate-500">{formatDateTime(record.createdAt)}</span></button>
          ))}</div>}
        </div>
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="text-lg font-black text-slate-950">Detalhe do evento</h3>
          {!detail ? <p className="mt-4 text-sm text-slate-500">Selecione um evento para consultar os dados persistidos.</p> : <div className="mt-5 space-y-4 text-sm"><Detail label="Identificador" value={detail.id} /><Detail label="Tabela / registro" value={`${detail.tabela || '-'} / ${detail.registroId || '-'}`} /><Detail label="Ação" value={detail.acao} /><Detail label="Usuário" value={`${detail.usuarioId ?? 'sistema'} · ${detail.usuarioTipo || '-'}`} /><Detail label="IP" value={detail.ipAddress || '-'} /><Detail label="Endpoint" value={detail.endpoint || '-'} /><JsonBlock label="Dados anteriores" value={detail.dadosAnteriores} /><JsonBlock label="Dados novos" value={detail.dadosNovos} /></div>}
        </div>
      </div>
    </section>
  );
}

function PanelHeader({ eyebrow, title, description, icon: Icon }) { return <div className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6 shadow-sm sm:flex-row sm:items-center"><div className="inline-flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-slate-950 text-[#C9A227]"><Icon size={23} /></div><div><p className="text-xs font-black uppercase tracking-[0.18em] text-amber-700">{eyebrow}</p><h2 className="mt-1 text-2xl font-black text-slate-950">{title}</h2><p className="mt-1 text-sm text-slate-500">{description}</p></div></div>; }
function Feedback({ value }) { return value.text ? <div className={`rounded-lg border px-5 py-4 text-sm font-bold ${value.type === 'error' ? 'border-red-200 bg-red-50 text-red-800' : 'border-emerald-200 bg-emerald-50 text-emerald-800'}`}>{value.text}</div> : null; }
function Input({ label, onChange, ...props }) { return <label className="block text-sm font-bold text-slate-700">{label}<input {...props} onChange={(event) => onChange(event.target.value)} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10" /></label>; }
function Detail({ label, value }) { return <div><p className="text-xs font-black uppercase tracking-[0.14em] text-slate-400">{label}</p><p className="mt-1 break-words font-semibold text-slate-800">{value}</p></div>; }
function JsonBlock({ label, value }) { return <div><p className="text-xs font-black uppercase tracking-[0.14em] text-slate-400">{label}</p><pre className="mt-2 max-h-48 overflow-auto rounded-lg bg-slate-950 p-3 text-xs text-slate-100">{formatJson(value)}</pre></div>; }
function formatDateTime(value) { return value ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : '-'; }
function formatJson(value) { if (!value) return 'Sem conteúdo registrado.'; try { return JSON.stringify(JSON.parse(value), null, 2); } catch { return String(value); } }
