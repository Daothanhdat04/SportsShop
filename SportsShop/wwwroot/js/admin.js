// Admin Dashboard JavaScript

$(document).ready(function () {
    // Sidebar toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Active menu highlight
    var path = window.location.pathname;
    $('#sidebar ul li a').each(function () {
        var href = $(this).attr('href');
        if (path === href || path.indexOf(href) === 0) {
            $(this).parent().addClass('active');
        }
    });

    // Auto hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);
});