using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class StatHelper
    {
        private static Random _rand = new Random();
        private static List<LogEntry> _logEntries;

        public static void Init()
        {
            _logEntries = new List<LogEntry>();
            Imposter.ImpersonateRobot();
            ReadLogFiles("Sykefravær", Mappetype.Sykefravær);
            ReadLogFiles("Ansattforhold", Mappetype.Ansattforhold);
            ReadJsonLogFile();
            Imposter.UndoImpersonation();
            var k = 42;
        }

        private static void ReadLogFiles(string mappenavn, int type)
        {
            var filbane = $@"{Paths.UNC_ADDRESS}{Paths.PORTAL_LOG}{mappenavn}";

            if (Directory.Exists(filbane))
            {
                var logFiles = Directory.GetFiles(filbane);

                foreach (var file in logFiles)
                {
                    var content = File.ReadAllLines(file);

                    foreach (var line in content)
                    {
                        _logEntries.Add(new LogEntry() { Date = file.Split('\\').Last().Replace(".txt", ""), Name = line, Type = type });
                    }
                }
            }
        }

        public static Response<List<LogEntry>> GetOppfølginger()
        {
            var filbane = $@"{Paths.UNC_ADDRESS}{Paths.PORTAL_LOG}Sykefravær";
            Imposter.ImpersonateRobot();
            if (Directory.Exists(filbane))
            {
                var logFiles = Directory.GetFiles(filbane);
                var result = new List<LogEntry>();

                foreach (var file in logFiles)
                {
                    var content = File.ReadAllLines(file);

                    foreach (var line in content)
                    {
                        if (line.StartsWith("Sykefraværsoppfølging")) // Kun sykefraværsoppfølginger skal telles
                            result.Add(new LogEntry() { Date = file.Split('\\').Last().Replace(".txt", ""), Name = line, Type = Mappetype.Sykefravær });
                    }
                }
                Imposter.UndoImpersonation();
                return new Response<List<LogEntry>>(result, "OK", Codes.Code.OK);
            }
            else
            {
                Imposter.UndoImpersonation();
                return new Response<List<LogEntry>>(null, $"Mappen {filbane} ble ikke funnet", Codes.Code.ERROR);
            }
        }

        private static void ReadJsonLogFile()
        {
            // historisk opplastingsdata fra excel lagret i json-format, frem til ca. 1.6.2019
            var filbane = $@"{Paths.UNC_ADDRESS}{Paths.PORTAL_LOG}json_data.txt";

            if (File.Exists(filbane))
            {
                var json = File.ReadAllText(filbane);

                json = json.Replace("}", "").Replace("{", "").Replace("\"", "");

                var jParts = json.Split(',');

                foreach (var s in jParts)
                {
                    var w = s.Trim();

                    var ts = long.Parse(w.Split(':').First());
                    var cn = int.Parse(w.Split(':').Last());

                    for (int i = 0; i < cn; i++)
                        _logEntries.Add(new LogEntry() { Date = TimeHelper.GetDateFromUnix(ts) });
                }

                var k = 3;
            }
        }

        public static List<LogEntry> GetData()
        {
            if (_logEntries == null)
                Init();

            return _logEntries;
        }

        public static string GetJsonData()
        {
            if (_logEntries == null)
                Init();
            var json = "{";
            for (int i = 0; i < _logEntries.Count; i++)
            {
                var count = GetUploadsForDay(_logEntries[i].Date).Count();
                var timeStamp = TimeHelper.GetUnixTime(_logEntries[i].Date);
                json += $"\"{timeStamp}\": {count}" + ((i + 1) < _logEntries.Count ? "," : "");
            }
            return json + "}";
        }

        public static List<LogEntry> GetUploadsToday()
        {
            var today = DateTime.Now.ToString("dd.MM.yyyy");
            return GetUploadsForDay(today);
        }

        public static List<LogEntry> GetUploadsForDay(string day)
        {
            if (_logEntries == null)
                Init();
            return _logEntries.Where(l => l.Date.Equals(day)).ToList();
        }

        public static Response<List<VarselWeb>> GetVarselLogg()
        {
            Imposter.ImpersonateRobot();

            if (File.Exists(Paths.VARSEL_WEB_FILE))
            {
                var list = new List<VarselWeb>();
                var content = File.ReadAllLines(Paths.VARSEL_WEB_FILE, System.Text.Encoding.Default);
                foreach (var line in content)
                {
                    var lineContent = line.Split(';');
                    if (!"".Equals(lineContent[5]) && !"".Equals(lineContent[7]) && !lineContent[5].Equals("Behandles av"))
                        list.Add(new VarselWeb(lineContent[1], lineContent[3], lineContent[5], lineContent[6]));
                }

                var dateMod = File.GetLastWriteTime(Paths.VARSEL_WEB_FILE).ToString("dd/MM/yyyy 'kl.'hh/mm/ss");

                Imposter.UndoImpersonation();
                return new Response<List<VarselWeb>>(list, $"Sist oppdatert {dateMod}", Codes.Code.OK);
            }
            else
            {
                Imposter.UndoImpersonation();
                return new Response<List<VarselWeb>>(null, $"Filen {Paths.VARSEL_WEB_FILE} ble ikke funnet", Codes.Code.ERROR);
            }
        }
    }

    // Hjelpeklasse for å organisere loggdata for opplastinger fra disk
    public class LogEntry
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public long TimeStamp { get; set; }
        public int Type { get; set; }
    }

    public class VarselWeb
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public string Status { get; set; }

        public VarselWeb(string name, string date, string user, string status)
        {
            Name = name;
            Date = date;
            User = user;
            Status = status;
        }
    }

    public class VarselLeder
    {
        public string User { get; set; }
        public string Dag5 { get; set; }
        public string Dag14 { get; set; }
        public string Dag28 { get; set; }

        public double PS { get; set; } = 100;

        public VarselLeder(string user, string dag5, string dag14, string dag28)
        {
            User = user;
            Dag5 = dag5;
            Dag14 = dag14;
            Dag28 = dag28;
        }
    }
}