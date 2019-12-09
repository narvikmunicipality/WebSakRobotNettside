using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    // Egen logge-klasse
    public class Logger
    {
        public static void Log(string filename, string arg)
        {
            try
            {
#if DEBUG
                Debug.WriteLine(arg);
#else
                System.IO.File.AppendAllText($"{Paths.LOG_DEFAULT}/{filename}", StringConstants.GetLogHeader() + Environment.NewLine + arg + Environment.NewLine);
#endif
            }
            catch (UnauthorizedAccessException)
            {
                // hvis vi havner her betyr det at prosessen kjører med robot-rettigheter der loggingen ble kalt
                Imposter.UndoImpersonation();
                Log(filename, arg);
                Imposter.ImpersonateRobot();
            }
        }
    }
}