/**
 * Address Autocomplete using Leaflet Control Geocoder
 */

class AddressAutocomplete {
    constructor(options) {
        this.addressInput = document.getElementById(options.addressInputId || 'Address');
        this.suggestionsDiv = document.getElementById(options.suggestionsId || 'address-suggestions');
        this.cityInput = document.getElementById(options.cityInputId || 'City');
        this.stateInput = document.getElementById(options.stateInputId || 'State');
        this.zipInput = document.getElementById(options.zipInputId || 'ZipCode');

        this.debounceTimer = null;
        this.debounceDelay = options.debounceDelay || 500;
        this.minChars = options.minChars || 3;

        // Initialize Leaflet's Nominatim geocoder
        this.geocoder = L.Control.Geocoder.nominatim({
            geocodingQueryParams: {
                countrycodes: 'us',
                limit: 5,
                addressdetails: 1
            }
        });

        this.init();
    }

    init() {
        if (!this.addressInput || !this.suggestionsDiv) {
            console.error('Address autocomplete: Required elements not found');
            return;
        }

        this.attachEventListeners();
    }

    attachEventListeners() {
        this.addressInput.addEventListener('input', (e) => {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => {
                this.search(e.target.value);
            }, this.debounceDelay);
        });

        // Close suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (e.target !== this.addressInput) {
                this.hideSuggestions();
            }
        });
    }

    search(query) {
        if (query.length < this.minChars) {
            this.hideSuggestions();
            return;
        }

        // Use Leaflet Geocoder to search
        this.geocoder.geocode(query, (results) => {
            this.displaySuggestions(results);
        });
    }

    displaySuggestions(results) {
        this.suggestionsDiv.innerHTML = '';

        if (!results || results.length === 0) {
            this.suggestionsDiv.innerHTML = '<div class="list-group-item">No addresses found</div>';
            this.showSuggestions();
            return;
        }

        // Filter to only show street addresses
        const filteredResults = results.filter(result => {
            const props = result.properties || {};
            return props.address && (props.address.road || props.address.house_number);
        });

        if (filteredResults.length === 0) {
            this.suggestionsDiv.innerHTML = '<div class="list-group-item">No street addresses found</div>';
            this.showSuggestions();
            return;
        }

        filteredResults.forEach((result) => {
            const item = document.createElement('a');
            item.href = '#';
            item.className = 'list-group-item list-group-item-action';
            item.textContent = this.formatDisplayText(result);

            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectAddress(result);
            });

            this.suggestionsDiv.appendChild(item);
        });

        this.showSuggestions();
    }

    formatDisplayText(result) {
        const addr = result.properties?.address || {};
        const parts = [];

        // Street address
        if (addr.house_number || addr.road) {
            let street = '';
            if (addr.house_number) street += addr.house_number + ' ';
            if (addr.road) street += addr.road;
            if (street) parts.push(street.trim());
        }

        // City
        const city = addr.city || addr.town || addr.village;
        if (city) parts.push(city);

        // State abbreviation
        if (addr.state) {
            parts.push(this.getStateAbbreviation(addr.state));
        }

        // ZIP code
        if (addr.postcode) parts.push(addr.postcode);

        return parts.length > 0 ? parts.join(', ') : result.name;
    }

    selectAddress(result) {
        const addr = result.properties?.address || {};

        // Build street address
        let streetAddress = '';
        if (addr.house_number) streetAddress += addr.house_number + ' ';
        if (addr.road) streetAddress += addr.road;

        this.addressInput.value = streetAddress.trim() || result.name;

        // Set city
        if (this.cityInput) {
            this.cityInput.value = addr.city || addr.town || addr.village || '';
        }

        // Set state
        if (this.stateInput && addr.state) {
            this.stateInput.value = this.getStateAbbreviation(addr.state);
            // Trigger input event for uppercase handler
            this.stateInput.dispatchEvent(new Event('input', { bubbles: true }));
        }

        // Set ZIP
        if (this.zipInput && addr.postcode) {
            this.zipInput.value = addr.postcode;
        }

        this.hideSuggestions();
    }

    getStateAbbreviation(stateName) {
        const states = {
            'Alabama': 'AL', 'Alaska': 'AK', 'Arizona': 'AZ', 'Arkansas': 'AR',
            'California': 'CA', 'Colorado': 'CO', 'Connecticut': 'CT', 'Delaware': 'DE',
            'Florida': 'FL', 'Georgia': 'GA', 'Hawaii': 'HI', 'Idaho': 'ID',
            'Illinois': 'IL', 'Indiana': 'IN', 'Iowa': 'IA', 'Kansas': 'KS',
            'Kentucky': 'KY', 'Louisiana': 'LA', 'Maine': 'ME', 'Maryland': 'MD',
            'Massachusetts': 'MA', 'Michigan': 'MI', 'Minnesota': 'MN', 'Mississippi': 'MS',
            'Missouri': 'MO', 'Montana': 'MT', 'Nebraska': 'NE', 'Nevada': 'NV',
            'New Hampshire': 'NH', 'New Jersey': 'NJ', 'New Mexico': 'NM', 'New York': 'NY',
            'North Carolina': 'NC', 'North Dakota': 'ND', 'Ohio': 'OH', 'Oklahoma': 'OK',
            'Oregon': 'OR', 'Pennsylvania': 'PA', 'Rhode Island': 'RI', 'South Carolina': 'SC',
            'South Dakota': 'SD', 'Tennessee': 'TN', 'Texas': 'TX', 'Utah': 'UT',
            'Vermont': 'VT', 'Virginia': 'VA', 'Washington': 'WA', 'West Virginia': 'WV',
            'Wisconsin': 'WI', 'Wyoming': 'WY'
        };
        return states[stateName] || stateName.toUpperCase().substring(0, 2);
    }

    showSuggestions() {
        this.suggestionsDiv.style.display = 'block';
    }

    hideSuggestions() {
        this.suggestionsDiv.style.display = 'none';
        this.suggestionsDiv.innerHTML = '';
    }
}

// Make it available globally
window.AddressAutocomplete = AddressAutocomplete;