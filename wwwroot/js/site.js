
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return decodeURIComponent(parts.pop().split(';').shift());
}

function whenConsentReady(callback) {
    const consentCookieName = 'CookieConsent';
    const consentCookie = getCookie(consentCookieName);

    if (consentCookie) {
        try {
            const preferences = JSON.parse(consentCookie);
            callback(preferences);
        } catch (e) {
            console.error('Fel vid tolkning av samtyckes-cookie:', e);
        }
    }
}

(() => {
    'use strict';
    const cookieBanner = document.getElementById('cookie-banner');
    const cookiePrefsPanel = document.getElementById('cookie-prefs');
    const openPrefsBtn = document.getElementById('cookie-open-prefs');
    const closePrefsBtn = document.getElementById('cookie-close-prefs');
    const consentCookieName = 'CookieConsent';

    if (cookieBanner && !getCookie(consentCookieName)) {
        cookieBanner.hidden = false;
    }

    if (openPrefsBtn && cookiePrefsPanel) {
        openPrefsBtn.addEventListener('click', () => {
            cookiePrefsPanel.hidden = false;
        });
    }

    if (closePrefsBtn && cookiePrefsPanel) {
        closePrefsBtn.addEventListener('click', () => {
            cookiePrefsPanel.hidden = true;
        });
    }
})();

(() => {
    const nodes = document.querySelectorAll('.reveal');
    if (!nodes.length) return;

    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReduced) {
        nodes.forEach(n => n.classList.add('in'));
        return;
    }

    const io = new IntersectionObserver((entries) => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                e.target.classList.add('in');
                io.unobserve(e.target);
            }
        });
    }, { threshold: .15, rootMargin: '0px 0px -10% 0px' });

    nodes.forEach(n => io.observe(n));
})();

(() => {
    document.querySelectorAll('.svc-grid').forEach(grid => {
        const wrap = grid.closest('.svc-carousel') || grid.parentElement;
        const prev = wrap?.querySelector('.snap-prev');
        const next = wrap?.querySelector('.snap-next');
        if (!prev || !next) return;

        const oneCard = grid.querySelector('.svc-card');
        const step = () => {
            const cw = oneCard ? oneCard.getBoundingClientRect().width : 300;
            return Math.max(240, Math.round(cw * 1.05));
        };

        prev.addEventListener('click', () => grid.scrollBy({ left: -step(), behavior: 'smooth' }));
        next.addEventListener('click', () => grid.scrollBy({ left: step(), behavior: 'smooth' }));

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

(() => {
    const form = document.querySelector('.contact-form');
    if (!form) return;

    const btn = document.getElementById('sendBtn');
    const msg = form.querySelector('textarea');
    const counter = document.getElementById('msg-count');

    const fit = () => { if (!msg) return; msg.style.height = 'auto'; msg.style.height = (msg.scrollHeight + 2) + 'px'; };
    const count = () => { if (!msg || !counter) return; counter.textContent = (msg.value || '').length; fit(); };
    msg?.addEventListener('input', count);
    count();

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

document.addEventListener('DOMContentLoaded', () => {
    const sel = '.contact-form .input.input-validation-error, .contact-form .select.input-validation-error, .contact-form .textarea.input-validation-error';
    document.querySelectorAll(sel).forEach(el => el.classList.add('is-invalid'));
});

document.addEventListener('submit', (e) => {
    const btn = e.target?.querySelector('button[type="submit"][data-loading]');
    if (!btn) return;
    btn.dataset.label = btn.textContent.trim();
    btn.disabled = true;
    btn.textContent = btn.getAttribute('data-loading');
});

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

    items.forEach((item) => {
        const btn = item.querySelector('.faq-q');
        const panel = item.querySelector('.faq-a');
        if (!btn || !panel) return;

        btn.setAttribute('aria-expanded', 'false');
        panel.style.height = '0px';
        panel.style.overflow = 'hidden';

        btn.addEventListener('click', () => {
            const expanded = btn.getAttribute('aria-expanded') === 'true';
            items.forEach((other) => { if (other !== item) closeItem(other); });
            expanded ? closeItem(item) : openItem(item);
        });
    });

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        const openBtn = document.querySelector('.faq-q[aria-expanded="true"]');
        if (!openBtn) return;
        closeItem(openBtn.closest('.faq-item'));
        openBtn.focus();
    });
})();

(() => {
    const btn = document.querySelector('.nav-toggle');
    const menu = document.getElementById('mainNav');
    if (!btn || !menu) return;

    let isOpen = false;

    function setNavVars() {
        const header = document.querySelector('.site-header');
        const hh = header?.getBoundingClientRect().height || 64;
        const vh = (window.visualViewport?.height || window.innerHeight);

        const openH = Math.max(vh - hh, 320);
        document.documentElement.style.setProperty('--header-h', hh + 'px');
        document.documentElement.style.setProperty('--nav-open-h', openH + 'px');

        const wasOpen = menu.classList.contains('is-open');
        if (!wasOpen) menu.classList.add('is-open');

        const contentH = menu.scrollHeight;
        const filler = Math.max(0, openH - contentH + 24);
        document.documentElement.style.setProperty('--nav-fill', filler + 'px');
        if (!wasOpen) menu.classList.remove('is-open');
    }

    setNavVars();
    window.addEventListener('resize', setNavVars);
    window.visualViewport?.addEventListener('resize', setNavVars);

    function onTransitionEnd(e) {
        if (e.target !== menu || e.propertyName !== 'max-height') return;
        if (!isOpen) {
            document.body.classList.remove('menu-open', 'menu-closing');
            document.body.style.overflow = '';
            menu.removeEventListener('transitionend', onTransitionEnd);
        } else {
            document.body.style.overflow = 'hidden';
        }
    }

    function openMenu() {
        setNavVars();
        isOpen = true;
        document.body.classList.add('menu-open');
        btn.setAttribute('aria-expanded', 'true');
        menu.classList.add('is-open');
        menu.addEventListener('transitionend', onTransitionEnd, { once: false });
    }

    function closeMenu() {
        isOpen = false;
        document.body.classList.add('menu-closing');
        btn.setAttribute('aria-expanded', 'false');
        menu.classList.remove('is-open');
        menu.addEventListener('transitionend', onTransitionEnd, { once: false });
    }

    btn.addEventListener('click', () => {
        (btn.getAttribute('aria-expanded') === 'true') ? closeMenu() : openMenu();
    });

    menu.addEventListener('click', (e) => {
        if (e.target.closest('a')) closeMenu();
    });

    window.addEventListener('resize', () => {
        if (window.innerWidth > 960 && (menu.classList.contains('is-open') || document.body.classList.contains('menu-open'))) {
            btn.setAttribute('aria-expanded', 'false');
            menu.classList.remove('is-open');
            document.body.classList.remove('menu-open', 'menu-closing');
            document.body.style.overflow = '';
        }
    });
})();

(() => {
    const mqDesktop = window.matchMedia('(min-width: 961px)');
    const item = document.querySelector('.nav-item.has-sub');
    if (!item) return;

    const btn = item.querySelector('.nav-link--services');
    const panel = item.querySelector('.subnav');

    btn?.setAttribute('aria-expanded', 'false');

    const openMobile = () => {
        item.classList.add('is-open');
        btn.setAttribute('aria-expanded', 'true');
    };
    const closeMobile = () => {
        item.classList.remove('is-open');
        btn.setAttribute('aria-expanded', 'false');
    };

    const onToggle = (e) => {
        if (mqDesktop.matches) return;
        e.preventDefault();
        item.classList.contains('is-open') ? closeMobile() : openMobile();
    };

    btn?.addEventListener('click', onToggle);

    panel?.addEventListener('click', (e) => {
        if (mqDesktop.matches) return;
        if (e.target.closest('a')) closeMobile();
    });

    document.addEventListener('click', (e) => {
        if (!mqDesktop.matches) return;
        if (!item.contains(e.target)) {
            btn.setAttribute('aria-expanded', 'false');
        } else {
            if (e.target === btn) btn.setAttribute('aria-expanded', 'true');
        }
    });

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        if (mqDesktop.matches) {
            btn.setAttribute('aria-expanded', 'false');
            btn.focus();
        } else if (item.classList.contains('is-open')) {
            closeMobile(); btn.focus();
        }
    });

    const onChange = () => {
        if (mqDesktop.matches) {
            closeMobile();
            btn.setAttribute('aria-expanded', 'false');
        }
    };
    mqDesktop.addEventListener?.('change', onChange);
})();

(() => {
    const header = document.querySelector('.site-header');
    if (!header) return;


    let lastY = window.pageYOffset || document.documentElement.scrollTop || 0;
    const REVEAL_OFFSET = 80;
    const MIN_DELTA = 8;

    const shouldIgnore = () =>
        document.body.classList.contains('menu-open') ||
        document.body.classList.contains('menu-closing');

    const onScroll = () => {
        const y = Math.max(0, window.pageYOffset || document.documentElement.scrollTop || 0);
        const dy = y - lastY;

        if (shouldIgnore()) {
            header.classList.remove('is-hidden');
            lastY = y;
            return;
        }

        if (y <= REVEAL_OFFSET) {
            header.classList.remove('is-hidden');
            lastY = y;
            return;
        }

        if (Math.abs(dy) > MIN_DELTA) {
            if (dy > 0) {
                header.classList.add('is-hidden');
            } else {
                header.classList.remove('is-hidden');
            }
            lastY = y;
        }
    };

    let ticking = false;
    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                onScroll();
                ticking = false;
            });
            ticking = true;
        }
    }, { passive: true });

    window.addEventListener('keydown', (e) => {
        if (e.key === 'Home' || e.key === 'PageUp') header.classList.remove('is-hidden');
    });
})();


whenConsentReady(function (consent) {
    if (consent.analytics) {

        var s1 = document.createElement('script');
        s1.async = true; s1.src = "https://www.googletagmanager.com/gtag/js?id=G-XXXXXXX";
        document.head.appendChild(s1);

        s1.onload = function () {
            window.dataLayer = window.dataLayer || [];
            function gtag() { dataLayer.push(arguments); }
            window.gtag = gtag;
            gtag('js', new Date());
            gtag('config', 'G-HMTCG6RQL9', { 'anonymize_ip': true });
        };
    }
});

(() => {
    const scroller = document.querySelector(".logo-scroller .scroller__inner");
    if (scroller) {
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (!prefersReducedMotion) {
            const scrollerContent = Array.from(scroller.children);
            scrollerContent.forEach(item => {
                const duplicatedItem = item.cloneNode(true);
                duplicatedItem.setAttribute("aria-hidden", true);
                scroller.appendChild(duplicatedItem);
            });
        }
    }
})();

document.addEventListener('DOMContentLoaded', () => {
    const section = document.querySelector('.section--word-planet');
    const surface = document.querySelector('.tagcloud-surface');
    const sourceSpans = document.querySelectorAll('.tag-source span');

    if (!section || !surface || sourceSpans.length === 0) {
        return;
    }

    const myTags = Array.from(sourceSpans).map(span => span.textContent);
    let tagCloudInstance = null;
    let isInitialized = false;

    const getResponsiveRadius = () => {
        if (window.innerWidth < 768) {
            return 180;
        } else {
            return 280;
        }
    };

    const initializeCloud = () => {
        if (isInitialized) return;
        isInitialized = true;

        const options = {
            radius: getResponsiveRadius(),
            maxSpeed: 'slow',
            initSpeed: 'slow',
            direction: 135,
            keep: true
        };
        tagCloudInstance = TagCloud(surface, myTags, options);
    };

    const updateCloudOnResize = () => {
        if (!isInitialized) return;

        if (tagCloudInstance) {
            try {
                tagCloudInstance.destroy();
            } catch (e) {
                console.error("Kunde inte förstöra TagCloud-instans:", e);
            }
        }
        initializeCloud();
        isInitialized = false;
        initializeCloud();
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {

            if (entry.isIntersecting) {
                initializeCloud();
                observer.unobserve(section);
            }
        });
    }, { threshold: 0.1 });

    observer.observe(section);

    let resizeTimer;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(updateCloudOnResize, 200);
    });
});


(() => {
    const toTopBtn = document.getElementById('toTopBtn');
    if (!toTopBtn) return;

    const toggleButtonVisibility = () => {
        if (window.scrollY > 300) {
            toTopBtn.classList.add('is-visible');
        } else {
            toTopBtn.classList.remove('is-visible');
        }
    };

    const scrollToTop = () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    };

    window.addEventListener('scroll', toggleButtonVisibility, { passive: true });

    toTopBtn.addEventListener('click', scrollToTop);

})();