$(function () {
    // Load default view - Dashboard
    loadContent('/User/UserDashboard');

    // Click event for Dashboard link
    $('#dashboard-link').on('click', function (e) {
        e.preventDefault();
        loadContent('/User/UserDashboard');
    });

    // Click event for Transaction History link
    $('#transactions-link').on('click', function (e) {
        e.preventDefault();
        loadContent('/User/TransactionHistory');
    });

    // Click event for Money Transfer link
    $('#moneytransfer-link').on('click', function (e) {
        e.preventDefault();
        loadContent('/User/MoneyTransfer');
    });

    // Click event for User Profile link
    $('#profile-link').on('click', function (e) {
        e.preventDefault();
        loadContent('/User/UserProfile');
    });

    // Function to load content via AJAX
    function loadContent(url) {
        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
                $('#dashboard-content').html(data);
            },
            error: function () {
                alert('Failed to load content');
            }
        });
    }
});
