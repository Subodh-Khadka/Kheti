
$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip();
});

$(document).ready(function () {
    $('#queryStatusFilter').change(function () {
        $('#queryStatusForm').submit();
    });
});
    


spotReload = function () {
    location.reload();
}

