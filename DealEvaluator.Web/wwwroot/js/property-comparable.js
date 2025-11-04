// ============================================================
// PROPERTY COMPARABLE - Comparable Modal Functionality
// ============================================================
// This file handles the "Add Comparable" modal including:
// - Loading market data from the API
// - Displaying properties on a map
// - Filtering properties
// - Adding properties as comparables

// ============================================================
// DATA LOADING
// ============================================================

/**
 * Loads market data for the property's zip code from the API
 */
PropertyPage.loadMarketData = async function() {
    // Show loading, hide error and map
    const loadingIndicator = document.getElementById('loading-indicator');
    const mapContainer = document.getElementById('map-container');
    const errorMessage = document.getElementById('error-message');

    if (loadingIndicator) loadingIndicator.style.display = 'block';
    if (mapContainer) mapContainer.style.display = 'none';
    if (errorMessage) errorMessage.style.display = 'none';

    try {
        const response = await fetch(`/Property/GetMarketData?zipCode=${PropertyPage.zipCode}`);
        const data = await response.json();

        if (data.success && data.properties && data.properties.length > 0) {
            await PropertyPage.initializeComparableMap(data.properties);
        } else {
            PropertyPage.showError('No market data found for zip code ' + PropertyPage.zipCode);
        }
    } catch (error) {
        console.error('Error fetching market data:', error);
        PropertyPage.showError('Failed to load market data. Please try again.');
    } finally {
        if (loadingIndicator) loadingIndicator.style.display = 'none';
    }
};

// ============================================================
// MAP INITIALIZATION
// ============================================================

/**
 * Initializes the comparable map with market data properties
 */
PropertyPage.initializeComparableMap = async function(properties) {
    // Show map container
    const mapContainer = document.getElementById('map-container');
    if (mapContainer) mapContainer.style.display = 'block';

    // Filter out the main property (subject property) from comparables
    const mainAddress = PropertyPage.mainProperty.address.toLowerCase().trim();
    const filteredProperties = properties.filter(p => {
        if (!p.address) return true; // Keep properties without address
        const compAddress = p.address.toLowerCase().trim();
        // Check if addresses match (both exact and partial matches like "123 Main St" vs "123 Main Street")
        return !compAddress.includes(mainAddress) && !mainAddress.includes(compAddress);
    });

    console.log(`Filtered out main property. Total properties: ${properties.length}, After filtering: ${filteredProperties.length}`);

    // Store filtered properties for filtering
    PropertyPage.allProperties = filteredProperties;

    // Calculate center point from properties with coordinates
    const propsWithCoords = filteredProperties.filter(p => p.latitude && p.longitude);

    if (propsWithCoords.length === 0) {
        PropertyPage.showError('No properties with valid coordinates found.');
        return;
    }

    const avgLat = propsWithCoords.reduce((sum, p) => sum + p.latitude, 0) / propsWithCoords.length;
    const avgLng = propsWithCoords.reduce((sum, p) => sum + p.longitude, 0) / propsWithCoords.length;

    // Initialize map
    PropertyPage.comparableMap = L.map('comparable-map').setView([avgLat, avgLng], 15);

    // Add CartoDB Positron tiles
    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
        maxZoom: 19
    }).addTo(PropertyPage.comparableMap);

    // Add the main property marker (red pin)
    await PropertyPage.addMainPropertyMarkerToComparableMap();

    // Initialize total and filtered counts
    const totalCountEl = document.getElementById('total-count');
    const filteredCountEl = document.getElementById('filtered-count');
    if (totalCountEl) totalCountEl.textContent = PropertyPage.allProperties.length;
    if (filteredCountEl) filteredCountEl.textContent = propsWithCoords.length;

    // Show filter panel
    const filterPanel = document.getElementById('filter-panel');
    if (filterPanel) filterPanel.style.display = 'block';

    // Add markers using the filtering system (this will apply default filters)
    PropertyPage.applyFilters();

    console.log(`Comparable map initialized with ${propsWithCoords.length} properties`);
};

/**
 * Adds the main property marker to the comparable map
 */
PropertyPage.addMainPropertyMarkerToComparableMap = async function() {
    try {
        let lat, lng;

        // Check if we already have coordinates stored
        if (PropertyPage.mainProperty.latitude && PropertyPage.mainProperty.longitude) {
            lat = PropertyPage.mainProperty.latitude;
            lng = PropertyPage.mainProperty.longitude;
            console.log('Using stored coordinates for main property on comparable map');
        } else {
            // Geocode the address
            const fullAddress = `${PropertyPage.mainProperty.address}, ${PropertyPage.mainProperty.city}, ${PropertyPage.mainProperty.state}`;
            console.log('Geocoding main property address for comparable map:', fullAddress);

            const coords = await PropertyPage.geocodeAddress(fullAddress);
            if (coords) {
                lat = coords.lat;
                lng = coords.lng;
                console.log('Geocoded main property to:', lat, lng);

                // Save coordinates to database for future use
                await PropertyPage.saveCoordinatesToDatabase(lat, lng);
            } else {
                console.warn('Could not geocode main property address');
                return; // Skip adding marker if geocoding fails
            }
        }

        // Create custom red icon for main property
        const redIcon = PropertyPage.createRedIcon();

        // Add marker for main property
        const marker = L.marker([lat, lng], { icon: redIcon }).addTo(PropertyPage.comparableMap);

        // Create popup content for main property
        const popupContent = `
            <div style="min-width: 250px;">
                <h6 class="fw-bold mb-2 text-danger">📍 YOUR PROPERTY</h6>
                <div class="mb-2">
                    <strong>Address:</strong> ${PropertyPage.mainProperty.address}<br>
                    <strong>City:</strong> ${PropertyPage.mainProperty.city}, ${PropertyPage.mainProperty.state}<br>
                    ${PropertyPage.mainProperty.price ? `<strong>Price:</strong> $${PropertyPage.mainProperty.price.toLocaleString()}<br>` : ''}
                    ${PropertyPage.mainProperty.sqft ? `<strong>Sqft:</strong> ${PropertyPage.mainProperty.sqft.toLocaleString()} sqft<br>` : ''}
                    ${PropertyPage.mainProperty.bedrooms || PropertyPage.mainProperty.bathrooms ? `<strong>Bed/Bath:</strong> ${PropertyPage.mainProperty.bedrooms || '?'} / ${PropertyPage.mainProperty.bathrooms || '?'}` : ''}
                </div>
            </div>
        `;

        marker.bindPopup(popupContent);

        // Center the map on the main property
        PropertyPage.comparableMap.setView([lat, lng], 15);

        console.log('Main property marker added to comparable map successfully');
    } catch (error) {
        console.error('Error adding main property marker to comparable map:', error);
        // Don't fail the whole map if we can't add the main property
    }
};

// ============================================================
// FILTER FUNCTIONS
// ============================================================

/**
 * Toggles the visibility of the filter controls
 */
PropertyPage.toggleFilters = function() {
    const filterControls = document.getElementById('filter-controls');
    const toggleText = document.getElementById('filter-toggle-text');

    if (!filterControls || !toggleText) return;

    const isCollapsed = !filterControls.classList.contains('show');

    if (isCollapsed) {
        filterControls.classList.add('show');
        toggleText.textContent = 'Hide Filters';
    } else {
        filterControls.classList.remove('show');
        toggleText.textContent = 'Show Filters';
    }
};

/**
 * Resets all filters to their default values
 */
PropertyPage.resetFilters = function() {
    // Reset all filter inputs
    const filterIds = [
        'filter-price-min', 'filter-price-max',
        'filter-sqft-min', 'filter-sqft-max',
        'filter-bedrooms-min', 'filter-bedrooms-max',
        'filter-bathrooms-min', 'filter-bathrooms-max',
        'filter-days-max', 'filter-zestimate-min',
        'filter-date-sold-from', 'filter-date-sold-to'
    ];

    filterIds.forEach(id => {
        const element = document.getElementById(id);
        if (element) element.value = '';
    });

    // Uncheck all property type checkboxes
    const propertyTypeIds = [
        'filter-type-single-family',
        'filter-type-condo',
        'filter-type-townhouse',
        'filter-type-multi-family'
    ];

    propertyTypeIds.forEach(id => {
        const element = document.getElementById(id);
        if (element) element.checked = false;
    });

    // Reset listing status checkboxes (only Sold checked by default)
    const statusIds = {
        'filter-status-sold': true,
        'filter-status-for-sale': false,
        'filter-status-pending': false,
        'filter-status-off-market': false
    };

    Object.entries(statusIds).forEach(([id, checked]) => {
        const element = document.getElementById(id);
        if (element) element.checked = checked;
    });

    // Apply filters to show properties with default filters
    PropertyPage.applyFilters();
};

/**
 * Applies all active filters to the property list and updates the map
 */
PropertyPage.applyFilters = function() {
    if (!PropertyPage.comparableMap || PropertyPage.allProperties.length === 0) {
        return;
    }

    // Show filter panel if not already visible
    const filterPanel = document.getElementById('filter-panel');
    if (filterPanel) filterPanel.style.display = 'block';

    // Get filter values
    const filters = PropertyPage.getFilterValues();

    // Filter properties
    const filteredProperties = PropertyPage.allProperties.filter(property => {
        // Price filter
        if (filters.priceMin && (!property.price || property.price < filters.priceMin)) return false;
        if (filters.priceMax && (!property.price || property.price > filters.priceMax)) return false;

        // Sqft filter
        if (filters.sqftMin && (!property.livingArea || property.livingArea < filters.sqftMin)) return false;
        if (filters.sqftMax && (!property.livingArea || property.livingArea > filters.sqftMax)) return false;

        // Bedrooms filter
        if (filters.bedroomsMin && (!property.bedrooms || property.bedrooms < filters.bedroomsMin)) return false;
        if (filters.bedroomsMax && (!property.bedrooms || property.bedrooms > filters.bedroomsMax)) return false;

        // Bathrooms filter
        if (filters.bathroomsMin && (!property.bathrooms || property.bathrooms < filters.bathroomsMin)) return false;
        if (filters.bathroomsMax && (!property.bathrooms || property.bathrooms > filters.bathroomsMax)) return false;

        // Days on Zillow filter
        if (filters.daysMax && (!property.daysOnZillow || property.daysOnZillow > filters.daysMax)) return false;

        // Zestimate filter
        if (filters.zestimateMin && (!property.zestimate || property.zestimate < filters.zestimateMin)) return false;

        // Property Type filter
        if (filters.propertyTypes.length > 0) {
            if (!property.propertyType || !filters.propertyTypes.includes(property.propertyType)) return false;
        }

        // Listing Status filter
        if (filters.listingStatuses.length > 0) {
            if (!property.listingStatus || !filters.listingStatuses.includes(property.listingStatus)) return false;
        }

        // Date Sold filter
        if (filters.dateSoldFrom || filters.dateSoldTo) {
            if (!property.dateSold) return false;

            const soldDate = new Date(property.dateSold);
            if (filters.dateSoldFrom) {
                const fromDate = new Date(filters.dateSoldFrom);
                if (soldDate < fromDate) return false;
            }
            if (filters.dateSoldTo) {
                const toDate = new Date(filters.dateSoldTo);
                toDate.setHours(23, 59, 59, 999); // End of day
                if (soldDate > toDate) return false;
            }
        }

        return true;
    });

    // Update map markers
    PropertyPage.updateMapMarkers(filteredProperties);

    // Update counts
    const filteredCountEl = document.getElementById('filtered-count');
    const totalCountEl = document.getElementById('total-count');
    if (filteredCountEl) filteredCountEl.textContent = filteredProperties.length;
    if (totalCountEl) totalCountEl.textContent = PropertyPage.allProperties.length;

    // Update active filter count badge
    const activeFilterCount = PropertyPage.countActiveFilters(filters);
    const badge = document.getElementById('active-filter-count');
    if (badge) {
        if (activeFilterCount > 0) {
            badge.textContent = activeFilterCount;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }

    console.log(`Filtered ${filteredProperties.length} properties out of ${PropertyPage.allProperties.length}`);
};

/**
 * Gets all current filter values from the UI
 */
PropertyPage.getFilterValues = function() {
    const getValue = (id) => {
        const el = document.getElementById(id);
        return el ? el.value : '';
    };

    const isChecked = (id) => {
        const el = document.getElementById(id);
        return el ? el.checked : false;
    };

    const filters = {
        priceMin: parseFloat(getValue('filter-price-min')) || null,
        priceMax: parseFloat(getValue('filter-price-max')) || null,
        sqftMin: parseFloat(getValue('filter-sqft-min')) || null,
        sqftMax: parseFloat(getValue('filter-sqft-max')) || null,
        bedroomsMin: parseInt(getValue('filter-bedrooms-min')) || null,
        bedroomsMax: parseInt(getValue('filter-bedrooms-max')) || null,
        bathroomsMin: parseFloat(getValue('filter-bathrooms-min')) || null,
        bathroomsMax: parseFloat(getValue('filter-bathrooms-max')) || null,
        daysMax: parseInt(getValue('filter-days-max')) || null,
        zestimateMin: parseFloat(getValue('filter-zestimate-min')) || null,
        dateSoldFrom: getValue('filter-date-sold-from') || null,
        dateSoldTo: getValue('filter-date-sold-to') || null,
        propertyTypes: [],
        listingStatuses: []
    };

    // Get selected property types
    if (isChecked('filter-type-single-family')) filters.propertyTypes.push('SINGLE_FAMILY');
    if (isChecked('filter-type-condo')) filters.propertyTypes.push('CONDO');
    if (isChecked('filter-type-townhouse')) filters.propertyTypes.push('TOWNHOUSE');
    if (isChecked('filter-type-multi-family')) filters.propertyTypes.push('MULTI_FAMILY');

    // Get selected listing statuses
    if (isChecked('filter-status-sold')) filters.listingStatuses.push('SOLD');
    if (isChecked('filter-status-for-sale')) filters.listingStatuses.push('FOR_SALE');
    if (isChecked('filter-status-pending')) filters.listingStatuses.push('PENDING');
    if (isChecked('filter-status-off-market')) filters.listingStatuses.push('OFF_MARKET');

    return filters;
};

/**
 * Counts how many filters are currently active
 */
PropertyPage.countActiveFilters = function(filters) {
    let count = 0;

    if (filters.priceMin) count++;
    if (filters.priceMax) count++;
    if (filters.sqftMin) count++;
    if (filters.sqftMax) count++;
    if (filters.bedroomsMin) count++;
    if (filters.bedroomsMax) count++;
    if (filters.bathroomsMin) count++;
    if (filters.bathroomsMax) count++;
    if (filters.daysMax) count++;
    if (filters.zestimateMin) count++;
    if (filters.dateSoldFrom) count++;
    if (filters.dateSoldTo) count++;
    if (filters.propertyTypes.length > 0) count++;
    // Only count status filter if it's not the default (all statuses or just Sold)
    if (filters.listingStatuses.length > 0 && filters.listingStatuses.length < 4) count++;

    return count;
};

/**
 * Updates map markers to show only filtered properties
 */
PropertyPage.updateMapMarkers = function(filteredProperties) {
    // Remove all existing property markers (but keep main property marker)
    PropertyPage.propertyMarkers.forEach(marker => {
        PropertyPage.comparableMap.removeLayer(marker);
    });
    PropertyPage.propertyMarkers = [];

    // Add markers for filtered properties
    filteredProperties.forEach(property => {
        if (property.latitude && property.longitude) {
            const marker = L.marker([property.latitude, property.longitude])
                .addTo(PropertyPage.comparableMap);

            // Create popup content with property details and Add button
            const popupContent = `
                <div style="min-width: 250px;">
                    <h6 class="fw-bold mb-2">${property.address || 'Unknown Address'}</h6>
                    <div class="mb-2">
                        <strong>Price:</strong> ${property.price ? '$' + property.price.toLocaleString() : 'N/A'}<br>
                        <strong>Sqft:</strong> ${property.livingArea ? property.livingArea.toLocaleString() + ' sqft' : 'N/A'}<br>
                        <strong>Bed/Bath:</strong> ${property.bedrooms || '?'} / ${property.bathrooms || '?'}<br>
                        ${property.propertyType ? `<strong>Type:</strong> ${property.propertyType}<br>` : ''}
                        ${property.listingStatus ? `<strong>Status:</strong> ${property.listingStatus}<br>` : ''}
                        <strong>Sold On:</strong> ${property.dateSold || "N/A"}
                    </div>
                    <button class="btn btn-sm btn-primary w-100" onclick="PropertyPage.addAsComparable('${PropertyPage.encodeProperty(property)}')">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" class="bi bi-plus-circle me-1" viewBox="0 0 16 16">
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
                            <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                        </svg>
                        Add as Comparable
                    </button>
                </div>
            `;

            marker.bindPopup(popupContent);
            PropertyPage.propertyMarkers.push(marker);
        }
    });

    console.log(`Added ${PropertyPage.propertyMarkers.length} markers to map`);
};

// ============================================================
// ADD COMPARABLE
// ============================================================

/**
 * Adds a property as a comparable to the database
 */
PropertyPage.addAsComparable = async function(encodedProperty) {
    const property = JSON.parse(atob(encodedProperty));
    console.log('Adding property as comparable:', property);

    // Convert Unix timestamp to ISO date string if present
    let saleDate = null;
    if (property.dateSold) {
        const date = new Date(property.dateSold);
        saleDate = date.toISOString();
    }

    // Parse address if needed (Zillow gives full address as single string)
    const addressParts = PropertyPage.parseAddress(property.address);

    // Map ZillowProperty to CreateComparableDto
    const dto = {
        PropertyId: PropertyPage.propertyId,
        SaleDate: saleDate,
        ListingStatus: PropertyPage.mapListingStatus(property.listingStatus),
        Source: 'Zillow',
        Address: addressParts.street || property.address || '',
        City: addressParts.city || '',
        State: addressParts.state || '',
        ZipCode: addressParts.zip || '',
        Price: property.price,
        Sqft: property.livingArea,
        Bedrooms: property.bedrooms,
        Bathrooms: property.bathrooms ? Math.floor(property.bathrooms) : null,
        LotSizeSqft: null,
        YearBuilt: null
    };

    console.log('DTO to send:', dto);

    try {
        const response = await fetch('/Property/AddComparable', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dto)
        });

        const result = await response.json();

        if (result.success) {
            // Close the modal
            const modalElement = document.getElementById('addComparableModal');
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            // Reload the page to show the new comparable
            location.reload();
        } else {
            alert('Error adding comparable: ' + (result.error || 'Unknown error'));
        }
    } catch (error) {
        console.error('Error adding comparable:', error);
        alert('Failed to add comparable. Please try again.');
    }
};

// ============================================================
// EVENT LISTENERS
// ============================================================

// Initialize modal and map when modal is shown
const addComparableModal = document.getElementById('addComparableModal');
if (addComparableModal) {
    addComparableModal.addEventListener('shown.bs.modal', function() {
        PropertyPage.loadMarketData();
    });

    // Clean up map when modal is hidden
    addComparableModal.addEventListener('hidden.bs.modal', function() {
        if (PropertyPage.comparableMap) {
            PropertyPage.comparableMap.remove();
            PropertyPage.comparableMap = null;
            PropertyPage.allProperties = [];
            PropertyPage.propertyMarkers = [];
        }
    });
}

// ============================================================
// GLOBAL FUNCTION ALIASES
// ============================================================
// These are needed for onclick handlers in the HTML

function toggleFilters() {
    PropertyPage.toggleFilters();
}

function resetFilters() {
    PropertyPage.resetFilters();
}

function applyFilters() {
    PropertyPage.applyFilters();
}