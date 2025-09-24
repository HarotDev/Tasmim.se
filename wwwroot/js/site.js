/* =========================
   NAV: mobilmeny toggle
========================= */
(() => {
    const btn = document.querySelector('.nav-toggle');
    const menu = document.getElementById('mainNav');
    if (!btn || !menu) return;

    btn.addEventListener('click', () => {
        const open = btn.getAttribute('aria-expanded') === 'true';
        btn.setAttribute('aria-expanded', String(!open));
        menu.classList.toggle('is-open', !open);
        document.body.style.overflow = !open ? 'hidden' : '';
    });

    const mq = window.matchMedia('(min-width: 961px)');
    (mq.addEventListener || mq.addListener).call(mq, 'addEventListener' in mq ? 'change' : 'listener', e => {
        if (e.matches) {
            btn.setAttribute('aria-expanded', 'false');
            menu.classList.remove('is-open');
            document.body.style.overflow = '';
        }
    });
})();

/* ===========================================
   REVEAL on scroll (respekt för reduced motion)
=========================================== */
(() => {
    const nodes = document.querySelectorAll('.reveal');
    if (!nodes.length) return;

    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReduced) { nodes.forEach(n => n.classList.add('in')); return; }

    const io = new IntersectionObserver((entries) => {
        entries.forEach(e => {
            if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); }
        });
    }, { threshold: .15, rootMargin: '0px 0px -10% 0px' });

    nodes.forEach(n => io.observe(n));
})();

/* =========================================================
   SERVICE-GRID: swipe-knappar (stöder både allmän & [data-snap])
========================================================= */
(() => {
    document.querySelectorAll('.svc-grid').forEach(grid => {
        const wrap = grid.closest('.svc-carousel') || grid.parentElement;
        const prev = wrap?.querySelector('.snap-prev');
        const next = wrap?.querySelector('.snap-next');
        if (!prev || !next) return;

        const oneCard = grid.querySelector('.svc-card');
        const step = () => {
            const cw = oneCard ? oneCard.getBoundingClientRect().width : 300;
            // flytta ungefär ett kort i taget
            return Math.max(240, Math.round(cw * 1.05));
        };

        prev.addEventListener('click', () => grid.scrollBy({ left: -step(), behavior: 'smooth' }));
        next.addEventListener('click', () => grid.scrollBy({ left: step(), behavior: 'smooth' }));

        // smarta pilar (disable-liknande)
        const updateArrows = () => {
            const atStart = grid.scrollLeft <= 4;
            const atEnd = grid.scrollLeft + grid.clientWidth >= grid.scrollWidth - 4;
            prev.style.opacity = atStart ? .4 : 1;
            next.style.opacity = atEnd ? .4 : 1;
            prev.style.pointerEvents = atStart ? 'none' : '';
            next.style.pointerEvents = atEnd ? 'none' : '';
        };
        grid.addEventListener('scroll', updateArrows, { passive: true });
        window.addEventListener('resize', updateArrows);
        updateArrows();
    });
})();

/* ===================================
   CONTACT-FORM: autosize & validering
=================================== */
(() => {
    const form = document.querySelector('.contact-form');
    if (!form) return;

    const btn = document.getElementById('sendBtn');
    const msg = form.querySelector('textarea');
    const counter = document.getElementById('msg-count');

    // Autosize & counter (om element finns)
    const fit = () => { if (!msg) return; msg.style.height = 'auto'; msg.style.height = (msg.scrollHeight + 2) + 'px'; };
    const count = () => { if (!msg || !counter) return; counter.textContent = (msg.value || '').length; fit(); };
    msg?.addEventListener('input', count);
    count();

    // Client validity styles + disable double submit
    form.addEventListener('submit', e => {
        if (!form.checkValidity()) {
            e.preventDefault();
            [...form.elements].forEach(el => {
                if (el.matches?.('input,select,textarea')) {
                    el.classList.toggle('is-invalid', !el.checkValidity());
                }
            });
            return;
        }
        if (btn) { btn.disabled = true; btn.textContent = 'Skickar…'; }
    });

    form.addEventListener('input', e => {
        const el = e.target;
        if (el?.matches?.('input,select,textarea')) {
            el.classList.toggle('is-invalid', !el.checkValidity() && el.value !== '');
        }
    });
})();

/* ===========================================
   PRIS-TABS (.p-tabs) – robusta null-guards
=========================================== */
(() => {
    const tabsWrap = document.querySelector('.p-tabs');
    const tabs = Array.from(document.querySelectorAll('.p-tabs .p-tab'));
    const panels = {
        hemsida: document.getElementById('panel-hemsida'),
        webbutik: document.getElementById('panel-webbutik')
    };
    if (!tabsWrap || tabs.length === 0 || !panels.hemsida || !panels.webbutik) return;

    function setActive(name, pushHash = true) {
        tabs.forEach(btn => {
            const on = btn.dataset.tab === name;
            btn.classList.toggle('is-active', on);
            btn.setAttribute('aria-selected', on ? 'true' : 'false');
            btn.tabIndex = on ? 0 : -1;
        });
        panels.hemsida.classList.toggle('is-active', name === 'hemsida');
        panels.webbutik.classList.toggle('is-active', name === 'webbutik');
        if (pushHash) history.replaceState?.(null, '', '#' + name);
    }

    const initial = (location.hash || '#hemsida').slice(1);
    setActive(initial, false);
    tabs.forEach(btn => btn.addEventListener('click', () => setActive(btn.dataset.tab)));

    tabsWrap.addEventListener('keydown', (e) => {
        if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(e.key)) return;
        e.preventDefault();
        const i = tabs.findIndex(t => t.classList.contains('is-active'));
        let ni = i;
        if (e.key === 'ArrowRight') ni = (i + 1) % tabs.length;
        if (e.key === 'ArrowLeft') ni = (i - 1 + tabs.length) % tabs.length;
        if (e.key === 'Home') ni = 0;
        if (e.key === 'End') ni = tabs.length - 1;
        const t = tabs[ni];
        setActive(t.dataset.tab);
        t.focus();
    });
})();

/* =====================================================
   MVC-valideringsklass vid initial render (serverfel)
===================================================== */
document.addEventListener('DOMContentLoaded', () => {
    const sel = '.contact-form .input.input-validation-error, .contact-form .select.input-validation-error, .contact-form .textarea.input-validation-error';
    document.querySelectorAll(sel).forEach(el => el.classList.add('is-invalid'));
});

/* ===================================================
   Loading-state på submit-knappar (generellt skydd)
=================================================== */
document.addEventListener('submit', (e) => {
    const btn = e.target?.querySelector('button[type="submit"][data-loading]');
    if (!btn) return;
    btn.dataset.label = btn.textContent.trim();
    btn.disabled = true;
    btn.textContent = btn.getAttribute('data-loading');
});

/* ==========================
   FAQ: accordion + animation
========================== */
(() => {
    const items = document.querySelectorAll('.faq-item');
    if (!items.length) return;

    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    const closeItem = (item) => {
        const btn = item.querySelector('.faq-q');
        const panel = item.querySelector('.faq-a');
        if (!btn || !panel) return;

        if (prefersReduced) {
            btn.setAttribute('aria-expanded', 'false');
            panel.style.height = '0px';
            return;
        }

        // från auto -> explicit -> 0 för mjuk transition
        panel.style.height = panel.scrollHeight + 'px';
        requestAnimationFrame(() => {
            btn.setAttribute('aria-expanded', 'false');
            panel.style.height = '0px';
        });
    };

    const openItem = (item) => {
        const btn = item.querySelector('.faq-q');
        const panel = item.querySelector('.faq-a');
        if (!btn || !panel) return;

        btn.setAttribute('aria-expanded', 'true');

        if (prefersReduced) {
            panel.style.height = 'auto';
            return;
        }

        panel.style.height = '0px';
        requestAnimationFrame(() => {
            panel.style.height = panel.scrollHeight + 'px';
        });

        const tidy = (e) => {
            if (e.propertyName === 'height' && btn.getAttribute('aria-expanded') === 'true') {
                panel.style.height = 'auto';
                panel.removeEventListener('transitionend', tidy);
            }
        };
        panel.addEventListener('transitionend', tidy);
    };

    // init & bind
    items.forEach((item) => {
        const btn = item.querySelector('.faq-q');
        const panel = item.querySelector('.faq-a');
        if (!btn || !panel) return;

        // startläge: stängd
        btn.setAttribute('aria-expanded', 'false');
        panel.style.height = '0px';
        panel.style.overflow = 'hidden';

        btn.addEventListener('click', () => {
            const expanded = btn.getAttribute('aria-expanded') === 'true';
            // stäng andra (accordion)
            items.forEach((other) => { if (other !== item) closeItem(other); });
            expanded ? closeItem(item) : openItem(item);
        });
    });

    // ESC stänger öppet item
    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        const openBtn = document.querySelector('.faq-q[aria-expanded="true"]');
        if (!openBtn) return;
        closeItem(openBtn.closest('.faq-item'));
        openBtn.focus();
    });
})();
