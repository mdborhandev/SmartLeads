/**
 * Global application scripts
 */

$(document).ready(function() {
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
