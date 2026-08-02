import { useState, useEffect, useCallback, useId, useRef, memo } from 'react';
import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeRaw from 'rehype-raw';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';
import mermaid from 'mermaid';
import Zoom from 'react-medium-image-zoom';
import 'react-medium-image-zoom/dist/styles.css';
import './App.css';

const S3_URL = 'https://prashant-learning-notes-content.s3.ap-south-1.amazonaws.com';

const CATEGORY_ICONS = {
  devops: '🛠️', backend: '⚙️', frontend: '🎨', cloud: '☁️',
  security: '🔒', architecture: '🏗️', default: '📝'
};

function getCategoryIcon(category) {
  const key = category?.toLowerCase().trim();
  return CATEGORY_ICONS[key] || CATEGORY_ICONS.default;
}

mermaid.initialize({
  startOnLoad: false,
  theme: 'base',
  themeVariables: {
    background: '#c6c6c6', primaryColor: '#8b8b8b', primaryTextColor: '#000',
    primaryBorderColor: '#3c3c3c', lineColor: '#000', secondaryColor: '#55ff55',
    tertiaryColor: '#aaaaaa', fontFamily: '"Press Start 2P", monospace', fontSize: '12px'
  }
});

const MermaidDiagram = memo(function MermaidDiagram({ chart }) {
  const containerRef = useRef(null);
  const uniqueId = useId();
  const [svg, setSvg] = useState('');
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    async function render() {
      try {
        const safeId = `mermaid-${uniqueId.replace(/:/g, '')}`;
        const { svg: renderedSvg } = await mermaid.render(safeId, chart);
        if (!cancelled) { setSvg(renderedSvg); setError(null); }
      } catch (err) {
        if (!cancelled) { setError(err.message); setSvg(''); }
      }
    }
    render();
    return () => { cancelled = true; };
  }, [chart, uniqueId]);

  if (error) return <div className="mermaid-error"><pre>{error}</pre></div>;
  return <Zoom><div ref={containerRef} className="mermaid-container" dangerouslySetInnerHTML={{ __html: svg }} /></Zoom>;
});

// ═══════════════════════════════════════════════════════════════════════════
// NOTE MODAL (The Reader)
// ═══════════════════════════════════════════════════════════════════════════
function NoteModal({ note, content, loading, onClose }) {
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = 'auto'; };
  }, []);

  return (
    <motion.div 
      className="modal-overlay" 
      onClick={onClose}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
    >
      <motion.div 
        className="modal-content mc-panel" 
        onClick={(e) => e.stopPropagation()}
        initial={{ y: 50, scale: 0.95, opacity: 0 }}
        animate={{ y: 0, scale: 1, opacity: 1 }}
        exit={{ y: 50, scale: 0.95, opacity: 0 }}
        transition={{ type: "spring", damping: 25, stiffness: 300 }}
      >
        <button className="modal-close" onClick={onClose}>✕</button>
        
        {loading ? (
          <div className="modal-loading">
            <motion.div 
              className="car-spinner"
              animate={{ x: [-20, 20], y: [-5, 5] }}
              transition={{ repeat: Infinity, duration: 0.5, repeatType: "mirror" }}
            >
              🏎️💨
            </motion.div>
            <p>DRIVING TO NOTE...</p>
          </div>
        ) : (
          <div className="note-viewer">
            <header className="note-viewer__header">
              <span className="note-viewer__category-icon">{getCategoryIcon(note.category)}</span>
              <div>
                <h1 className="note-viewer__title">{note.title}</h1>
                <div className="note-viewer__meta">
                  <span className="note-viewer__date">📅 {note.date}</span>
                </div>
              </div>
            </header>
            <article className="note-viewer__content markdown-body">
              <ReactMarkdown
                children={content}
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeRaw]}
                components={{
                  code({ inline, className, children, ...props }) {
                    const match = /language-(\w+)/.exec(className || '');
                    const lang = match ? match[1] : '';
                    const codeString = String(children).replace(/\n$/, '');
                    if (lang === 'mermaid') return <MermaidDiagram chart={codeString} />;
                    if (inline) return <code className="inline-code" {...props}>{children}</code>;
                    if (lang) {
                      return (
                        <div className="code-block-wrapper">
                          <div className="code-block-header">
                            <span className="code-block-lang">{lang.toUpperCase()}</span>
                          </div>
                          <SyntaxHighlighter style={oneDark} language={lang} PreTag="div" customStyle={{ margin:0, borderRadius:'0 0 8px 8px', fontSize:'13px', border:'2px solid #000', borderTop:'none' }} {...props}>{codeString}</SyntaxHighlighter>
                        </div>
                      );
                    }
                    return <div className="code-block-wrapper"><SyntaxHighlighter style={oneDark} PreTag="div" customStyle={{ margin:0, borderRadius:'8px', fontSize:'13px', border:'2px solid #000' }} {...props}>{codeString}</SyntaxHighlighter></div>;
                  },
                  img({ src, alt, ...props }) {
                    return <Zoom><img src={src} alt={alt} {...props} style={{ cursor: 'zoom-in' }} /></Zoom>;
                  },
                  table({ children }) { return <div className="table-wrapper"><table>{children}</table></div>; },
                  blockquote({ children }) { return <blockquote className="retro-blockquote">{children}</blockquote>; },
                  a({ href, children, ...props }) {
                    const isExt = href?.startsWith('http');
                    return <a href={href} {...(isExt ? { target:'_blank', rel:'noopener noreferrer'} : {})} {...props}>{children}</a>;
                  }
                }}
              />
            </article>
          </div>
        )}
      </motion.div>
    </motion.div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// RACE TRACK NODE
// ═══════════════════════════════════════════════════════════════════════════
function TrackNode({ note, index, isActive, onClick }) {
  const isLeft = index % 2 === 0;
  
  return (
    <motion.div 
      className={`track-node ${isLeft ? 'node-left' : 'node-right'}`}
      initial={{ opacity: 0, x: isLeft ? -100 : 100 }}
      whileInView={{ opacity: 1, x: 0 }}
      viewport={{ once: true, margin: "-100px" }}
      transition={{ type: "spring", stiffness: 100, damping: 20 }}
    >
      <motion.div 
        className={`node-content mc-panel ${isActive ? 'active' : ''}`} 
        onClick={onClick}
        whileHover={{ scale: 1.05, y: -5 }}
        whileTap={{ scale: 0.95 }}
      >
        <div className="node-icon">{getCategoryIcon(note.category)}</div>
        <div className="node-details">
          <h3>{note.title}</h3>
          <p>{note.date}</p>
        </div>
        {isActive && <div className="node-flag">🏁</div>}
      </motion.div>
      <div className="node-connector"></div>
    </motion.div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// WORLD MAP (The Race Track)
// ═══════════════════════════════════════════════════════════════════════════
const NODE_HEIGHT = 200;

function WorldMap({ notes }) {
  const [activeNote, setActiveNote] = useState(null);
  const [noteContent, setNoteContent] = useState('');
  const [loadingNote, setLoadingNote] = useState(false);
  const [carPosition, setCarPosition] = useState(0);

  const handleNoteClick = useCallback(async (note, index) => {
    setActiveNote(note);
    
    const targetY = index * NODE_HEIGHT;
    setCarPosition(targetY);
    
    window.scrollTo({
      top: targetY - window.innerHeight / 3 + 100, 
      behavior: 'smooth'
    });

    try {
      setLoadingNote(true);
      const res = await fetch(`${S3_URL}/notes/${note.filename}`);
      const text = await res.text();
      
      // Artificial delay so user can watch the spring physics of the car arriving
      setTimeout(() => {
        setNoteContent(text);
        setLoadingNote(false);
      }, 700);
    } catch (err) {
      setNoteContent('# Error\nFailed to load note.');
      setLoadingNote(false);
    }
  }, []);

  return (
    <>
      <motion.header 
        className="world-header"
        initial={{ y: -50, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ duration: 0.5, delay: 0.2 }}
      >
        <h1>RACING MINDS</h1>
        <p>A Knowledge Base Adventure</p>
      </motion.header>

      <div className="track-container" style={{ height: `${(notes.length + 1) * NODE_HEIGHT}px` }}>
        {/* The Road */}
        <div className="the-road">
          {/* The Player Character (Race Car) */}
          <motion.div 
            className="player-character"
            animate={{ y: carPosition }}
            transition={{ type: "spring", stiffness: 60, damping: 15 }}
          >
            🏎️
          </motion.div>
        </div>

        <div className="track-nodes">
          {notes.map((note, idx) => {
            const isLeft = idx % 2 === 0;
            return (
              <div key={note.filename} className={`track-node-wrapper ${isLeft ? 'wrapper-left' : 'wrapper-right'}`} style={{ top: `${idx * NODE_HEIGHT}px` }}>
                <TrackNode 
                  note={note} 
                  index={idx} 
                  isActive={activeNote?.filename === note.filename}
                  onClick={() => handleNoteClick(note, idx)}
                />
              </div>
            );
          })}
        </div>
        
        <div className="finish-line" style={{ top: `${notes.length * NODE_HEIGHT}px` }}>
          🏁 END OF THE ROAD 🏁
        </div>
      </div>

      <AnimatePresence>
        {activeNote && (
          <NoteModal 
            key="modal"
            note={activeNote}
            content={noteContent}
            loading={loadingNote}
            onClose={() => setActiveNote(null)}
          />
        )}
      </AnimatePresence>
    </>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// STATS PAGE (The New Page)
// ═══════════════════════════════════════════════════════════════════════════
function StatsPage({ notes }) {
  const tags = notes.flatMap(n => n.tags || []);
  const uniqueTags = [...new Set(tags)];
  
  const categories = notes.map(n => n.category);
  const catCounts = categories.reduce((acc, cat) => {
    acc[cat] = (acc[cat] || 0) + 1;
    return acc;
  }, {});

  const containerVariants = {
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: { staggerChildren: 0.1 }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    show: { opacity: 1, y: 0, transition: { type: "spring" } }
  };

  return (
    <motion.div 
      className="stats-page"
      initial={{ opacity: 0, x: 50 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -50 }}
    >
      <h1>SYSTEM METRICS</h1>
      
      <motion.div className="stats-grid" variants={containerVariants} initial="hidden" animate="show">
        <motion.div className="stat-card mc-panel" variants={itemVariants}>
          <h2>TOTAL NOTES</h2>
          <div className="stat-number">{notes.length}</div>
        </motion.div>
        <motion.div className="stat-card mc-panel" variants={itemVariants}>
          <h2>UNIQUE TAGS</h2>
          <div className="stat-number">{uniqueTags.length}</div>
        </motion.div>
      </motion.div>
      
      <h2 style={{ marginTop: '60px' }}>CATEGORIES</h2>
      <motion.div className="category-list" variants={containerVariants} initial="hidden" animate="show">
        {Object.entries(catCounts).map(([cat, count]) => (
          <motion.div key={cat} className="category-item mc-panel" variants={itemVariants}>
            <span>{getCategoryIcon(cat)} {cat.toUpperCase()}</span>
            <span className="category-count">{count}</span>
          </motion.div>
        ))}
      </motion.div>
    </motion.div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// JOB HUNT PAGE (Modern Design)
// ═══════════════════════════════════════════════════════════════════════════
function JobHuntPage() {
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`${S3_URL}/notes/career-jobs.md`)
      .then(res => res.text())
      .then(text => {
        setContent(text);
        setLoading(false);
      })
      .catch(err => {
        setContent('# Error\nFailed to load job hunt data.');
        setLoading(false);
      });
  }, []);

  return (
    <motion.div 
      className="job-hunt-page"
      style={{
        padding: '40px',
        maxWidth: '900px',
        margin: '40px auto',
        background: 'rgba(15, 23, 42, 0.7)',
        backdropFilter: 'blur(12px)',
        borderRadius: '24px',
        border: '1px solid rgba(255, 255, 255, 0.08)',
        color: '#e2e8f0',
        fontFamily: '"Inter", "Roboto", sans-serif',
        boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.5)'
      }}
      initial={{ opacity: 0, y: 30 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -30 }}
      transition={{ duration: 0.5, ease: 'easeOut' }}
    >
      <div style={{ textAlign: 'center', marginBottom: '40px' }}>
        <h1 style={{ fontSize: '3rem', fontWeight: '800', background: 'linear-gradient(135deg, #38bdf8, #818cf8, #c084fc)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', margin: 0, letterSpacing: '-0.05em' }}>
          Job Radar
        </h1>
        <p style={{ color: '#94a3b8', marginTop: '12px', fontSize: '1.15rem' }}>Your automated, daily curated feed for top software engineering roles.</p>
      </div>
      
      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '60px' }}>
          <motion.div 
            animate={{ rotate: 360 }} 
            transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
            style={{ width: '40px', height: '40px', border: '3px solid rgba(255,255,255,0.1)', borderTopColor: '#38bdf8', borderRadius: '50%' }}
          />
        </div>
      ) : (
        <div style={{ lineHeight: '1.8', fontSize: '16px' }}>
          <ReactMarkdown
            children={content}
            remarkPlugins={[remarkGfm]}
            rehypePlugins={[rehypeRaw]}
            components={{
              h1: ({node, ...props}) => null, // Hide the H1 from markdown since we have a custom header
              h2: ({node, ...props}) => <h2 style={{ borderBottom: '1px solid rgba(255,255,255,0.1)', paddingBottom: '12px', marginTop: '50px', color: '#f8fafc', fontWeight: '700', fontSize: '1.8rem', letterSpacing: '-0.02em' }} {...props} />,
              a: ({node, ...props}) => <a style={{ color: '#38bdf8', textDecoration: 'none', fontWeight: '600', transition: 'color 0.2s' }} onMouseOver={(e) => e.target.style.color = '#818cf8'} onMouseOut={(e) => e.target.style.color = '#38bdf8'} target="_blank" rel="noopener noreferrer" {...props} />,
              li: ({node, ...props}) => (
                <motion.li 
                  whileHover={{ scale: 1.01, backgroundColor: 'rgba(255,255,255,0.05)' }}
                  style={{ marginBottom: '16px', background: 'rgba(0,0,0,0.3)', padding: '20px', borderRadius: '12px', listStyle: 'none', border: '1px solid rgba(255,255,255,0.03)', display: 'flex', alignItems: 'center' }} 
                  {...props} 
                />
              ),
              ul: ({node, ...props}) => <ul style={{ padding: 0 }} {...props} />,
              ol: ({node, ...props}) => <ol style={{ padding: 0 }} {...props} />,
              blockquote: ({node, ...props}) => <blockquote style={{ borderLeft: '4px solid #818cf8', margin: '30px 0', padding: '20px 25px', background: 'rgba(129, 140, 248, 0.1)', borderRadius: '0 12px 12px 0', fontStyle: 'italic', color: '#cbd5e1' }} {...props} />,
              code: ({inline, node, ...props}) => inline ? <code style={{ background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8', padding: '2px 6px', borderRadius: '4px', fontSize: '0.9em', fontWeight: '600' }} {...props} /> : <code {...props} />
            }}
          />
        </div>
      )}
    </motion.div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// NAVIGATION BAR
// ═══════════════════════════════════════════════════════════════════════════
function NavBar() {
  const location = useLocation();
  
  return (
    <nav className="nav-bar mc-panel">
      <div className="nav-logo">MAP_VIEW</div>
      <div className="nav-links">
        <Link to="/" className={location.pathname === '/' ? 'active' : ''}>WORLD MAP</Link>
        <Link to="/jobs" className={location.pathname === '/jobs' ? 'active' : ''} style={location.pathname === '/jobs' ? { color: '#38bdf8', textShadow: '0 0 10px rgba(56,189,248,0.5)' } : {}}>JOB HUNT</Link>
        <Link to="/stats" className={location.pathname === '/stats' ? 'active' : ''}>METRICS</Link>
      </div>
    </nav>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ROOT COMPONENT
// ═══════════════════════════════════════════════════════════════════════════
function App() {
  const [notes, setNotes] = useState([]);

  useEffect(() => {
    fetch(`${S3_URL}/notes-index.json`)
      .then(r => r.json())
      .then(data => setNotes(Array.isArray(data) ? data : data.notes || []))
      .catch(console.error);
  }, []);

  return (
    <Router>
      <div className="game-world">
        <NavBar />
        <AnimatePresence mode="wait">
          <Routes>
            <Route path="/" element={<WorldMap notes={notes.filter(n => n.id !== 'career-jobs')} />} />
            <Route path="/jobs" element={<JobHuntPage />} />
            <Route path="/stats" element={<StatsPage notes={notes.filter(n => n.id !== 'career-jobs')} />} />
          </Routes>
        </AnimatePresence>
      </div>
    </Router>
  );
}

export default App;
