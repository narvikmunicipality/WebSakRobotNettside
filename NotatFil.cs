using System;
using System.IO;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Hjelpeklasse som holder på innholdet i notatteksten, konvertert til en pdf-fil
     */
    public class NotatFil : HttpPostedFileBase
    {
        private byte[] _documentBytes;
        private string _fileName;

        public override string ContentType
        {
            get
            {
                return "application/pdf";
            }
        }

        public override string FileName
        {
            get
            {
                return _fileName + ".pdf";
            }
        }

        public override Stream InputStream
        {
            get
            {
                return new MemoryStream(_documentBytes);
            }
        }

        public NotatFil(byte[] pdfFile, string name)
        {
            _documentBytes = pdfFile;
            _fileName = name;
        }


        public override void SaveAs(string filename)
        {
            try
            {
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(_documentBytes, 0, _documentBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                File.AppendAllText(@"C:\inetpub\logs\main_error.txt", ex.Message);
            }
        }
    }
}