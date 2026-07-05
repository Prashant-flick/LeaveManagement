import { useState, useEffect, useRef, useCallback, memo, useId } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeRaw from 'rehype-raw';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';
import mermaid from 'mermaid';
import './App.css';

// ─── S3 Content Bucket URL ──────────────────────────────────────────────────
const S3_URL = 'https://prashant-learning-notes-content.s3.ap-south-1.amazonaws.com';

// ─── Category Icons (pixel-art emoji mapping) ──────────────────────────────
const CATEGORY_ICONS = {
  devops: '🛠️',
  backend: '⚙️',
  frontend: '🎨',
  database: '🗄️',
  cloud: '☁️',
  security: '🔒',
  networking: '🌐',
  linux: '🐧',
  programming: '💻',
  architecture: '🏗️',
  testing: '🧪',
  ai: '🤖',
  default: '📝',
};

function getCategoryIcon(category) {
  if (!category) return CATEGORY_ICONS.default;
  const key = category.toLowerCase().trim();
  return CATEGORY_ICONS[key] || CATEGORY_ICONS.default;
}

// ─── Mermaid Init ───────────────────────────────────────────────────────────
mermaid.initialize({
  startOnLoad: false,
  theme: 'base',
  themeVariables: {
    background: '#c6c6c6',
    primaryColor: '#8b8b8b',
    primaryTextColor: '#000000',
    primaryBorderColor: '#3c3c3c',
    lineColor: '#000000',
    secondaryColor: '#55ff55',
    tertiaryColor: '#aaaaaa',
    fontFamily: '"Press Start 2P", monospace',
    fontSize: '12px',
  },
});

// ═══════════════════════════════════════════════════════════════════════════
// MermaidDiagram — renders mermaid code blocks as SVGs
// ═══════════════════════════════════════════════════════════════════════════
const MermaidDiagram = memo(function MermaidDiagram({ chart }) {
  const containerRef = useRef(null);
  const uniqueId = useId();
  const [svg, setSvg] = useState('');
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    async function render() {
      try {
        // mermaid requires a valid DOM id (no colons from useId)
        const safeId = `mermaid-${uniqueId.replace(/:/g, '')}`;
        const { svg: renderedSvg } = await mermaid.render(safeId, chart);
        if (!cancelled) {
          setSvg(renderedSvg);
          setError(null);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message || 'Failed to render diagram');
          setSvg('');
        }
      }
    }
    render();
    return () => { cancelled = true; };
  }, [chart, uniqueId]);

  if (error) {
    return (
      <div className="mermaid-error">
        <span className="mermaid-error-label">⚠ DIAGRAM ERROR</span>
        <pre>{error}</pre>
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className="mermaid-container"
      dangerouslySetInnerHTML={{ __html: svg }}
    />
  );
});

// ═══════════════════════════════════════════════════════════════════════════
// TagBadge — styled retro tag
// ═══════════════════════════════════════════════════════════════════════════
function TagBadge({ tag, active, onClick }) {
  return (
    <button
      type="button"
      className={`tag-badge ${active ? 'tag-badge--active' : ''}`}
      onClick={() => onClick?.(tag)}
      title={`Filter by: ${tag}`}
    >
      #{tag}
    </button>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// NoteCard — individual note entry in the sidebar
// ═══════════════════════════════════════════════════════════════════════════
function NoteCard({ note, isActive, onClick }) {
  return (
    <button
      type="button"
      className={`note-card ${isActive ? 'note-card--active' : ''}`}
      onClick={() => onClick(note)}
    >
      <div className="note-card__icon">{getCategoryIcon(note.category)}</div>
      <div className="note-card__info">
        <span className="note-card__title">{note.title}</span>
        <span className="note-card__date">{note.date || 'Unknown date'}</span>
        {note.tags && note.tags.length > 0 && (
          <div className="note-card__tags">
            {note.tags.slice(0, 3).map((t) => (
              <span key={t} className="note-card__tag">#{t}</span>
            ))}
            {note.tags.length > 3 && (
              <span className="note-card__tag note-card__tag--more">+{note.tags.length - 3}</span>
            )}
          </div>
        )}
      </div>
      <div className="note-card__arrow">▶</div>
    </button>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// WelcomeScreen — shown when no note is selected
// ═══════════════════════════════════════════════════════════════════════════
function WelcomeScreen({ noteCount }) {
  return (
    <div className="welcome">
      <div className="welcome__art">
        <pre className="welcome__ascii">{`
  ╔══════════════════════════════╗
  ║                              ║
  ║   ██╗     ███╗   ██╗        ║
  ║   ██║     ████╗  ██║        ║
  ║   ██║     ██╔██╗ ██║        ║
  ║   ██║     ██║╚██╗██║        ║
  ║   ███████╗██║ ╚████║        ║
  ║   ╚══════╝╚═╝  ╚═══╝        ║
  ║                              ║
  ║   L E A R N I N G           ║
  ║       N O T E S             ║
  ║                              ║
  ╚══════════════════════════════╝
        `}</pre>
      </div>
      <h2 className="welcome__title">SELECT A NOTE TO BEGIN</h2>
      <p className="welcome__subtitle">
        {noteCount > 0
          ? `${noteCount} notes available in your knowledge base`
          : 'Loading notes from the archives...'}
      </p>
      <div className="welcome__controls">
        <span className="welcome__key">◀ ▶</span> Navigate
        <span className="welcome__separator">│</span>
        <span className="welcome__key">ENTER</span> Select
        <span className="welcome__separator">│</span>
        <span className="welcome__key">ESC</span> Close
      </div>
      <div className="welcome__scanline" />
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// PixelLoader — retro loading animation
// ═══════════════════════════════════════════════════════════════════════════
function PixelLoader({ text = 'LOADING' }) {
  return (
    <div className="pixel-loader">
      <div className="pixel-loader__dots">
        <span className="pixel-loader__dot" />
        <span className="pixel-loader__dot" />
        <span className="pixel-loader__dot" />
      </div>
      <span className="pixel-loader__text">
        {text}<span className="pixel-loader__cursor">█</span>
      </span>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// NoteViewer — renders markdown content
// ═══════════════════════════════════════════════════════════════════════════
function NoteViewer({ note, content, loading, error }) {
  if (loading) {
    return (
      <div className="note-viewer note-viewer--loading">
        <PixelLoader text="LOADING NOTE" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="note-viewer note-viewer--error">
        <div className="error-box">
          <span className="error-box__icon">✖</span>
          <h3>SYSTEM ERROR</h3>
          <p>{error}</p>
          <span className="error-box__code">ERR_FETCH_FAILED</span>
        </div>
      </div>
    );
  }

  if (!content) return null;

  return (
    <div className="note-viewer">
      <header className="note-viewer__header">
        <span className="note-viewer__category-icon">{getCategoryIcon(note?.category)}</span>
        <div>
          <h1 className="note-viewer__title">{note?.title || 'Untitled'}</h1>
          <div className="note-viewer__meta">
            {note?.date && <span className="note-viewer__date">📅 {note.date}</span>}
            {note?.category && <span className="note-viewer__cat">📂 {note.category}</span>}
          </div>
        </div>
      </header>

      {note?.tags && note.tags.length > 0 && (
        <div className="note-viewer__tags">
          {note.tags.map((t) => (
            <span key={t} className="tag-badge tag-badge--inline">#{t}</span>
          ))}
        </div>
      )}

      <article className="note-viewer__content markdown-body">
        <ReactMarkdown
          children={content}
          remarkPlugins={[remarkGfm]}
          rehypePlugins={[rehypeRaw]}
          components={{
            code({ node, inline, className, children, ...props }) {
              const match = /language-(\w+)/.exec(className || '');
              const lang = match ? match[1] : '';
              const codeString = String(children).replace(/\n$/, '');

              // Mermaid diagrams
              if (lang === 'mermaid') {
                return <MermaidDiagram chart={codeString} />;
              }

              // Inline code
              if (inline) {
                return (
                  <code className="inline-code" {...props}>
                    {children}
                  </code>
                );
              }

              // Fenced code block with syntax highlighting
              if (lang) {
                return (
                  <div className="code-block-wrapper">
                    <div className="code-block-header">
                      <span className="code-block-lang">{lang.toUpperCase()}</span>
                      <button
                        type="button"
                        className="code-block-copy"
                        onClick={() => {
                          navigator.clipboard?.writeText(codeString);
                        }}
                        title="Copy code"
                      >
                        📋 COPY
                      </button>
                    </div>
                    <SyntaxHighlighter
                      style={oneDark}
                      language={lang}
                      PreTag="div"
                      customStyle={{
                        margin: 0,
                        borderRadius: '0 0 8px 8px',
                        fontSize: '13px',
                        border: '1px solid #2d2d4e',
                        borderTop: 'none',
                      }}
                      {...props}
                    >
                      {codeString}
                    </SyntaxHighlighter>
                  </div>
                );
              }

              // Fallback: plain code block without language
              return (
                <div className="code-block-wrapper">
                  <SyntaxHighlighter
                    style={oneDark}
                    PreTag="div"
                    customStyle={{
                      margin: 0,
                      borderRadius: '8px',
                      fontSize: '13px',
                      border: '1px solid #2d2d4e',
                    }}
                    {...props}
                  >
                    {codeString}
                  </SyntaxHighlighter>
                </div>
              );
            },

            // Styled tables
            table({ children }) {
              return (
                <div className="table-wrapper">
                  <table>{children}</table>
                </div>
              );
            },

            // External links open in new tab
            a({ href, children, ...props }) {
              const isExternal = href && (href.startsWith('http') || href.startsWith('//'));
              return (
                <a
                  href={href}
                  {...(isExternal ? { target: '_blank', rel: 'noopener noreferrer' } : {})}
                  {...props}
                >
                  {children}
                </a>
              );
            },

            // Styled blockquotes
            blockquote({ children }) {
              return <blockquote className="retro-blockquote">{children}</blockquote>;
            },
          }}
        />
      </article>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// Sidebar — note list with search & tag filter
// ═══════════════════════════════════════════════════════════════════════════
function Sidebar({
  notes,
  activeNote,
  onNoteSelect,
  allTags,
  activeTag,
  onTagSelect,
  searchQuery,
  onSearchChange,
  isOpen,
  onClose,
}) {
  return (
    <>
      {/* Mobile overlay */}
      {isOpen && <div className="sidebar-overlay" onClick={onClose} />}

      <aside className={`sidebar ${isOpen ? 'sidebar--open' : ''}`}>
        {/* Header */}
        <div className="sidebar__header">
          <h1 className="sidebar__logo">
            <span className="sidebar__logo-icon">🕹️</span>
            <span>NOTES</span>
          </h1>
          <button type="button" className="sidebar__close" onClick={onClose} aria-label="Close sidebar">
            ✕
          </button>
        </div>

        {/* Search */}
        <div className="sidebar__search">
          <span className="sidebar__search-icon">🔍</span>
          <input
            type="text"
            placeholder="Search notes..."
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            className="sidebar__search-input"
          />
          {searchQuery && (
            <button
              type="button"
              className="sidebar__search-clear"
              onClick={() => onSearchChange('')}
              aria-label="Clear search"
            >
              ✕
            </button>
          )}
        </div>

        {/* Tag filters */}
        {allTags.length > 0 && (
          <div className="sidebar__tags">
            <TagBadge
              tag="ALL"
              active={!activeTag}
              onClick={() => onTagSelect(null)}
            />
            {allTags.map((tag) => (
              <TagBadge
                key={tag}
                tag={tag}
                active={activeTag === tag}
                onClick={() => onTagSelect(activeTag === tag ? null : tag)}
              />
            ))}
          </div>
        )}

        {/* Note list */}
        <div className="sidebar__list">
          {notes.length === 0 ? (
            <div className="sidebar__empty">
              <span className="sidebar__empty-icon">🔎</span>
              <p>NO NOTES FOUND</p>
            </div>
          ) : (
            notes.map((note) => (
              <NoteCard
                key={note.filename}
                note={note}
                isActive={activeNote?.filename === note.filename}
                onClick={(n) => {
                  onNoteSelect(n);
                  onClose();
                }}
              />
            ))
          )}
        </div>

        {/* Footer */}
        <div className="sidebar__footer">
          <span>{notes.length} note{notes.length !== 1 ? 's' : ''}</span>
        </div>
      </aside>
    </>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// App — main layout
// ═══════════════════════════════════════════════════════════════════════════
function App() {
  const [notes, setNotes] = useState([]);
  const [activeNote, setActiveNote] = useState(null);
  const [noteContent, setNoteContent] = useState('');
  const [loadingIndex, setLoadingIndex] = useState(true);
  const [loadingNote, setLoadingNote] = useState(false);
  const [indexError, setIndexError] = useState(null);
  const [noteError, setNoteError] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTag, setActiveTag] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // ── Fetch notes index on mount ─────────────────────────────────────────
  useEffect(() => {
    async function fetchIndex() {
      try {
        setLoadingIndex(true);
        setIndexError(null);
        const res = await fetch(`${S3_URL}/notes-index.json`);
        if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
        const data = await res.json();
        // Support both array and { notes: [...] } formats
        const notesList = Array.isArray(data) ? data : data.notes || [];
        setNotes(notesList);
      } catch (err) {
        setIndexError(err.message || 'Failed to load notes index');
      } finally {
        setLoadingIndex(false);
      }
    }
    fetchIndex();
  }, []);

  // ── Fetch note content when active note changes ────────────────────────
  const fetchNoteContent = useCallback(async (note) => {
    if (!note?.filename) return;
    try {
      setLoadingNote(true);
      setNoteError(null);
      setNoteContent('');
      const res = await fetch(`${S3_URL}/notes/${note.filename}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
      const text = await res.text();
      setNoteContent(text);
    } catch (err) {
      setNoteError(err.message || 'Failed to load note content');
    } finally {
      setLoadingNote(false);
    }
  }, []);

  function handleNoteSelect(note) {
    setActiveNote(note);
    fetchNoteContent(note);
  }

  // ── Derive filtered notes ──────────────────────────────────────────────
  const filteredNotes = notes.filter((note) => {
    const matchesSearch =
      !searchQuery ||
      note.title?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      note.category?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      note.tags?.some((t) => t.toLowerCase().includes(searchQuery.toLowerCase()));

    const matchesTag = !activeTag || note.tags?.includes(activeTag);

    return matchesSearch && matchesTag;
  });

  // ── Derive all unique tags ─────────────────────────────────────────────
  const allTags = [...new Set(notes.flatMap((n) => n.tags || []))].sort();

  // ── Keyboard: close sidebar on Escape ──────────────────────────────────
  useEffect(() => {
    function handleKeyDown(e) {
      if (e.key === 'Escape' && sidebarOpen) {
        setSidebarOpen(false);
      }
    }
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [sidebarOpen]);

  // ── Render ─────────────────────────────────────────────────────────────
  return (
    <div className="app">
      {/* Mobile hamburger */}
      <button
        type="button"
        className="hamburger"
        onClick={() => setSidebarOpen(true)}
        aria-label="Open sidebar"
      >
        <span className="hamburger__line" />
        <span className="hamburger__line" />
        <span className="hamburger__line" />
      </button>

      <Sidebar
        notes={filteredNotes}
        activeNote={activeNote}
        onNoteSelect={handleNoteSelect}
        allTags={allTags}
        activeTag={activeTag}
        onTagSelect={setActiveTag}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />

      <main className="main-content">
        {loadingIndex ? (
          <div className="main-content__center">
            <PixelLoader text="LOADING INDEX" />
          </div>
        ) : indexError ? (
          <div className="main-content__center">
            <div className="error-box">
              <span className="error-box__icon">✖</span>
              <h3>CONNECTION FAILED</h3>
              <p>{indexError}</p>
              <button
                type="button"
                className="error-box__retry"
                onClick={() => window.location.reload()}
              >
                ↻ RETRY
              </button>
            </div>
          </div>
        ) : !activeNote ? (
          <WelcomeScreen noteCount={notes.length} />
        ) : (
          <NoteViewer
            note={activeNote}
            content={noteContent}
            loading={loadingNote}
            error={noteError}
          />
        )}
      </main>
    </div>
  );
}

export default App;
