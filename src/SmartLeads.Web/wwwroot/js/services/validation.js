// Regular expression for validating email
var emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

// Function to validate email format
function validateEmail(emailValue) {
    return emailPattern.test(emailValue);
}

