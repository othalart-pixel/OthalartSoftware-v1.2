using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProductQuoteConfigurationService
    {
        public static ProductQuoteConfiguration Crear(Producto2DDefinicion producto)
        {
            ProductQuoteConfiguration config = new ProductQuoteConfiguration();
            if (producto == null)
            {
                return config;
            }

            config.CommercialUnit = ResolverUnidadComercial(producto);
            config.Quantity = ResolverCantidadDefault(producto, config.CommercialUnit);

            List<QuoteParameterDefinition> definidos = producto.ParametrosCotizacion ?? new List<QuoteParameterDefinition>();
            config.Parameters = definidos.Count > 0
                ? definidos.OrderBy(p => p.DisplayOrder).ToList()
                : InferirParametros(producto, config.CommercialUnit);

            foreach (QuoteParameterDefinition parametro in config.Parameters)
            {
                if (!config.Values.ContainsKey(parametro.Id))
                {
                    config.Values[parametro.Id] = parametro.DefaultValue ?? "";
                }
            }

            return config;
        }

        public static ProductQuoteConfiguration Clonar(ProductQuoteConfiguration origen)
        {
            ProductQuoteConfiguration copia = new ProductQuoteConfiguration
            {
                CommercialUnit = origen == null ? "unidades" : origen.CommercialUnit,
                Quantity = origen == null ? 1m : origen.Quantity,
                HasShotBreakdown = origen != null && origen.HasShotBreakdown,
                Parameters = origen == null
                    ? new List<QuoteParameterDefinition>()
                    : origen.Parameters.ToList(),
                Values = origen == null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string>(origen.Values),
                Shots = origen == null
                    ? new List<ProductShotBreakdownItem>()
                    : origen.Shots.Select(s => new ProductShotBreakdownItem
                    {
                        Name = s.Name,
                        DurationSeconds = s.DurationSeconds,
                        Density = s.Density,
                        Complexity = s.Complexity,
                        Characters = s.Characters
                    }).ToList()
            };
            return copia;
        }

        public static void AplicarAEntregable(
            ProductQuoteConfiguration config,
            EntregableBrief entregable
        )
        {
            if (config == null || entregable == null)
            {
                return;
            }

            decimal cantidad = config.Quantity <= 0m ? 1m : config.Quantity;
            string unidad = (config.CommercialUnit ?? "").Trim();
            bool esDuracion = EsUnidadDuracion(unidad);

            if (esDuracion)
            {
                entregable.Cantidad = 1;
                entregable.DuracionPorUnidad = (double)cantidad;
                entregable.UnidadDuracion = string.IsNullOrWhiteSpace(unidad) ? "segundos" : unidad;
                entregable.UnidadCantidad = "pieza";
                entregable.SegundosAnimadosEfectivos = (double)cantidad;
            }
            else
            {
                entregable.Cantidad = (int)Math.Max(1, Math.Round(cantidad));
                entregable.UnidadCantidad = string.IsNullOrWhiteSpace(unidad) ? entregable.UnidadCantidad : unidad;
            }

            if (config.Values.TryGetValue("nivel_acabado", out string acabado) &&
                !string.IsNullOrWhiteSpace(acabado))
            {
                entregable.NivelCalidadEstimado = NormalizarCalidad(acabado);
            }
            else if (config.Values.TryGetValue("complejidad", out string complejidad) &&
                !string.IsNullOrWhiteSpace(complejidad))
            {
                entregable.NivelCalidadEstimado = NormalizarCalidad(complejidad);
            }

            if (config.Values.TryGetValue("personajes", out string personajes) &&
                int.TryParse(personajes, NumberStyles.Integer, CultureInfo.InvariantCulture, out int personajesInt))
            {
                entregable.PersonajesEstimados = Math.Max(1, personajesInt);
            }

            if (config.Values.TryGetValue("planos", out string planos) &&
                int.TryParse(planos, NumberStyles.Integer, CultureInfo.InvariantCulture, out int planosInt))
            {
                entregable.PlanosEstimados = Math.Max(1, planosInt);
            }

            if (config.HasShotBreakdown && config.Shots.Count > 0)
            {
                double total = config.Shots.Sum(s => Math.Max(0.0, s.DurationSeconds));
                if (total > 0.0)
                {
                    entregable.DuracionPorUnidad = total;
                    entregable.SegundosAnimadosEfectivos = total;
                    entregable.PlanosEstimados = config.Shots.Count;
                    entregable.PersonajesEstimados = Math.Max(1, config.Shots.Max(s => s.Characters));
                }
            }
        }

        public static List<string> Validar(ProductQuoteConfiguration config)
        {
            List<string> mensajes = new List<string>();
            if (config == null)
            {
                mensajes.Add("Falta configuracion de cotizacion");
                return mensajes;
            }

            if (string.IsNullOrWhiteSpace(config.CommercialUnit))
            {
                mensajes.Add("Falta unidad comercial");
            }

            if (config.Quantity <= 0m)
            {
                mensajes.Add("Cantidad invalida");
            }

            if (config.HasShotBreakdown)
            {
                double suma = config.Shots.Sum(s => Math.Max(0.0, s.DurationSeconds));
                double total = (double)Math.Max(0m, config.Quantity);
                if (Math.Abs(suma - total) > 0.001)
                {
                    mensajes.Add(
                        "La duracion total de los planos es " +
                        suma.ToString("0.##") +
                        " segundos, pero el producto esta configurado con " +
                        total.ToString("0.##") +
                        " segundos."
                    );
                }
            }

            return mensajes;
        }

        private static string ResolverUnidadComercial(Producto2DDefinicion producto)
        {
            if (!string.IsNullOrWhiteSpace(producto.UnidadComercialPrincipal))
            {
                return producto.UnidadComercialPrincipal;
            }

            bool usaSegundos = (producto.Subproductos ?? new List<Subproducto2D>())
                .Any(s => (s.VariablesEcuacion ?? "").ToLowerInvariant().Contains("segundos"));

            if (usaSegundos &&
                !string.IsNullOrWhiteSpace(producto.UnidadDuracionSugerida) &&
                !producto.UnidadDuracionSugerida.Equals("no aplica", StringComparison.OrdinalIgnoreCase))
            {
                return producto.UnidadDuracionSugerida;
            }

            return string.IsNullOrWhiteSpace(producto.UnidadCantidadSugerida)
                ? "unidades"
                : producto.UnidadCantidadSugerida;
        }

        private static decimal ResolverCantidadDefault(Producto2DDefinicion producto, string unidad)
        {
            if (EsUnidadDuracion(unidad) && producto.DuracionSugerida > 0.0)
            {
                return Convert.ToDecimal(producto.DuracionSugerida);
            }

            return 1m;
        }

        private static List<QuoteParameterDefinition> InferirParametros(
            Producto2DDefinicion producto,
            string unidadComercial
        )
        {
            string variables = string.Join(";",
                (producto.Subproductos ?? new List<Subproducto2D>())
                    .Select(s => (s.VariablesEcuacion ?? "") + ";" + (s.ImpactoEcuacion ?? ""))
            ).ToLowerInvariant();

            List<QuoteParameterDefinition> parametros = new List<QuoteParameterDefinition>();

            parametros.Add(new QuoteParameterDefinition
            {
                Id = "complejidad",
                Label = "Complejidad",
                DataType = "selector",
                DefaultValue = "Media",
                Options = new List<string> { "Baja", "Media", "Alta", "Premium" },
                DisplayOrder = 20,
                HelpText = "Se traduce al nivel de calidad usado por las ecuaciones existentes."
            });

            parametros.Add(new QuoteParameterDefinition
            {
                Id = "nivel_acabado",
                Label = "Nivel de acabado",
                DataType = "selector",
                DefaultValue = "Final",
                Options = new List<string> { "Borrador", "Estándar", "Final", "Premium" },
                DisplayOrder = 30
            });

            if (variables.Contains("personas") || variables.Contains("personajes"))
            {
                parametros.Add(new QuoteParameterDefinition
                {
                    Id = "personajes",
                    Label = "Personajes activos",
                    DataType = "integer",
                    Unit = "personajes",
                    DefaultValue = "1",
                    MinValue = 1,
                    MaxValue = 100,
                    AffectsBreakdown = true,
                    DisplayOrder = 40
                });
            }

            if (variables.Contains("planos") || variables.Contains("viñetas") || variables.Contains("vinetas"))
            {
                parametros.Add(new QuoteParameterDefinition
                {
                    Id = "planos",
                    Label = "Planos estimados",
                    DataType = "integer",
                    Unit = "planos",
                    DefaultValue = "1",
                    MinValue = 1,
                    MaxValue = 1000,
                    AffectsBreakdown = true,
                    DisplayOrder = 50
                });
            }

            if (EsUnidadDuracion(unidadComercial))
            {
                parametros.Add(new QuoteParameterDefinition
                {
                    Id = "densidad_animacion",
                    Label = "Densidad de animacion",
                    DataType = "selector",
                    DefaultValue = "Media",
                    Options = new List<string> { "Baja", "Media", "Alta", "Personalizada" },
                    DisplayOrder = 60,
                    HelpText = "No existe un ponderador de densidad persistido; se conserva como parametro de simulacion y validacion."
                });
            }

            return parametros.OrderBy(p => p.DisplayOrder).ToList();
        }

        private static bool EsUnidadDuracion(string unidad)
        {
            string u = (unidad ?? "").Trim().ToLowerInvariant();
            return u.Contains("segundo") || u == "s" || u.Contains("minuto") || u.Contains("frame");
        }

        private static string NormalizarCalidad(string valor)
        {
            string v = (valor ?? "").Trim().ToLowerInvariant();
            if (v.Contains("baja") || v.Contains("borrador"))
            {
                return "Baja";
            }

            if (v.Contains("alta") || v.Contains("final"))
            {
                return "Alta";
            }

            if (v.Contains("premium"))
            {
                return "Premium";
            }

            return "Estándar";
        }
    }
}
