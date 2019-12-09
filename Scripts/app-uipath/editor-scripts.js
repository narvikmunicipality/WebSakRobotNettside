
var isEditorBlank = true;

$(document).ready(function () {

    $('#editor').wysiwyg({
        activeToolbarClass: 'btn-info'
    });

    document.getElementById('editor').onkeyup = function () {

        $('#notat-text').val(encodeURI($('#editor').cleanHtml()));
        markEditorDirty();
    };

    document.getElementById('editor').onclick = function () {

        if (isEditorBlank) {
            $('#btn-textsize-normal').click(); // sett tekststørrelse til 'normal'
            isEditorBlank = false;
        }
    };

    if ($('#btn-autocomplete').length) {
        document.getElementById('btn-autocomplete').onclick = function () {
            autocomplete();
            console.log('btn-autocomplete');
            markEditorDirty();
        };
    }

    document.getElementById('btn-submit').onclick = function () {
        submitNoteText();
        console.log('btn-submit');
    }

    if ($('#send-kopi-chk').length) {
        document.getElementById('send-kopi-chk').onclick = function () {
            updateCheckboxValue();
        };
    }


    updateCheckboxValue();

    // be om bekreftelse før brukeren forlater siden uten å trykke 'send inn først'
    $(window).on('beforeunload', function (e) {

        console.log('pootis');

        if ($('dirty').length) {
            var confirmationMessage = 'null';

            (e || window.event).returnValue = confirmationMessage; //Gecko + IE
            return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
        }
        if (!isEditorBlank) {
            console.log("!isEditorBlank");
            return false;
        }
    });
});

function markEditorDirty() {
    console.log("marked dirty");
    $('#editor').addClass('dirty'); // flagg tekstfeltet hvis teksten har blitt endret
}

function submitNoteText() {

    isEditorBlank = true;
    $('#notat-text').val(encodeURI($('#editor').cleanHtml()));
}

function updateCheckboxValue() {
    if ($('#send-kopi-chk').length) {
        $('#send-kopi').val($('#send-kopi-chk').is(':checked'));
    }
}

function autocomplete() {
    $('#editor').append($('#autocomplete-text').val());
}
