using System;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Klasse som inneholder små containere for diverse info som flyttes rundt inne i løsningen
     * (c) Andreas DJ 2019
     */
    public class Bundles
    {
        // hjelpecontainer for strukturert ansattinfo
        [Serializable]
        public struct AnsattInfo
        {
            public string AnsattNr { get; set; }
            public string Fornavn { get; set; }
            public string Etternavn { get; set; }
            public string AnsattEPost { get; set; }
            public bool IsTest { get; set; }
            public LederInfo[] Lederliste { get; set; }

            public string Navn { get { return (Fornavn + " " + Etternavn).Trim(); } }
            public static AnsattInfo Empty { get { return new AnsattInfo(); } }
            public static AnsattInfo Skipped { get { return new AnsattInfo() { AnsattNr = "-1", Fornavn = "person utenfor organisasjonen" }; } }
        }

        // hjelpecontainer for strukturert lederinfo
        [Serializable]
        public struct LederInfo
        {
            public string AD;
            public string EPost;
            public string Enhet;
            public string Navn;
            public bool MottaFeilmeldinger;

            public static LederInfo Empty { get { return new LederInfo(); } }
        }

        // inneholder info om et aktivt e-signeringsoppdrag, vises på "min side" under aktive signeringsoppdrag
        public struct ESignInfo
        {
            public string Dato { get; set; }
            public string Mottaker { get; set; }
            public string MottakerEpost { get; set; }
            public string SignDato { get; set; }
            public string Status { get; set; }
            public string JournalpostID { get; set; } // Where we're going, we don't need WebSak
            public bool Signert { get; set; }
        }
    }
}