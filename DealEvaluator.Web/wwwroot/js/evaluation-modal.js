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
            // Listen for modal show event to refresh comparables and auto-prefill
            const modal = document.getElementById('createEvaluationModal');
            if (modal && !this._eventListenerAttached) {
                // Refresh comparables list before modal shows
                modal.addEventListener('show.bs.modal', async () => {
                    // Refresh comparables
                    if (window.PropertyPageConfig && window.PropertyPageConfig.propertyId) {
                        const response = await fetch(`/Property/GetComparablesSelectionList?propertyId=${window.PropertyPageConfig.propertyId}`);
                        if (response.ok) {
                            const html = await response.text();
                            const container = document.getElementById('comparables-selection-container');
                            if (container) {
                                container.innerHTML = html;
                                // Re-initialize checkbox listeners after refresh
                                this.initializeComparableSelection();
                            }
                        }
                    }

                    // Auto-prefill rehab line items
                    if (this.lineItems.length === 0) {
                        await this.autoPrefillFromProperty();
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

        // Auto-prefill from property details using backend API
        async autoPrefillFromProperty() {
            if (!window.PropertyPageConfig || !window.PropertyPageConfig.mainProperty) {
                return;
            }

            const property = window.PropertyPageConfig.mainProperty;

            if (property.condition === null) {
                return;
            }

            try {
                // Call backend API to get auto-generated rehab estimate
                const response = await fetch(`/Property/GetAutoRehabEstimate?propertyId=${property.id}`);
                const data = await response.json();

                if (data.success && data.lineItems) {
                    this.lineItems = data.lineItems.map(item => ({
                        lineItemType: item.lineItemType,
                        condition: item.condition,
                        quantity: item.quantity,
                        unitCost: item.unitCost,
                        notes: item.notes || ''
                    }));
                } else {
                    console.error('Failed to fetch auto rehab estimate:', data.error);
                    this.lineItems = [];
                }
            } catch (error) {
                console.error('Error fetching auto rehab estimate:', error);
                this.lineItems = [];
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