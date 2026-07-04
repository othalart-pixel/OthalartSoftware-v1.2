using System;

namespace Cotizador_animacion_Othalart.Models
{
    public class TipoCambio
    {
        public string Codigo { get; set; } = "CLP";
        public string Nombre { get; set; } = "Peso chileno";

        // Cuántos CLP vale 1 unidad de esta moneda.
        // Ejemplo:
        // USD = 950
        // EUR = 1050
        // JPY = 6.5
        // UF = 39000
        public double ValorEnCLP { get; set; } = 1.0;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
        public string Fuente { get; set; } = "Manual";

        // CLP no debería editarse; otras monedas sí.
        public bool EsEditable { get; set; } = true;

        public string NombreCompleto
        {
            get
            {
                return $"{Codigo} - {Nombre}";
            }
        }
    }
}