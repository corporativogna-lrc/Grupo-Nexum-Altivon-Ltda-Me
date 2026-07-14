/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Image, ImagePlus, LoaderCircle, MonitorUp, RefreshCw, Save, Trash2 } from 'lucide-react';
import { siteAPI } from '../services/api';

const MAX_IMAGE_BYTES = 8 * 1024 * 1024;
const ACCEPTED_IMAGE_TYPES = new Set(['image/png', 'image/jpeg', 'image/webp']);
const mediaTypes = ['Logo', 'Banner', 'Loja', 'Institucional'];

const initialUploadForm = {
  nome: '',
  tipo: 'Banner',
  textoAlternativo: '',
  file: null,
};

const getErrorMessage = (error, defaultMessage) =>
  error?.response?.data?.mensagem
  || error?.response?.data?.message
  || error?.response?.data?.detail
  || error?.message
  || defaultMessage;

const fileToDataUrl = (file) => new Promise((resolve, reject) => {
  const reader = new FileReader();
  reader.onload = () => resolve(String(reader.result || ''));
  reader.onerror = () => reject(new Error('O navegador não conseguiu ler o arquivo selecionado.'));
  reader.readAsDataURL(file);
});

const formatBytes = (bytes) => {
  const value = Number(bytes || 0);
  if (value < 1024) return `${value} B`;
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`;
  return `${(value / (1024 * 1024)).toFixed(2)} MB`;
};

const parseHeroSlides = (value) => {
  try {
    const parsed = JSON.parse(value || '[]');
    if (!Array.isArray(parsed) || parsed.length === 0) {
      return { slides: [], error: 'Cadastre ao menos um slide no JSON antes de vincular um banner.' };
    }

    return { slides: parsed, error: '' };
  } catch {
    return { slides: [], error: 'O JSON dos slides é inválido. Corrija-o antes de vincular um banner.' };
  }
};

function SiteMediaLibrary({ logoUrl, heroSlidesJson, onLogoChange, onHeroSlidesChange }) {
  const [midias, setMidias] = useState([]);
  const [uploadForm, setUploadForm] = useState(initialUploadForm);
  const [selectedSlideIndex, setSelectedSlideIndex] = useState(0);
  const [editingId, setEditingId] = useState('');
  const [editForm, setEditForm] = useState({ nome: '', tipo: 'Banner', textoAlternativo: '', rowVersion: '' });
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [deletingId, setDeletingId] = useState('');
  const [status, setStatus] = useState('');
  const [error, setError] = useState('');
  const fileInputRef = useRef(null);
  const heroSlidesState = useMemo(() => parseHeroSlides(heroSlidesJson), [heroSlidesJson]);

  const loadMedia = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await siteAPI.listarMidias();
      if (!Array.isArray(response.data)) {
        throw new Error('A API retornou uma biblioteca de mídia em formato inválido.');
      }
      setMidias(response.data);
    } catch (loadError) {
      setError(getErrorMessage(loadError, 'Não foi possível carregar a biblioteca de mídia.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadMedia();
  }, [loadMedia]);

  useEffect(() => {
    if (selectedSlideIndex >= heroSlidesState.slides.length) {
      setSelectedSlideIndex(0);
    }
  }, [heroSlidesState.slides.length, selectedSlideIndex]);

  const submitUpload = async (event) => {
    event.preventDefault();
    setStatus('');
    setError('');

    const nome = uploadForm.nome.trim();
    const textoAlternativo = uploadForm.textoAlternativo.trim();
    const file = uploadForm.file;
    if (nome.length < 3 || nome.length > 150) {
      setError('Informe um nome entre 3 e 150 caracteres.');
      return;
    }
    if (textoAlternativo.length < 3 || textoAlternativo.length > 240) {
      setError('Informe um texto alternativo entre 3 e 240 caracteres.');
      return;
    }
    if (!file) {
      setError('Selecione uma imagem PNG, JPEG ou WebP.');
      return;
    }
    if (!ACCEPTED_IMAGE_TYPES.has(file.type)) {
      setError('Formato não permitido. Use PNG, JPEG ou WebP.');
      return;
    }
    if (file.size <= 0 || file.size > MAX_IMAGE_BYTES) {
      setError('A imagem deve possuir conteúdo e ter no máximo 8 MB.');
      return;
    }

    setSubmitting(true);
    try {
      const dataUrl = await fileToDataUrl(file);
      const response = await siteAPI.uploadMidia({
        nome,
        tipo: uploadForm.tipo,
        textoAlternativo,
        fileName: file.name,
        contentType: file.type,
        dataUrl,
      });
      if (!response.data?.id || !response.data?.url || !response.data?.rowVersion) {
        throw new Error('A API não confirmou a persistência da mídia.');
      }

      setMidias((current) => [response.data, ...current.filter((item) => item.id !== response.data.id)]);
      setUploadForm(initialUploadForm);
      if (fileInputRef.current) fileInputRef.current.value = '';
      setStatus(`Mídia persistida: ${response.data.nome} (${response.data.largura} x ${response.data.altura}).`);
    } catch (uploadError) {
      setError(getErrorMessage(uploadError, 'Não foi possível persistir a mídia.'));
    } finally {
      setSubmitting(false);
    }
  };

  const startEditing = (midia) => {
    setEditingId(midia.id);
    setEditForm({
      nome: midia.nome,
      tipo: midia.tipo,
      textoAlternativo: midia.textoAlternativo,
      rowVersion: midia.rowVersion,
    });
    setStatus('');
    setError('');
  };

  const saveMetadata = async (event) => {
    event.preventDefault();
    setStatus('');
    setError('');
    setSubmitting(true);
    try {
      const response = await siteAPI.atualizarMidia(editingId, editForm);
      if (!response.data?.id || !response.data?.rowVersion) {
        throw new Error('A API não confirmou a atualização dos metadados.');
      }
      setMidias((current) => current.map((item) => (item.id === response.data.id ? response.data : item)));
      setEditingId('');
      setStatus(`Metadados atualizados: ${response.data.nome}.`);
    } catch (updateError) {
      setError(getErrorMessage(updateError, 'Não foi possível atualizar os metadados.'));
    } finally {
      setSubmitting(false);
    }
  };

  const deleteMedia = async (midia) => {
    setStatus('');
    setError('');
    setDeletingId(midia.id);
    try {
      await siteAPI.excluirMidia(midia.id, midia.rowVersion);
      setMidias((current) => current.filter((item) => item.id !== midia.id));
      setStatus(`Mídia removida da biblioteca: ${midia.nome}.`);
    } catch (deleteError) {
      setError(getErrorMessage(deleteError, 'Não foi possível remover a mídia.'));
    } finally {
      setDeletingId('');
    }
  };

  const assignLogo = (midia) => {
    onLogoChange(midia.url);
    setError('');
    setStatus(`Logo selecionado: ${midia.nome}. Salve a configuração pública para efetivar a alteração.`);
  };

  const assignBanner = (midia) => {
    if (heroSlidesState.error) {
      setError(heroSlidesState.error);
      return;
    }
    const nextSlides = heroSlidesState.slides.map((slide, index) => (
      index === selectedSlideIndex
        ? { ...slide, image: midia.url, imageAlt: midia.textoAlternativo }
        : slide
    ));
    onHeroSlidesChange(JSON.stringify(nextSlides));
    setError('');
    setStatus(`Banner selecionado para o slide ${selectedSlideIndex + 1}. Salve a configuração pública para efetivar a alteração.`);
  };

  return (
    <section className="border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-4 border-b border-slate-200 px-5 py-4">
        <div>
          <p className="text-xs font-black uppercase text-slate-500">Arquivos públicos</p>
          <h3 className="mt-1 text-base font-black text-slate-950">Biblioteca de mídia</h3>
          <p className="mt-1 text-sm text-slate-500">Imagens validadas, persistidas no MySQL e servidas pela API oficial.</p>
        </div>
        <button
          type="button"
          onClick={loadMedia}
          disabled={loading}
          title="Recarregar biblioteca"
          className="inline-flex h-10 w-10 items-center justify-center border border-slate-200 text-slate-700 transition hover:border-slate-400 disabled:opacity-50"
        >
          <RefreshCw size={17} className={loading ? 'animate-spin' : ''} />
        </button>
      </div>

      <form onSubmit={submitUpload} className="grid gap-3 border-b border-slate-200 bg-slate-50 p-5 md:grid-cols-2">
        <label className="text-sm font-bold text-slate-700">
          Nome
          <input
            value={uploadForm.nome}
            onChange={(event) => setUploadForm((current) => ({ ...current, nome: event.target.value }))}
            maxLength={150}
            className="mt-1 h-10 w-full border border-slate-300 bg-white px-3 outline-none focus:border-[#C9A227]"
          />
        </label>
        <label className="text-sm font-bold text-slate-700">
          Uso
          <select
            value={uploadForm.tipo}
            onChange={(event) => setUploadForm((current) => ({ ...current, tipo: event.target.value }))}
            className="mt-1 h-10 w-full border border-slate-300 bg-white px-3 outline-none focus:border-[#C9A227]"
          >
            {mediaTypes.map((type) => <option key={type} value={type}>{type}</option>)}
          </select>
        </label>
        <label className="text-sm font-bold text-slate-700 md:col-span-2">
          Texto alternativo
          <input
            value={uploadForm.textoAlternativo}
            onChange={(event) => setUploadForm((current) => ({ ...current, textoAlternativo: event.target.value }))}
            maxLength={240}
            className="mt-1 h-10 w-full border border-slate-300 bg-white px-3 outline-none focus:border-[#C9A227]"
          />
        </label>
        <label className="text-sm font-bold text-slate-700 md:col-span-2">
          Arquivo PNG, JPEG ou WebP
          <input
            ref={fileInputRef}
            type="file"
            accept="image/png,image/jpeg,image/webp"
            onChange={(event) => setUploadForm((current) => ({ ...current, file: event.target.files?.[0] || null }))}
            className="mt-1 block w-full border border-slate-300 bg-white p-2 text-sm file:mr-3 file:border-0 file:bg-slate-950 file:px-3 file:py-2 file:font-bold file:text-white"
          />
        </label>
        <button
          type="submit"
          disabled={submitting}
          className="inline-flex h-10 items-center justify-center gap-2 bg-slate-950 px-4 text-sm font-black text-white disabled:opacity-50 md:col-span-2"
        >
          {submitting ? <LoaderCircle size={17} className="animate-spin" /> : <ImagePlus size={17} />}
          Persistir mídia
        </button>
      </form>

      <div className="space-y-3 p-5">
        {status && <p className="border border-emerald-300 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-800">{status}</p>}
        {error && <p className="border border-red-300 bg-red-50 px-4 py-3 text-sm font-bold text-red-800">{error}</p>}
        {heroSlidesState.error ? (
          <p className="border border-amber-300 bg-amber-50 px-4 py-3 text-sm font-bold text-amber-900">{heroSlidesState.error}</p>
        ) : (
          <label className="block text-sm font-bold text-slate-700">
            Slide que receberá o banner
            <select
              value={selectedSlideIndex}
              onChange={(event) => setSelectedSlideIndex(Number(event.target.value))}
              className="mt-1 h-10 w-full border border-slate-300 bg-white px-3 outline-none focus:border-[#C9A227]"
            >
              {heroSlidesState.slides.map((slide, index) => (
                <option key={slide.id || index} value={index}>{index + 1}. {slide.title || slide.id || 'Slide'}</option>
              ))}
            </select>
          </label>
        )}

        {loading && (
          <div className="flex min-h-32 items-center justify-center gap-2 text-sm font-bold text-slate-500">
            <LoaderCircle size={18} className="animate-spin" /> Carregando registros da API
          </div>
        )}

        {!loading && midias.length === 0 && (
          <div className="flex min-h-32 flex-col items-center justify-center border border-dashed border-slate-300 text-center text-slate-500">
            <Image size={24} />
            <p className="mt-2 text-sm font-bold">Nenhuma mídia persistida para este tenant.</p>
          </div>
        )}

        {!loading && midias.map((midia) => (
          <article key={midia.id} className="border border-slate-200">
            <div className="grid gap-4 p-4 sm:grid-cols-[120px_minmax(0,1fr)]">
              <div className="flex aspect-square items-center justify-center overflow-hidden bg-slate-100">
                <img src={midia.url} alt={midia.textoAlternativo} className="h-full w-full object-contain" />
              </div>
              <div className="min-w-0">
                {editingId === midia.id ? (
                  <form onSubmit={saveMetadata} className="grid gap-2">
                    <input
                      value={editForm.nome}
                      onChange={(event) => setEditForm((current) => ({ ...current, nome: event.target.value }))}
                      maxLength={150}
                      className="h-9 border border-slate-300 px-3 text-sm font-bold outline-none focus:border-[#C9A227]"
                    />
                    <select
                      value={editForm.tipo}
                      onChange={(event) => setEditForm((current) => ({ ...current, tipo: event.target.value }))}
                      className="h-9 border border-slate-300 px-3 text-sm outline-none focus:border-[#C9A227]"
                    >
                      {mediaTypes.map((type) => <option key={type} value={type}>{type}</option>)}
                    </select>
                    <input
                      value={editForm.textoAlternativo}
                      onChange={(event) => setEditForm((current) => ({ ...current, textoAlternativo: event.target.value }))}
                      maxLength={240}
                      className="h-9 border border-slate-300 px-3 text-sm outline-none focus:border-[#C9A227]"
                    />
                    <div className="flex gap-2">
                      <button type="submit" disabled={submitting} className="inline-flex h-9 items-center gap-2 bg-slate-950 px-3 text-xs font-black text-white disabled:opacity-50">
                        <Save size={15} /> Salvar metadados
                      </button>
                      <button type="button" onClick={() => setEditingId('')} className="h-9 border border-slate-300 px-3 text-xs font-black text-slate-700">Cancelar</button>
                    </div>
                  </form>
                ) : (
                  <>
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div className="min-w-0">
                        <h4 className="truncate text-sm font-black text-slate-950">{midia.nome}</h4>
                        <p className="mt-1 text-xs font-semibold text-slate-500">{midia.tipo} · {midia.largura} x {midia.altura} · {formatBytes(midia.tamanhoBytes)}</p>
                        <p className="mt-1 line-clamp-2 text-xs text-slate-500">{midia.textoAlternativo}</p>
                      </div>
                      <button type="button" onClick={() => startEditing(midia)} className="h-8 border border-slate-300 px-2 text-xs font-black text-slate-700">Editar</button>
                    </div>
                    <div className="mt-4 grid gap-2 sm:grid-cols-3">
                      <button type="button" onClick={() => assignLogo(midia)} className={`inline-flex h-9 items-center justify-center gap-2 border px-2 text-xs font-black ${logoUrl === midia.url ? 'border-[#C9A227] bg-amber-50 text-amber-900' : 'border-slate-300 text-slate-700'}`}>
                        <Image size={15} /> Usar como logo
                      </button>
                      <button type="button" onClick={() => assignBanner(midia)} disabled={Boolean(heroSlidesState.error)} className="inline-flex h-9 items-center justify-center gap-2 border border-slate-300 px-2 text-xs font-black text-slate-700 disabled:opacity-40">
                        <MonitorUp size={15} /> Usar no banner
                      </button>
                      <button type="button" onClick={() => deleteMedia(midia)} disabled={deletingId === midia.id} className="inline-flex h-9 items-center justify-center gap-2 border border-red-300 px-2 text-xs font-black text-red-700 disabled:opacity-40">
                        {deletingId === midia.id ? <LoaderCircle size={15} className="animate-spin" /> : <Trash2 size={15} />} Remover
                      </button>
                    </div>
                  </>
                )}
              </div>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}

export default SiteMediaLibrary;
