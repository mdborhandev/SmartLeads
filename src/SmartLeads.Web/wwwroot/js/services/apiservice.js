const select2FirstLoadFlags = {};
function select2Initializer(selector, url, type, parent,autoOpen=false,AutoScroll='',multiselect=false) {
    const key = selector;
    $(selector).select2({
        dropdownParent: (parent && $(parent).length) ? $(parent) : null,        
        multiple: multiselect,
        ajax: {
            url: url,
            data: function (params) {
                return {
                    searchTerm: params.term,
                    type: type
                };
            },
            processResults: function (data) { 
                if (autoOpen) {

                if (!select2FirstLoadFlags[key]) {
                    const selectedItem = data.find(x => x.selected === true);
                    if (selectedItem) {
                        DefaultSelected(selector,selectedItem.id,selectedItem.text)
                    }
                    select2FirstLoadFlags[key] = true;
                    $(selector).select2("close"); 
                   
                    if (AutoScroll != '') {
                        setTimeout(() => {
                            const goToTop = document.getElementById(AutoScroll);
                        goToTop.scrollIntoView({ behavior: "smooth", block: "end" });

                        },1000)
                    }
                }
                }
                return {
                    results: data
                };
            }
        }
    }).on('select2:open', function () {
        setTimeout(() => {
            document.querySelector('.select2-container--open .select2-search__field')?.focus();
        }, 100);
    }).on('select2:close', function () {
        
    });
    if (autoOpen) {
        $(selector).select2("open");
    }

    // Initial rendering of the custom icon
    
}

function DefaultSelected(selector, value, text) {
    var data =
    {
        id: value == null ? "" : value,
        text: text == null ? "" : text
    };

    var dropdown = $(selector);
    var option = new Option(data.text, data.id, true, true);
    dropdown.append(option).trigger('change');

    //// manually trigger the `select2:select` event
    dropdown.trigger({
        type: 'select2:select',
        params: {
            data: data
        }
    });
}


// Function to set default selected value, enhanced for multi-select
function DefaultSelectedForMultiple(selector, idOrArray, nameOrText) {
    const $select = $(selector);
    $select.empty(); // Clear existing options to avoid duplicates

    // Handle single value
    if (typeof idOrArray === 'string' || typeof idOrArray === 'number') {
        if ($select.find(`option[value='${idOrArray}']`).length) {
            $select.val(idOrArray).trigger('change');
        } else {
            const newOption = new Option(nameOrText || idOrArray, idOrArray, true, true);
            $select.append(newOption).trigger('change');
        }
    }
    // Handle array for multi-select
    else if (Array.isArray(idOrArray)) {
        idOrArray.forEach(item => {
            const id = item.id;
            const text = item.text;
            if ($select.find(`option[value='${id}']`).length) {
                $select.val($select.val().concat(id).filter(Boolean)); // Append to existing selection
            } else {
                const newOption = new Option(text || id, id, true, true);
                $select.append(newOption);
            }
        });
        $select.val(idOrArray.map(item => item.id)).trigger('change'); // Set all selected values
    }
}


// Generic function to preview files
function PreviewFiles(input, callback) {
    let selectedFiles = [];
    let previewContainer = $("#previewContainer");
    previewContainer.empty(); // Clear previous previews

    // Handle single file or FileList/array
    if (input instanceof FileList || Array.isArray(input)) {
        selectedFiles = Array.from(input); // Convert FileList or array to an array
    } else if (input instanceof File) {
        selectedFiles = [input]; // Wrap single file in an array
    } else {
        console.warn("Invalid input type. Expected File, FileList, or array.");
        return;
    }

    if (selectedFiles.length === 0) {
        console.warn("No files selected.");
        return;
    }

    // Adjust preview layout based on the number of files
    if (selectedFiles.length === 1) {
        previewContainer.css({
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            flexWrap: "nowrap", // Single file, no wrapping
        });
    } else {
        previewContainer.css({
            display: "flex",
            flexWrap: "wrap", // Wrap for multiple files
            gap: "10px",
        });
    }

    // Generate previews
    selectedFiles.forEach(file => {
        const reader = new FileReader();
        reader.onload = function (e) {
            let previewElement;
            if (file.type.startsWith("image/")) {
                previewElement = `<img src="${e.target.result}" class="preview-img" alt="Preview" style="max-width: 100%; max-height: 80vh;">`;
            } else if (file.type === "application/pdf") {
                previewElement = `<embed src="${e.target.result}" class="preview-pdf" type="application/pdf" style="width: 100%; height: 80vh;">`;
            } else {
                previewElement = `<p class="preview-text">${file.name}</p>`;
            }
            previewContainer.append(previewElement);
        };
        reader.readAsDataURL(file);
    });

    // Show modal
    let modal = new bootstrap.Modal(document.getElementById("previewModal"));
    modal.show();

    // Confirm button (returns true)
    $("#confirmBtn").off("click").on("click", function () {
        modal.hide();
        callback(true); // User confirmed the preview
    });

    // Cancel button (returns false)
    $("#cancelBtn").off("click").on("click", function () {
        selectedFiles = []; // Clear selection
        $("#fileInput").val(""); // Reset input
        callback(false); // User rejected the preview
    });
}

/*new code added by shahinur 2 july 25*/


// === Fullscreen Mode ===
// Initialize layout on page load or after login
// Initialize layout on page load
function initializeLayout() {
    let layout = $('.layout'); // Adjust selector if the class is on a different element, e.g., '.layout-container'
    let layoutmenu = $('#layout-menu');

    // Get stored state; default to false (menu open, content on left)
    const menuCollapsed = localStorage.getItem('menuCollapsed') === 'true';

    layoutmenu.show();

    // Toggle class to handle layout (CSS will manage paddings/positions)
    if (menuCollapsed) {
        layout.addClass('layout-menu-collapsed');
    } else {
        layout.removeClass('layout-menu-collapsed');
    }
}

// Call initializeLayout on document ready or after login
$(document).ready(function () {
    setTimeout(() => {
        initializeLayout();
    }, 500); // Increased slightly for DOM stability; test and adjust if needed
});

function ToggleFullScreen(isfullscreen) {
    let layout = $('.layout'); // Adjust selector if needed
    let layoutmenu = $('#layout-menu');
    let layoutmenutoggle = $('.layout-menu-toggle');
    let layoutpage = $('.layout-page');
    let navbar = $('.layout-navbar');
    let fullscreen = $('#fullScreen');
    let hideFs = $('#hideFs');

    // Get current state
    const menuCollapsed = localStorage.getItem('menuCollapsed') === 'true';

    if (isfullscreen) {
        // Enter fullscreen: hide menu, remove paddings, full width
        layoutmenu.hide();
        layoutpage.css({ 'padding-left': '0' });
        navbar.css({ left: '0', width: '100%' });
        fullscreen.addClass('d-none');
        hideFs.removeClass('d-none');
        localStorage.setItem('isFullScreen', true);
    } else {
        // Exit fullscreen: show menu, restore class-based state
        layoutmenu.show();
        layoutpage.removeAttr('style'); // Clear any inline
        navbar.removeAttr('style'); // Clear any inline
        if (menuCollapsed) {
            layout.addClass('layout-menu-collapsed');
        } else {
            layout.removeClass('layout-menu-collapsed');
        }
        fullscreen.removeClass('d-none');
        hideFs.addClass('d-none');
        localStorage.setItem('isFullScreen', false);
    }
}

$(".layout-menu-toggle").on("click", function () {
    let layoutpage = $('.layout-page');
    let navbar = $('.layout-navbar');
    navbar.removeAttr('style');
    layoutpage.removeAttr('style');
    // Assuming the framework toggles 'layout-menu-collapsed' here or elsewhere; if not, add:
    // $('.layout').toggleClass('layout-menu-collapsed');
    // localStorage.setItem('menuCollapsed', $('.layout').hasClass('layout-menu-collapsed'));
});

// === Exit Fullscreen Mode ===
function disableFullScreen() {
    let layout = $('.layout'); // Adjust selector if needed
    let layoutmenu = $('#layout-menu');
    let navbar = $('.layout-navbar');

    layoutmenu.show();

    // Restore based on stored collapsed state
    const menuCollapsed = localStorage.getItem('menuCollapsed') === 'true';
    if (menuCollapsed) {
        layout.addClass('layout-menu-collapsed');
    } else {
        layout.removeClass('layout-menu-collapsed');
    }

    // Clear any lingering inline styles
    navbar.removeAttr('style');
    $('.layout-page').removeAttr('style');

    // Toggle buttons
    $('#fullScreen').removeClass('d-none');
    $('#hideFs').addClass('d-none');
}