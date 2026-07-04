namespace Cotizador_animacion_Othalart.Models
{
    public class PerfilTipoProducto
    {
        public string TipoProducto { get; set; } = "";

        public string UnidadDuracionSugerida { get; set; } = "segundos";
        public string UsoPrincipalSugerido { get; set; } = "";
        public string FormatoEntregaSugerido { get; set; } = "";

        public bool RequierePersonajes { get; set; } = false;
        public bool RequiereFondos { get; set; } = false;
        public bool RequiereProps { get; set; } = false;
        public bool RequiereAnimacionPersonajes { get; set; } = false;
        public bool RequiereMotionGraphics { get; set; } = false;
        public bool RequiereEdicion { get; set; } = false;
        public bool RequiereAudio { get; set; } = false;
        public bool RequiereExportFinal { get; set; } = true;

        public string Nota { get; set; } = "";
    }
}