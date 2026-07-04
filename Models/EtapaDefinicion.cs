namespace Cotizador_animacion_Othalart.Models
{
    public class EtapaDefinicion
    {
        public string Clave { get; set; } = "";

        public string Nombre { get; set; } = "";

        public int Orden { get; set; } = 0;

        public bool Activa { get; set; } = true;

        public int ColorArgb { get; set; } = 0;
    }
}
