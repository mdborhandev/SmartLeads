/**
 * Global application scripts
 */

$(document).ready(function() {
    // Menu Initialization - Ensure menu is properly initialized
    try {
        if (typeof Menu !== 'undefined' && document.getElementById('layout-menu')) {
            const layoutMenuEl = document.getElementById('layout-menu');
            // Check if menu is already initialized by main.js
            if (!layoutMenuEl.menuInstance) {
                const isHorizontalLayout = layoutMenuEl.classList.contains('menu-horizontal');
                window.menu = new Menu(layoutMenuEl, {
                    orientation: isHorizontalLayout ? 'horizontal' : 'vertical',
                    closeChildren: isHorizontalLayout ? true : false,
                    showDropdownOnHover: false
                });
                window.Helpers.mainMenu = window.menu;
                console.log('Menu initialized successfully');
            } else {
                console.log('Menu already initialized by main.js');
            }
        }
        
        // Re-bind menu toggler events if not working
        if (typeof Helpers !== 'undefined' && typeof Helpers.toggleCollapsed === 'function') {
            const menuTogglers = document.querySelectorAll('.layout-menu-toggle');
            menuTogglers.forEach(function(toggler) {
                // Remove existing event listeners by cloning
                const newToggler = toggler.cloneNode(true);
                toggler.parentNode.replaceChild(newToggler, toggler);
                
                // Add click event
                newToggler.addEventListener('click', function(event) {
                    event.preventDefault();
                    Helpers.toggleCollapsed();
                    
                    // Save state to localStorage
                    if (config && config.enableMenuLocalStorage && !Helpers.isSmallScreen()) {
                        try {
                            localStorage.setItem(
                                'templateCustomizer-' + templateName + '--LayoutCollapsed',
                                String(Helpers.isCollapsed())
                            );
                        } catch (e) {}
                    }
                });
            });
            console.log('Menu toggler events bound successfully');
            
            // Also bind overlay click to close menu on mobile
            const overlay = document.querySelector('.layout-overlay');
            if (overlay) {
                overlay.addEventListener('click', function(event) {
                    event.preventDefault();
                    if (Helpers.isSmallScreen()) {
                        Helpers.setCollapsed(true);
                    }
                });
                console.log('Overlay click handler bound successfully');
            }
        }
    } catch (error) {
        console.error('Menu initialization error:', error);
    }

    // Global Email Validation on Input
    $(document).on('input blur', 'input[type="email"]', function() {
        const $input = $(this);
        const email = $input.val().trim();

        // Don't validate if empty (other required validators will handle that)
        if (email.length === 0) {
            $input.removeClass('is-invalid is-valid');
            return;
        }

        if (typeof validateEmail === 'function') {
            if (!validateEmail(email)) {
                $input.addClass('is-invalid').removeClass('is-valid');

                // Find or create error feedback element
                let $feedback = $input.siblings('.invalid-feedback');
                if ($feedback.length === 0) {
                    $feedback = $('<div class="invalid-feedback">Please enter a valid email address.</div>');
                    $input.after($feedback);
                }
            } else {
                $input.addClass('is-valid').removeClass('is-invalid');
            }
        }
    });
});
