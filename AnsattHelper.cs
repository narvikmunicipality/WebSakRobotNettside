using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Hjelpeklasse som lager fiktiv ansattinformasjon under testing
     */
    public class AnsattHelper
    {

        public static List<Bundles.LederInfo> Superbrukere { get; private set; } = new List<Bundles.LederInfo>
        {
            new Bundles.LederInfo { AD = "ANDDYR", EPost = "andreas.jansson@narvik.kommune.no", MottaFeilmeldinger = true},
            new Bundles.LederInfo { AD = "ROBOEN", EPost = "roboen@narvik.kommune.no", MottaFeilmeldinger = true},
        };

        public static List<Bundles.LederInfo> Raadmenn { get; private set; } = new List<Bundles.LederInfo>();

        public static List<Bundles.AnsattInfo> Testbrukere { get; private set; } = new List<Bundles.AnsattInfo>
        {
            new Bundles.AnsattInfo { AnsattNr = "12345", Fornavn = "Adagio", Etternavn = "Dazzle" },
            new Bundles.AnsattInfo { AnsattNr = "12344", Fornavn = "Donald", Etternavn = "Duck" },
            new Bundles.AnsattInfo { AnsattNr = "12321", Fornavn = "Klaus", Etternavn = "Knegg" },
            new Bundles.AnsattInfo { AnsattNr = "76554", Fornavn = "Obi-Wan",Etternavn = "Kenobi" },
            new Bundles.AnsattInfo { AnsattNr = "10374", Fornavn = "Qui-Gon", Etternavn = "Jinn" },
            new Bundles.AnsattInfo { AnsattNr = "27384", Fornavn = "James T.", Etternavn = "Kirk" },
            new Bundles.AnsattInfo { AnsattNr = "46573", Fornavn = "Shrek" },
            new Bundles.AnsattInfo { AnsattNr = "98716", Fornavn = "Stjernesnurr", Etternavn = "Den Skjeggete" },
            new Bundles.AnsattInfo { AnsattNr = "18982", Fornavn = "Stilongsmannen" },
            new Bundles.AnsattInfo { AnsattNr = "27183", Fornavn = "Bruce", Etternavn = "Wayne" },
            new Bundles.AnsattInfo { AnsattNr = "36271", Fornavn = "Gloriosa",Etternavn = "Daisy" },
            new Bundles.AnsattInfo { AnsattNr = "10374", Fornavn = "Chrisp", Etternavn = "Trott" },
        };

        public static void Init()
        {
            Imposter.ImpersonateRobot();
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(Paths.TEST_USERS))
            {
                var stream = new FileStream(Paths.TEST_USERS, FileMode.Open, FileAccess.Read);
                Testbrukere = (List<Bundles.AnsattInfo>)formatter.Deserialize(stream);
                stream.Close();
            }

            if (File.Exists(Paths.SUPER_USERS))
            {
                var stream = new FileStream(Paths.SUPER_USERS, FileMode.Open, FileAccess.Read);
                Superbrukere = (List<Bundles.LederInfo>)formatter.Deserialize(stream);
                stream.Close();
            }

            if (File.Exists(Paths.RAADMENN_USERS))
            {
                var stream = new FileStream(Paths.RAADMENN_USERS, FileMode.Open, FileAccess.Read);
                Raadmenn = (List<Bundles.LederInfo>)formatter.Deserialize(stream);
                stream.Close();
            }

            Imposter.UndoImpersonation();
        }

        public static Response<int> AddTestUser(string id, string fornavn, string etternavn)
        {
            var user = Testbrukere.Where(t => t.AnsattNr.Equals(id)).FirstOrDefault();

            if (Bundles.AnsattInfo.Empty.Equals(user))
            {
                // legg til brukeren i listen og overskriv filen
                Testbrukere.Add(new Bundles.AnsattInfo { AnsattNr = id, Fornavn = fornavn, Etternavn = etternavn });
                var result = WriteObject(Paths.TEST_USERS, Testbrukere);

                if (result.Success)
                {
                    return new Response<int>(0, string.Format(StringConstants.CREATE_TESTUSER_OK, id), Codes.Code.OK);
                }
                else
                {
                    return new Response<int>(1, result.Message, Codes.Code.ERROR);
                }
            }
            else
            {
                return new Response<int>(1, string.Format(StringConstants.CREATE_TESTUSER_DUPLICATE, id), Codes.Code.ERROR);
            }
        }

        public static Response<int> DeleteTestUser(string ansattNr)
        {
            var userToRemove = Testbrukere.Where(t => t.AnsattNr.Equals(ansattNr)).FirstOrDefault();
            if (!Bundles.AnsattInfo.Empty.Equals(userToRemove))
            {
                // fjern brukeren fra listen og overskriv filen
                Testbrukere.Remove(userToRemove);
                var result = WriteObject(Paths.TEST_USERS, Testbrukere);

                if (result.Success)
                {
                    return new Response<int>(0, string.Format(StringConstants.DELETE_TESTUSER_OK, ansattNr), Codes.Code.OK);
                }
                else
                {
                    return new Response<int>(1, result.Message, Codes.Code.ERROR);
                }
            }
            else
            {
                return new Response<int>(0, string.Format(StringConstants.DELETE_TESTUSER_ERROR, ansattNr), Codes.Code.ERROR);
            }
        }

        private static Response<int> WriteObject(string filename, object obj)
        {
            try
            {
                Imposter.ImpersonateRobot();
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, obj);
                stream.Close();
                Imposter.UndoImpersonation();
                return new Response<int>(0, "Great success", Codes.Code.OK);
            }
            catch (IOException e)
            {
                return new Response<int>(1, e.Message, Codes.Code.ERROR);
            }
        }

        public static Response<int> AddSuperUser(string ADbrukernavn)
        {
            if (ADbrukernavn != null)
            {
                ADbrukernavn = ADbrukernavn.ToUpper();
                var user = Superbrukere.Where(t => t.AD.ToUpper().Equals(ADbrukernavn.ToUpper())).FirstOrDefault();

                if (Bundles.LederInfo.Empty.Equals(user))
                {
                    var result = ADHelper.GetLederBundle(ADbrukernavn);
                    if (result.Success)
                    {
                        Superbrukere.Add(result.Get());

                        var ioResult = WriteObject(Paths.SUPER_USERS, Superbrukere);

                        if (ioResult.Success)
                        {
                            MailHelper.SendAddSuperUserMail(result.Get().EPost);
                            return new Response<int>(0, string.Format(StringConstants.SUPERUSER_ACCESS_GRANT, ADbrukernavn), Codes.Code.OK);
                        }
                        else
                        {
                            return new Response<int>(1, ioResult.Message, Codes.Code.ERROR);
                        }
                    }
                    else
                    {
                        return new Response<int>(1, result.Message, result.Code);
                    }
                }
                else
                {
                    return new Response<int>(1, string.Format(StringConstants.SUPERUSER_ACCES_DUPLICATE, ADbrukernavn), Codes.Code.ERROR);
                }
            }
            else
            {
                return new Response<int>(1, StringConstants.ERROR_INVALID_ARGUMENT, Codes.Code.ERROR);
            }
        }

        public static Response<int> ModifySuperUser(Bundles.LederInfo superUser)
        {
            int index = Superbrukere.IndexOf(Superbrukere.Where(s => s.AD.ToUpper().Equals(superUser.AD.ToUpper())).FirstOrDefault());
            if (index != -1)
            {
                Superbrukere[index] = superUser;
            }
            var ioResult = WriteObject(Paths.SUPER_USERS, Superbrukere);

            if (ioResult.Success)
            {
                return new Response<int>(0, StringConstants.SUPERUSER_ACCESS_MODIFY, Codes.Code.OK);
            }
            else
            {
                return new Response<int>(1, ioResult.Message, Codes.Code.ERROR);
            }
        }

        public static Response<int> DeleteSuperUser(string AD)
        {
            var userToRemove = Superbrukere.Where(t => t.AD.Equals(AD)).FirstOrDefault();
            if (!Bundles.LederInfo.Empty.Equals(userToRemove))
            {
                Superbrukere.Remove(userToRemove);
                var ioResponse = WriteObject(Paths.SUPER_USERS, Superbrukere);
                if (ioResponse.Success)
                {
                    MailHelper.SendDeleteSuperUserMail(userToRemove.EPost);
                    return new Response<int>(0, string.Format(StringConstants.SUPERUSER_ACCESS_REVOKE, AD), Codes.Code.OK);
                }
                else
                    return new Response<int>(1, ioResponse.Message, Codes.Code.ERROR);
            }
            else
            {
                var msg = "Feil: Administratortilgangen til " + AD + "ble ikke fjernet. Prøv på nytt senere";
                return new Response<int>(1, msg, Codes.Code.ERROR);
            }
        }

        public static Response<int> AddRaadmann(string epostadresse)
        {
            if (epostadresse != null)
            {
                var user = Raadmenn.Where(t => t.EPost.ToLower().Equals(epostadresse.ToLower())).FirstOrDefault();

                if (Bundles.LederInfo.Empty.Equals(user))
                {
                    var resultAnsatt = ADHelper.GetAnsattNr(epostadresse);
                    if (resultAnsatt.Success)
                    {
                        var resultLeder = ADHelper.GetLederBundle(resultAnsatt.Get());

                        if (resultLeder.Success)
                        {
                            Raadmenn.Add(resultLeder.Get());

                            var ioResult = WriteObject(Paths.RAADMENN_USERS, Raadmenn);

                            if (ioResult.Success)
                            {
                                MailHelper.SendAddRaadmannUserMail(resultLeder.Get().EPost);
                                var msg = epostadresse + " har nå tilgang til å se logger på app.uipath";
                                return new Response<int>(0, msg, Codes.Code.OK);
                            }
                            else
                            {
                                return new Response<int>(1, ioResult.Message, Codes.Code.ERROR);
                            }
                        }
                        else
                        {
                            return new Response<int>(1, resultLeder.Message, resultLeder.Code);
                        }
                    }
                    else
                    {
                        return new Response<int>(1, resultAnsatt.Message, resultAnsatt.Code);
                    }
                }
                else
                {
                    var msg = "Feil: " + epostadresse + " har allerede tilgang";
                    return new Response<int>(1, msg, Codes.Code.ERROR);
                }
            }
            else
            {
                var msg = "Feil: Ugyldig parameter oppgitt. Prøv på nytt senere";
                return new Response<int>(1, msg, Codes.Code.ERROR);
            }
        }

        public static Response<int> DeleteRaadmann(string AD)
        {
            var userToRemove = Raadmenn.Where(t => t.AD.Equals(AD)).FirstOrDefault();
            if (!Bundles.LederInfo.Empty.Equals(userToRemove))
            {
                Raadmenn.Remove(userToRemove);
                var msg = "Tilgangen til " + userToRemove.EPost + " har blitt fjernet";

                var ioResponse = WriteObject(Paths.RAADMENN_USERS, Raadmenn);
                if (ioResponse.Success)
                {
                    MailHelper.SendDeleteRaadmannUserMail(userToRemove.EPost);
                    return new Response<int>(0, msg, Codes.Code.OK);
                }
                else
                    return new Response<int>(1, ioResponse.Message, Codes.Code.ERROR);
            }
            else
            {
                var msg = "Feil: Tilgangen til " + userToRemove.EPost + "ble ikke fjernet. Prøv på nytt senere";
                return new Response<int>(1, msg, Codes.Code.ERROR);
            }
        }

        public static Response<Bundles.AnsattInfo> GetTestUser(string ansattnr, string userAD)
        {
            try
            {
                var user = Testbrukere.Where(b => b.AnsattNr.Equals(ansattnr)).First();
                user.Lederliste = Superbrukere.ToArray();
                user.IsTest = true;
                return new Response<Bundles.AnsattInfo>(user, "OK", Codes.Code.OK);
            }
            catch
            {
                var bundle = new Bundles.AnsattInfo
                {
                    AnsattNr = "-1",
                    Fornavn = "These aren't the droids you're looking for",
                    Lederliste = new Bundles.LederInfo[] { new Bundles.LederInfo { AD = userAD } },
                    IsTest = true
                };
                return new Response<Bundles.AnsattInfo>(bundle, "OK", Codes.Code.OK);
            }

        }

        public static bool IsUserSuper(string userAD)
        {
            userAD = userAD.Replace(@"NARKOM\", "");

            var dude = Superbrukere.Where(b => b.AD.ToUpper().Equals(userAD.ToUpper())).ToList();
            return dude.Count() > 0;
        }

        public static bool IsUserRaadmann(string userAD)
        {
            if (IsUserSuper(userAD))
                return true;
            else
            {
                userAD = userAD.Replace(@"NARKOM\", "");

                var dude = Raadmenn.Where(b => b.AD.ToUpper().Equals(userAD.ToUpper())).ToList();
                return dude.Count() > 0;
            }
        }
    }
}