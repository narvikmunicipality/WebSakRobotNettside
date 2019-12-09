
$(document).ready(function () {
    var ansattnr = $('#hidden-ansattnr').val();

    if (ansattnr == -1) {
        $('.btn-primary').addClass("btn-secondary disabled");
        $('.btn-primary').removeClass("btn-primary");
        $('.btn-secondary').prop('disabled', true);
        $('.ansattnr-not-required').removeClass("btn-secondary disabled");
        $('.ansattnr-not-required').addClass("btn-primary");
        $('.ansattnr-not-required').prop('disabled', false);
    }
});
