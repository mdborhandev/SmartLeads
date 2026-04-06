// Select2 Initializer for SmartLeads
const select2FirstLoadFlags = {};

function select2Initializer(selector, url, type, parent, autoOpen = false, AutoScroll = '', multiselect = false) {
    const key = selector;
    
    // Check if element exists
    if (!$(selector).length) {
        console.error(`Select2: Element ${selector} not found`);
        return;
    }

    $(selector).select2({
        dropdownParent: (parent && $(parent).length) ? $(parent) : null,
        multiple: multiselect,
        ajax: {
            url: url,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    searchTerm: params.term || '',
                    type: type,
                    selectedvalue: ''
                };
            },
            processResults: function (data) {
                console.log('Select2 response:', data);
                
                if (!Array.isArray(data)) {
                    console.error('Select2: Expected array response but got:', typeof data);
                    return { results: [] };
                }
                
                if (autoOpen) {
                    if (!select2FirstLoadFlags[key]) {
                        const selectedItem = data.find(x => x.selected === true);
                        if (selectedItem) {
                            DefaultSelected(selector, selectedItem.id, selectedItem.text);
                        }
                        select2FirstLoadFlags[key] = true;
                        $(selector).select2("close");

                        if (AutoScroll != '') {
                            setTimeout(() => {
                                const goToTop = document.getElementById(AutoScroll);
                                if (goToTop) {
                                    goToTop.scrollIntoView({ behavior: "smooth", block: "end" });
                                }
                            }, 1000);
                        }
                    }
                }
                return {
                    results: data
                };
            },
            error: function(xhr, status, error) {
                console.error('Select2 AJAX Error:', status, error);
                console.error('Response Status:', xhr.status);
                console.error('Response:', xhr.responseText);
                
                // Show user-friendly error message
                if (xhr.status === 400) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        console.error('Server Error:', response.error || response.message);
                        toastr.error(response.error || response.message, 'Error Loading Data');
                    } catch (e) {
                        console.error('Failed to parse error response');
                    }
                } else if (xhr.status === 401 || xhr.status === 403) {
                    toastr.error('Please log in to access this feature', 'Authentication Required');
                } else {
                    toastr.error('Failed to load data. Please try again.', 'Error');
                }
            }
        },
        minimumInputLength: 0,
        placeholder: 'Select an option'
    }).on('select2:open', function () {
        setTimeout(() => {
            document.querySelector('.select2-container--open .select2-search__field')?.focus();
        }, 100);
    }).on('select2:close', function () {
        // Empty handler
    });
    
    if (autoOpen) {
        $(selector).select2("open");
    }
}

// Helper function for default selection
function DefaultSelected(selector, value, text) {
    var data = {
        id: value == null ? "" : value,
        text: text == null ? "" : text
    };
    var dropdown = $(selector);
    var option = new Option(data.text, data.id, true, true);
    dropdown.append(option).trigger('change');
    dropdown.trigger({
        type: 'select2:select',
        params: { data: data }
    });
}