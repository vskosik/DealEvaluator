// ============================================================
// PROPERTY SHARED - Configuration, State, and Utilities
// ============================================================
// This file contains shared state and utility functions used
// across the property details page.

// Create global namespace to avoid polluting global scope
const PropertyPage = {
    // Configuration from server
    config: window.PropertyPageConfig || {},

    // Derived config values
    propertyId: null,
    zipCode: null,
    mainProperty: null,

    // Shared state
    comparableMap: null,
    propertyLocationMap: null,
    allProperties: [],
    propertyMarkers: []
};

// Initialize derived config values
PropertyPage.propertyId = PropertyPage.config.propertyId;
PropertyPage.zipCode = PropertyPage.config.zipCode;
PropertyPage.mainProperty = PropertyPage.config.mainProperty;

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

/**
 * Displays an error message in the error div
 */
PropertyPage.showError = function(message) {
    const errorDiv = document.getElementById('error-message');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    }
};

/**
 * Encodes a property object to base64 for passing to onclick handlers
 */
PropertyPage.encodeProperty = function(property) {
    return btoa(JSON.stringify(property));
};

/**
 * Parses a full address string into components
 * Example: "9218 Success Ave, Los Angeles, CA 90002"
 */
PropertyPage.parseAddress = function(fullAddress) {
    if (!fullAddress) return { street: '', city: '', state: '', zip: '' };

    const parts = fullAddress.split(',').map(p => p.trim());

    if (parts.length >= 3) {
        // Last part should have "STATE ZIP"
        const lastPart = parts[parts.length - 1];
        const stateZipMatch = lastPart.match(/([A-Z]{2})\s+(\d{5})/);

        return {
            street: parts[0] || '',
            city: parts[1] || '',
            state: stateZipMatch ? stateZipMatch[1] : '',
            zip: stateZipMatch ? stateZipMatch[2] : ''
        };
    }

    // If parsing fails, return the full address as street
    return {
        street: fullAddress,
        city: '',
        state: '',
        zip: ''
    };
};

/**
 * Maps Zillow listing status to our ListingStatuses enum
 * ListingStatuses: Sold (0), Pending (1), Listed (2), OffMarket (3)
 */
PropertyPage.mapListingStatus = function(zillowStatus) {
    if (!zillowStatus) return 0; // Sold (default)

    const status = zillowStatus.toLowerCase();
    if (status.includes('sold')) return 0; // Sold
    if (status.includes('pending')) return 1; // Pending
    if (status.includes('sale') || status.includes('listed')) return 2; // Listed
    return 3; // OffMarket
};

/**
 * Creates a custom red Leaflet icon for the main property marker
 */
PropertyPage.createRedIcon = function() {
    return L.icon({
        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });
};

/**
 * Checks if we should prompt for new evaluation after property edit
 * Looks for ?promptEvaluation=True query parameter
 */
PropertyPage.checkEvaluationPrompt = function() {
    const urlParams = new URLSearchParams(window.location.search);
    const promptEvaluation = urlParams.get('promptEvaluation');

    if (promptEvaluation === 'True') {
        // Show the confirmation modal
        const promptModal = document.getElementById('evaluationPromptModal');
        if (promptModal) {
            const promptModalInstance = new bootstrap.Modal(promptModal);
            promptModalInstance.show();

            // Set up the confirm button handler
            const confirmBtn = document.getElementById('confirmCreateEvaluation');
            if (confirmBtn) {
                confirmBtn.onclick = function() {
                    // Close the prompt modal
                    promptModalInstance.hide();

                    // Open the evaluation modal
                    const evaluationModal = document.getElementById('createEvaluationModal');
                    if (evaluationModal) {
                        const evaluationModalInstance = new bootstrap.Modal(evaluationModal);
                        evaluationModalInstance.show();
                    }
                };
            }
        }

        // Clean up URL by removing the promptEvaluation parameter
        const newUrl = window.location.pathname;
        window.history.replaceState({}, document.title, newUrl);
    }
};

// Make PropertyPage globally accessible
window.PropertyPage = PropertyPage;

// Check for evaluation prompt on page load
document.addEventListener('DOMContentLoaded', function() {
    PropertyPage.checkEvaluationPrompt();
});