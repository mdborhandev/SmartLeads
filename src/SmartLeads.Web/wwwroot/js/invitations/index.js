// Global variables
let invitationsTable = null;

// Initialize on document ready
$(document).ready(function() {
    // Initialize Tabulator table
    initializeTable();

    // Load invitations
    loadInvitations();
});

// Initialize Tabulator table
function initializeTable() {
    invitationsTable = new Tabulator("#invitationsTable", {
        data: [],
        layout: "fitColumns",
        responsiveLayout: "collapse",
        placeholder: "No invitations found",
        columns: [
            {
                title: "Email",
                field: "email",
                minWidth: 200,
                responsive: 0
            },
            {
                title: "Role",
                field: "role",
                minWidth: 100,
                formatter: roleFormatter,
                responsive: 0
            },
            {
                title: "Invited By",
                field: "invitedByUserName",
                minWidth: 150,
                responsive: 0
            },
            {
                title: "Invited Date",
                field: "invitedAt",
                minWidth: 120,
                formatter: dateFormatter,
                responsive: 0
            },
            {
                title: "Expires",
                field: "expiresAt",
                minWidth: 120,
                formatter: expiryFormatter,
                responsive: 0
            },
            {
                title: "Status",
                field: "status",
                minWidth: 100,
                formatter: statusFormatter,
                responsive: 0
            }
        ],
        rowFormatter: function(row) {
            // Add custom styling based on status
            const data = row.getData();
            if (data.status === "Expired") {
                row.getElement().style.backgroundColor = "rgba(255, 0, 0, 0.05)";
            } else if (data.status === "Accepted") {
                row.getElement().style.backgroundColor = "rgba(0, 255, 0, 0.05)";
            }
        }
    });
}

// Custom formatters
function roleFormatter(cell, formatterParams, onRendered) {
    const role = cell.getValue();
    const roleNames = { "0": "User", "1": "Admin", "2": "Manager", "3": "SuperAdmin" };
    const roleColors = { "0": "info", "1": "danger", "2": "warning", "3": "primary" };
    const roleName = roleNames[role] || role;
    const color = roleColors[role] || "secondary";
    return '<span class="badge bg-' + color + '">' + roleName + '</span>';
}

function dateFormatter(cell, formatterParams, onRendered) {
    const date = new Date(cell.getValue());
    return date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
}

function expiryFormatter(cell, formatterParams, onRendered) {
    const date = new Date(cell.getValue());
    const isExpired = date < new Date();
    const formattedDate = date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
    if (isExpired) {
        return '<span class="text-danger"><i class="bx bx-error me-1"></i>' + formattedDate + '</span>';
    }
    return '<span>' + formattedDate + '</span>';
}

function statusFormatter(cell, formatterParams, onRendered) {
    const status = cell.getValue();
    const statusColors = {
        "Pending": "warning",
        "Accepted": "success",
        "Rejected": "danger",
        "Expired": "secondary",
        "Cancelled": "secondary"
    };
    const color = statusColors[status] || "secondary";
    return '<span class="badge bg-' + color + '">' + status + '</span>';
}

// Load invitations from API
async function loadInvitations() {
    try {
        const response = await fetch('/Invitations/GetAll');
        if (response.ok) {
            const data = await response.json();
            invitationsTable.setData(data);
            
            // Show/hide empty state
            if (data.length === 0) {
                document.getElementById('emptyState').style.display = 'block';
                document.getElementById('invitationsTable').style.display = 'none';
            } else {
                document.getElementById('emptyState').style.display = 'none';
                document.getElementById('invitationsTable').style.display = 'block';
            }
        }
    } catch (error) {
        console.error('Error loading invitations:', error);
        toastr.error('Failed to load invitations', 'Error');
    }
}

// Open invite modal
function openInviteModal() {
    // Reset form
    document.getElementById('inviteForm').reset();
    clearErrors();
    
    // Show modal
    const inviteModal = new bootstrap.Modal(document.getElementById('inviteModal'));
    inviteModal.show();
}

// Clear error messages
function clearErrors() {
    document.getElementById('emailError').textContent = '';
    document.getElementById('roleError').textContent = '';
    document.getElementById('expiryDaysError').textContent = '';
}

// Send invitation
async function sendInvitation() {
    clearErrors();
    
    const email = document.getElementById('email').value.trim();
    const role = parseInt(document.getElementById('role').value);
    const expiryDays = parseInt(document.getElementById('expiryDays').value);
    
    // Validate
    let isValid = true;
    if (!email) {
        document.getElementById('emailError').textContent = 'Email is required';
        isValid = false;
    } else if (!isValidEmail(email)) {
        document.getElementById('emailError').textContent = 'Invalid email address';
        isValid = false;
    }
    
    if (!isValid) return;
    
    // Disable button
    const sendButton = document.querySelector('#inviteModal .btn-primary');
    const originalText = sendButton.innerHTML;
    sendButton.disabled = true;
    sendButton.innerHTML = '<i class="bx bx-loader bx-spin me-1"></i> Sending...';
    
    try {
        const formData = new FormData();
        formData.append('Email', email);
        formData.append('Role', role);
        formData.append('ExpiryDays', expiryDays);
        formData.append('__RequestVerificationToken', getAntiForgeryToken());
        
        const response = await fetch('/Invitations/Create', {
            method: 'POST',
            body: formData
        });
        
        if (response.ok) {
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('inviteModal'));
            modal.hide();
            
            // Show success message
            toastr.success('Invitation sent successfully!', 'Success');
            
            // Reload table
            loadInvitations();
        } else {
            const result = await response.json();
            if (result && result.errors) {
                // Display validation errors
                Object.keys(result.errors).forEach(key => {
                    const errorKey = key.charAt(0).toLowerCase() + key.slice(1) + 'Error';
                    const errorElement = document.getElementById(errorKey);
                    if (errorElement) {
                        errorElement.textContent = result.errors[key].join(', ');
                    }
                });
            } else if (result && result.message) {
                toastr.error(result.message, 'Error');
            } else {
                toastr.error('Failed to send invitation', 'Error');
            }
        }
    } catch (error) {
        console.error('Error sending invitation:', error);
        toastr.error('Failed to send invitation', 'Error');
    } finally {
        // Re-enable button
        sendButton.disabled = false;
        sendButton.innerHTML = originalText;
    }
}

// Validate email
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Get anti-forgery token
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
           document.querySelector('[name="__RequestVerificationToken"]')?.value || '';
}
