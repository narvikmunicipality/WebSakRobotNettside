using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Configuration;
using System.Xml;

namespace WebSakFilopplaster.Net_AD
{
    public class HRMHelper
    {
        private static string Username
        {
            get
            {
                return WebConfigurationManager.AppSettings["HRMUsername"];
            }
        }
        private static string Password
        {
            get
            {
                return WebConfigurationManager.AppSettings["HRMPassword"];
            }
        }
        private static string hrm_URL = @"http://grid:8090/hrm_ws/secure/persons";

        public static string HRM_FLAG = "-999";

        public static Response<Bundles.LederInfo[]> FinnLedereForAnsatt(string fornavn, string etternavn, string ansattNr)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;
            try
            {
                var response = client.DownloadString($"{hrm_URL}/name/firstname/{Uri.EscapeDataString(fornavn)}/lastname/{Uri.EscapeDataString(etternavn)}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var persons = doc.SelectNodes(@"//personsXML/person");

                foreach (XmlNode person in persons)
                {
                    var employeeId = person.SelectSingleNode("./employments/employment/employeeId").InnerText;
                    if (employeeId.Equals(ansattNr))
                    {
                        var positions = person.SelectNodes("./employments/employment/positions/position");

                        var ledere = new List<Bundles.LederInfo>();

                        foreach (XmlNode position in positions)
                        {
                            var hrmId = position.SelectSingleNode("./chart/unit/manager").Attributes["id"].Value;
                            var lederResult = FinnLeder(hrmId);
                            if (lederResult.Success)
                                ledere.Add(lederResult.Get());
                        }

                        return new Response<Bundles.LederInfo[]>(ledere.ToArray(), "OK", Codes.Code.OK);
                    }
                }

                return new Response<Bundles.LederInfo[]>(null, $"Ingen ansatte med dette navnet ({etternavn}) som stemte med ansattnr {ansattNr} ble funnet i HRM", Codes.Code.ERROR);
            }
            catch (Exception e)
            {
                return new Response<Bundles.LederInfo[]>(null, e.Message, Codes.Code.ERROR);
            }

        }

        public static Response<Bundles.LederInfo[]> FinnLedereForAnsatt(Bundles.AnsattInfo ansatt)
        {
            return FinnLedereForAnsatt(ansatt.Fornavn, ansatt.Etternavn, ansatt.AnsattNr);
        }

        public static Response<Bundles.LederInfo[]> FinnLedereForAnsatt(string fodselsnummer)
        {
            var res = FinnAnsatt(fodselsnummer);

            if (res.Success)
            {
                return FinnLedereForAnsatt(res.Get());
            }
            else
            {
                return new Response<Bundles.LederInfo[]>(null, res.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.LederInfo> FinnLeder(string userID)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;

            try
            {
                var response = client.DownloadString($"{hrm_URL}/id/{Uri.EscapeDataString(userID)}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var lederAD = doc.SelectSingleNode(@"//person/authentication/initials").InnerText;

                return ADHelper.GetLederBundle(lederAD);
            }
            catch (NullReferenceException)
            {
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, $"Ansatt med id {userID} ble ikke funnet i HRM", Codes.Code.ERROR);
            }
            catch (WebException wex)
            {
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, wex.Message, Codes.Code.ERROR);
            }
        }

        public static Response<string> FinnPersonnrForAnsatt(Bundles.AnsattInfo ansatt)
        {
            return FinnPersonnrForAnsatt(ansatt.Fornavn, ansatt.Etternavn, ansatt.AnsattNr);
        }

        public static Response<string> FinnPersonnrForAnsatt(string fornavn, string etternavn, string ansattNr)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;
            try
            {
                var response = client.DownloadString($"{hrm_URL}/name/firstname/{Uri.EscapeDataString(fornavn)}/lastname/{Uri.EscapeDataString(etternavn)}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var persons = doc.SelectNodes(@"//personsXML/person");

                foreach (XmlNode person in persons)
                {
                    var employments = person.SelectNodes(@"./employments/employment");
                    foreach (XmlNode employment in employments)
                    {
                        var employeeId = employment.SelectSingleNode("./employeeId").InnerText;
                        if (ansattNr.Equals(employeeId))
                        {
                            var ssn = person.SelectSingleNode(@"./ssn").InnerText;
                            return new Response<string>(ssn, "OK", Codes.Code.OK);
                        }
                    }
                }

                return new Response<string>(null, $"Ingen ansatte med dette navnet ({etternavn}) som stemte med ansattnr {ansattNr} ble funnet i HRM", Codes.Code.ERROR);
            }
            catch (Exception e)
            {
                return new Response<string>(null, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.AnsattInfo> FinnAnsatt(string fodselsnummer)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;
            try
            {
                var response = client.DownloadString($"{hrm_URL}/ssn/{fodselsnummer}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var fornavn = doc.SelectSingleNode(@"//person/givenName").InnerText;
                var etternavn = doc.SelectSingleNode(@"//person/familyName").InnerText;
                var ansattnr = doc.SelectSingleNode(@"//person/employments/employment/employeeId").InnerText;
                var ledereResult = FinnLedereForAnsatt(fornavn, etternavn, ansattnr);

                if (ledereResult.Success)
                {
                    var bundle = new Bundles.AnsattInfo
                    {
                        AnsattNr = ansattnr,
                        Fornavn = fornavn,
                        Etternavn = etternavn,
                        Lederliste = ledereResult.Get(),
                    };

                    return new Response<Bundles.AnsattInfo>(bundle, "OK", Codes.Code.OK);
                }
                else
                {
                    return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, ledereResult.Message, ledereResult.Code);
                }
            }
            catch (NullReferenceException)
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, StringConstants.ERROR_SSN_NOT_FOUND_HRM, Codes.Code.ERROR);
            }
            catch (WebException wex)
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, wex.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.AnsattInfo> FinnAnsatt(int personIdHRM)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;
            try
            {
                var response = client.DownloadString($"{hrm_URL}/id/{personIdHRM}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var fornavn = doc.SelectSingleNode(@"//person/givenName").InnerText;
                var etternavn = doc.SelectSingleNode(@"//person/familyName").InnerText;
                var ansattnr = doc.SelectSingleNode(@"//person/employments/employment/employeeId").InnerText;
                var ledereResult = FinnLedereForAnsatt(fornavn, etternavn, ansattnr);

                if (ledereResult.Success)
                {
                    var bundle = new Bundles.AnsattInfo
                    {
                        AnsattNr = ansattnr,
                        Fornavn = fornavn,
                        Etternavn = etternavn,
                        Lederliste = ledereResult.Get(),
                    };

                    return new Response<Bundles.AnsattInfo>(bundle, "OK", Codes.Code.OK);
                }
                else
                {
                    return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, ledereResult.Message, ledereResult.Code);
                }
            }
            catch (NullReferenceException)
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, StringConstants.ERROR_SSN_NOT_FOUND_HRM, Codes.Code.ERROR);
            }
            catch (WebException wex)
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, wex.Message, Codes.Code.ERROR);
            }
        }

        public static Response<List<AvtaleInfo>> HentAnsattStillingsInfo(Bundles.AnsattInfo ansatt, string innloggetAD)
        {
            var client = new WebClient { Credentials = new NetworkCredential(Username, Password) };
            client.Encoding = Encoding.UTF8;
            var resultList = new List<AvtaleInfo>(); // liste med alle stillinger som innlogget bruker er leder for denne personen
            try
            {
                var response = client.DownloadString($"{hrm_URL}/name/firstname/{Uri.EscapeDataString(ansatt.Fornavn)}/lastname/{Uri.EscapeDataString(ansatt.Etternavn)}");

                var doc = new XmlDocument();
                doc.LoadXml(response);

                var persons = doc.SelectNodes(@"//personsXML/person");

                foreach (XmlNode person in persons)
                {
                    var employments = person.SelectNodes(@"./employments/employment");
                    foreach (XmlNode employment in employments)
                    {
                        var employeeId = employment.SelectSingleNode("./employeeId").InnerText;
                        if (ansatt.AnsattNr.Equals(employeeId))
                        {
                            // hent alle stillinger til den ansatte
                            var positions = employment.SelectNodes(@"./positions/position");
                            foreach (XmlNode position in positions)
                            {
                                // finn nærmeste leder for denne stillingen
                                var managerIdHRM = position.SelectSingleNode(@"./chart/unit/manager").Attributes["id"].Value;
                                var lederHRM = FinnLeder(managerIdHRM);

                                if (lederHRM.Success)
                                {
                                    // sjekk at innlogget bruker er nærmeste leder for denne stillingen
                                    //if (innloggetAD.ToUpper().Equals(lederHRM.Get().AD.ToUpper()))
                                    {
                                        var stillingTittel = position.SelectSingleNode(@"./positionInfo/positionCode").Attributes["name"].Value;
                                        var stillingKode = position.SelectSingleNode(@"./positionInfo/positionCode").Attributes["positionCode"].Value;
                                        var fastAnsatt = position.SelectSingleNode(@"./positionInfo/positionType").Attributes["value"].Value;
                                        var prosent = position.SelectSingleNode(@"./positionPercentage").InnerText;
                                        var ssn = person.SelectSingleNode(@"./ssn").InnerText;
                                        var enhetNavn = position.SelectSingleNode(@"./chart/unit").Attributes["name"].Value;
                                        var arbeidssted = position.SelectSingleNode(@"./chart").Attributes["name"].Value;
                                        var enhetDim3 = position.SelectSingleNode(@"./costCentres/dimension3").Attributes["name"].Value;
                                        //enhetNavn = enhetNavn.Substring(0, 1).ToUpper() + enhetNavn.ToLower().Substring(1, enhetNavn.Length - 1);
                                        var lonn = position.SelectSingleNode(@"./salaryInfo/yearlySalary").InnerText;
                                        var timerPrUke = position.SelectSingleNode(@"./weeklyHours").InnerText;
                                        var fast = position.SelectSingleNode(@"./positionInfo/positionType ").Attributes["value"].Value;
                                        if (fast.ToUpper().Equals("F"))
                                        {
                                            var ai = new AvtaleInfo()
                                            {
                                                Navn = ansatt.Navn,
                                                Personnr = ssn,
                                                Arbeidssted = enhetNavn + " " + enhetDim3,
                                                TittelStilling = stillingKode + " " + stillingTittel,
                                                Lonn = lonn,
                                                Prosent = prosent,
                                                TimerPrUke = timerPrUke
                                            };

                                            resultList.Add(ai);
                                        }
                                        else
                                        {
                                            var sluttdato = position.SelectSingleNode(@"./positionEndDate").InnerText;
                                            var ai = new AvtaleInfoMidlertidlig()
                                            {
                                                Navn = ansatt.Navn,
                                                Personnr = ssn,
                                                Arbeidssted = arbeidssted,//enhetNavn + " " + enhetDim3,
                                                TittelStilling = stillingKode + " " + stillingTittel,
                                                Lonn = lonn,
                                                Hjemmel = fast.ToLower(),
                                                Prosent = prosent,
                                                Sluttdato = sluttdato
                                            };

                                            resultList.Add(ai);
                                        }
                                    }
                                }
                                else
                                {
                                    return new Response<List<AvtaleInfo>>(null, "Nærmeste leder ble ikke funnet i HRM", Codes.Code.ERROR);
                                }
                            }
                        }
                    }
                }

                return new Response<List<AvtaleInfo>>(resultList, "OK", Codes.Code.OK);
            }
            catch (Exception e)
            {
                return new Response<List<AvtaleInfo>>(null, e.Message, Codes.Code.ERROR);
            }
        }
    }
}