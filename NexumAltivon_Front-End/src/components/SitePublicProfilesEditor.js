/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { Building2, Image, LoaderCircle, Plus, RefreshCw, Save, Store, Trash2 } from 'lucide-react';
import { resolvePublicAssetUrl, siteAPI } from '../services/api';

const profileTypes = [
  { value: 'Loja', label: 'Loja do grupo' },
  { value: 'Fornecedor', label: 'Fornecedor' },
  { value: 'ParceiroVenda', label: 'Parceiro de venda' },
  { value: 'ParceiroCompra', label: 'Parceiro de compra' },
  { value: 'Marketplace', label: 'Marketplace' },
];

const sourceTypesByProfile = {
  Loja: ['Loja'],
  Fornecedor: ['Fornecedor'],
  Marketplace: ['Marketplace'],
  ParceiroVenda: ['Parceiro', 'Fornecedor', 'Marketplace'],
  ParceiroCompra: ['Parceiro', 'Fornecedor', 'Marketplace'],
};

const emptyProfile = {
  id: '',
  tipoPerfil: 'Loja',
  origemTipo: 'Loja',
  origemId: '',
  nome: '',
  slug: '',
  segmento: '',
  atividade: '',
  descricao: '',
  logoUrl: '',
  bannerUrl: '',
  icone: 'Store',
  ctaTexto: 'Conhecer',
  ctaUrl: '',
  siteUrl: '',
  emailPublico: '',
  telefonePublico: '',
  enderecoPublico: '',
  corPrimaria: '#C9A227',
  corSecundaria: '#0A0A0A',
  corFundo: '#050505',
  corTexto: '#FFFFFF',
  produtoIds: [],
  publicado: false,
  ordemExibicao: 0,
  rowVersion: '',
};

const getErrorMessage = (error, fallback) =>
  error?.response?.data?.detail
  || error?.response?.data?.mensagem
  || error?.response?.data?.message
  || error?.message
  || fallback;

const normalizeSlug = (value) => String(value || '')
  .normalize('NFD')
  .replace(/[\u0300-\u036f]/g, '')
  .toLowerCase()
  .replace(/[^a-z0-9]+/g, '-')
  .replace(/^-+|-+$/g, '');

const mapProfileToForm = (profile) => ({
  ...emptyProfile,
  ...profile,
  origemId: profile?.origemId ? String(profile.origemId) : '',
  logoUrl: profile?.logoUrl || '',
  bannerUrl: profile?.bannerUrl || '',
  icone: profile?.icone || 'Building2',
  ctaTexto: profile?.ctaTexto || '',
  ctaUrl: profile?.ctaUrl || '',
  siteUrl: profile?.siteUrl || '',
  emailPublico: profile?.emailPublico || '',
  telefonePublico: profile?.telefonePublico || '',
  enderecoPublico: profile?.enderecoPublico || '',
  corPrimaria: profile?.corPrimaria || '#C9A227',
  corSecundaria: profile?.corSecundaria || '#0A0A0A',
  corFundo: profile?.corFundo || '#050505',
  corTexto: profile?.corTexto || '#FFFFFF',
  produtoIds: Array.isArray(profile?.produtoIds) ? profile.produtoIds : [],
});

function FormField({ label, value, onChange, required = false, type = 'text', maxLength, children }) {
  return (
    <label className="grid gap-2 text-xs font-black uppercase tracking-[0.12em] text-slate-500">
      {label}
      {children || (
        <input
          type={type}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          required={required}
          maxLength={maxLength}
          className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
        />
      )}
    </label>
  );
}

function ColorField({ label, value, onChange }) {
  return (
    <label className="grid gap-2 text-xs font-black uppercase tracking-[0.12em] text-slate-500">
      {label}
      <span className="flex h-11 items-center gap-3 rounded-lg border border-slate-200 bg-white px-3">
        <input type="color" value={/^#[0-9A-F]{6}$/i.test(value) ? value : '#000000'} onChange={(event) => onChange(event.target.value.toUpperCase())} className="h-7 w-9 cursor-pointer border-0 bg-transparent p-0" />
        <input type="text" value={value} onChange={(event) => onChange(event.target.value.toUpperCase())} pattern="#[0-9A-Fa-f]{6}" maxLength={7} className="min-w-0 flex-1 bg-transparent text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none" />
      </span>
    </label>
  );
}

export default function SitePublicProfilesEditor() {
  const [profiles, setProfiles] = useState([]);
  const [origins, setOrigins] = useState([]);
  const [media, setMedia] = useState([]);
  const [products, setProducts] = useState([]);
  const [productSearch, setProductSearch] = useState('');
  const [form, setForm] = useState(emptyProfile);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [status, setStatus] = useState({ tone: '', message: '' });

  const loadData = useCallback(async () => {
    setLoading(true);
    setStatus({ tone: '', message: '' });
    try {
      const [profilesResponse, originsResponse, mediaResponse, productsResponse] = await Promise.all([
        siteAPI.listarPerfisPublicos(),
        siteAPI.listarOrigensPerfisPublicos(),
        siteAPI.listarMidias(),
        siteAPI.listarProdutosDisponiveisPerfisPublicos(),
      ]);
      if (!Array.isArray(profilesResponse.data) || !Array.isArray(originsResponse.data) || !Array.isArray(mediaResponse.data) || !Array.isArray(productsResponse.data)) {
        throw new Error('A API retornou dados inválidos para os perfis públicos.');
      }
      setProfiles(profilesResponse.data);
      setOrigins(originsResponse.data);
      setMedia(mediaResponse.data);
      setProducts(productsResponse.data);
      setForm((current) => {
        if (!current.id) return current;
        const refreshed = profilesResponse.data.find((item) => item.id === current.id);
        return refreshed ? mapProfileToForm(refreshed) : emptyProfile;
      });
    } catch (error) {
      setStatus({ tone: 'error', message: getErrorMessage(error, 'Não foi possível carregar os perfis públicos.') });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const allowedSourceTypes = sourceTypesByProfile[form.tipoPerfil] || ['Parceiro'];
  const availableOrigins = useMemo(
    () => origins.filter((origin) => origin.tipo === form.origemTipo),
    [form.origemTipo, origins],
  );
  const availableMedia = useMemo(() => {
    const items = [...media];
    [form.logoUrl, form.bannerUrl].filter(Boolean).forEach((path) => {
      if (!items.some((item) => item.caminhoRelativo === path)) {
        items.push({ id: path, caminhoRelativo: path, nome: `Arquivo configurado: ${path}` });
      }
    });
    return items;
  }, [form.bannerUrl, form.logoUrl, media]);
  const eligibleProducts = useMemo(() => {
    const originId = Number(form.origemId || 0);
    const search = productSearch.trim().toLocaleLowerCase('pt-BR');
    return products.filter((product) => {
      if (form.origemTipo === 'Loja' && product.lojaId !== originId) return false;
      if (form.origemTipo === 'Fornecedor' && product.fornecedorId !== originId) return false;
      if (!search) return true;
      return `${product.sku} ${product.nome}`.toLocaleLowerCase('pt-BR').includes(search);
    });
  }, [form.origemId, form.origemTipo, productSearch, products]);

  const setField = (field, value) => setForm((current) => ({ ...current, [field]: value }));

  const changeProfileType = (value) => {
    const sourceType = sourceTypesByProfile[value]?.[0] || 'Parceiro';
    setForm((current) => ({
      ...current,
      tipoPerfil: value,
      origemTipo: sourceType,
      origemId: '',
      produtoIds: [],
      icone: value === 'Loja' ? 'Store' : value === 'Fornecedor' ? 'Truck' : 'Building2',
    }));
  };

  const changeOrigin = (value) => {
    const origin = origins.find((item) => item.tipo === form.origemTipo && String(item.id) === value);
    setForm((current) => ({
      ...current,
      origemId: value,
      produtoIds: [],
      nome: current.nome || origin?.nome || '',
      slug: current.slug || origin?.slug || '',
    }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setStatus({ tone: '', message: '' });

    const payload = {
      tipoPerfil: form.tipoPerfil,
      origemTipo: form.origemTipo,
      origemId: form.origemTipo === 'Parceiro' ? null : Number(form.origemId || 0),
      nome: form.nome.trim(),
      slug: normalizeSlug(form.slug),
      segmento: form.segmento.trim(),
      atividade: form.atividade.trim(),
      descricao: form.descricao.trim(),
      logoUrl: form.logoUrl.trim() || null,
      bannerUrl: form.bannerUrl.trim() || null,
      icone: form.icone.trim() || null,
      ctaTexto: form.ctaTexto.trim() || null,
      ctaUrl: form.ctaUrl.trim() || null,
      siteUrl: form.siteUrl.trim() || null,
      emailPublico: form.emailPublico.trim() || null,
      telefonePublico: form.telefonePublico.trim() || null,
      enderecoPublico: form.enderecoPublico.trim() || null,
      corPrimaria: form.corPrimaria.trim() || null,
      corSecundaria: form.corSecundaria.trim() || null,
      corFundo: form.corFundo.trim() || null,
      corTexto: form.corTexto.trim() || null,
      produtoIds: form.produtoIds,
      publicado: Boolean(form.publicado),
      ordemExibicao: Number(form.ordemExibicao || 0),
      rowVersion: form.rowVersion || null,
    };

    if (!payload.logoUrl && !payload.bannerUrl) {
      setStatus({ tone: 'error', message: 'Selecione ao menos uma logomarca ou um banner oficial.' });
      return;
    }
    if (payload.origemTipo !== 'Parceiro' && payload.origemId <= 0) {
      setStatus({ tone: 'error', message: 'Selecione o cadastro de origem deste perfil.' });
      return;
    }

    setSaving(true);
    try {
      const response = form.id
        ? await siteAPI.atualizarPerfilPublico(form.id, payload)
        : await siteAPI.criarPerfilPublico(payload);
      if (!response.data?.id || !response.data?.rowVersion) {
        throw new Error('A API não confirmou a gravação do perfil público.');
      }
      setProfiles((current) => {
        const next = current.filter((item) => item.id !== response.data.id).concat(response.data);
        return next.sort((a, b) => a.tipoPerfil.localeCompare(b.tipoPerfil) || a.ordemExibicao - b.ordemExibicao || a.nome.localeCompare(b.nome));
      });
      setForm(mapProfileToForm(response.data));
      setStatus({ tone: 'success', message: `${response.data.nome} foi gravado e relido do banco oficial.` });
    } catch (error) {
      setStatus({ tone: 'error', message: getErrorMessage(error, 'Não foi possível gravar o perfil público.') });
    } finally {
      setSaving(false);
    }
  };

  const remove = async () => {
    if (!form.id || !form.rowVersion || !window.confirm(`Remover o perfil público ${form.nome}?`)) return;
    setDeleting(true);
    setStatus({ tone: '', message: '' });
    try {
      await siteAPI.excluirPerfilPublico(form.id, form.rowVersion);
      setProfiles((current) => current.filter((item) => item.id !== form.id));
      setForm(emptyProfile);
      setStatus({ tone: 'success', message: 'Perfil público removido e exclusão confirmada no banco oficial.' });
    } catch (error) {
      setStatus({ tone: 'error', message: getErrorMessage(error, 'Não foi possível remover o perfil público.') });
    } finally {
      setDeleting(false);
    }
  };

  return (
    <section className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-col gap-4 border-b border-slate-100 px-6 py-5 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Divulgação comercial</p>
          <h3 className="mt-2 text-lg font-black text-slate-950">Lojas, fornecedores, parceiros e marketplaces</h3>
        </div>
        <div className="flex gap-2">
          <button type="button" onClick={() => setForm(emptyProfile)} className="inline-flex h-10 items-center gap-2 rounded-lg border border-slate-200 px-3 text-sm font-black text-slate-700">
            <Plus size={16} /> Novo
          </button>
          <button type="button" onClick={loadData} disabled={loading} className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200 text-slate-700" title="Recarregar perfis">
            <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          </button>
        </div>
      </div>

      <div className="grid lg:grid-cols-[280px_minmax(0,1fr)]">
        <div className="max-h-[900px] overflow-y-auto border-b border-slate-100 bg-slate-50 p-3 lg:border-b-0 lg:border-r">
          {loading ? (
            <div className="flex h-32 items-center justify-center text-slate-500"><LoaderCircle className="animate-spin" size={22} /></div>
          ) : profiles.length === 0 ? (
            <p className="p-4 text-sm font-semibold text-slate-500">Nenhum perfil cadastrado.</p>
          ) : profiles.map((profile) => (
            <button
              type="button"
              key={profile.id}
              onClick={() => setForm(mapProfileToForm(profile))}
              className={`mb-2 w-full rounded-lg border p-3 text-left transition ${form.id === profile.id ? 'border-slate-950 bg-slate-950 text-white' : 'border-slate-200 bg-white text-slate-800 hover:border-slate-400'}`}
            >
              <div className="flex items-start gap-3">
                {profile.tipoPerfil === 'Loja' ? <Store size={18} /> : <Building2 size={18} />}
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-black">{profile.nome}</p>
                  <p className={`mt-1 truncate text-xs font-semibold ${form.id === profile.id ? 'text-slate-300' : 'text-slate-500'}`}>{profile.tipoPerfil} · {profile.publicado ? 'Publicado' : 'Oculto'}</p>
                </div>
              </div>
            </button>
          ))}
        </div>

        <form onSubmit={submit} className="space-y-5 p-6">
          <div className="grid gap-4 md:grid-cols-2">
            <FormField label="Tipo do perfil">
              <select value={form.tipoPerfil} onChange={(event) => changeProfileType(event.target.value)} className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none">
                {profileTypes.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
              </select>
            </FormField>
            <FormField label="Origem do cadastro">
              <select value={form.origemTipo} onChange={(event) => setForm((current) => ({ ...current, origemTipo: event.target.value, origemId: '', produtoIds: [] }))} className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none">
                {allowedSourceTypes.map((item) => <option key={item} value={item}>{item}</option>)}
              </select>
            </FormField>
          </div>

          {form.origemTipo !== 'Parceiro' && (
            <FormField label="Registro de origem">
              <select value={form.origemId} onChange={(event) => changeOrigin(event.target.value)} required className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none">
                <option value="">Selecione</option>
                {availableOrigins.map((origin) => <option key={`${origin.tipo}-${origin.id}`} value={origin.id}>{origin.nome} · {origin.status}</option>)}
              </select>
            </FormField>
          )}

          <div className="grid gap-4 md:grid-cols-2">
            <FormField label="Nome público" value={form.nome} onChange={(value) => setField('nome', value)} required maxLength={160} />
            <FormField label="Slug" value={form.slug} onChange={(value) => setField('slug', normalizeSlug(value))} required maxLength={80} />
            <FormField label="Segmento" value={form.segmento} onChange={(value) => setField('segmento', value)} required maxLength={120} />
            <FormField label="Atividade em destaque" value={form.atividade} onChange={(value) => setField('atividade', value)} required maxLength={240} />
          </div>

          <FormField label="Descrição comercial">
            <textarea value={form.descricao} onChange={(event) => setField('descricao', event.target.value)} required maxLength={2000} className="min-h-32 rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold normal-case leading-6 tracking-normal text-slate-950 outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10" />
          </FormField>

          <div className="grid gap-4 md:grid-cols-2">
            <FormField label="Logomarca">
              <select value={form.logoUrl} onChange={(event) => setField('logoUrl', event.target.value)} className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none">
                <option value="">Sem logomarca</option>
                {availableMedia.map((item) => <option key={`logo-${item.id}`} value={item.caminhoRelativo}>{item.nome}</option>)}
              </select>
            </FormField>
            <FormField label="Banner">
              <select value={form.bannerUrl} onChange={(event) => setField('bannerUrl', event.target.value)} className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold normal-case tracking-normal text-slate-950 outline-none">
                <option value="">Sem banner</option>
                {availableMedia.map((item) => <option key={`banner-${item.id}`} value={item.caminhoRelativo}>{item.nome}</option>)}
              </select>
            </FormField>
          </div>

          {(form.bannerUrl || form.logoUrl) && (
            <div className="relative h-52 overflow-hidden rounded-lg border border-slate-200 bg-slate-950">
              <img src={resolvePublicAssetUrl(form.bannerUrl || form.logoUrl)} alt={form.nome || 'Prévia do perfil'} className="h-full w-full object-cover" />
              {form.logoUrl && <img src={resolvePublicAssetUrl(form.logoUrl)} alt="" className="absolute bottom-4 left-4 h-20 w-20 rounded-lg border border-white/30 bg-black/70 object-contain p-2" />}
              <span className="absolute right-3 top-3 inline-flex items-center gap-2 rounded-full bg-black/75 px-3 py-2 text-xs font-black text-white"><Image size={14} /> Prévia</span>
            </div>
          )}

          <div className="grid gap-4 md:grid-cols-2">
            <FormField label="Texto da ação" value={form.ctaTexto} onChange={(value) => setField('ctaTexto', value)} maxLength={80} />
            <FormField label="URL da ação" value={form.ctaUrl} onChange={(value) => setField('ctaUrl', value)} maxLength={500} />
            <FormField label="Ícone" value={form.icone} onChange={(value) => setField('icone', value)} maxLength={80} />
            <FormField label="Ordem" value={form.ordemExibicao} onChange={(value) => setField('ordemExibicao', value)} type="number" />
          </div>

          <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
            <h4 className="text-sm font-black text-slate-950">Canais públicos autorizados</h4>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <FormField label="Site HTTPS" value={form.siteUrl} onChange={(value) => setField('siteUrl', value)} maxLength={500} />
              <FormField label="E-mail público" value={form.emailPublico} onChange={(value) => setField('emailPublico', value)} type="email" maxLength={254} />
              <FormField label="Telefone público" value={form.telefonePublico} onChange={(value) => setField('telefonePublico', value)} maxLength={30} />
              <FormField label="Endereço público" value={form.enderecoPublico} onChange={(value) => setField('enderecoPublico', value)} maxLength={300} />
            </div>
          </section>

          <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
            <h4 className="text-sm font-black text-slate-950">Identidade visual do perfil</h4>
            <div className="mt-4 grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              <ColorField label="Principal" value={form.corPrimaria} onChange={(value) => setField('corPrimaria', value)} />
              <ColorField label="Secundária" value={form.corSecundaria} onChange={(value) => setField('corSecundaria', value)} />
              <ColorField label="Fundo" value={form.corFundo} onChange={(value) => setField('corFundo', value)} />
              <ColorField label="Texto" value={form.corTexto} onChange={(value) => setField('corTexto', value)} />
            </div>
          </section>

          <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h4 className="text-sm font-black text-slate-950">Produtos autorizados neste perfil</h4>
                <p className="mt-1 text-xs font-semibold text-slate-500">{form.produtoIds.length} produto(s) selecionado(s) de até 200.</p>
              </div>
              <input value={productSearch} onChange={(event) => setProductSearch(event.target.value)} placeholder="Buscar SKU ou produto" className="h-10 rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold text-slate-950 outline-none" />
            </div>
            {form.origemTipo !== 'Parceiro' && !form.origemId ? (
              <p className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm font-bold text-amber-800">Selecione a origem para listar os produtos permitidos.</p>
            ) : eligibleProducts.length === 0 ? (
              <p className="mt-4 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-500">Nenhum produto publicável corresponde a esta origem e busca.</p>
            ) : (
              <div className="mt-4 max-h-64 space-y-2 overflow-y-auto pr-1">
                {eligibleProducts.map((product) => {
                  const checked = form.produtoIds.includes(product.id);
                  return (
                    <label key={product.id} className="flex cursor-pointer items-center gap-3 rounded-lg border border-slate-200 bg-white p-3">
                      <input
                        type="checkbox"
                        checked={checked}
                        disabled={!checked && form.produtoIds.length >= 200}
                        onChange={(event) => setField('produtoIds', event.target.checked
                          ? [...form.produtoIds, product.id]
                          : form.produtoIds.filter((id) => id !== product.id))}
                        className="h-4 w-4 rounded border-slate-300 text-slate-950"
                      />
                      <img src={resolvePublicAssetUrl(product.imagemUrl)} alt="" className="h-10 w-10 rounded object-cover" />
                      <span className="min-w-0 flex-1">
                        <span className="block truncate text-sm font-black text-slate-900">{product.nome}</span>
                        <span className="block truncate text-xs font-semibold text-slate-500">{product.sku} · {Number(product.preco).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</span>
                      </span>
                    </label>
                  );
                })}
              </div>
            )}
          </section>

          <label className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm font-black text-slate-800">
            <input type="checkbox" checked={form.publicado} onChange={(event) => setField('publicado', event.target.checked)} className="h-4 w-4 rounded border-slate-300 text-slate-950" />
            Publicado no portal
          </label>

          {status.message && (
            <div className={`rounded-lg border px-4 py-3 text-sm font-bold ${status.tone === 'success' ? 'border-emerald-200 bg-emerald-50 text-emerald-800' : 'border-red-200 bg-red-50 text-red-800'}`} role={status.tone === 'error' ? 'alert' : 'status'}>
              {status.message}
            </div>
          )}

          <div className="flex flex-wrap gap-3">
            <button type="submit" disabled={saving || deleting} className="inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-60">
              {saving ? <LoaderCircle size={17} className="animate-spin" /> : <Save size={17} />}
              {saving ? 'Salvando...' : form.id ? 'Salvar alterações' : 'Criar perfil'}
            </button>
            {form.id && (
              <button type="button" onClick={remove} disabled={saving || deleting} className="inline-flex h-11 items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-5 text-sm font-black text-red-700 disabled:opacity-60">
                {deleting ? <LoaderCircle size={17} className="animate-spin" /> : <Trash2 size={17} />}
                Remover
              </button>
            )}
          </div>
        </form>
      </div>
    </section>
  );
}
