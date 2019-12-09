using Digipost.Signature.Api.Client.Core;
using Digipost.Signature.Api.Client.Portal;

namespace WebSakFilopplaster.Net_AD
{
    public class CustomPortalClient : PortalClient
    {
        public string DocumentFileName { get; set; } // filnavnet det signerte dokumentet skal lagres som
        public string QueueID { get; private set; }

        private static readonly char split_token = '|';

        private readonly static string org_nr = "959469059"; // Narvik kommune
        private readonly static string certificate_thumbprint = "53 2f b2 10 5a 50 6c 66 3e 30 7b 30 1d 75 04 84 e9 61 de 52";

        public CustomPortalClient(string queueID, string documentFileName)
            : base(new ClientConfiguration(Environment.Production, certificate_thumbprint, new Sender(org_nr, new PollingQueue(queueID))))
        {
            QueueID = queueID;
            DocumentFileName = documentFileName.Replace(" ", "_");
        }

        public string GetReference()
        {
            return $"{QueueID}{split_token}{DocumentFileName}";
        }

        // Hjelpemetode som instansierer en klient basert på referanse lagret i fil
        public static CustomPortalClient FromReference(string reference)
        {
            var parts = reference.Split(split_token);
            return new CustomPortalClient(parts[0], parts[1]);
        }
    }
}