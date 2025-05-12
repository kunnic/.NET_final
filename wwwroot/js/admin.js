/**
 * admin.js - JavaScript functionality for admin interface
 */

// Enable Bootstrap tooltips
document.addEventListener('DOMContentLoaded', function() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Also enable tooltips for elements with title attribute
    var titleTooltipList = [].slice.call(document.querySelectorAll('[title]:not([data-bs-toggle="tooltip"])'));
    titleTooltipList.map(function (tooltipEl) {
        return new bootstrap.Tooltip(tooltipEl);
    });

    // Fade out alerts after 5 seconds
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            setTimeout(() => {
                alert.classList.add('fade');
                setTimeout(() => {
                    bsAlert.close();
                }, 500);
            }, 5000);
        });
    }, 2000);

    // Add confirm dialog to delete buttons
    const deleteButtons = document.querySelectorAll('a[href*="Delete"], button[data-action="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Bạn có chắc chắn muốn xóa mục này không?')) {
                e.preventDefault();
                return false;
            }
        });
    });

    // For tables with checkboxes
    const selectAllCheckbox = document.getElementById('select-all');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('table input[type="checkbox"].item-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = selectAllCheckbox.checked;
            });
            updateBulkActions();
        });

        const itemCheckboxes = document.querySelectorAll('table input[type="checkbox"].item-checkbox');
        itemCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', updateBulkActions);
        });
    }

    // Handle responsive sidebar toggle
    const sidebarToggle = document.getElementById('sidebar-toggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.querySelector('.admin-sidebar').classList.toggle('show');
        });
    }

    // Handle card expand/collapse
    const cardTogglers = document.querySelectorAll('[data-toggle="card-collapse"]');
    cardTogglers.forEach(toggler => {
        toggler.addEventListener('click', function() {
            const targetId = this.getAttribute('data-target');
            const targetCard = document.querySelector(targetId);
            if (targetCard) {
                const cardBody = targetCard.querySelector('.card-body');
                if (cardBody.style.display === 'none') {
                    cardBody.style.display = '';
                    this.querySelector('i').classList.replace('bi-chevron-down', 'bi-chevron-up');
                } else {
                    cardBody.style.display = 'none';
                    this.querySelector('i').classList.replace('bi-chevron-up', 'bi-chevron-down');
                }
            }
        });
    });

    // Handle filter form submission
    const filterForms = document.querySelectorAll('.filter-form');
    filterForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Remove empty fields from form submission
            const formElements = this.elements;
            for (let i = 0; i < formElements.length; i++) {
                const element = formElements[i];
                if (element.value === '' && element.name) {
                    element.disabled = true;
                }
            }
        });
    });
});

// Update bulk actions based on selected items
function updateBulkActions() {
    const checkedCount = document.querySelectorAll('table input[type="checkbox"].item-checkbox:checked').length;
    const bulkActions = document.querySelectorAll('.bulk-action');
    
    bulkActions.forEach(action => {
        if (checkedCount > 0) {
            action.classList.remove('disabled');
            action.querySelector('span.counter').textContent = checkedCount;
        } else {
            action.classList.add('disabled');
            action.querySelector('span.counter').textContent = '0';
        }
    });
}

// Format number with commas
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
