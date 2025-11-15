// Evaluation Modal - Alpine.js Component
function evaluationModal() {
    return {
        // Data
        lineItemTypes: ['Kitchen', 'Bathroom', 'Bedroom', 'Living Room', 'Dining Room', 'Basement', 'Exterior', 'Roof', 'HVAC', 'Plumbing', 'Electrical', 'Flooring', 'Windows', 'Doors', 'Other', 'General'],
        conditions: ['Cosmetic', 'Moderate', 'Heavy'],
        lineItems: [],
        editingIndex: null,
        _eventListenerAttached: false,

        // New item form
        newItem: {
            lineItemType: 0,
            condition: 0,
            quantity: 1,
            unitCost: 0,
            notes: ''
        },

        // Computed total
        get total() {
            return this.lineItems.reduce((sum, item) => sum + (item.quantity * item.unitCost), 0);
        },

        // Format currency
        formatCurrency(value) {
            return value.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
        },

        // Escape HTML to prevent XSS
        escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        },

        // Initialize
        async init() {
            // Listen for modal show event to auto-prefill (only attach once)
            const modal = document.getElementById('createEvaluationModal');
            if (modal && !this._eventListenerAttached) {
                modal.addEventListener('shown.bs.modal', () => {
                    if (this.lineItems.length === 0) {
                        this.autoPrefillFromProperty();
                    }
                });
                this._eventListenerAttached = true;
            }

            // Initialize comparable selection
            this.initializeComparableSelection();

            // Load initial template cost
            await this.fetchTemplateAndUpdateCost();
        },

        // Add new line item
        async addLineItem() {
            if (this.newItem.quantity <= 0 || this.newItem.unitCost < 0) {
                alert('Please enter valid quantity and unit cost');
                return;
            }

            this.lineItems.push({
                lineItemType: parseInt(this.newItem.lineItemType),
                condition: parseInt(this.newItem.condition),
                quantity: parseInt(this.newItem.quantity),
                unitCost: parseFloat(this.newItem.unitCost),
                notes: this.newItem.notes
            });

            // Reset form
            this.newItem.quantity = 1;
            this.newItem.notes = '';

            // Fetch new template cost
            await this.fetchTemplateAndUpdateCost();
        },

        // Edit line item
        editItem(index) {
            this.editingIndex = index;
        },

        // Save edited line item
        saveItem(index) {
            const item = this.lineItems[index];

            if (item.quantity <= 0 || item.unitCost < 0) {
                alert('Please enter valid quantity and unit cost');
                return;
            }

            this.editingIndex = null;
        },

        // Cancel editing
        cancelEdit() {
            this.editingIndex = null;
            // Reset to original values by triggering reactivity
            this.lineItems = [...this.lineItems];
        },

        // Delete line item
        deleteItem(index) {
            this.lineItems.splice(index, 1);
            this.editingIndex = null;
        },

        // Fetch template cost from API
        async fetchTemplateCost(lineItemType, condition) {
            try {
                const response = await fetch(`/RehabTemplate/GetTemplate?lineItemType=${lineItemType}&condition=${condition}`);
                const data = await response.json();
                return data.success && data.template ? data.template.defaultCost : 0;
            } catch (error) {
                console.error('Error fetching template cost:', error);
                return 0;
            }
        },

        // Update cost when type or condition changes
        async fetchTemplateAndUpdateCost() {
            try {
                const cost = await this.fetchTemplateCost(this.newItem.lineItemType, this.newItem.condition);
                this.newItem.unitCost = cost;
            } catch (error) {
                console.error('Error updating cost:', error);
                this.newItem.unitCost = 0;
            }
        },

        // Map property condition to rehab condition
        mapPropertyConditionToRehabCondition(propertyCondition) {
            // PropertyCondition enum: 0-LikeNew, 1-Excellent, 2-Good, 3-MinorRepairs, 4-MajorRepairs, 5-NeedsRenovation, 6-TearDown
            // RehabCondition: 0-Cosmetic, 1-Moderate, 2-Heavy
            if (propertyCondition <= 2) return 0; // Cosmetic
            if (propertyCondition === 3) return 1; // Moderate
            return 2; // Heavy
        },

        // Auto-prefill from property details
        async autoPrefillFromProperty() {
            if (!window.PropertyPageConfig || !window.PropertyPageConfig.mainProperty) {
                return;
            }

            const property = window.PropertyPageConfig.mainProperty;

            if (property.condition === null) {
                return;
            }

            const rehabCondition = this.mapPropertyConditionToRehabCondition(property.condition);

            this.lineItems = [];

            // Add Bedroom line items
            if (property.bedrooms && property.bedrooms > 0) {
                const cost = await this.fetchTemplateCost(2, rehabCondition); // 2 = Bedroom
                this.lineItems.push({
                    lineItemType: 2,
                    condition: rehabCondition,
                    quantity: property.bedrooms,
                    unitCost: cost,
                    notes: ''
                });
            }

            // Add Bathroom line items
            if (property.bathrooms && property.bathrooms > 0) {
                const cost = await this.fetchTemplateCost(1, rehabCondition); // 1 = Bathroom
                this.lineItems.push({
                    lineItemType: 1,
                    condition: rehabCondition,
                    quantity: Math.ceil(property.bathrooms),
                    unitCost: cost,
                    notes: ''
                });
            }

            // Add General line item (sqft-based)
            if (property.sqft && property.sqft > 0) {
                const cost = await this.fetchTemplateCost(15, rehabCondition); // 15 = General
                this.lineItems.push({
                    lineItemType: 15,
                    condition: rehabCondition,
                    quantity: property.sqft,
                    unitCost: cost,
                    notes: 'General rehab estimate based on property square footage'
                });
            }
        },

        // Initialize comparable selection checkboxes
        initializeComparableSelection() {
            this.$nextTick(() => {
                const selectAllCheckbox = document.getElementById('selectAllComps');
                if (selectAllCheckbox) {
                    selectAllCheckbox.addEventListener('change', function() {
                        const checkboxes = document.querySelectorAll('.comparable-checkbox');
                        checkboxes.forEach(cb => cb.checked = this.checked);
                    });

                    // Update Select All state when individual checkboxes change
                    document.querySelectorAll('.comparable-checkbox').forEach(cb => {
                        cb.addEventListener('change', function() {
                            const allCheckboxes = document.querySelectorAll('.comparable-checkbox');
                            const checkedCheckboxes = document.querySelectorAll('.comparable-checkbox:checked');

                            if (selectAllCheckbox) {
                                selectAllCheckbox.checked = allCheckboxes.length === checkedCheckboxes.length;
                                selectAllCheckbox.indeterminate = checkedCheckboxes.length > 0 && checkedCheckboxes.length < allCheckboxes.length;
                            }
                        });
                    });
                }
            });
        },

        // Handle form submission - add hidden inputs for line items
        submitForm(event) {
            const form = event.target;

            // Remove existing line item inputs
            const existingInputs = form.querySelectorAll('input[name^="RehabLineItems"]');
            existingInputs.forEach(input => input.remove());

            // Add line items to form
            this.lineItems.forEach((item, index) => {
                const fields = [
                    { name: `RehabLineItems[${index}].LineItemType`, value: item.lineItemType },
                    { name: `RehabLineItems[${index}].Condition`, value: item.condition },
                    { name: `RehabLineItems[${index}].Quantity`, value: item.quantity },
                    { name: `RehabLineItems[${index}].UnitCost`, value: item.unitCost },
                    { name: `RehabLineItems[${index}].Notes`, value: item.notes || '' }
                ];

                fields.forEach(field => {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = field.name;
                    input.value = field.value;
                    form.appendChild(input);
                });
            });
        }
    };
}