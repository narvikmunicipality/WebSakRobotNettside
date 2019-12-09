using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Inneholder strengkonstanter brukt i løsningen 
     */
    public class StringConstants
    {
        public static readonly string ERROR_GENERIC = "FEIL: Beklager, en feil har oppstått. Vennligst prøv på nytt";
        public static readonly string ERROR_MISSING_NAME = "Vennligst skriv inn fornavn og etternavn til mottaker";
        public static readonly string ERROR_MISSING_SESSION = "Du må velge en ansatt for å laste opp dokumenter";
        public static readonly string ERROR_INVALID_ARGUMENT = "Feil: Ugyldig parameter oppgitt. Prøv på nytt senere";
        public static readonly string SIGN_REQUEST = "Vennligst signér vedlagte dokument";
        public static readonly string WARN_ID_NOT_FOUND_AD = "Ansattnr {0} ble ikke funnet i AD. Forsøk med fødselsnummer";
        public static readonly string ERROR_USRN_NOT_FOUND_AD = "Brukernavn '{0}' ble ikke funnet i AD";
        public static readonly string ERROR_ID_NOT_FOUND_AD = "Ansattnummer '{0}' ble ikke funnet i AD";
        public static readonly string ERROR_MAIL_NOT_FOUND_AD = "E-postadressen '{0}' ble ikke funnet i AD. Forsøk med ansattnr";
        public static readonly string ERROR_MANAGER_NOT_FOUND_AD = "Lederen for ansattnr {0} ble ikke funnet i AD";
        public static readonly string ERROR_SSN_NOT_FOUND_HRM = "Dette fødselsnummeret ble ikke funnet i HRM";

        public static readonly string ESIGN_SUCCESS = "E-signeringsoppdrag opprettet og sendt til {0}! Husk at dokumentet må signeres innen 4 uker.";
        public static readonly string INVALID_SSN = "Feil: oppgitt personnr {0} ble ikke godkjent. Vennligst oppgi et gyldig norsk fødselsnummer eller D-nummer.";

        public static readonly string CREATE_TESTUSER_OK = "Testbruker med ansattnummer {0} ble opprettet";
        public static readonly string CREATE_TESTUSER_DUPLICATE = "Feil: Testbruker med ansattnummer {0} finnes allerede";
        public static readonly string DELETE_TESTUSER_OK = "Testbruker med ansattnummer {0} ble slettet";
        public static readonly string DELETE_TESTUSER_ERROR = "Feil: Testbruker med ansattnummer {0} ble ikke slettet. Prøv på nytt senere";

        public static readonly string SUPERUSER_ACCESS_GRANT = "{0} har nå administratortilgang på app.uipath";
        public static readonly string SUPERUSER_ACCESS_MODIFY = "Superbrukere oppdatert";
        public static readonly string SUPERUSER_ACCES_DUPLICATE = "Feil: {0} har allerede administratortilgang";
        public static readonly string SUPERUSER_ACCESS_REVOKE = "Administratortilgangen til {0} har blitt fjernet";

        public static readonly string ESIGNATURE_REJECTED = "Mottaker med fødselsnummer {0} har avvist signeringsoppdraget";
        public static readonly string ESIGNATURE_MISSING_INFO = "Mottaker med fødselsnummer {0} har ikke registrert kontaktinformasjon i Kontakt- og reservasjonsregisteret";
        public static readonly string ESIGNATURE_RESERVED = "Mottaker med fødselsnummer {0} har reservert seg mot digital kommunikasjon fra det offentlige, og kan ikke signere elektronisk";
        public static readonly string ESIGNATURE_EXPIRED = "E-signeringsoppdraget har utløpt. Mottaker med fødselsnummer {0} har ikke signert innen fristen";

        public static readonly string MAIL_DONOTREPLY = "<hr><i>Dette er en automatisk generert e-post og kan ikke besvares.</i>";

        public static readonly string PURRE_MAIL_LINK = "https://mail.google.com/mail/u/0/?view=cm&fs=1&to={0}&su=Dokument%20til%20signering%20fra%20Narvik%20kommune&body=Hei%20kan%20du%20vennligst%20få%20ut%20fingeren%20og%20signere%20dokumentet%20jeg%20har%20sendt%20deg&tf=1";

        public static class Varsel
        {
            public static string GetVarsel(int dag) { return $"Sykefraværsoppfølging dag {dag}"; }
        }

        public static readonly string ACTION_SYKEFRAVAR = "Journalføre sykefraværsdag 5, 14, 28, tilretteleggingssamtale, dialogmøteinnkalling, laste opp dokumenter";
        public static readonly string ACTION_ANSATTMAPPE = "Arbeidskontrakt, permisjonssøknad, medarbeidersamtale, vedlegg til politiattest, sende utgående brev";
        public static readonly string ACTION_ESIGNERING = "Arbeidsavtale, taushetserklæring mm. Signert dokument blir automatisk arkivert i WebSak";

        public static string GetLogHeader() { return "------------------" + DateTime.Now.ToShortTimeString() + "------------------"; }
    }

    public class Paths
    {
        public static readonly string UNC_ADDRESS = @"\\njord\robot1$\";
        public static readonly string DRIVE_LETTER = @"G:\";
        public static readonly string OUTPUT_FOLDER = @"Arkivrobot\Rapport ut\";
        public static readonly string INPUT_FOLDER = @"Arkivrobot\Rapport inn\";

#if DEBUG
        public static readonly string UPLOAD_FOLDER = @"Filopplastinger_test\";
#else
        public static readonly string UPLOAD_FOLDER = @"Filopplastinger\";
#endif

        public static readonly string PORTAL_LOG = @"Webportal\logs\";

        public static readonly string SUPER_USERS = UNC_ADDRESS + @"Webportal\config\superbrukere\users.bin";
        public static readonly string TEST_USERS = UNC_ADDRESS + @"Webportal\config\testbrukere\users.bin";
        public static readonly string RAADMENN_USERS = UNC_ADDRESS + @"Webportal\config\rådmenn\users.bin";

        public static readonly string VARSEL_WEB_FILE = UNC_ADDRESS + @"Sykefravær\Rapport Inn\varsel web.csv";

        public static readonly string ESIGN_LOG = UNC_ADDRESS + PORTAL_LOG + @"Esignering\";

        public static readonly string LOG_DEFAULT = @"C:\inetpub\logs";

        public static string GetUploadFolder()
        {
            return UNC_ADDRESS + INPUT_FOLDER + UPLOAD_FOLDER;
        }
    }

    public class HelperFile
    {
        public static readonly string ARCHIVE_CONFIRMATION = "sendConfirmation.txt";
        public static readonly string SIGNATURE_STATUS = "signaturstatus.txt";
        public static readonly string SIGNATURE_DATE = "signaturdato.txt";
        public static readonly string REFERENCE = "referanse.txt";
        public static readonly string EMPLOYEE_NAME = "ansattnavn.txt";
        public static readonly string SSIN = "personnr.txt";
        public static readonly string EMPLOYEE_ID = "ansattnr.txt";
        public static readonly string HRM_ERROR = "HRM_FEIL.txt";
        public static readonly string SIGNATURE_ERROR = "e-signeringsfeil.txt";
        public static readonly string SVARUT = ".svarut";
        public static readonly string MANAGER_AD = "lederAD.txt";
        public static readonly string FILE_LIST = "filliste.txt";
        public static readonly string DEPARTMENT = "enhetinfo.txt";
        public static readonly string MANAGER_EMAIL = "lederepost.txt";
        public static readonly string FOLDER_TYPE = "mappetype.txt";
        public static readonly string DATE = "dato.txt";
        public static readonly string POST_NAME = "journalpostnavn.txt";
    }

    public class LogFile
    {
        public static readonly string MAIL_LOG = "mail_log.txt";
        public static readonly string POLL_LOG = "esignpollernew.txt";
        public static readonly string VISITORS = "visitors.txt";
        public static readonly string CRASH = "crash.txt";
    }
}