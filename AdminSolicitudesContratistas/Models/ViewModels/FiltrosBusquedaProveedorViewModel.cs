using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{


    public class FiltroItemViewModel
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    }

    // Una clase específica para las regiones
    public class RegionFiltroViewModel
    {
        public string CodRegion { get; set; }
        public string NombreRegion { get; set; }
    }


    public class FiltrosBusquedaProveedorViewModel
    {
        public List<FiltroItemViewModel> TiposRegistro { get; set; }
        public List<FiltroItemViewModel> EstadosProveedor { get; set; }
        public List<RegionFiltroViewModel> Regiones { get; set; }

        public FiltrosBusquedaProveedorViewModel()
        {
            TiposRegistro = new List<FiltroItemViewModel>();
            EstadosProveedor = new List<FiltroItemViewModel>();
            Regiones = new List<RegionFiltroViewModel>();
        }
    }




}