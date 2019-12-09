var antallVedlegg = 0;
var divVedleggCounter = 0;
var currentDay = $('#hovedfilnavn').val();

$(document).ready(function () {
    document.getElementById('btn-add-vedlegg').onclick = function () {
        leggTilVedlegg();
        console.log('btn-add-vedlegg');
    };

    document.getElementById('form-main').onsubmit = function (evt) {
        if (!confirmDialog()) {
            evt.preventDefault();
        }
    };

    if ($('#upload-hidden').length) {
        document.getElementById('upload-hidden').onchange = function () {
            $('#selected-file').text('Valgt hoveddokument: ' + this.value.split('\\').pop());
        };
    }
});

function confirmDialog() {

    var dag28Ok = true;
    var kopiOK = true;

    if (currentDay == "Sykefraværsoppfølging dag 28") {
        if (antallVedlegg == 0) {
            dag28Ok = confirm("Du har ikke lastet opp vedlegg. Du gjør nå et oppfølgingsnotat på dag 28, er du sikker på du ikke skal laste opp en oppfølgingsplan?");
        }
    }

    if ($('#send-kopi').is(':checked') || $('#send-kopi').val() == "True") {
        kopiOK = confirmCopy();
    }

    return dag28Ok && kopiOK;
}

function confirmCopy() {
    var msg = 'Du sender nå ut kopi til den ansatte. Hvis den ansatte har digital postkasse vil den bli sendt elektronisk, hvis ikke blir den sendt som brev i posten.';

    if (antallVedlegg > 0) {
        msg += " " + antallVedlegg + ' vedlegg vil også bli sendt i kopi.';
    }

    return confirm(msg);
}

function leggTilVedlegg() {
    $('#div-vedlegg').append('<div class="row" id="divVedlegg' + divVedleggCounter + '"><div class="col-md-9"><input id="liVedlegg'
        + divVedleggCounter + '" type="file" name="vedlegg'
        + '" id="vedleggFil' + divVedleggCounter + '"></div>'
        + '<div class="col-md-3"><button type="button" class="btn btn-danger btn-block" onClick="fjernVedlegg('
        + divVedleggCounter + ')">Fjern vedlegg</button></div></div><br />');

    $("#liVedlegg" + divVedleggCounter).click(); // Åpne filopplasterdialog automatisk
    $("#antallVedlegg").val(++divVedleggCounter); // inkrementer og sett vedleggteller
    antallVedlegg++;
    $('#btn-submit').text('Send inn hoveddokument og ' + antallVedlegg + ' vedlegg');

    markEditorDirty();
}

function fjernVedlegg(idx) {
    $("#divVedlegg" + idx).remove();
    antallVedlegg--;
    $('#btn-submit').text('Send inn hoveddokument og ' + antallVedlegg + ' vedlegg');
}