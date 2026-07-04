using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class BibliotecaDependenciasSubEtapasService
    {
        public static List<DependenciaSubEtapa> CrearBibliotecaBase()
        {
            List<DependenciaSubEtapa> dependencias = new List<DependenciaSubEtapa>();

            /*
             * DESARROLLO / INSUMOS BASE
             */

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Preproduccion",
                SubEtapaObjetivo = "Storyboard",
                EtapaRequisito = "Desarrollo",
                SubEtapaRequisito = "Guion base",
                Motivo = "Storyboard necesita una estructura narrativa o guion aprobado como insumo previo.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Preproduccion",
                SubEtapaObjetivo = "Animatic",
                EtapaRequisito = "Preproduccion",
                SubEtapaRequisito = "Storyboard",
                Motivo = "Animatic necesita storyboard o estructura visual de escenas.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Preproduccion",
                SubEtapaObjetivo = "Diseño de personajes",
                EtapaRequisito = "Desarrollo",
                SubEtapaRequisito = "Definición de estilo",
                Motivo = "El diseño de personajes necesita una dirección visual mínima.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Preproduccion",
                SubEtapaObjetivo = "Diseño de fondos",
                EtapaRequisito = "Desarrollo",
                SubEtapaRequisito = "Definición de estilo",
                Motivo = "El diseño de fondos necesita una línea visual o criterio estético base.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            /*
             * PREPRODUCCIÓN → PRODUCCIÓN
             */

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Layout",
                EtapaRequisito = "Preproduccion",
                SubEtapaRequisito = "Storyboard",
                Motivo = "Layout necesita estructura de planos o escenas.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Animación",
                EtapaRequisito = "Preproduccion",
                SubEtapaRequisito = "Storyboard",
                Motivo = "Animación necesita una guía de escenas, planos o acciones.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Animación",
                EtapaRequisito = "Preproduccion",
                SubEtapaRequisito = "Diseño de personajes",
                Motivo = "Animación necesita modelos o referencias de personajes.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Animación",
                EtapaRequisito = "Desarrollo",
                SubEtapaRequisito = "Definición de estilo",
                Motivo = "Animación necesita una dirección visual mínima o referencias aprobadas.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Backgrounds",
                EtapaRequisito = "Preproduccion",
                SubEtapaRequisito = "Diseño de fondos",
                Motivo = "Backgrounds necesita diseño de fondos o guía visual de ambientes.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            /*
             * PRODUCCIÓN INTERNA
             */

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Compositing",
                EtapaRequisito = "Produccion",
                SubEtapaRequisito = "Animación",
                Motivo = "Compositing necesita material animado o visual producido.",
                Bloqueante = false,
                Severidad = "Riesgo alto",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Produccion",
                SubEtapaObjetivo = "Compositing",
                EtapaRequisito = "Produccion",
                SubEtapaRequisito = "Backgrounds",
                Motivo = "Compositing requiere fondos o piezas visuales base.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = false,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoCreativo"
            });

            /*
             * PRODUCCIÓN → POSTPRODUCCIÓN
             */

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Postproduccion",
                SubEtapaObjetivo = "Edición final",
                EtapaRequisito = "Produccion",
                SubEtapaRequisito = "Compositing",
                Motivo = "Edición final necesita material visual compuesto o versiones finales.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Postproduccion",
                SubEtapaObjetivo = "Correcciones finales",
                EtapaRequisito = "Postproduccion",
                SubEtapaRequisito = "Edición final",
                Motivo = "Correcciones finales normalmente requieren una versión editada para revisar.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "InsumoPrevio"
            });

            dependencias.Add(new DependenciaSubEtapa
            {
                EtapaObjetivo = "Postproduccion",
                SubEtapaObjetivo = "Entrega final",
                EtapaRequisito = "Postproduccion",
                SubEtapaRequisito = "Correcciones finales",
                Motivo = "La entrega final debería ocurrir después de correcciones o revisión final.",
                Bloqueante = false,
                Severidad = "Advertencia",
                RequiereOrdenTemporal = true,
                HolguraSemanas = 0,
                TipoRequisito = "GateFinal"
            });

            return dependencias;
        }

        public static List<AlertaDependenciaSubEtapa> ValidarDependencias(
            List<SubEtapaProyecto> subEtapas,
            List<ResolucionDependenciaSubEtapa> resoluciones
        )
        {
            List<AlertaDependenciaSubEtapa> alertas = new List<AlertaDependenciaSubEtapa>();

            if (subEtapas == null || subEtapas.Count == 0)
            {
                return alertas;
            }

            if (resoluciones == null)
            {
                resoluciones = new List<ResolucionDependenciaSubEtapa>();
            }

            List<DependenciaSubEtapa> dependencias = CrearBibliotecaBase();

            foreach (DependenciaSubEtapa dependencia in dependencias)
            {
                SubEtapaProyecto objetivo = BuscarSubEtapa(
                    subEtapas,
                    dependencia.EtapaObjetivo,
                    dependencia.SubEtapaObjetivo
                );

                if (objetivo == null || !objetivo.Activa)
                {
                    continue;
                }

                SubEtapaProyecto requisito = BuscarSubEtapa(
                    subEtapas,
                    dependencia.EtapaRequisito,
                    dependencia.SubEtapaRequisito
                );

                bool requisitoInternoCumplido = requisito != null && requisito.Activa;

                bool requisitoExternoCumplido = EstaDependenciaResueltaExternamente(
                    resoluciones,
                    dependencia.EtapaRequisito,
                    dependencia.SubEtapaRequisito
                );

                if (!requisitoInternoCumplido && !requisitoExternoCumplido)
                {
                    alertas.Add(new AlertaDependenciaSubEtapa
                    {
                        EtapaObjetivo = dependencia.EtapaObjetivo,
                        SubEtapaObjetivo = dependencia.SubEtapaObjetivo,
                        EtapaRequisito = dependencia.EtapaRequisito,
                        SubEtapaRequisito = dependencia.SubEtapaRequisito,
                        Motivo = dependencia.Motivo,
                        Severidad = dependencia.Severidad,
                        Bloqueante = dependencia.Bloqueante,
                        RequiereOrdenTemporal = dependencia.RequiereOrdenTemporal,
                        TipoRequisito = dependencia.TipoRequisito
                    });
                }
            }

            return alertas;
        }

        public static List<AlertaDependenciaSubEtapa> ValidarDependenciasDeSubEtapa(
            List<SubEtapaProyecto> subEtapas,
            SubEtapaProyecto subEtapaObjetivo,
            List<ResolucionDependenciaSubEtapa> resoluciones
        )
        {
            List<AlertaDependenciaSubEtapa> todas = ValidarDependencias(
                subEtapas,
                resoluciones
            );

            if (subEtapaObjetivo == null)
            {
                return todas;
            }

            string etapaObjetivo = Normalizar(subEtapaObjetivo.EtapaPadre);
            string nombreObjetivo = Normalizar(subEtapaObjetivo.Nombre);

            return todas
                .Where(a =>
                    Normalizar(a.EtapaObjetivo) == etapaObjetivo &&
                    CoincideNombre(Normalizar(a.SubEtapaObjetivo), nombreObjetivo))
                .ToList();
        }

        public static void AplicarPropuestaMinimaOrdenTemporal(
            List<SubEtapaProyecto> subEtapas,
            List<ResolucionDependenciaSubEtapa> resoluciones
        )
        {
            if (subEtapas == null || subEtapas.Count == 0)
            {
                return;
            }

            if (resoluciones == null)
            {
                resoluciones = new List<ResolucionDependenciaSubEtapa>();
            }

            List<DependenciaSubEtapa> dependencias = CrearBibliotecaBase();

            foreach (DependenciaSubEtapa dependencia in dependencias)
            {
                if (!dependencia.RequiereOrdenTemporal)
                {
                    continue;
                }

                SubEtapaProyecto objetivo = BuscarSubEtapa(
                    subEtapas,
                    dependencia.EtapaObjetivo,
                    dependencia.SubEtapaObjetivo
                );

                if (objetivo == null || !objetivo.Activa)
                {
                    continue;
                }

                SubEtapaProyecto requisito = BuscarSubEtapa(
                    subEtapas,
                    dependencia.EtapaRequisito,
                    dependencia.SubEtapaRequisito
                );

                bool requisitoActivo = requisito != null && requisito.Activa;

                bool requisitoExterno = EstaDependenciaResueltaExternamente(
                    resoluciones,
                    dependencia.EtapaRequisito,
                    dependencia.SubEtapaRequisito
                );

                /*
                 * Si el requisito no está activo:
                 * - Si está resuelto externamente, no movemos fechas.
                 * - Si no está resuelto, tampoco movemos fechas;
                 *   eso se informa como alerta.
                 */
                if (!requisitoActivo || requisitoExterno)
                {
                    continue;
                }

                if (requisito.InicioSemana < 1.0)
                {
                    requisito.InicioSemana = 1.0;
                }

                if (requisito.DuracionSemanas <= 0.0)
                {
                    requisito.DuracionSemanas = 0.1;
                }

                /*
                 * Ahora InicioSemana / DuracionSemanas / FinSemana son double.
                 * No se redondea aquí.
                 * Solo se impone que el objetivo comience después del requisito.
                 *
                 * Si quieres holgura de 0 semanas:
                 * objetivo empieza justo en requisito.FinSemana.
                 *
                 * Si HolguraSemanas = 1:
                 * objetivo empieza una semana después del fin del requisito.
                 */
                double inicioMinimoObjetivo =
                    requisito.FinSemana + Convert.ToDouble(dependencia.HolguraSemanas);

                if (inicioMinimoObjetivo < 1.0)
                {
                    inicioMinimoObjetivo = 1.0;
                }

                if (objetivo.InicioSemana < inicioMinimoObjetivo)
                {
                    objetivo.InicioSemana = inicioMinimoObjetivo;

                    if (objetivo.DuracionSemanas <= 0.0)
                    {
                        objetivo.DuracionSemanas = 0.1;
                    }
                }
            }
        }

        public static List<string> ObtenerMensajesDependenciasDeSubEtapa(
            List<SubEtapaProyecto> subEtapas,
            SubEtapaProyecto subEtapaObjetivo,
            List<ResolucionDependenciaSubEtapa> resoluciones
        )
        {
            List<string> mensajes = new List<string>();

            List<AlertaDependenciaSubEtapa> alertas = ValidarDependenciasDeSubEtapa(
                subEtapas,
                subEtapaObjetivo,
                resoluciones
            );

            foreach (AlertaDependenciaSubEtapa alerta in alertas)
            {
                mensajes.Add(alerta.Mensaje);
            }

            return mensajes;
        }

        private static SubEtapaProyecto BuscarSubEtapa(
            List<SubEtapaProyecto> subEtapas,
            string etapa,
            string nombreSubEtapa
        )
        {
            string etapaNormalizada = Normalizar(etapa);
            string subNormalizada = Normalizar(nombreSubEtapa);

            return subEtapas.FirstOrDefault(s =>
                s != null &&
                Normalizar(s.EtapaPadre) == etapaNormalizada &&
                CoincideNombre(Normalizar(s.Nombre), subNormalizada)
            );
        }

        private static bool EstaDependenciaResueltaExternamente(
            List<ResolucionDependenciaSubEtapa> resoluciones,
            string etapaRequisito,
            string subEtapaRequisito
        )
        {
            string etapaNormalizada = Normalizar(etapaRequisito);
            string subNormalizada = Normalizar(subEtapaRequisito);

            return resoluciones.Any(r =>
                r != null &&
                Normalizar(r.EtapaRequisito) == etapaNormalizada &&
                CoincideNombre(Normalizar(r.SubEtapaRequisito), subNormalizada) &&
                (
                    r.ModoResolucion == "EntregadaPorCliente" ||
                    r.ModoResolucion == "YaExiste" ||
                    r.ModoResolucion == "NoAplica" ||
                    r.ModoResolucion == "RiesgoAceptado"
                )
            );
        }

        private static bool CoincideNombre(string nombreReal, string nombreBuscado)
        {
            if (string.IsNullOrWhiteSpace(nombreReal) ||
                string.IsNullOrWhiteSpace(nombreBuscado))
            {
                return false;
            }

            return nombreReal.Contains(nombreBuscado) ||
                   nombreBuscado.Contains(nombreReal);
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