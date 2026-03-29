(function () {
    const body = document.body;
    if (!body || !body.classList.contains("landing-page-body")) {
        return;
    }

    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const revealItems = document.querySelectorAll(".reveal-on-scroll");
    const sections = document.querySelectorAll("section[id]");
    const tabLinks = document.querySelectorAll("[data-tab-link]");
    const counters = document.querySelectorAll("[data-counter]");
    const progressBars = document.querySelectorAll("[data-progress-width]");
    const tiltCards = document.querySelectorAll("[data-tilt-card]");

    const setActiveTab = function (id) {
        tabLinks.forEach(function (link) {
            const isActive = link.getAttribute("href") === "#" + id;
            link.classList.toggle("is-active", isActive);
        });
    };

    if (revealItems.length) {
        if (reduceMotion || !("IntersectionObserver" in window)) {
            revealItems.forEach(function (item) {
                item.classList.add("revealed");
            });
        } else {
            const revealObserver = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.classList.add("revealed");
                    observer.unobserve(entry.target);
                });
            }, {
                threshold: 0.16,
                rootMargin: "0px 0px -40px 0px"
            });

            revealItems.forEach(function (item, index) {
                item.style.transitionDelay = Math.min(index * 55, 280) + "ms";
                revealObserver.observe(item);
            });
        }
    }

    if (sections.length) {
        if ("IntersectionObserver" in window) {
            const sectionObserver = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        setActiveTab(entry.target.id);
                    }
                });
            }, {
                threshold: 0.35,
                rootMargin: "-20% 0px -45% 0px"
            });

            sections.forEach(function (section) {
                sectionObserver.observe(section);
            });
        } else if (sections[0]) {
            setActiveTab(sections[0].id);
        }
    }

    tabLinks.forEach(function (link) {
        link.addEventListener("click", function (event) {
            const targetId = link.getAttribute("href");
            if (!targetId || !targetId.startsWith("#")) {
                return;
            }

            const target = document.querySelector(targetId);
            if (!target) {
                return;
            }

            event.preventDefault();
            target.scrollIntoView({
                behavior: reduceMotion ? "auto" : "smooth",
                block: "start"
            });
            setActiveTab(target.id);
        });
    });

    if (counters.length) {
        const startCounter = function (counter) {
            if (counter.dataset.counted === "true") {
                return;
            }

            counter.dataset.counted = "true";

            const target = Number(counter.dataset.counter || 0);
            if (reduceMotion || Number.isNaN(target)) {
                counter.textContent = String(target);
                return;
            }

            const duration = 1200;
            const start = performance.now();

            const tick = function (now) {
                const progress = Math.min((now - start) / duration, 1);
                counter.textContent = String(Math.round(target * progress));

                if (progress < 1) {
                    window.requestAnimationFrame(tick);
                }
            };

            window.requestAnimationFrame(tick);
        };

        if ("IntersectionObserver" in window) {
            const counterObserver = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    startCounter(entry.target);
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0.55 });

            counters.forEach(function (counter) {
                counterObserver.observe(counter);
            });
        } else {
            counters.forEach(startCounter);
        }
    }

    if (progressBars.length) {
        const fillBar = function (bar) {
            if (bar.dataset.filled === "true") {
                return;
            }

            bar.dataset.filled = "true";
            bar.style.width = bar.dataset.progressWidth || "0%";
        };

        if ("IntersectionObserver" in window) {
            const progressObserver = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    fillBar(entry.target);
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0.5 });

            progressBars.forEach(function (bar) {
                progressObserver.observe(bar);
            });
        } else {
            progressBars.forEach(fillBar);
        }
    }

    if (!reduceMotion) {
        tiltCards.forEach(function (card) {
            card.addEventListener("pointermove", function (event) {
                const rect = card.getBoundingClientRect();
                const x = event.clientX - rect.left;
                const y = event.clientY - rect.top;
                const rotateX = ((y / rect.height) - 0.5) * -8;
                const rotateY = ((x / rect.width) - 0.5) * 8;
                card.style.transform = "perspective(900px) rotateX(" + rotateX.toFixed(2) + "deg) rotateY(" + rotateY.toFixed(2) + "deg) translateY(-6px)";
            });

            card.addEventListener("pointerleave", function () {
                card.style.transform = "";
            });
        });
    }
})();
