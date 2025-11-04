// ============================================================
// PROPERTY MAIN MAP - Property Location Display
// ============================================================
// This file handles the property location map shown on the
// property details page (not the comparable modal map).

/**
 * Initializes the property location map on the details page
 * Shows the main property with a red marker
 */
PropertyPage.initializePropertyLocationMap = async function() {
    try {
        // Check if we have coordinates from the server
        if (!PropertyPage.mainProperty.latitude || !PropertyPage.mainProperty.longitude) {
            console.warn('Property coordinates not available');
            // Hide the map card if coordinates are not available
            const mapElement = document.getElementById('property-location-map');
            if (mapElement) {
                const mapCard = mapElement.closest('.card');
                if (mapCard) {
                    mapCard.style.display = 'none';
                }
            }
            return;
        }

        const lat = PropertyPage.mainProperty.latitude;
        const lng = PropertyPage.mainProperty.longitude;
        console.log('Using coordinates for property location map:', lat, lng);

        // Initialize the property location map
        PropertyPage.propertyLocationMap = L.map('property-location-map').setView([lat, lng], 15);

        // Add CartoDB Positron tiles (clean, light map style)
        L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            maxZoom: 19
        }).addTo(PropertyPage.propertyLocationMap);

        // Create custom red icon for main property
        const redIcon = PropertyPage.createRedIcon();

        // Add marker for property
        const marker = L.marker([lat, lng], { icon: redIcon }).addTo(PropertyPage.propertyLocationMap);

        // Create popup content
        const popupContent = `
            <div style="min-width: 250px;">
                <h6 class="fw-bold mb-2 text-danger">📍 ${PropertyPage.mainProperty.address}</h6>
                <div class="mb-2">
                    <strong>City:</strong> ${PropertyPage.mainProperty.city}, ${PropertyPage.mainProperty.state}<br>
                    ${PropertyPage.mainProperty.price ? `<strong>Price:</strong> $${PropertyPage.mainProperty.price.toLocaleString()}<br>` : ''}
                    ${PropertyPage.mainProperty.sqft ? `<strong>Sqft:</strong> ${PropertyPage.mainProperty.sqft.toLocaleString()} sqft<br>` : ''}
                    ${PropertyPage.mainProperty.bedrooms || PropertyPage.mainProperty.bathrooms ? `<strong>Bed/Bath:</strong> ${PropertyPage.mainProperty.bedrooms || '?'} / ${PropertyPage.mainProperty.bathrooms || '?'}` : ''}
                </div>
            </div>
        `;

        marker.bindPopup(popupContent);

        console.log('Property location map initialized successfully');
    } catch (error) {
        console.error('Error initializing property location map:', error);
        // Hide the map card if there's an error
        const mapElement = document.getElementById('property-location-map');
        if (mapElement) {
            const mapCard = mapElement.closest('.card');
            if (mapCard) {
                mapCard.style.display = 'none';
            }
        }
    }
};

// ============================================================
// EVENT LISTENERS
// ============================================================

// Initialize property location map when the page loads
document.addEventListener('DOMContentLoaded', function() {
    PropertyPage.initializePropertyLocationMap();
});