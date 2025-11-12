// Evaluation Modal - Rehab Line Items Management
(function() {
    'use strict';

    const lineItemTypes = ['Kitchen', 'Bathroom', 'Master Bedroom', 'Bedroom', 'Living Room', 'Dining Room', 'Basement', 'Exterior', 'Roof', 'HVAC', 'Plumbing', 'Electrical', 'Flooring', 'Windows', 'Doors', 'Landscaping', 'Other'];
    const conditions = ['Cosmetic', 'Moderate', 'Heavy'];
    let lineItems = [];

    function updateTotal() {
        const total = lineItems.reduce((sum, item) => sum + (item.quantity * item.unitCost), 0);
        const totalElement = document.getElementById('totalCost');
        if (totalElement) {
            totalElement.textContent = total.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
        }
    }

    function renderLineItems() {
        const container = document.getElementById('lineItemsList');
        const emptyState = document.getElementById('emptyState');

        if (!container || !emptyState) return;

        if (lineItems.length === 0) {
            emptyState.style.display = 'block';
            container.innerHTML = '';
            container.appendChild(emptyState);
            return;
        }

        emptyState.style.display = 'none';
        container.innerHTML = '';

        lineItems.forEach((item, index) => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'list-group-item';
            itemDiv.innerHTML = `
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <div class="d-flex align-items-center mb-1">
                            <strong>${lineItemTypes[item.lineItemType]}</strong>
                            <span class="badge bg-secondary ms-2">${conditions[item.condition]}</span>
                        </div>
                        <small class="text-muted">
                            Qty: ${item.quantity} × $${item.unitCost.toLocaleString()} = $${(item.quantity * item.unitCost).toLocaleString()}
                        </small>
                        ${item.notes ? `<div><small class="text-muted fst-italic">${item.notes}</small></div>` : ''}
                    </div>
                    <button type="button" class="btn btn-sm btn-danger ms-2" data-index="${index}">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" class="bi bi-trash" viewBox="0 0 16 16">
                            <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z"/>
                            <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4zM2.5 3h11V2h-11z"/>
                        </svg>
                    </button>
                </div>
            `;

            // Add event listener for delete button
            const deleteBtn = itemDiv.querySelector('button[data-index]');
            deleteBtn.addEventListener('click', function() {
                const index = parseInt(this.getAttribute('data-index'));
                removeLineItem(index);
            });

            container.appendChild(itemDiv);
        });
    }

    function removeLineItem(index) {
        lineItems.splice(index, 1);
        renderLineItems();
        updateTotal();
    }

    async function fetchTemplateAndUpdateCost() {
        const lineItemType = document.getElementById('newLineItemType').value;
        const condition = document.getElementById('newCondition').value;
        const unitCostInput = document.getElementById('newUnitCost');

        if (!unitCostInput) return;

        try {
            const response = await fetch(`/RehabTemplate/GetTemplate?lineItemType=${lineItemType}&condition=${condition}`);
            const data = await response.json();

            if (data.success && data.template) {
                unitCostInput.value = data.template.defaultCost;
            } else {
                unitCostInput.value = 0;
            }
        } catch (error) {
            console.error('Error fetching template:', error);
            unitCostInput.value = 0;
        }
    }

    function initializeTemplateSelection() {
        const lineItemTypeSelect = document.getElementById('newLineItemType');
        const conditionSelect = document.getElementById('newCondition');

        if (lineItemTypeSelect) {
            lineItemTypeSelect.addEventListener('change', fetchTemplateAndUpdateCost);
        }

        if (conditionSelect) {
            conditionSelect.addEventListener('change', fetchTemplateAndUpdateCost);
        }

        // Load initial template on page load
        fetchTemplateAndUpdateCost();
    }

    function initializeAddLineItemButton() {
        const addBtn = document.getElementById('addLineItemBtn');
        if (!addBtn) return;

        addBtn.addEventListener('click', function() {
            const lineItemType = parseInt(document.getElementById('newLineItemType').value);
            const condition = parseInt(document.getElementById('newCondition').value);
            const quantity = parseInt(document.getElementById('newQuantity').value);
            const unitCost = parseFloat(document.getElementById('newUnitCost').value);
            const notes = document.getElementById('newNotes').value;

            if (quantity <= 0 || unitCost < 0) {
                alert('Please enter valid quantity and unit cost');
                return;
            }

            lineItems.push({
                lineItemType,
                condition,
                quantity,
                unitCost,
                notes
            });

            // Reset form
            document.getElementById('newQuantity').value = 1;
            document.getElementById('newNotes').value = '';

            // Fetch template cost for the selected type and condition
            fetchTemplateAndUpdateCost();

            renderLineItems();
            updateTotal();
        });
    }

    function initializeFormSubmit() {
        const form = document.getElementById('evaluationForm');
        if (!form) return;

        form.addEventListener('submit', function(e) {
            // Remove existing line item inputs
            const existingInputs = form.querySelectorAll('input[name^="RehabLineItems"]');
            existingInputs.forEach(input => input.remove());

            // Add line items to form
            lineItems.forEach((item, index) => {
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
        });
    }

    function initializeComparableSelection() {
        // Select All functionality for comparables
        const selectAllCheckbox = document.getElementById('selectAllComps');
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener('change', function() {
                const checkboxes = document.querySelectorAll('.comparable-checkbox');
                checkboxes.forEach(cb => cb.checked = this.checked);
            });
        }

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

    // Initialize when DOM is ready
    function initialize() {
        initializeTemplateSelection();
        initializeAddLineItemButton();
        initializeFormSubmit();
        initializeComparableSelection();
        renderLineItems();
        updateTotal();
    }

    // Run on DOMContentLoaded or immediately if already loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

})();