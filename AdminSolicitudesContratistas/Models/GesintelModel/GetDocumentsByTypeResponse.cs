using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentsByTypeResponse
    {
        public class Record
        {
            public string id { get; set; }
            public long? creationDate { get; set; }
            public string rut { get; set; }
            public string name { get; set; }
            public string category { get; set; }
            public object email { get; set; }
            public string type { get; set; }
            public string extId { get; set; }
            public object userId { get; set; }
            public object userName { get; set; }
            public object clientId { get; set; }
            public object subcliente { get; set; }
            public object amlStatus { get; set; }
            public object pdfFile { get; set; }
            public object basesAml { get; set; }
        }

        public class Document
        {
            public string id { get; set; }
            public long? creationDate { get; set; }
            public string clientId { get; set; }
            public int folio { get; set; }
            public int folio2 { get; set; }
            public string type { get; set; }
            public Record record { get; set; }
            public object stage { get; set; }
            public string statusAsig { get; set; }
            public string origin { get; set; }
            public object asignedTo { get; set; }
            public object asignedToId { get; set; }
            public object dateAsigned { get; set; }
            public object daysAsigned { get; set; }
            public object dateLastReq { get; set; }
            public int nroReqs { get; set; }
            public object priorityLastReq { get; set; }
            public object prevStage { get; set; }
            public object datePrevStage { get; set; }
            public object daysStage { get; set; }
            public string name { get; set; }
            public int size { get; set; }
            public object validity { get; set; }
            public int nroTareasComp { get; set; }
            public string description { get; set; }
        }

        public class Root
        {
            public List<Document> documents { get; set; }
            public string status { get; set; }
        }
    }
}