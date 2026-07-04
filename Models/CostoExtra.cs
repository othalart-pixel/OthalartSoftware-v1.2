namespace Cotizador_animacion_Othalart.Models
{
    public class CostoExtra
    {
        public string Categoria { get; set; } = "";
        public string Descripcion { get; set; } = "";

        // Moneda original en que se ingresó el costo.
        public string MonedaIngreso { get; set; } = "CLP";

        // Monto original ingresado por el usuario.
        public double MontoIngreso { get; set; }

        // Monto convertido a CLP. Este es el que usa el cálculo interno.
        public double Monto { get; set; }

        // Mensual, anual o una sola vez.
        public string Periodicidad { get; set; } = "Una sola vez";

        // Total calculado considerando periodicidad y duración.
        public double MontoCalculado { get; set; }

        // Si se muestra o no al cliente.
        public bool IncluirEnCliente { get; set; } = true;
    }
}