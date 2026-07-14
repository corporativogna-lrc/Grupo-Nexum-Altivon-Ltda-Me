/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useCallback, useEffect, useState } from 'react';
import { CheckCircle2, Pencil, RefreshCw, Save, ShieldAlert, Trash2, XCircle } from 'lucide-react';
import { dropshippingAPI } from '../services/api';

const emptyForm = {
  id: null,
  nome: '',
  slug: '',
  tipo: 'CJDropshipping',
  apiEndpoint: '',
  ativo: false,
  rowVersion: null,
};

const readData = (response) => response?.data?.dados ?? response?.data?.Dados ?? [];
const readError = (error) =>
  error?.response?.data?.mensagem
  || error?.response?.data?.Mensagem
  || error?.message
  || 'A operacao nao foi concluida pela API.';

export default function DropshippingAdminPanel() {
  const [canais, setCanais] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const response = await dropshippingAPI.listar();
      setCanais(readData(response));
      setMessage(null);
    } catch (error) {
      setMessage({ type: 'error', text: readError(error) });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const edit = (canal) => {
    setForm({
      id: canal.id,
      nome: canal.nome,
      slug: canal.slug,
      tipo: canal.tipo,
      apiEndpoint: canal.apiEndpoint || '',
      ativo: Boolean(canal.ativo),
      rowVersion: canal.rowVersion,
    });
    setMessage(null);
  };

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setMessage(null);
    const payload = {
      nome: form.nome.trim(),
      slug: form.slug.trim(),
      tipo: form.tipo,
      apiEndpoint: form.apiEndpoint.trim() || null,
      ativo: form.ativo,
      rowVersion: form.rowVersion,
    };

    try {
      if (form.id) {
        await dropshippingAPI.atualizar(form.id, payload);
      } else {
        await dropshippingAPI.criar(payload);
      }
      setForm(emptyForm);
      await load();
      setMessage({ type: 'success', text: form.id ? 'Canal atualizado no banco.' : 'Canal criado no banco.' });
    } catch (error) {
      setMessage({ type: 'error', text: readError(error) });
    } finally {
      setSaving(false);
    }
  };

  const remove = async (canal) => {
    if (!window.confirm(`Excluir o canal ${canal.nome}?`)) {
      return;
    }

    setMessage(null);
    try {
      await dropshippingAPI.excluir(canal.id, canal.rowVersion);
      if (form.id === canal.id) {
        setForm(emptyForm);
      }
      await load();
      setMessage({ type: 'success', text: 'Canal excluido com soft-delete e auditoria.' });
    } catch (error) {
      setMessage({ type: 'error', text: readError(error) });
    }
  };

  return (
    <div className="mt-8 border-t border-slate-200 pt-7">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.16em] text-[#9A7415]">Canais persistidos</p>
          <h4 className="mt-1 text-xl font-black text-slate-950">Operacao de dropshipping</h4>
        </div>
        <button type="button" onClick={load} disabled={loading} title="Atualizar canais" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-300 bg-white text-slate-800 disabled:opacity-50">
          <RefreshCw size={18} className={loading ? 'animate-spin' : ''} />
        </button>
      </div>

      {message && (
        <div className={`mt-4 flex items-start gap-3 rounded-lg border p-4 text-sm font-bold ${message.type === 'success' ? 'border-emerald-200 bg-emerald-50 text-emerald-950' : 'border-red-200 bg-red-50 text-red-950'}`}>
          {message.type === 'success' ? <CheckCircle2 size={19} /> : <ShieldAlert size={19} />}
          <span>{message.text}</span>
        </div>
      )}

      <form onSubmit={submit} className="mt-5 grid gap-4 border-y border-slate-200 py-5 md:grid-cols-2">
        <label className="text-sm font-bold text-slate-700">Nome<input required minLength={3} maxLength={100} value={form.nome} onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))} className="mt-2 h-11 w-full rounded-lg border border-slate-300 px-3 outline-none focus:border-slate-950" /></label>
        <label className="text-sm font-bold text-slate-700">Slug<input required minLength={2} maxLength={50} value={form.slug} onChange={(event) => setForm((current) => ({ ...current, slug: event.target.value }))} className="mt-2 h-11 w-full rounded-lg border border-slate-300 px-3 outline-none focus:border-slate-950" /></label>
        <label className="text-sm font-bold text-slate-700">Tipo<select value={form.tipo} onChange={(event) => setForm((current) => ({ ...current, tipo: event.target.value }))} className="mt-2 h-11 w-full rounded-lg border border-slate-300 bg-white px-3 outline-none focus:border-slate-950"><option>CJDropshipping</option><option>Shopify</option><option>AliExpress</option><option>Dropi</option><option>Cartpanda</option><option>Nuvemshop</option><option>Outro</option></select></label>
        <label className="text-sm font-bold text-slate-700">Endpoint HTTPS<input type="url" value={form.apiEndpoint} onChange={(event) => setForm((current) => ({ ...current, apiEndpoint: event.target.value }))} className="mt-2 h-11 w-full rounded-lg border border-slate-300 px-3 outline-none focus:border-slate-950" /></label>
        <label className="flex items-center gap-3 text-sm font-black text-slate-800"><input type="checkbox" checked={form.ativo} onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))} className="h-5 w-5 accent-slate-950" />Ativar canal apos validar credenciais privadas</label>
        <div className="flex justify-end gap-3">
          {form.id && <button type="button" onClick={() => setForm(emptyForm)} className="inline-flex h-11 items-center gap-2 rounded-lg border border-slate-300 px-4 text-sm font-black"><XCircle size={17} />Cancelar</button>}
          <button disabled={saving} className="inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-50"><Save size={17} />{saving ? 'Salvando...' : form.id ? 'Atualizar' : 'Criar canal'}</button>
        </div>
      </form>

      <div className="mt-5 overflow-x-auto">
        <table className="w-full min-w-[760px] text-left text-sm">
          <thead className="border-b border-slate-200 text-xs uppercase text-slate-500"><tr><th className="px-3 py-3">Canal</th><th className="px-3 py-3">Tipo</th><th className="px-3 py-3">Credenciais</th><th className="px-3 py-3">Operacao</th><th className="px-3 py-3 text-right">Acoes</th></tr></thead>
          <tbody>
            {canais.map((canal) => (
              <tr key={canal.id} className="border-b border-slate-100">
                <td className="px-3 py-4"><p className="font-black text-slate-950">{canal.nome}</p><p className="mt-1 text-xs font-semibold text-slate-500">{canal.slug}</p></td>
                <td className="px-3 py-4 font-bold text-slate-700">{canal.tipo}</td>
                <td className="px-3 py-4"><span className={`inline-flex items-center gap-2 font-black ${canal.credenciaisConfiguradas ? 'text-emerald-700' : 'text-amber-800'}`}>{canal.credenciaisConfiguradas ? <CheckCircle2 size={16} /> : <ShieldAlert size={16} />}{canal.statusCredenciais}</span><p className="mt-1 max-w-md text-xs font-semibold text-slate-500">{canal.detalheCredenciais}</p></td>
                <td className="px-3 py-4 font-black">{canal.ativo ? <span className="text-emerald-700">Ativo</span> : <span className="text-slate-500">Inativo</span>}</td>
                <td className="px-3 py-4"><div className="flex justify-end gap-2"><button type="button" onClick={() => edit(canal)} title="Editar canal" className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-slate-300"><Pencil size={16} /></button><button type="button" onClick={() => remove(canal)} title="Excluir canal" className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-red-200 text-red-700"><Trash2 size={16} /></button></div></td>
              </tr>
            ))}
          </tbody>
        </table>
        {!loading && canais.length === 0 && <p className="py-8 text-center text-sm font-bold text-slate-500">Nenhum canal cadastrado para este tenant.</p>}
      </div>
    </div>
  );
}
