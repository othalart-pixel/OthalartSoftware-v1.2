using System;
using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class ProyectoOthalartGuardado
    {
        public string Formato { get; set; } = "OthalartProject";
        public int Version { get; set; } = 1;
        public DateTime FechaGuardado { get; set; } = DateTime.Now;

        public Cotizacion Cotizacion { get; set; } = new Cotizacion();

        public ProyectoCotizacion ProyectoProductivo { get; set; } = null;

        public string TipoSolicitudDatosGuardado { get; set; } = "";
        public string ProductoServicioDatosGuardado { get; set; } = "";

        public List<Pieza2DSeleccionGuardada> Piezas2DDatosGuardadas { get; set; } =
            new List<Pieza2DSeleccionGuardada>();

        public List<EtapaDefinicion> BibliotecaEtapas { get; set; } =
            new List<EtapaDefinicion>();

        public List<SubEtapaProyecto> BibliotecaSubEtapas { get; set; } =
            new List<SubEtapaProyecto>();

        public List<CategoriaTrabajador> BibliotecaCargos { get; set; } =
            new List<CategoriaTrabajador>();

        public List<PersonaEquipo> BibliotecaPersonal { get; set; } =
            new List<PersonaEquipo>();

        public List<Producto2DDefinicion> BibliotecaProductos2D { get; set; } =
            new List<Producto2DDefinicion>();

        public List<RendimientoProductivo> BibliotecaRendimientosProductivos { get; set; } =
            new List<RendimientoProductivo>();

        public List<EcuacionProductivaDefinicion> BibliotecaEcuacionesProductivas { get; set; } =
            new List<EcuacionProductivaDefinicion>();

        public List<GestionProductivaRegla> BibliotecaGestionesProductivas { get; set; } =
            new List<GestionProductivaRegla>();

        public List<ResolucionDependenciaSubEtapa> ResolucionesDependenciasSubEtapas { get; set; } =
            new List<ResolucionDependenciaSubEtapa>();

        public bool ModoOscuroActivo { get; set; } = false;
    }
}
