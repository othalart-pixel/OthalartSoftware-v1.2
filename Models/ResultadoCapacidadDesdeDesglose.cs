using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public enum ModoDuracionDesglose
    {
        Minimo,
        Estandar,
        Holgura
    }

    public class ResultadoCapacidadProyecto
    {
        public double PlazoClienteSemanas { get; set; } = 0.0;

        public double DiasCalendarioDisponibles { get; set; } = 0.0;

        public double DiasPersonaTotales { get; set; } = 0.0;

        public double FactorPresionGlobal { get; set; } = 0.0;

        public int PersonasEquivalentesGlobales { get; set; } = 1;

        public string Diagnostico { get; set; } = "";

        public List<ResultadoCapacidadEtapa> Etapas { get; set; } =
            new List<ResultadoCapacidadEtapa>();
    }

    public class ResultadoCapacidadEtapa
    {
        public string Etapa { get; set; } = "";

        public double DiasPersonaEtapa { get; set; } = 0.0;

        public double PesoEtapa { get; set; } = 0.0;

        public double DiasCalendarioAsignados { get; set; } = 0.0;

        public double SemanasCalendarioAsignadas { get; set; } = 0.0;

        public double FactorPresionEtapa { get; set; } = 0.0;

        public int PersonasMinimasEtapa { get; set; } = 1;

        public string Diagnostico { get; set; } = "";

        public List<ResultadoCapacidadCargo> Cargos { get; set; } =
            new List<ResultadoCapacidadCargo>();
    }

    public class ResultadoCapacidadCargo
    {
        public string Etapa { get; set; } = "";

        public string Cargo { get; set; } = "";

        public double DiasPersonaCargo { get; set; } = 0.0;

        public double FactorPresionCargo { get; set; } = 0.0;

        public int PersonasSugeridas { get; set; } = 1;

        public int PersonasExtra { get; set; } = 0;

        public bool Paralelizable { get; set; } = false;

        public int MaximoPersonasRazonable { get; set; } = 1;

        public int PrioridadAumento { get; set; } = 999;

        public string Nota { get; set; } = "";
    }
}