using System;
using System.DirectoryServices;
using System.Linq;
using System.Web.Configuration;

namespace WebSakFilopplaster.Net_AD
{
    public class ADHelper
    {
        private static string Username
        {
            get
            {
                return WebConfigurationManager.AppSettings["LDAPUsername"];
            }
        }
        private static string Password
        {
            get
            {
                return WebConfigurationManager.AppSettings["LDAPPassword"];
            }
        }

        public static Response<Bundles.AnsattInfo> GetADBrukerForEpost(string epost)
        {
            var ansattNrResponse = GetAnsattNr(epost);
            if (ansattNrResponse.Success)
            {
                return GetADBruker(ansattNrResponse.Get());
            }
            else
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, ansattNrResponse.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.AnsattInfo> GetADBruker(int ansattNr)
        {
            return GetADBruker(ansattNr.ToString());
        }

        private static Response<Bundles.AnsattInfo> GetADBruker(string ansattNr)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("employeeID");
                        dSearcher.PropertiesToLoad.Add("givenName");
                        dSearcher.PropertiesToLoad.Add("sn");
                        dSearcher.PropertiesToLoad.Add("mail");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        // filtrerer ut deaktiverte brukere
                        // https://forums.asp.net/t/1172159.aspx?using+C+to+access+active+directory+and+pull+active+users+ 21.05.2019
                        dSearcher.Filter = $"(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2)(employeeID={ansattNr}))";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {
                            var value = resultAD.GetDirectoryEntry();
                            var lederResult = GetInfoForLeder(Convert.ToInt32(ansattNr));

                            if (lederResult.Success)
                            {
                                var ansattEpost = "";

                                if (value.Properties["mail"] != null)
                                {
                                    var mailValue = value.Properties["mail"].Value;
                                    if (mailValue != null)
                                        ansattEpost = value.Properties["mail"].Value.ToString();
                                }

                                var bundle = new Bundles.AnsattInfo
                                {
                                    AnsattNr = ansattNr,
                                    Fornavn = value.Properties["givenName"].Value.ToString(),
                                    Etternavn = value.Properties["sn"].Value.ToString(),
                                    AnsattEPost = ansattEpost,
                                    Lederliste = new Bundles.LederInfo[] { lederResult.Get() }
                                };

                                return new Response<Bundles.AnsattInfo>(bundle, "OK", Codes.Code.OK);
                            }
                            else
                            {
                                var bundle = new Bundles.AnsattInfo
                                {
                                    AnsattNr = ansattNr,
                                    Fornavn = value.Properties["givenName"].Value.ToString(),
                                    Etternavn = value.Properties["sn"].Value.ToString(),
                                    //AnsattEPost = value.Properties["mail"].Value.ToString(),
                                    Lederliste = HRMHelper.FinnLedereForAnsatt(value.Properties["givenName"].Value.ToString(), value.Properties["sn"].Value.ToString(), ansattNr).Get(),
                                };


                                return new Response<Bundles.AnsattInfo>(bundle, lederResult.Message, Codes.Code.OK);
                            }
                        }
                        else
                        {
                            return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, string.Format(StringConstants.WARN_ID_NOT_FOUND_AD, ansattNr), Codes.Code.WARNING);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.AnsattInfo> GetAnsattInfoForLeder(string lederAD)
        {
            var lederBundle = GetLederBundle(lederAD);
            if (lederBundle.Success)
            {
                return GetADBrukerForEpost(lederBundle.Get().EPost);
            }
            else
            {
                return new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Empty, lederBundle.Message, Codes.Code.ERROR);
            }
        }

        public static Response<string> GetAnsattEpost(string ansattNr)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("employeeID");
                        dSearcher.PropertiesToLoad.Add("mail");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        // filtrerer ut deaktiverte brukere
                        // https://forums.asp.net/t/1172159.aspx?using+C+to+access+active+directory+and+pull+active+users+ 21.05.2019
                        dSearcher.Filter = $"(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2)(employeeID={ansattNr}))";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {
                            var value = resultAD.GetDirectoryEntry();
                            var epost = value.Properties["mail"].Value.ToString();
                            return new Response<string>(epost, "OK", Codes.Code.OK);
                        }
                        else
                        {
                            return new Response<string>(string.Empty, "", Codes.Code.WARNING);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response<string>(string.Empty, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.LederInfo> GetLederBundle(string lederAD)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("mail");
                        dSearcher.PropertiesToLoad.Add("department");
                        dSearcher.PropertiesToLoad.Add("displayName");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        dSearcher.Filter = $"sAMAccountName={lederAD}";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {

                            var mail = resultAD.GetDirectoryEntry().Properties["mail"].Value.ToString();

                            //var dept = resultAD.GetDirectoryEntry().Properties["department"];
                            //var deptValue = dept.Value;

                            var lederEnhet = resultAD.GetDirectoryEntry().Properties["department"].Value?.ToString();
                            var ledernavn = resultAD.GetDirectoryEntry().Properties["displayName"].Value.ToString();

                            var bundle = new Bundles.LederInfo()
                            {
                                AD = lederAD,
                                EPost = mail,
                                Enhet = lederEnhet,
                                Navn = ledernavn
                            };

                            return new Response<Bundles.LederInfo>(bundle, "OK", Codes.Code.OK);
                        }
                        else
                        {
                            return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, string.Format(StringConstants.ERROR_USRN_NOT_FOUND_AD, lederAD), Codes.Code.ERROR);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.LederInfo> GetLederBundle(int ansattNR)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("mail");
                        dSearcher.PropertiesToLoad.Add("department");
                        dSearcher.PropertiesToLoad.Add("displayName");
                        dSearcher.PropertiesToLoad.Add("sAMAccountName");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        dSearcher.Filter = $"employeeID={ansattNR}";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {

                            var mail = resultAD.GetDirectoryEntry().Properties["mail"].Value.ToString();

                            //var dept = resultAD.GetDirectoryEntry().Properties["department"];
                            //var deptValue = dept.Value;

                            var lederAD = resultAD.GetDirectoryEntry().Properties["sAMAccountName"].Value.ToString();
                            var lederEnhet = resultAD.GetDirectoryEntry().Properties["department"].Value?.ToString();
                            var ledernavn = resultAD.GetDirectoryEntry().Properties["displayName"].Value.ToString();

                            var bundle = new Bundles.LederInfo()
                            {
                                AD = lederAD,
                                EPost = mail,
                                Enhet = lederEnhet,
                                Navn = ledernavn
                            };

                            return new Response<Bundles.LederInfo>(bundle, "OK", Codes.Code.OK);
                        }
                        else
                        {
                            return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, string.Format(StringConstants.ERROR_ID_NOT_FOUND_AD, ansattNR), Codes.Code.ERROR);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<int> GetAnsattNr(string ansattEpost)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("employeeID");
                        dSearcher.PropertiesToLoad.Add("mail");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        dSearcher.Filter = $"mail={ansattEpost}";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {
                            var noe = resultAD.GetDirectoryEntry();
                            var id = int.Parse(noe.Properties["employeeID"].Value.ToString());
                            return new Response<int>(id, "OK", Codes.Code.OK);
                        }
                        else
                        {
                            return new Response<int>(-1, string.Format(StringConstants.ERROR_MAIL_NOT_FOUND_AD, ansattEpost), Codes.Code.WARNING);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response<int>(-1, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.LederInfo> GetInfoForLeder(string epostAnsatt)
        {
            var ansattNrResult = GetAnsattNr(epostAnsatt);
            if (ansattNrResult.Success)
                return GetInfoForLeder(ansattNrResult.Get());
            else
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, ansattNrResult.Message, Codes.Code.ERROR);
        }

        public static Response<Bundles.LederInfo> GetInfoForLeder(int ansattNr)
        {
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry("", Username, Password))
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(searchRoot))
                    {
                        dSearcher.PropertiesToLoad.Add("employeeID");
                        dSearcher.PropertiesToLoad.Add("manager");
                        dSearcher.PropertiesToLoad.Add("sAMAccountName");
                        dSearcher.ReferralChasing = ReferralChasingOption.All;
                        dSearcher.SearchScope = SearchScope.Subtree;
                        // filtrerer ut deaktiverte brukere
                        // https://forums.asp.net/t/1172159.aspx?using+C+to+access+active+directory+and+pull+active+users+ 21.05.2019
                        dSearcher.Filter = $"(&(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2)(employeeID={ansattNr}))";

                        var resultAD = dSearcher.FindOne();

                        if (resultAD != null)
                        {
                            var entry = resultAD.GetDirectoryEntry();
                            var manVar = entry.Properties["manager"].Value;
                            if (manVar != null)
                            {
                                var manager = manVar.ToString();

                                var lederAD = manager.Split(',')[0].Split('-').Last().Trim(); // på formen 'Fornavn Etternavn - FORETT'
                                                                                              //var lederEnhet = manager.Split(',')[1].Split('-').Last().Trim(); // på formen 'NAR_<kode> - Enhetsnavn'

                                dSearcher.Filter = $"sAMAccountName={lederAD}";

                                // nytt oppslag for å finne leders epostadresse
                                var resultLeder = dSearcher.FindOne();

                                if (resultLeder != null)
                                {
                                    var mail = resultLeder.GetDirectoryEntry().Properties["mail"].Value.ToString();
                                    var lederEnhet = resultLeder.GetDirectoryEntry().Properties["department"].Value.ToString();

                                    var bundle = new Bundles.LederInfo()
                                    {
                                        AD = lederAD,
                                        EPost = mail,
                                        Enhet = lederEnhet
                                    };

                                    return new Response<Bundles.LederInfo>(bundle, "OK", Codes.Code.OK);
                                }
                                else
                                {
                                    return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, string.Format(StringConstants.ERROR_MANAGER_NOT_FOUND_AD, ansattNr), Codes.Code.ERROR);
                                }
                            }
                            else
                            {
                                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, string.Format(StringConstants.ERROR_MANAGER_NOT_FOUND_AD, ansattNr), Codes.Code.ERROR);
                            }

                        }
                        else
                        {
                            return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, string.Format(StringConstants.ERROR_ID_NOT_FOUND_AD, ansattNr), Codes.Code.ERROR);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                return new Response<Bundles.LederInfo>(Bundles.LederInfo.Empty, e.Message, Codes.Code.ERROR);
            }
        }
    }
}