// Evaluation Modal - Rehab Line Items Management
(function() {
    'use strict';

    const lineItemTypes = ['Kitchen', 'Bathroom', 'Master Bedroom', 'Bedroom', 'Living Room', 'Dining Room', 'Basement', 'Exterior', 'Roof', 'HVAC', 'Plumbing', 'Electrical', 'Flooring', 'Windows', 'Doors', 'Landscaping', 'Other'];
    const conditions = ['Cosmetic', 'Moderate', 'Heavy'];
    let lineItems = [];
    let editingIndex = null;

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

        if (!container) return;

        // Clear all items except emptyState
        Array.from(container.children).forEach(child => {
            if (child.id !== 'emptyState') {
                child.remove();
            }
        });

        if (lineItems.length === 0) {
            if (emptyState) {
                emptyState.style.display = 'block';
            }
            return;
        }

        if (emptyState) {
            emptyState.style.display = 'none';
        }

        lineItems.forEach((item, index) => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'list-group-item';

            // Check if this item is being edited
            if (editingIndex === index) {
                // EDIT MODE - Render inline edit form
                itemDiv.innerHTML = `
                    <div class="row g-2">
                        <div class="col-md-3">
                            <select class="form-select form-select-sm" id="edit-type-${index}">
                                ${lineItemTypes.map((type, i) => `<option value="${i}" ${i === item.lineItemType ? 'selected' : ''}>${type}</option>`).join('')}
                            </select>
                        </div>
                        <div class="col-md-3">
                            <select class="form-select form-select-sm" id="edit-condition-${index}">
                                ${conditions.map((cond, i) => `<option value="${i}" ${i === item.condition ? 'selected' : ''}>${cond}</option>`).join('')}
                            </select>
                        </div>
                        <div class="col-md-2">
                            <input type="number" class="form-control form-control-sm" id="edit-quantity-${index}" value="${item.quantity}" min="1">
                        </div>
                        <div class="col-md-2">
                            <input type="number" class="form-control form-control-sm" id="edit-unitcost-${index}" value="${item.unitCost}" min="0" step="100">
                        </div>
                        <div class="col-md-2 d-flex gap-1 justify-content-end">
                            <button type="button" class="btn btn-sm btn-success" data-action="save" data-index="${index}" title="Save">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-3.99 4.99a.75.75 0 0 1-1.08.02L4.324 8.384a.75.75 0 1 1 1.06-1.06l2.094 2.093 3.473-4.425z"/>
                                </svg>
                            </button>
                            <button type="button" class="btn btn-sm btn-secondary" data-action="cancel" data-index="${index}" title="Cancel">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8z"/>
                                </svg>
                            </button>
                        </div>
                        <div class="col-12">
                            <input type="text" class="form-control form-control-sm" id="edit-notes-${index}" value="${item.notes || ''}" placeholder="Notes (optional)">
                        </div>
                    </div>
                `;
            } else {
                // DISPLAY MODE - Normal view
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
                        <div class="d-flex gap-1">
                            <button type="button" class="btn btn-sm btn-outline-primary" data-action="edit" data-index="${index}" title="Edit">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325"/>
                                </svg>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger" data-action="delete" data-index="${index}" title="Delete">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z"/>
                                    <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4zM2.5 3h11V2h-11z"/>
                                </svg>
                            </button>
                        </div>
                    </div>
                `;
            }

            // Add event listeners for action buttons
            const actionButtons = itemDiv.querySelectorAll('[data-action]');
            actionButtons.forEach(btn => {
                btn.addEventListener('click', function() {
                    const action = this.getAttribute('data-action');
                    const idx = parseInt(this.getAttribute('data-index'));
                    handleLineItemAction(action, idx);
                });
            });

            container.appendChild(itemDiv);
        });
    }

    function handleLineItemAction(action, index) {
        switch(action) {
            case 'edit':
                editingIndex = index;
                renderLineItems();
                break;
            case 'save':
                saveLineItem(index);
                break;
            case 'cancel':
                editingIndex = null;
                renderLineItems();
                break;
            case 'delete':
                removeLineItem(index);
                break;
        }
    }

    function saveLineItem(index) {
        const lineItemType = parseInt(document.getElementById(`edit-type-${index}`).value);
        const condition = parseInt(document.getElementById(`edit-condition-${index}`).value);
        const quantity = parseInt(document.getElementById(`edit-quantity-${index}`).value);
        const unitCost = parseFloat(document.getElementById(`edit-unitcost-${index}`).value);
        const notes = document.getElementById(`edit-notes-${index}`).value;

        if (quantity <= 0 || unitCost < 0) {
            alert('Please enter valid quantity and unit cost');
            return;
        }

        // Update the item in the array
        lineItems[index] = {
            lineItemType,
            condition,
            quantity,
            unitCost,
            notes
        };

        // Exit edit mode
        editingIndex = null;

        // Re-render and update total
        renderLineItems();
        updateTotal();
    }

    function removeLineItem(index) {
        lineItems.splice(index, 1);
        editingIndex = null; // Reset edit mode if deleting
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