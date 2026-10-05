using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class ProveedorDetalleCompletoViewModel
    {
        // Datos de cabecera
        public DetalleProveedorPrincipalViewModel DetallePrincipal { get; set; }

        // Tab Razón Social
        public List<HistorialLogViewModel> HistorialRazonSocial { get; set; }

        // Tab Estado MOP
        public EstadoMopViewModel EstadoMopActual { get; set; }
        public List<HistorialLogViewModel> HistorialEstadoMop { get; set; }

        // Tab Capacidad Económica
        public CapacidadEconomicaViewModel CapacidadEconomicaActual { get; set; }
        public List<CapacidadEconomicaViewModel> HistorialCapacidadEconomica { get; set; }

        // Tab Especialidades y Categorías
        public List<EspecialidadCompletaViewModel> EspecialidadesCompletas { get; set; }
        public List<HistorialLogViewModel> HistorialEspecialidades { get; set; }

        // Tab Documentos
        public List<DocumentoProveedorViewModel> Documentos { get; set; }

        // Constructor para inicializar las listas y evitar errores de nulos
        public ProveedorDetalleCompletoViewModel()
        {
            HistorialRazonSocial = new List<HistorialLogViewModel>();
            HistorialEstadoMop = new List<HistorialLogViewModel>();
            HistorialCapacidadEconomica = new List<CapacidadEconomicaViewModel>();
            EspecialidadesCompletas = new List<EspecialidadCompletaViewModel>();
            HistorialEspecialidades = new List<HistorialLogViewModel>();
            Documentos = new List<DocumentoProveedorViewModel>();
        }




    }
}