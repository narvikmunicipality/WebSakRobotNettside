using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Hjelpeklasse som holder på innholdet i notatteksten, lagret som ren html
     */
    public class HtmlNotatFil : HttpPostedFileBase
    {
        private string _content;

        public override string ContentType
        {
            get
            {
                return "text/html";
            }
        }

        public override string FileName
        {
            get
            {
                return "svarut.html";
            }
        }

        public string Journalpostnavn { get; set; }

        public HtmlNotatFil(string content, string journalpostnavn)
        {
            _content = $"<html>{content}</html>";
            Journalpostnavn = journalpostnavn;
        }


        public override void SaveAs(string filename)
        {
            System.IO.File.WriteAllText(filename, _content);
        }
    }
}