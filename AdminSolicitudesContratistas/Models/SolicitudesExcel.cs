using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class SolicitudesExcel
    {
        public int ID { get; set; }
        public string ID_SOL_PADRE { get; set; }
        public string RUT { get; set; }
        public string RAZON_SOCIAL { get; set; }
        public string ESTADO { get; set; }
        public string TIPO_SOL { get; set; }
        public string TIPO_REG { get; set; }
        //public int COD_REGION { get; set; }
        public string REGION { get; set; }
        public string FECHA_RECEPCION{ get; set; }
        public string FECHA_INGRESO { get; set; }
        public string FECHA_ANALISIS_TEC { get; set; }
        public string ESTADO_ANALISIS_TEC { get; set; }
        public string FECHA_ANALISIS_LEG { get; set; }
        public string ESTADO_ANALISIS_LEG { get; set; }
        public string FECHA_ANALISIS_CONTABLE { get; set; }
        public string ESTADO_ANALISIS_CONTABLE { get; set; }
        public string FECHA_RESPUESTA { get; set; }
        public int DIAS_DESDE_INGRESO { get; set; }

    }
}