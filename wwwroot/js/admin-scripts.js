/**
 * admin-scripts.js - Enhanced JavaScript functionality for admin interface
 * 
 * This file contains common JavaScript functionality for the admin area:
 * - Tooltip initialization
 * - Mobile sidebar toggle
 * - Alert auto-dismiss
 * - Form validation enhancement
 * - Dynamic UI components
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('Admin scripts loaded successfully');
      // Log CSS loading status for debugging
    const styles = Array.from(document.styleSheets);
    console.log('Total stylesheets loaded:', styles.length);
    
    // Check if admin CSS files are loaded
    const adminCssLoaded = styles.some(sheet => {
        try {
            return sheet.href && sheet.href.includes('admin.css');
        } catch (e) {
            return false;
        }
    });
    
    const adminStylesLoaded = styles.some(sheet => {
        try {
            return sheet.href && sheet.href.includes('admin-styles.css');
        } catch (e) {
            return false;
        }
    });
    
    const emergencyStylesLoaded = styles.some(sheet => {
        try {
            return sheet.href && sheet.href.includes('emergency-admin-styles.css');
        } catch (e) {
            return false;
        }
    });
    
    console.log('Admin CSS files loaded:', {
        'admin.css': adminCssLoaded,
        'admin-styles.css': adminStylesLoaded,
        'emergency-admin-styles.css': emergencyStylesLoaded
    });
    
    // Enable Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function(tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Also enable tooltips for elements with title attribute
    var titleTooltipList = [].slice.call(document.querySelectorAll('[title]:not([data-bs-toggle="tooltip"])'));
    titleTooltipList.forEach(function(tooltipEl) {
        new bootstrap.Tooltip(tooltipEl);
    });

    // Mobile sidebar toggle
    var sidebarToggle = document.getElementById('sidebar-toggle');
    var sidebar = document.querySelector('.admin-sidebar');
    
    if (sidebarToggle && sidebar) {
        console.log('Sidebar toggle initialized');
        
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
            console.log('Sidebar toggle clicked, show:', sidebar.classList.contains('show'));
        });
        
        // Close sidebar when clicking outside
        document.addEventListener('click', function(event) {
            var isClickInside = sidebar.contains(event.target) || sidebarToggle.contains(event.target);
            
            if (!isClickInside && sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
                console.log('Sidebar closed by outside click');
            }
        });
    }

    // Fade out alerts after 5 seconds
    setTimeout(function() {
        var alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(function(alert) {
            var bsAlert = new bootstrap.Alert(alert);
            setTimeout(function() {
                alert.classList.add('fade');
                setTimeout(function() {
                    bsAlert.close();
                }, 500);
            }, 5000);
        });
    }, 2000);

    // Add confirm dialog to delete buttons
    var deleteButtons = document.querySelectorAll('a[href*="Delete"], button[data-action="delete"]');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!confirm('Bạn có chắc chắn muốn xóa mục này không?')) {
                e.preventDefault();
                return false;
            }
        });
    });

    // For tables with checkboxes
    var selectAllCheckbox = document.getElementById('select-all');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            var checkboxes = document.querySelectorAll('table input[type="checkbox"].item-checkbox');
            checkboxes.forEach(function(checkbox) {
                checkbox.checked = selectAllCheckbox.checked;
            });
            updateBulkActions();
        });

        var itemCheckboxes = document.querySelectorAll('table input[type="checkbox"].item-checkbox');
        itemCheckboxes.forEach(function(checkbox) {
            checkbox.addEventListener('change', updateBulkActions);
        });
    }

    // Handle filter form submission
    var filterForms = document.querySelectorAll('.filter-form');
    filterForms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            // Remove empty fields from form submission
            Array.from(this.elements).forEach(function(element) {
                if (element.value === '' && element.name) {
                    element.disabled = true;
                }
            });
        });
    });

    // Toggle filter section visibility
    var filterToggle = document.querySelector('[data-bs-toggle="collapse"][data-bs-target="#filterSection"]');
    if (filterToggle) {
        filterToggle.addEventListener('click', function() {
            var icon = this.querySelector('i');
            if (icon) {
                if (icon.classList.contains('bi-funnel')) {
                    icon.classList.replace('bi-funnel', 'bi-funnel-fill');
                } else {
                    icon.classList.replace('bi-funnel-fill', 'bi-funnel');
                }
            }
        });
    }
    
    // Log that initialization is complete
    console.log('Admin UI initialization complete');
});

// Update bulk actions based on selected items
function updateBulkActions() {
    var checkedCount = document.querySelectorAll('table input[type="checkbox"].item-checkbox:checked').length;
    var bulkActions = document.querySelectorAll('.bulk-action');
    
    bulkActions.forEach(function(action) {
        if (checkedCount > 0) {
            action.classList.remove('disabled');
            var counter = action.querySelector('span.counter');
            if (counter) counter.textContent = checkedCount;
        } else {
            action.classList.add('disabled');
            var counter = action.querySelector('span.counter');
            if (counter) counter.textContent = '0';
        }
    });
}
