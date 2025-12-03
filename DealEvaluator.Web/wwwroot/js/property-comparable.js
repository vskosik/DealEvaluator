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
    // Show loading overlay
    PropertyPage.showLoadingOverlay(`Loading market data for zip code ${PropertyPage.zipCode}...`);

    const mapContainer = document.getElementById('map-container');
    const errorMessage = document.getElementById('error-message');

    if (mapContainer) mapContainer.style.display = 'none';
    if (errorMessage) errorMessage.style.display = 'none';

    try {
        const response = await fetch(`/Property/GetMarketData?propertyId=${PropertyPage.propertyId}`);
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
        PropertyPage.hideLoadingOverlay();
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

    // Initialize marker cluster group for property markers
    PropertyPage.markerClusterGroup = L.markerClusterGroup({
        maxClusterRadius: 60,
        spiderfyOnMaxZoom: true,
        showCoverageOnHover: false,
        zoomToBoundsOnClick: true,
        iconCreateFunction: function(cluster) {
            const count = cluster.getChildCount();
            let size = 'small';
            if (count >= 100) size = 'large';
            else if (count >= 10) size = 'medium';

            return L.divIcon({
                html: '<div><span>' + count + '</span></div>',
                className: 'marker-cluster marker-cluster-' + size,
                iconSize: L.point(40, 40)
            });
        }
    });
    PropertyPage.comparableMap.addLayer(PropertyPage.markerClusterGroup);

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

    // Initialize zip code boundaries feature
    await PropertyPage.initializeZipCodeBoundaries();

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

    // Reset listing status checkboxes
    const statusIds = {
        'filter-status-sold': false,
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
 * Formats a date from Unix timestamp
 */
PropertyPage.formatDate = function(timestamp) {
    if (!timestamp) return 'N/A';
    const date = new Date(timestamp);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
};

/**
 * Creates custom marker icon with price label
 */
PropertyPage.createCustomMarkerIcon = function(property) {
    const price = property.price ? `$${(property.price / 1000).toFixed(0)}k` : 'N/A';
    const beds = property.bedrooms || '?';
    const baths = property.bathrooms || '?';

    return L.divIcon({
        className: 'custom-marker-icon',
        html: `
            <div style="position: relative;">
                <div style="
                    position: absolute;
                    bottom: 35px;
                    left: 50%;
                    transform: translateX(-50%);
                    background: white;
                    border: 2px solid #0066cc;
                    border-radius: 4px;
                    padding: 2px 6px;
                    font-size: 11px;
                    font-weight: bold;
                    white-space: nowrap;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.3);
                    color: #333;
                ">
                    ${price} · ${beds}bd ${baths}ba
                </div>
                <svg width="30" height="40" viewBox="0 0 30 40" style="filter: drop-shadow(0 2px 4px rgba(0,0,0,0.3));">
                    <path d="M15 0C6.7 0 0 6.7 0 15c0 8.3 15 25 15 25s15-16.7 15-25c0-8.3-6.7-15-15-15z" fill="#0066cc"/>
                    <circle cx="15" cy="15" r="6" fill="white"/>
                </svg>
            </div>
        `,
        iconSize: [30, 40],
        iconAnchor: [15, 40],
        popupAnchor: [0, -40]
    });
};

/**
 * Updates map markers to show only filtered properties
 */
PropertyPage.updateMapMarkers = function(filteredProperties) {
    // Clear all markers from the cluster group
    if (PropertyPage.markerClusterGroup) {
        PropertyPage.markerClusterGroup.clearLayers();
    }

    // Reset the property markers array
    PropertyPage.propertyMarkers = [];

    // Add markers for filtered properties to the cluster group
    filteredProperties.forEach(property => {
        if (property.latitude && property.longitude) {
            const customIcon = PropertyPage.createCustomMarkerIcon(property);
            const marker = L.marker([property.latitude, property.longitude], { icon: customIcon });

            // Format the sold date
            const formattedDate = PropertyPage.formatDate(property.dateSold);

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
                        <strong>Sold On:</strong> ${formattedDate}<br>
                        ${property.detailUrl ? `<a href="https://zillow.com${property.detailUrl}" target="_blank" class="btn btn-sm btn-link p-0 mt-1">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" class="bi bi-box-arrow-up-right me-1" viewBox="0 0 16 16">
                                <path fill-rule="evenodd" d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5"/>
                                <path fill-rule="evenodd" d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0z"/>
                            </svg>
                            View on Zillow
                        </a><br>` : ''}
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

            // Add marker to the cluster group instead of directly to the map
            PropertyPage.markerClusterGroup.addLayer(marker);
            PropertyPage.propertyMarkers.push(marker);
        }
    });

    console.log(`Added ${PropertyPage.propertyMarkers.length} markers to cluster group`);
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
        ListingUrl: property.detailUrl || null,
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
            // Show success notification
            toastr.success(`Successfully added ${property.address || 'property'} as a comparable!`, 'Comparable Added');

            // Refresh the comparables table on the main page
            await PropertyPage.refreshComparablesTable();
        } else {
            toastr.error(result.error || 'Unknown error', 'Failed to Add Comparable');
        }
    } catch (error) {
        console.error('Error adding comparable:', error);
        toastr.error('Failed to add comparable. Please try again.', 'Error');
    }
};

/**
 * Refreshes the comparables table by fetching the updated partial view from the server
 */
PropertyPage.refreshComparablesTable = async function() {
    try {
        console.log('Refreshing comparables table...');

        const response = await fetch(`/Property/GetComparablesTable?propertyId=${PropertyPage.propertyId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();

        // Update the table container
        const container = document.querySelector('#comparables-table-container');
        if (container) {
            container.innerHTML = html;
            console.log('Comparables table updated successfully');
        }

        // Update the count badge by counting rows in the new table
        const tableRows = document.querySelectorAll('#comparables-table-container tbody tr');
        const count = tableRows.length;

        const countBadge = document.querySelector('#comparables-count-badge');
        if (countBadge) {
            countBadge.textContent = count;
        }

        console.log(`Comparables table refreshed: ${count} comparables`);
    } catch (error) {
        console.error('Error refreshing comparables table:', error);
        toastr.error('Failed to refresh comparables table', 'Error');
    }
};

// ============================================================
// EVENT LISTENERS
// ============================================================

// Initialize modal and map when modal is shown
const addComparableModal = document.getElementById('addComparableModal');
if (addComparableModal) {
    addComparableModal.addEventListener('shown.bs.modal', function() {
        // Only load data if not already loaded
        if (!PropertyPage.comparableMap) {
            PropertyPage.loadMarketData();
        } else {
            // Map already exists, just make sure it's visible and properly sized
            const mapContainer = document.getElementById('map-container');
            if (mapContainer) {
                mapContainer.style.display = 'block';
            }
            // Invalidate size to handle any container size changes
            setTimeout(() => {
                if (PropertyPage.comparableMap) {
                    PropertyPage.comparableMap.invalidateSize();
                }
            }, 100);
        }
    });
}

// ============================================================
// ZIP CODE BOUNDARY FEATURES
// ============================================================

// Initialize state for zip code boundaries
PropertyPage.zipCodeBoundaries = {
    all: [],              // All boundaries within 50 miles
    neighbors: [],        // Immediate neighbor boundaries
    loaded: [],           // Zip codes that have been loaded
    layer: null,          // Leaflet GeoJSON layer
    currentZipBoundary: null,  // Boundary of the current property's zip code
    layers: {}            // Map of zipCode -> Leaflet layer for managing tooltips
};

/**
 * Fetches zip code boundaries within specified radius via backend API
 */
PropertyPage.fetchZipCodeBoundaries = async function(centerLat, centerLng, radiusMeters) {
    try {
        console.log(`Fetching zip code boundaries within ${radiusMeters}m of ${centerLat},${centerLng}`);

        // Call our backend API endpoint (avoids CORS issues)
        const response = await fetch(`/Property/GetZipCodeBoundaries?lat=${centerLat}&lng=${centerLng}&radiusMeters=${radiusMeters}`);

        if (!response.ok) {
            throw new Error(`Backend API error: ${response.statusText}`);
        }

        const result = await response.json();

        if (!result.success) {
            throw new Error(result.error || 'Unknown error from backend');
        }

        const data = result.data;

        // Data is now a GeoJSON FeatureCollection from local files
        if (!data || !data.features) {
            console.warn('No features in response');
            return [];
        }

        const features = data.features;
        console.log(`Fetched ${features.length} zip code boundaries from local files`);

        // Log zip codes for debugging
        const returnedZips = features.map(f => f.properties?.ZCTA5CE10 || f.properties?.zipCode || f.properties?.zip).filter(z => z);
        console.log('Zip codes returned:', returnedZips);

        // Normalize the properties to ensure zipCode field exists
        const normalizedFeatures = features.map(f => {
            const zipCode = f.properties?.ZCTA5CE10 || f.properties?.zipCode || f.properties?.zip || f.properties?.GEOID10;
            return {
                ...f,
                properties: {
                    ...f.properties,
                    zipCode: zipCode
                }
            };
        });

        console.log('Normalized zip codes:', normalizedFeatures.map(f => f.properties.zipCode));
        return normalizedFeatures;
    } catch (error) {
        console.error('Error fetching zip code boundaries:', error);
        return [];
    }
};

/**
 * Converts Overpass API element to GeoJSON Feature
 */
PropertyPage.convertOverpassToGeoJSON = function(element) {
    try {
        const zipCode = element.tags.postal_code;

        // Build coordinates from members
        const coordinates = [];
        const outerWays = element.members.filter(m => m.role === 'outer' || m.role === '');

        for (const member of outerWays) {
            if (member.geometry && member.geometry.length > 0) {
                const ring = member.geometry.map(node => [node.lon, node.lat]);
                coordinates.push(ring);
            }
        }

        if (coordinates.length === 0) {
            return null;
        }

        return {
            type: 'Feature',
            properties: {
                zipCode: zipCode,
                name: element.tags.name || zipCode
            },
            geometry: {
                type: coordinates.length === 1 ? 'Polygon' : 'MultiPolygon',
                coordinates: coordinates.length === 1 ? coordinates[0] : [coordinates]
            }
        };
    } catch (error) {
        console.error('Error converting Overpass element:', error);
        return null;
    }
};

/**
 * Identifies immediate neighbor zip codes (those that share a boundary)
 */
PropertyPage.identifyNeighborZipCodes = function(currentZipBoundary, allBoundaries) {
    if (!currentZipBoundary || !allBoundaries || allBoundaries.length === 0) {
        console.warn('Cannot identify neighbors: missing boundary data');
        return [];
    }

    console.log(`Identifying neighbors for zip ${currentZipBoundary.properties.zipCode} from ${allBoundaries.length} boundaries`);

    const neighbors = [];
    const currentZipCode = currentZipBoundary.properties.zipCode;

    for (const boundary of allBoundaries) {
        // Skip the current zip code itself
        if (boundary.properties.zipCode === currentZipCode) {
            continue;
        }

        try {
            // Use Turf.js to check if polygons touch or intersect
            const touches = turf.booleanTouches(currentZipBoundary, boundary);
            const overlaps = turf.booleanOverlap(currentZipBoundary, boundary);

            if (touches || overlaps) {
                neighbors.push(boundary);
                console.log(`Found neighbor: ${boundary.properties.zipCode}`);
            }
        } catch (error) {
            console.warn(`Error checking boundary for ${boundary.properties.zipCode}:`, error);
        }
    }

    console.log(`Found ${neighbors.length} immediate neighbors`);
    return neighbors;
};

/**
 * Displays zip code boundaries on the map
 */
PropertyPage.displayZipCodeBoundaries = function(boundaries) {
    if (!PropertyPage.comparableMap || !boundaries || boundaries.length === 0) {
        console.warn('Cannot display boundaries: missing map or boundaries');
        return;
    }

    console.log(`Displaying ${boundaries.length} zip code boundaries`);

    // Remove existing boundary layer if present
    if (PropertyPage.zipCodeBoundaries.layer) {
        PropertyPage.comparableMap.removeLayer(PropertyPage.zipCodeBoundaries.layer);
    }

    // Create GeoJSON layer
    PropertyPage.zipCodeBoundaries.layer = L.geoJSON(boundaries, {
        style: function(feature) {
            return {
                color: '#0066cc',
                weight: 2,
                opacity: 0.7,
                fillColor: '#0066cc',
                fillOpacity: 0.05,
                className: 'zip-boundary'
            };
        },
        onEachFeature: function(feature, layer) {
            const zipCode = feature.properties.zipCode;

            // Store reference to layer for later manipulation
            PropertyPage.zipCodeBoundaries.layers[zipCode] = layer;

            // Add permanent "Click to Load" label
            layer.bindTooltip(`
                <div style="
                    background: #0066cc;
                    color: white;
                    padding: 6px 12px;
                    border-radius: 4px;
                    font-size: 12px;
                    font-weight: bold;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                    cursor: pointer;
                    white-space: nowrap;
                ">
                    Click to Load ${zipCode}
                </div>
            `, {
                permanent: true,
                direction: 'center',
                className: 'zip-boundary-label',
                opacity: 0.95
            });

            // Add hover effects
            layer.on('mouseover', function(e) {
                const layer = e.target;
                layer.setStyle({
                    weight: 3,
                    fillOpacity: 0.2,
                    cursor: 'pointer'
                });
            });

            layer.on('mouseout', function(e) {
                const layer = e.target;
                layer.setStyle({
                    weight: 2,
                    fillOpacity: 0.05
                });
            });

            // Add click handler
            layer.on('click', function(e) {
                L.DomEvent.stopPropagation(e);
                PropertyPage.loadComparablesForZipCode(zipCode);
            });
        }
    }).addTo(PropertyPage.comparableMap);

    console.log('Zip code boundaries displayed successfully');
};

/**
 * Loads comparables for a specific zip code when boundary is clicked
 */
PropertyPage.loadComparablesForZipCode = async function(zipCode) {
    // Check if already loaded
    if (PropertyPage.zipCodeBoundaries.loaded.includes(zipCode)) {
        console.log(`Zip code ${zipCode} already loaded, skipping`);
        return;
    }

    console.log(`Loading comparables for zip code: ${zipCode}`);

    // Show loading indicator
    PropertyPage.showZipCodeLoadingMessage(zipCode);

    try {
        const response = await fetch(`/Property/GetMarketData?propertyId=${PropertyPage.propertyId}&zipCode=${zipCode}`);
        const data = await response.json();

        if (data.success && data.properties && data.properties.length > 0) {
            console.log(`Loaded ${data.properties.length} properties for zip ${zipCode}`);

            // Filter out the main property
            const mainAddress = PropertyPage.mainProperty.address.toLowerCase().trim();
            const filteredProperties = data.properties.filter(p => {
                if (!p.address) return true;
                const compAddress = p.address.toLowerCase().trim();
                return !compAddress.includes(mainAddress) && !mainAddress.includes(compAddress);
            });

            // Add to allProperties array
            PropertyPage.allProperties = PropertyPage.allProperties.concat(filteredProperties);

            // Mark as loaded
            PropertyPage.zipCodeBoundaries.loaded.push(zipCode);

            // Remove the "Click to Load" label from the boundary
            const layer = PropertyPage.zipCodeBoundaries.layers[zipCode];
            if (layer) {
                layer.unbindTooltip();
                console.log(`Removed label for zip ${zipCode}`);
            }

            // Re-apply filters to show new properties
            PropertyPage.applyFilters();

            // Update counts
            const totalCountEl = document.getElementById('total-count');
            if (totalCountEl) totalCountEl.textContent = PropertyPage.allProperties.length;

            console.log(`Total properties now: ${PropertyPage.allProperties.length}`);
        } else {
            console.warn(`No properties found for zip code ${zipCode}`);
        }
    } catch (error) {
        console.error(`Error loading comparables for zip ${zipCode}:`, error);
        PropertyPage.showError(`Failed to load comparables for zip code ${zipCode}`);
    } finally {
        PropertyPage.hideZipCodeLoadingMessage();
    }
};

/**
 * Shows loading message for zip code
 */
PropertyPage.showZipCodeLoadingMessage = function(zipCode) {
    const loadingIndicator = document.getElementById('loading-indicator');
    if (loadingIndicator) {
        loadingIndicator.innerHTML = `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3">Loading data for zip code ${zipCode}...</p>
        `;
        loadingIndicator.style.display = 'block';
    }
};

/**
 * Hides loading message
 */
PropertyPage.hideZipCodeLoadingMessage = function() {
    const loadingIndicator = document.getElementById('loading-indicator');
    if (loadingIndicator) {
        loadingIndicator.style.display = 'none';
    }
};

/**
 * Geocodes a zip code to get its center coordinates via backend API
 */
PropertyPage.geocodeZipCode = async function(zipCode) {
    try {
        console.log(`Geocoding zip code: ${zipCode}`);

        // Call our backend API endpoint (avoids CORS issues)
        const response = await fetch(`/Property/GeocodeZipCode?zipCode=${zipCode}`);

        if (!response.ok) {
            throw new Error(`Backend API error: ${response.statusText}`);
        }

        const result = await response.json();

        if (!result.success) {
            throw new Error(result.error || 'Unknown error from backend');
        }

        const data = result.data;

        if (data && data.length > 0) {
            const coords = {
                lat: parseFloat(data[0].lat),
                lng: parseFloat(data[0].lon)
            };
            console.log(`Geocoded zip ${zipCode} to:`, coords);
            return coords;
        } else {
            console.warn(`No results found for zip code ${zipCode}`);
            return null;
        }
    } catch (error) {
        console.error(`Error geocoding zip code ${zipCode}:`, error);
        return null;
    }
};

/**
 * Initializes zip code boundaries feature after map is loaded
 */
PropertyPage.initializeZipCodeBoundaries = async function() {
    try {
        console.log('Initializing zip code boundaries feature...');

        // Get center point - use property coordinates or geocode zip code
        let centerLat, centerLng;

        if (PropertyPage.mainProperty.latitude && PropertyPage.mainProperty.longitude) {
            centerLat = PropertyPage.mainProperty.latitude;
            centerLng = PropertyPage.mainProperty.longitude;
            console.log('Using property coordinates:', centerLat, centerLng);
        } else {
            // Geocode the zip code to get center coordinates
            console.log('Property has no coordinates, geocoding zip code:', PropertyPage.zipCode);
            const coords = await PropertyPage.geocodeZipCode(PropertyPage.zipCode);

            if (!coords) {
                console.warn('Could not geocode zip code, cannot fetch zip boundaries');
                return;
            }

            centerLat = coords.lat;
            centerLng = coords.lng;
            console.log('Geocoded zip code to:', centerLat, centerLng);
        }

        // Fetch boundaries to get only immediate neighbors
        const radiusMeters = 4828;
        const allBoundaries = await PropertyPage.fetchZipCodeBoundaries(centerLat, centerLng, radiusMeters);

        if (allBoundaries.length === 0) {
            console.warn('No zip code boundaries found');
            return;
        }

        PropertyPage.zipCodeBoundaries.all = allBoundaries;

        // Find the current zip code's boundary
        const currentZipBoundary = allBoundaries.find(
            b => b.properties.zipCode === PropertyPage.zipCode
        );

        if (!currentZipBoundary) {
            console.warn(`Could not find boundary for current zip code ${PropertyPage.zipCode}`);
            return;
        }

        PropertyPage.zipCodeBoundaries.currentZipBoundary = currentZipBoundary;

        // Identify ONLY immediate neighbors (zip codes that physically touch the current zip)
        const neighbors = PropertyPage.identifyNeighborZipCodes(currentZipBoundary, allBoundaries);
        PropertyPage.zipCodeBoundaries.neighbors = neighbors;

        console.log(`Found ${neighbors.length} immediate neighbor zip codes from ${allBoundaries.length} zips in radius`);

        // Display only immediate neighbor boundaries
        PropertyPage.displayZipCodeBoundaries(neighbors);

        console.log('Zip code boundaries initialized successfully');
    } catch (error) {
        console.error('Error initializing zip code boundaries:', error);
    }
};

// ============================================================
// LOADING OVERLAY
// ============================================================

/**
 * Shows a loading overlay over the map
 */
PropertyPage.showLoadingOverlay = function(message) {
    // Remove existing overlay if present
    PropertyPage.hideLoadingOverlay();

    // Create overlay element
    const overlay = document.createElement('div');
    overlay.id = 'loading-overlay';
    overlay.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(255, 255, 255, 0.9);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        z-index: 9999;
    `;

    overlay.innerHTML = `
        <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
            <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-dark fw-bold">${message || 'Loading...'}</p>
    `;

    // Add to modal body
    const modalBody = document.querySelector('#addComparableModal .modal-body');
    if (modalBody) {
        modalBody.appendChild(overlay);
    }
};

/**
 * Hides the loading overlay
 */
PropertyPage.hideLoadingOverlay = function() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.remove();
    }
};

/**
 * Shows loading message for zip code
 */
PropertyPage.showZipCodeLoadingMessage = function(zipCode) {
    PropertyPage.showLoadingOverlay(`Loading data for zip code ${zipCode}...`);
};

/**
 * Hides loading message
 */
PropertyPage.hideZipCodeLoadingMessage = function() {
    PropertyPage.hideLoadingOverlay();
};

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