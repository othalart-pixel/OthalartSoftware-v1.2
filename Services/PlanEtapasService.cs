using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class PlanEtapasService
    {
        public static double ObtenerSueldoCategoria(CategoriaTrabajador categoria, string moneda)
        {
            if (moneda == "CLP")
            {
                return categoria.SueldoMensualCLP;
            }

            return categoria.SueldoMensualUSD;
        }

        public static int CantidadBloquesEtapa(double duracionMeses)
        {
            return (int)Math.Ceiling(duracionMeses);
        }

        public static double PesoBloque(double duracionMeses, int bloqueCeroIndexado)
        {
            double restante = duracionMeses - bloqueCeroIndexado;

            if (restante >= 1.0)
            {
                return 1.0;
            }

            if (restante > 0.0)
            {
                return restante;
            }

            return 0.0;
        }

        public static double CalcularCostoCargoPlan(
            CategoriaTrabajador categoria,
            List<double> personasPorBloque,
            double duracionMeses,
            string moneda
        )
        {
            double sueldoMensual = ObtenerSueldoCategoria(categoria, moneda);
            return sueldoMensual * personasPorBloque.Sum();
        }

        public static double CalcularPersonaMesCargo(
            List<double> personasPorBloque,
            double duracionMeses
        )
        {
            return personasPorBloque.Sum();
        }

        public static void RecalcularCargo(CargoPlanMensual cargo, double duracionMeses, string moneda)
        {
            AsegurarCantidadBloques(cargo, duracionMeses);

            cargo.PersonaMesTotal = CalcularPersonaMesCargo(
                cargo.PersonasPorBloque,
                duracionMeses
            );

            double sueldoMensual = 0.0;

            if (moneda == "USD")
            {
                sueldoMensual = cargo.SueldoMensualUSDEditable;

                if (sueldoMensual <= 0.0 && cargo.Categoria != null)
                {
                    sueldoMensual = cargo.Categoria.SueldoMensualUSD;
                }
            }
            else
            {
                sueldoMensual = cargo.SueldoMensualCLPEditable;

                if (sueldoMensual <= 0.0 && cargo.Categoria != null)
                {
                    sueldoMensual = cargo.Categoria.SueldoMensualCLP;
                }
            }

            cargo.CostoTotal = cargo.PersonaMesTotal * sueldoMensual;
        }

        public static double CalcularCostoEtapaPlan(List<CargoPlanMensual> plan)
        {
            return plan.Sum(cargo => cargo.CostoTotal);
        }

        public static double CalcularPersonaMesEtapaPlan(
            List<CargoPlanMensual> plan,
            double duracionMeses
        )
        {
            double total = 0.0;

            foreach (CargoPlanMensual cargo in plan)
            {
                total += CalcularPersonaMesCargo(cargo.PersonasPorBloque, duracionMeses);
            }

            return total;
        }

        public static void RecalcularEtapa(EtapaProyecto etapa, string moneda)
        {
            if (!etapa.Seleccionada)
            {
                etapa.PersonaMesTotal = 0.0;
                etapa.CostoTotal = 0.0;
                return;
            }

            if (etapa.UsaPlanDetallado)
            {
                foreach (CargoPlanMensual cargo in etapa.Plan)
                {
                    AsegurarCantidadBloques(cargo, etapa.DuracionMeses);
                    RecalcularCargo(cargo, etapa.DuracionMeses, moneda);
                }

                etapa.CostoTotal = CalcularCostoEtapaPlan(etapa.Plan);
                etapa.PersonaMesTotal = CalcularPersonaMesEtapaPlan(etapa.Plan, etapa.DuracionMeses);
            }
            else
            {
                etapa.PersonaMesTotal = etapa.CantidadPromedioPersonas * etapa.DuracionMeses;
                etapa.CostoTotal = etapa.PersonaMesTotal * etapa.SueldoPromedioMensual;
            }
        }

        public static void RecalcularGantt(Cotizacion cotizacion)
        {
            cotizacion.AcumuladoMeses = 0.0;

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa.Seleccionada)
                {
                    etapa.InicioMes = cotizacion.AcumuladoMeses;
                    etapa.FinMes = cotizacion.AcumuladoMeses + etapa.DuracionMeses;
                    cotizacion.AcumuladoMeses = etapa.FinMes;
                }
                else
                {
                    etapa.InicioMes = 0.0;
                    etapa.FinMes = 0.0;
                }
            }
        }

        public static double SumarCostoEtapas(List<EtapaProyecto> etapas)
        {
            return etapas
                .Where(etapa => etapa.Seleccionada)
                .Sum(etapa => etapa.CostoTotal);
        }

        public static double SumarPersonaMesEtapas(List<EtapaProyecto> etapas)
        {
            return etapas
                .Where(etapa => etapa.Seleccionada)
                .Sum(etapa => etapa.PersonaMesTotal);
        }

        public static double DuracionTotalProyecto(List<EtapaProyecto> etapas)
        {
            return etapas
                .Where(etapa => etapa.Seleccionada)
                .Sum(etapa => etapa.DuracionMeses);
        }

        public static void AsegurarCantidadBloques(CargoPlanMensual cargo, double duracionMeses)
        {
            int bloques = CantidadBloquesEtapa(duracionMeses);

            while (cargo.PersonasPorBloque.Count < bloques)
            {
                cargo.PersonasPorBloque.Add(0.0);
            }

            while (cargo.PersonasPorBloque.Count > bloques)
            {
                cargo.PersonasPorBloque.RemoveAt(cargo.PersonasPorBloque.Count - 1);
            }
        }

        public static bool CargoExisteEnEtapa(EtapaProyecto etapa, int idCategoria)
        {
            return etapa.Plan.Any(cargo => cargo.Categoria.Id == idCategoria);
        }

        public static CargoPlanMensual? BuscarCargoEnEtapa(EtapaProyecto etapa, int idCategoria)
        {
            return etapa.Plan.FirstOrDefault(cargo => cargo.Categoria.Id == idCategoria);
        }

        public static void AgregarCargoAEtapa(
            EtapaProyecto etapa,
            CategoriaTrabajador categoria,
            string moneda
        )
        {
            if (CargoExisteEnEtapa(etapa, categoria.Id))
            {
                return;
            }

            CargoPlanMensual nuevoCargo = new CargoPlanMensual
            {
                Categoria = categoria
            };

            AsegurarCantidadBloques(nuevoCargo, etapa.DuracionMeses);
            RecalcularCargo(nuevoCargo, etapa.DuracionMeses, moneda);

            etapa.Plan.Add(nuevoCargo);
            RecalcularEtapa(etapa, moneda);
        }

        public static void RecalcularFinesEtapas(Cotizacion cotizacion)
        {
            cotizacion.AcumuladoMeses = 0.0;

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa.Seleccionada)
                {
                    if (etapa.InicioMes < 0.0)
                    {
                        etapa.InicioMes = 0.0;
                    }

                    if (etapa.DuracionMeses < 1.0)
                    {
                        etapa.DuracionMeses = 1.0;
                    }

                    etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

                    if (etapa.FinMes > cotizacion.AcumuladoMeses)
                    {
                        cotizacion.AcumuladoMeses = etapa.FinMes;
                    }
                }
                else
                {
                    etapa.InicioMes = 0.0;
                    etapa.FinMes = 0.0;
                }
            }
        }

        public static void EliminarCargoDeEtapa(
            EtapaProyecto etapa,
            CargoPlanMensual cargo,
            string moneda
        )
        {
            etapa.Plan.Remove(cargo);
            RecalcularEtapa(etapa, moneda);
        }
    }
}
