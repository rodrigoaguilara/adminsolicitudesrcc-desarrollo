using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class EstadoMopViewModel
    {
        public string ESTADO { get; set; }
        public int? MES_RENOVACION { get; set; }
        public string COD_ESTADO { get; set; }
        public string FECHA_INGRESO_REG { get; set; } // Lo mantenemos como string si así viene de la BD
        public string FECHA_SUSPENSION_REG { get; set; } // Lo mantenemos como string si así viene de la BD
        public int NUM_SOLICITUD_ASOCIADA { get; set; }

    }
}