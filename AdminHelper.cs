using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class AdminHelper
    {
        public static Response<List<Bundles.ESignInfo>> HentAktiveSigneringsoppdragForBruker(string ADbruker)
        {
            Imposter.ImpersonateRobot();

            List<Bundles.ESignInfo> aktiveOppdrag = new List<Bundles.ESignInfo>();

            var mapper = Directory.GetDirectories(Paths.GetUploadFolder());

            foreach (var mappe in mapper)
            {
                var lederAD = File.ReadAllText($"{mappe}\\{HelperFile.MANAGER_AD}");
                if (ADbruker.ToUpper().Equals(lederAD.ToUpper()))
                {
                    if (File.Exists($"{mappe}\\{HelperFile.SIGNATURE_STATUS}"))
                    {
                        var dokDato = File.ReadAllText($"{mappe}\\{HelperFile.DATE}");
                        var signDatoFil = $"{mappe}\\{HelperFile.SIGNATURE_DATE}";
                        var signDato = File.Exists(signDatoFil) ? File.ReadAllText(signDatoFil) : "Ikke signert";
                        var ansattnavnfil = $"{mappe}\\{HelperFile.EMPLOYEE_NAME}";
                        var mottakernavn = File.Exists(ansattnavnfil) ? File.ReadAllText(ansattnavnfil) : "Ikke tilgjengelig";
                        var statusfil = $"{mappe}\\{HelperFile.SIGNATURE_ERROR}";
                        var status = File.Exists(statusfil) ? File.ReadAllText(statusfil) : "Ingen feil";

                        var mottakerEpost = ADHelper.GetAnsattEpost(File.ReadAllText($"{mappe}\\{HelperFile.EMPLOYEE_ID}")).Get();

                        aktiveOppdrag.Add(new Bundles.ESignInfo
                        {
                            Dato = dokDato,
                            Mottaker = mottakernavn,
                            SignDato = signDato,
                            JournalpostID = "Kommer snart?",
                            Signert = !signDato.Equals("Ikke signert"),
                            MottakerEpost = mottakerEpost,
                            Status = status.Split(':').First()
                        });
                    }
                }
            }

            Imposter.UndoImpersonation();

            return new Response<List<Bundles.ESignInfo>>(aktiveOppdrag, "hello there", Codes.Code.OK);
        }

        public static Response<List<Bundles.ESignInfo>> HentFullforteSigneringsoppdragForBruker(string ADbruker)
        {
            Imposter.ImpersonateRobot();

            List<Bundles.ESignInfo> fullforteOppdrag = new List<Bundles.ESignInfo>();

            var file = Paths.ESIGN_LOG + ADbruker + ".txt";
            if (File.Exists(file))
            {
                var content = File.ReadAllLines(file);

                foreach (var line in content)
                {
                    var row = line.Split(';');
                    var dokDato = row[0];
                    var mottakernavn = row[1];
                    var signDato = row[2];
                    var journalpostID = row[3];

                    fullforteOppdrag.Add(new Bundles.ESignInfo
                    {
                        Dato = dokDato,
                        Mottaker = mottakernavn,
                        SignDato = signDato,
                        JournalpostID = journalpostID
                    });
                }

                
            }

            Imposter.UndoImpersonation();

            return new Response<List<Bundles.ESignInfo>>(fullforteOppdrag, "ok", Codes.Code.OK);
        }
    }
}