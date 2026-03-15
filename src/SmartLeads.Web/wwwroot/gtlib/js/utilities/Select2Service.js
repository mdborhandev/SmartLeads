function select2Initializer(selector, url, type, parent) {
    $(selector).select2({
        theme: 'bootstrap-5',
        placeholder: 'Select an option',
        allowClear: true,
        dropdownParent: (parent && $(parent).length) ? $(parent) : null,
        ajax: {
            url: url,
            data: function (params) {
                return {
                    searchTerm: params.term,
                    type: type
                };
            },
            transport: async function (params, success, failure) {
                try {
                    params.headers = {
                        Authorization: await getRefreshToken()
                    };

                    const request = $.ajax(params);
                    request.done(success);
                    request.fail(failure);

                    return request;
                } catch (error) {
                    console.error("Refresh failed", error);
                    failure(error);
                }
            },
            processResults: function (data) {
                return {
                    results: data.value
                };
            }
        }
    })
}
function DefaultSelected(selector, value, text) {
    var data =
    {
        id: value == null ? "Please Select" : value,
        text: text == null ? "Please Select" : text
    };

    var dropdown = $(selector);
    var option = new Option(data.text, data.id, true, true);
    dropdown.append(option).trigger('change');

    // manually trigger the `select2:select` event
    dropdown.trigger({
        type: 'select2:select',
        params: {
            data: data
        }
    });
}