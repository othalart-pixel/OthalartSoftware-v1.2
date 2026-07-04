using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecasEtapas
    {
        public static List<EtapaProyecto> CrearEtapasBase()
        {
            return CrearEtapasBase(BibliotecaEtapasJsonService.CargarEtapas());
        }

        public static List<EtapaProyecto> CrearEtapasBase(List<EtapaDefinicion> definiciones)
        {
            if (definiciones == null || definiciones.Count == 0)
            {
                definiciones = BibliotecaEtapasJsonService.CrearEtapasBase();
            }

            List<EtapaProyecto> etapas = new List<EtapaProyecto>();

            foreach (EtapaDefinicion definicion in definiciones
                .Where(e => e != null && e.Activa)
                .OrderBy(e => e.Orden))
            {
                etapas.Add(new EtapaProyecto
                {
                    Nombre = string.IsNullOrWhiteSpace(definicion.Nombre)
                        ? definicion.Clave
                        : definicion.Nombre,
                    Seleccionada = false,
                    UsaPlanDetallado = true,
                    DuracionMeses = 0,
                    InicioMes = 0,
                    FinMes = 0,
                    Biblioteca = CrearBibliotecaCargosParaEtapa(definicion.Clave)
                });
            }

            return etapas;
        }

        private static List<CategoriaTrabajador> CrearBibliotecaCargosParaEtapa(string clave)
        {
            string n = Normalizar(clave);

            if (n.Contains("desarrollo"))
            {
                return Cargos.CrearBibliotecaDesarrollo();
            }

            if (n.Contains("preproduccion") || n == "pre")
            {
                return Cargos.CrearBibliotecaPreproduccion();
            }

            if (n.Contains("postproduccion") || n == "post")
            {
                return Cargos.CrearBibliotecaPostproduccion();
            }

            if (n.Contains("produccion") || n == "prod")
            {
                return Cargos.CrearBibliotecaProduccion();
            }

            return new List<CategoriaTrabajador>();
        }

        private static string Normalizar(string texto)
        {
            return (texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");
        }
    }
}
