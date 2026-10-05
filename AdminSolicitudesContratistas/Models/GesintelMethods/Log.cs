using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace AdminSolicitudesContratistas.Models.GesintelMethods
{
    public class Log
    {
        public static void EscribeLogEventViewer(TraceLevel level, String messageText)
        {

            EventLogEntryType LogEntryType;
            switch (level)
            {
                case TraceLevel.Error:
                    LogEntryType = EventLogEntryType.Error;
                    break;
                default:
                    LogEntryType = EventLogEntryType.Information;
                    break;
            }
            String LogName = "Application";
            if (!EventLog.SourceExists(LogName))
            {
                EventLog.CreateEventSource(LogName, "GesintelApi");
            }

            EventLog eventLog = new EventLog(LogName, ".", LogName);
            eventLog.WriteEntry(messageText, LogEntryType);

        }
    }
}