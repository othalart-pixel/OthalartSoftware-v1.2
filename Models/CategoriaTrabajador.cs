using System.Text.Json.Serialization;

namespace Cotizador_animacion_Othalart.Models
{
    public class CategoriaTrabajador
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";
        public string Nivel { get; set; } = "";

        /*
         * Bloque productivo al que pertenece el cargo.
         * Esto permite guardar / leer cargos desde JSON
         * y luego filtrarlos por etapa o bloque.
         *
         * Valores esperados:
         * General
         * Desarrollo
         * Preproduccion
         * Produccion
         * Postproduccion
         * VideojuegosAssets
         * TecnicaInterna
         */
        public string Bloque { get; set; } = "";

        /*
         * Separación funcional del cargo dentro del sistema.
         * Esta propiedad sí se guarda en JSON porque define cómo conversa el cargo
         * con rendimientos, ecuaciones, desglose y mano de obra.
         *
         * Valores esperados:
         * Productivo: ejecuta producción medible y requiere rendimiento/capacidad.
         * Gestion: dirección, producción, coordinación o administración del trabajo.
         * Apoyo: supervisión, revisión, QA, soporte o tareas auxiliares.
         */
        public string TipoCargo { get; set; } = "";

        public double SueldoMensualCLPMin { get; set; }
        public double SueldoMensualCLPTipico { get; set; }
        public double SueldoMensualCLPMax { get; set; }

        public double SueldoMensualUSDMin { get; set; }
        public double SueldoMensualUSDTipico { get; set; }
        public double SueldoMensualUSDMax { get; set; }

        /*
         * Compatibilidad con el código anterior:
         * el sistema calcula usando el valor típico.
         *
         * No se guarda en JSON porque es una propiedad derivada.
         */
        [JsonIgnore]
        public double SueldoMensualCLP
        {
            get { return SueldoMensualCLPTipico; }
            set { SueldoMensualCLPTipico = value; }
        }

        /*
         * Compatibilidad con el código anterior.
         * No se guarda en JSON porque es una propiedad derivada.
         */
        [JsonIgnore]
        public double SueldoMensualUSD
        {
            get { return SueldoMensualUSDTipico; }
            set { SueldoMensualUSDTipico = value; }
        }

        /*
         * Propiedad solo para mostrar en interfaz.
         * No debe guardarse en JSON.
         */
        [JsonIgnore]
        public string NombreCompleto
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nivel))
                {
                    return Nombre;
                }

                return $"{Nombre} ({Nivel})";
            }
        }

        /*
         * Propiedad solo para mostrar en interfaz.
         * No debe guardarse en JSON.
         */
        [JsonIgnore]
        public string RangoCLP
        {
            get
            {
                return $"{SueldoMensualCLPMin:N0} - {SueldoMensualCLPMax:N0}";
            }
        }

        /*
         * Propiedad solo para mostrar en interfaz.
         * No debe guardarse en JSON.
         */
        [JsonIgnore]
        public string RangoUSD
        {
            get
            {
                return $"{SueldoMensualUSDMin:N0} - {SueldoMensualUSDMax:N0}";
            }
        }
    }
}
