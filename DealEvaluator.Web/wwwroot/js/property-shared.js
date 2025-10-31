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
 * Saves coordinates to the database for future use
 */
PropertyPage.saveCoordinatesToDatabase = async function(latitude, longitude) {
    try {
        const response = await fetch('/Property/UpdateCoordinates', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                PropertyId: PropertyPage.propertyId,
                Latitude: latitude,
                Longitude: longitude
            })
        });

        const result = await response.json();
        if (result.success) {
            console.log('Coordinates saved to database successfully');
            // Update the mainProperty object so future loads don't need to geocode
            PropertyPage.mainProperty.latitude = latitude;
            PropertyPage.mainProperty.longitude = longitude;
        } else {
            console.warn('Failed to save coordinates:', result.error);
        }
    } catch (error) {
        console.error('Error saving coordinates to database:', error);
        // Don't fail the map display if saving fails
    }
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
 * Geocodes an address using OpenStreetMap's Nominatim service
 * Returns { lat, lng } or null if geocoding fails
 */
PropertyPage.geocodeAddress = async function(fullAddress) {
    try {
        const geocodeUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(fullAddress)}`;
        const response = await fetch(geocodeUrl, {
            headers: {
                'User-Agent': 'DealEvaluator/1.0' // Nominatim requires a user agent
            }
        });

        const results = await response.json();
        if (results && results.length > 0) {
            return {
                lat: parseFloat(results[0].lat),
                lng: parseFloat(results[0].lon)
            };
        }
        return null;
    } catch (error) {
        console.error('Error geocoding address:', error);
        return null;
    }
};

// Make PropertyPage globally accessible
window.PropertyPage = PropertyPage;