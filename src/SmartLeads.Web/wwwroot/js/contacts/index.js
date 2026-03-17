// Tabulator Table Initialization and AJAX Handling for Contacts
let contactsTable;
let deleteContactId = null;

// Initialize Tabulator on document ready
$(document).ready(function() {
    initializeTabulator();
    setupSearchFilter();
});

// Initialize Tabulator table
function initializeTabulator() {
    contactsTable = new Tabulator("#contactsTable", {
        data: [],
        dataLoader: true,
        layout: "fitColumns",
        placeholder: "No contacts found",
        pagination: true,
        paginationSize: 10,
        paginationSizeSelector: [10, 25, 50, 100],
        columns: [
            {
                title: "Name",
                field: "fullName",
                width: 250,
                formatter: function(cell, formatterParams, onRendered) {
                    const data = cell.getData();
                    const initials = (data.firstName[0] || '') + (data.lastName[0] || '');
                    return `
                        <div class="d-flex align-items-center">
                            <div class="avatar avatar-sm me-3">
                                <span class="avatar-initial rounded-circle bg-label-primary">
                                    ${initials}
                                </span>
                            </div>
                            <div>
                                <span class="fw-semibold">${data.firstName} ${data.lastName}</span>
                            </div>
                        </div>
                    `;
                }
            },
            {
                title: "Email",
                field: "email",
                width: 200,
                formatter: function(cell) {
                    const email = cell.getValue();
                    return email ? `<a href="mailto:${email}" class="text-decoration-none">${email}</a>` : '<span class="text-muted">-</span>';
                }
            },
            {
                title: "Phone",
                field: "phoneNumber",
                width: 150,
                formatter: function(cell) {
                    const phone = cell.getValue();
                    return phone ? `<a href="tel:${phone}" class="text-decoration-none">${phone}</a>` : '<span class="text-muted">-</span>';
                }
            },
            {
                title: "Company",
                field: "company",
                width: 150
            },
            {
                title: "Job Title",
                field: "jobTitle",
                width: 150
            },
            {
                title: "Actions",
                field: "id",
                width: 120,
                hozAlign: "center",
                formatter: function(cell) {
                    const id = cell.getValue();
                    return `
                        <div class="d-flex gap-1 justify-content-center">
                            <button type="button" class="btn btn-sm btn-icon btn-label-primary" onclick="openEditModal(${id})" title="Edit">
                                <i class="icon-base bx bx-edit-alt"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-icon btn-label-danger" onclick="openDeleteModal(${id})" title="Delete">
                                <i class="icon-base bx bx-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        ajaxConfig: "GET",
        ajaxResponse: function(url, params, response) {
            return response;
        },
        dataLoaded: function(data) {
            toggleEmptyState(data.length === 0);
        }
    });

    // Load data
    loadContacts();
}

// Load contacts from API
function loadContacts() {
    $.ajax({
        url: '/api/contacts',
        type: 'GET',
        dataType: 'json',
        success: function(response) {
            const contacts = response.map(contact => ({
                ...contact,
                fullName: `${contact.firstName} ${contact.lastName}`
            }));
            contactsTable.setData(contacts);
        },
        error: function(xhr) {
            showToast('Error loading contacts', 'error');
        }
    });
}

// Setup search filter
function setupSearchFilter() {
    $('#searchInput').on('input', function() {
        const value = $(this).val();
        contactsTable.setFilter([
            { field: "firstName", type: "like", value: value },
            { field: "lastName", type: "like", value: value },
            { field: "email", type: "like", value: value },
            { field: "company", type: "like", value: value }
        ]);
    });
}

// Toggle empty state visibility
function toggleEmptyState(show) {
    if (show) {
        $('#emptyState').fadeIn();
        $('#contactsTable').hide();
    } else {
        $('#emptyState').hide();
        $('#contactsTable').fadeIn();
    }
}

// Open Create Modal
function openCreateModal() {
    resetForm();
    $('#modalTitle').text('Add Contact');
    $('#saveButtonText').text('Save Contact');
    $('#contactId').val('');
    $('#contactModal').modal('show');
}

// Open Edit Modal
function openEditModal(id) {
    $.ajax({
        url: `/api/contacts/${id}`,
        type: 'GET',
        dataType: 'json',
        success: function(contact) {
            resetForm();
            $('#modalTitle').text('Edit Contact');
            $('#saveButtonText').text('Update Contact');
            $('#contactId').val(contact.id);
            $('#firstName').val(contact.firstName);
            $('#lastName').val(contact.lastName);
            $('#email').val(contact.email);
            $('#phoneNumber').val(contact.phoneNumber);
            $('#company').val(contact.company);
            $('#jobTitle').val(contact.jobTitle);
            $('#address').val(contact.address);
            $('#contactModal').modal('show');
        },
        error: function(xhr) {
            showToast('Error loading contact details', 'error');
        }
    });
}

// Save Contact (Create or Update)
function saveContact() {
    clearErrors();
    
    const contactId = $('#contactId').val();
    const data = {
        firstName: $('#firstName').val().trim(),
        lastName: $('#lastName').val().trim(),
        email: $('#email').val().trim(),
        phoneNumber: $('#phoneNumber').val().trim(),
        company: $('#company').val().trim(),
        jobTitle: $('#jobTitle').val().trim(),
        address: $('#address').val().trim()
    };

    // Basic validation
    let isValid = true;
    if (!data.firstName) {
        $('#firstNameError').text('First name is required');
        isValid = false;
    }
    if (!data.lastName) {
        $('#lastNameError').text('Last name is required');
        isValid = false;
    }
    if (data.email && !validateEmail(data.email)) {
        $('#emailError').text('Invalid email format');
        isValid = false;
    }

    if (!isValid) return;

    const url = contactId ? `/api/contacts/${contactId}` : '/api/contacts';
    const method = contactId ? 'PUT' : 'POST';

    const payload = contactId 
        ? { id: parseInt(contactId), ...data }
        : data;

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function(response) {
            $('#contactModal').modal('hide');
            showToast(contactId ? 'Contact updated successfully' : 'Contact created successfully', 'success');
            loadContacts();
        },
        error: function(xhr) {
            if (xhr.responseJSON && xhr.responseJSON.errors) {
                xhr.responseJSON.errors.forEach(error => {
                    showToast(error, 'error');
                });
            } else {
                showToast('Error saving contact', 'error');
            }
        }
    });
}

// Open Delete Modal
function openDeleteModal(id) {
    deleteContactId = id;
    $('#deleteModal').modal('show');
}

// Confirm Delete
function confirmDelete() {
    if (!deleteContactId) return;

    $.ajax({
        url: `/api/contacts/${deleteContactId}`,
        type: 'DELETE',
        success: function(response) {
            $('#deleteModal').modal('hide');
            showToast('Contact deleted successfully', 'success');
            loadContacts();
        },
        error: function(xhr) {
            showToast('Error deleting contact', 'error');
        }
    });
}

// Reset form fields
function resetForm() {
    $('#contactForm')[0].reset();
    clearErrors();
}

// Clear validation errors
function clearErrors() {
    $('.text-danger').text('');
}

// Show toast notification
function showToast(message, type = 'success') {
    const options = {
        closeButton: true,
        debug: false,
        newestOnTop: true,
        progressBar: true,
        positionClass: "toast-top-right",
        preventDuplicates: true,
        onclick: null,
        showDuration: "300",
        hideDuration: "1000",
        timeOut: "3000",
        extendedTimeOut: "1000",
        showEasing: "swing",
        hideEasing: "linear",
        showMethod: "fadeIn",
        hideMethod: "fadeOut"
    };

    if (type === 'error') {
        toastr.error(message, 'Error', options);
    } else if (type === 'success') {
        toastr.success(message, 'Success', options);
    } else if (type === 'warning') {
        toastr.warning(message, 'Warning', options);
    } else if (type === 'info') {
        toastr.info(message, 'Info', options);
    }
}

// Close modal on escape key
$(document).on('keydown', function(e) {
    if (e.key === 'Escape') {
        $('.modal').modal('hide');
    }
});
