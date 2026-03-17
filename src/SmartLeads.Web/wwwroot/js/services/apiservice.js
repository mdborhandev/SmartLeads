/**
 * API Service
 * Standardized wrapper for fetch API to handle backend requests.
 */

const ApiService = {
    /**
     * Generic request handler
     */
    async request(url, options = {}) {
        const defaultHeaders = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };

        options.headers = {
            ...defaultHeaders,
            ...options.headers
        };

        try {
            const response = await fetch(url, options);
            
            // Handle HTTP errors
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw {
                    status: response.status,
                    message: errorData.message || response.statusText,
                    errors: errorData.errors || null
                };
            }

            // Return null for No Content responses
            if (response.status === 204) return null;

            return await response.json();
        } catch (error) {
            console.error(`API Error [${options.method || 'GET'}] ${url}:`, error);
            throw error;
        }
    },

    /**
     * GET request
     */
    async get(url, headers = {}) {
        return this.request(url, { method: 'GET', headers });
    },

    /**
     * POST request
     */
    async post(url, data, headers = {}) {
        return this.request(url, {
            method: 'POST',
            headers,
            body: JSON.stringify(data)
        });
    },

    /**
     * PUT request
     */
    async put(url, data, headers = {}) {
        return this.request(url, {
            method: 'PUT',
            headers,
            body: JSON.stringify(data)
        });
    },

    /**
     * DELETE request
     */
    async delete(url, headers = {}) {
        return this.request(url, { method: 'DELETE', headers });
    }
};

// Export if using modules, or attach to window for global access
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiService;
} else {
    window.ApiService = ApiService;
}
