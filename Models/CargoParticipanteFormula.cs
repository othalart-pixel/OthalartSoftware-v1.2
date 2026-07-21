using System.Text.Json.Serialization;

namespace Cotizador_animacion_Othalart.Models
{
    public class CargoParticipanteFormula
    {
        public string Cargo { get; set; } = "";
        public bool Activo { get; set; } = true;
        public double TarifaDiariaCLP { get; set; } = 0.0;
        public double HorasPorDia { get; set; } = 8.0;
        public double DedicacionPorcentaje { get; set; } = 100.0;
        public string ModoCalculo { get; set; } = "Trabaja durante todo el periodo productivo";
        public string VariableBase { get; set; } = "";
        public double CantidadFactor { get; set; } = 1.0;
        public string FormulaPersonalizada { get; set; } = "";

        [JsonIgnore]
        public double TiempoCalculadoHoras { get; set; } = 0.0;

        [JsonIgnore]
        public double DiasTecnicos { get; set; } = 0.0;

        [JsonIgnore]
        public double CostoCalculadoCLP { get; set; } = 0.0;
    }
}
