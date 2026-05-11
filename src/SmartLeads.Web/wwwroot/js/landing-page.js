/**
 * SmartLeads Landing Page — Modern Interactions
 */
(function () {
    "use strict";

    const body = document.body;
    if (!body || !body.classList.contains("landing-page-body")) return;

    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    // --- DOM cache ---
    const revealItems = document.querySelectorAll(".reveal-on-scroll");
    const sections    = document.querySelectorAll("section[id]");
    const tabLinks    = document.querySelectorAll("[data-tab-link]");
    const counters    = document.querySelectorAll("[data-counter]");
    const progressBars = document.querySelectorAll("[data-progress-width]");
    const tiltCards   = document.querySelectorAll("[data-tilt-card]");

    // -----------------------------------------------------------------------
    // 1. Reveal-on-scroll (with stagger)
    // -----------------------------------------------------------------------
    function revealAll() {
        revealItems.forEach(function (item) {
            item.classList.add("revealed");
        });
    }

    if (revealItems.length) {
        if (reduceMotion || !("IntersectionObserver" in window)) {
            revealAll();
        } else {
            const revealObserver = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) return;
                    entry.target.classList.add("revealed");
                    obs.unobserve(entry.target);
                });
            }, {
                threshold: 0.12,
                rootMargin: "0px 0px -30px 0px"
            });

            revealItems.forEach(function (item, index) {
                // Stagger delay — cap at 350ms to keep feel snappy
                item.style.transitionDelay = Math.min(index * 45, 350) + "ms";
                revealObserver.observe(item);
            });
        }
    }

    // -----------------------------------------------------------------------
    // 2. Active tab highlight on scroll
    // -----------------------------------------------------------------------
    function setActiveTab(id) {
        tabLinks.forEach(function (link) {
            var isActive = link.getAttribute("href") === "#" + id;
            link.classList.toggle("is-active", isActive);
        });
    }

    if (sections.length && "IntersectionObserver" in window) {
        var sectionObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    setActiveTab(entry.target.id);
                }
            });
        }, {
            threshold: 0.30,
            rootMargin: "-15% 0px -40% 0px"
        });

        sections.forEach(function (section) {
            sectionObserver.observe(section);
        });
    } else if (sections[0]) {
        setActiveTab(sections[0].id);
    }

    // -----------------------------------------------------------------------
    // 3. Smooth-scroll tab links
    // -----------------------------------------------------------------------
    tabLinks.forEach(function (link) {
        link.addEventListener("click", function (e) {
            var targetId = link.getAttribute("href");
            if (!targetId || !targetId.startsWith("#")) return;

            var target = document.querySelector(targetId);
            if (!target) return;

            e.preventDefault();
            target.scrollIntoView({
                behavior: reduceMotion ? "auto" : "smooth",
                block: "start"
            });
            setActiveTab(target.id);
        });
    });

    // -----------------------------------------------------------------------
    // 4. Animated counters (with easing)
    // -----------------------------------------------------------------------
    function startCounter(el) {
        if (el.dataset.counted === "true") return;
        el.dataset.counted = "true";

        var target = Number(el.dataset.counter || 0);
        if (reduceMotion || Number.isNaN(target)) {
            el.textContent = String(target);
            return;
        }

        var duration = 1400;
        var startTime = performance.now();

        function tick(now) {
            var elapsed = now - startTime;
            var t = Math.min(elapsed / duration, 1);
            // Cubic ease-out
            var ease = 1 - Math.pow(1 - t, 3);
            el.textContent = String(Math.round(target * ease));

            if (t < 1) {
                window.requestAnimationFrame(tick);
            }
        }

        window.requestAnimationFrame(tick);
    }

    if (counters.length) {
        if ("IntersectionObserver" in window) {
            var counterObserver = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) return;
                    startCounter(entry.target);
                    obs.unobserve(entry.target);
                });
            }, { threshold: 0.50 });

            counters.forEach(function (c) {
                counterObserver.observe(c);
            });
        } else {
            counters.forEach(startCounter);
        }
    }

    // -----------------------------------------------------------------------
    // 5. Animated progress bars
    // -----------------------------------------------------------------------
    function fillBar(bar) {
        if (bar.dataset.filled === "true") return;
        bar.dataset.filled = "true";
        bar.style.width = bar.dataset.progressWidth || "0%";
    }

    if (progressBars.length) {
        if ("IntersectionObserver" in window) {
            var progressObserver = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) return;
                    fillBar(entry.target);
                    obs.unobserve(entry.target);
                });
            }, { threshold: 0.45 });

            progressBars.forEach(function (bar) {
                progressObserver.observe(bar);
            });
        } else {
            progressBars.forEach(fillBar);
        }
    }

    // -----------------------------------------------------------------------
    // 6. Subtle 3D tilt on hover (with requestAnimationFrame throttle)
    // -----------------------------------------------------------------------
    if (!reduceMotion && tiltCards.length) {
        tiltCards.forEach(function (card) {
            var ticking = false;

            card.addEventListener("pointermove", function (e) {
                if (ticking) return;
                window.requestAnimationFrame(function () {
                    var rect = card.getBoundingClientRect();
                    var x = e.clientX - rect.left;
                    var y = e.clientY - rect.top;
                    var rotateX = ((y / rect.height) - 0.5) * -6;
                    var rotateY = ((x / rect.width) - 0.5) * 6;
                    card.style.transform =
                        "perspective(800px) rotateX(" + rotateX.toFixed(2) + "deg) rotateY(" + rotateY.toFixed(2) + "deg) translateY(-4px)";
                    ticking = false;
                });
                ticking = true;
            });

            card.addEventListener("pointerleave", function () {
                card.style.transform = "";
            });
        });
    }

})();
