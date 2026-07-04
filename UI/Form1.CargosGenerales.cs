using System;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void InicializarCargosGenerales()
        {
            if (bibliotecaCargosGenerales == null)
            {
                bibliotecaCargosGenerales = new System.Collections.Generic.List<CategoriaTrabajador>();
            }

            if (bibliotecaCargosGenerales.Count > 0)
            {
                return;
            }

            // =========================================================
            // CARGOS GENERALES / GESTIÓN
            // =========================================================

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8001,
                Nombre = "Productor / Project manager",
                Nivel = "general",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 1000000,
                SueldoMensualCLPMax = 1500000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8002,
                Nombre = "Director/a creativo",
                Nivel = "general",
                SueldoMensualCLPMin = 900000,
                SueldoMensualCLPTipico = 1400000,
                SueldoMensualCLPMax = 2200000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8003,
                Nombre = "Director/a de animación",
                Nivel = "general",
                SueldoMensualCLPMin = 900000,
                SueldoMensualCLPTipico = 1400000,
                SueldoMensualCLPMax = 2200000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8004,
                Nombre = "Coordinador/a de producción",
                Nivel = "general",
                SueldoMensualCLPMin = 600000,
                SueldoMensualCLPTipico = 850000,
                SueldoMensualCLPMax = 1200000
            });

            // =========================================================
            // DESARROLLO / PREPRODUCCIÓN
            // =========================================================

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8101,
                Nombre = "Guionista",
                Nivel = "típico",
                SueldoMensualCLPMin = 650000,
                SueldoMensualCLPTipico = 900000,
                SueldoMensualCLPMax = 1300000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8102,
                Nombre = "Director de arte",
                Nivel = "típico",
                SueldoMensualCLPMin = 800000,
                SueldoMensualCLPTipico = 1200000,
                SueldoMensualCLPMax = 1800000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8103,
                Nombre = "Storyboard artist",
                Nivel = "típico",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 1000000,
                SueldoMensualCLPMax = 1500000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8104,
                Nombre = "Animatic editor",
                Nivel = "típico",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 1000000,
                SueldoMensualCLPMax = 1400000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8105,
                Nombre = "Diseñador de personajes",
                Nivel = "típico",
                SueldoMensualCLPMin = 750000,
                SueldoMensualCLPTipico = 1100000,
                SueldoMensualCLPMax = 1600000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8106,
                Nombre = "Diseñador de props",
                Nivel = "típico",
                SueldoMensualCLPMin = 650000,
                SueldoMensualCLPTipico = 900000,
                SueldoMensualCLPMax = 1300000
            });

            // =========================================================
            // PRODUCCIÓN
            // =========================================================

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8201,
                Nombre = "Animador 2D",
                Nivel = "típico",
                SueldoMensualCLPMin = 800000,
                SueldoMensualCLPTipico = 1100000,
                SueldoMensualCLPMax = 1600000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8202,
                Nombre = "Clean up artist",
                Nivel = "típico",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 950000,
                SueldoMensualCLPMax = 1300000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8203,
                Nombre = "Colorista",
                Nivel = "típico",
                SueldoMensualCLPMin = 650000,
                SueldoMensualCLPTipico = 850000,
                SueldoMensualCLPMax = 1200000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8204,
                Nombre = "Artista de fondos",
                Nivel = "típico",
                SueldoMensualCLPMin = 750000,
                SueldoMensualCLPTipico = 1250000,
                SueldoMensualCLPMax = 1800000
            });

            // =========================================================
            // POSTPRODUCCIÓN
            // =========================================================

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8301,
                Nombre = "Compositor",
                Nivel = "típico",
                SueldoMensualCLPMin = 800000,
                SueldoMensualCLPTipico = 1300000,
                SueldoMensualCLPMax = 1800000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8302,
                Nombre = "Diseñador sonoro",
                Nivel = "típico",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 1000000,
                SueldoMensualCLPMax = 1500000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8303,
                Nombre = "Editor",
                Nivel = "típico",
                SueldoMensualCLPMin = 700000,
                SueldoMensualCLPTipico = 1100000,
                SueldoMensualCLPMax = 1600000
            });

            bibliotecaCargosGenerales.Add(new CategoriaTrabajador
            {
                Id = 8304,
                Nombre = "Render / export manager",
                Nivel = "típico",
                SueldoMensualCLPMin = 600000,
                SueldoMensualCLPTipico = 850000,
                SueldoMensualCLPMax = 1200000
            });
        }

        private bool EsCargoGeneral(CategoriaTrabajador cargo)
        {
            return cargo != null && cargo.Id >= 8000 && cargo.Id < 9000;
        }

        private CategoriaTrabajador ClonarCargo(CategoriaTrabajador cargo)
        {
            return new CategoriaTrabajador
            {
                Id = cargo.Id,
                Nombre = cargo.Nombre,
                Nivel = cargo.Nivel,
                Bloque = cargo.Bloque,
                TipoCargo = cargo.TipoCargo,

                SueldoMensualCLPMin = cargo.SueldoMensualCLPMin,
                SueldoMensualCLPTipico = cargo.SueldoMensualCLPTipico,
                SueldoMensualCLPMax = cargo.SueldoMensualCLPMax,

                SueldoMensualUSDMin = cargo.SueldoMensualUSDMin,
                SueldoMensualUSDTipico = cargo.SueldoMensualUSDTipico,
                SueldoMensualUSDMax = cargo.SueldoMensualUSDMax
            };
        }

        private void CopiarValoresCargo(CategoriaTrabajador destino, CategoriaTrabajador origen)
        {
            if (destino == null || origen == null)
            {
                return;
            }

            destino.Nombre = origen.Nombre;
            destino.Nivel = origen.Nivel;
            destino.Bloque = origen.Bloque;
            destino.TipoCargo = origen.TipoCargo;

            destino.SueldoMensualCLPMin = origen.SueldoMensualCLPMin;
            destino.SueldoMensualCLPTipico = origen.SueldoMensualCLPTipico;
            destino.SueldoMensualCLPMax = origen.SueldoMensualCLPMax;

            destino.SueldoMensualUSDMin = origen.SueldoMensualUSDMin;
            destino.SueldoMensualUSDTipico = origen.SueldoMensualUSDTipico;
            destino.SueldoMensualUSDMax = origen.SueldoMensualUSDMax;
        }

        private void SincronizarCargosGeneralesEnTodasLasEtapas()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            InicializarCargosGenerales();

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                if (etapa.Biblioteca == null)
                {
                    etapa.Biblioteca = new System.Collections.Generic.List<CategoriaTrabajador>();
                }

                foreach (CategoriaTrabajador cargoGeneral in bibliotecaCargosGenerales)
                {
                    CategoriaTrabajador existente = etapa.Biblioteca
                        .FirstOrDefault(c => c.Id == cargoGeneral.Id);

                    if (existente == null)
                    {
                        etapa.Biblioteca.Add(ClonarCargo(cargoGeneral));
                    }
                    else
                    {
                        CopiarValoresCargo(existente, cargoGeneral);
                    }
                }
            }
        }

        private void QuitarCargoGeneralDeTodasLasEtapas(int idCargo)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null || etapa.Biblioteca == null)
                {
                    continue;
                }

                CategoriaTrabajador existente = etapa.Biblioteca
                    .FirstOrDefault(c => c.Id == idCargo);

                if (existente != null)
                {
                    etapa.Biblioteca.Remove(existente);
                }
            }
        }

        private int ObtenerNuevoIdCargoGeneral()
        {
            InicializarCargosGenerales();

            int maxId = bibliotecaCargosGenerales
                .Select(c => c.Id)
                .DefaultIfEmpty(8000)
                .Max();

            return maxId + 1;
        }

        private CategoriaTrabajador? ObtenerCargoGeneralPorId(int id)
        {
            InicializarCargosGenerales();

            return bibliotecaCargosGenerales.FirstOrDefault(c => c.Id == id);
        }
    }
}
