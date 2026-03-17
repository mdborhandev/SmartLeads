/**
 * Validation Service
 * Provides common validation functions for forms and data.
 */

const ValidationService = {
    /**
     * Checks if a string is a valid email address.
     * @param {string} email 
     * @returns {boolean}
     */
    validateEmail: function(email) {
        if (!email) return false;
        const re = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return re.test(String(email).toLowerCase());
    },

    /**
     * Checks if a string is not empty or whitespace.
     * @param {string} value 
     * @returns {boolean}
     */
    isRequired: function(value) {
        return value !== null && value !== undefined && value.trim() !== "";
    },

    /**
     * Checks if a password meets minimum length requirements.
     * @param {string} password 
     * @param {number} minLength 
     * @returns {boolean}
     */
    isValidPassword: function(password, minLength = 6) {
        return password && password.length >= minLength;
    },

    /**
     * Checks if two values match (e.g., password and confirm password).
     * @param {any} value1 
     * @param {any} value2 
     * @returns {boolean}
     */
    isMatch: function(value1, value2) {
        return value1 === value2;
    }
};

// Global function for easier access if requested
function validateEmail(email) {
    return ValidationService.validateEmail(email);
}

// Export if using modules, or attach to window for global access
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ValidationService;
} else {
    window.ValidationService = ValidationService;
}
