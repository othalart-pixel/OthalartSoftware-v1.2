using System;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoCotizacionPruebaService
    {
        public static ProyectoCotizacion CrearPilotoAnimado()
        {
            ProyectoCotizacion proyecto =
                ProyectoCotizacionJsonService.CrearProyectoVacio("Piloto animado", "Cliente ejemplo");
            proyecto.Id = "prj_piloto_animado";
            proyecto.Descripcion = "Proyecto de prueba para validar jerarquía productiva completa.";

            GrupoProyecto direccion = proyecto.Grupos.Find(g => g.Id == "grp_direccion_gestion");
            GrupoProyecto pre = proyecto.Grupos.Find(g => g.Id == "grp_preproduccion");
            GrupoProyecto prod = proyecto.Grupos.Find(g => g.Id == "grp_produccion");

            proyecto.ProcesosTransversales.Add(new ProcesoTransversalProyecto
            {
                Id = "ptr_direccion_arte",
                ProcesoBibliotecaId = "proc_direccion_arte_proyecto",
                Nombre = "Dirección de arte",
                TipoProceso = TipoProcesoProductivo.Direccion,
                AlcanceTemporal = AlcanceTemporalProceso.ProyectoCompleto,
                EtapasCubiertas = { "preproduccion", "produccion", "postproduccion" },
                SemanasActivas = 6,
                HorasPorSemana = 8,
                PorcentajeDedicacion = 100,
                CargoId = "cargo_director_arte",
                HorasCalculadas = 48,
                HorasAsignadas = 48,
                OrigenHoras = "Prueba funcional",
                ReglaProrrateo = ReglaProrrateoProceso.SinProrratear
            });

            proyecto.ProcesosTransversales.Add(new ProcesoTransversalProyecto
            {
                Id = "ptr_coordinacion_produccion",
                ProcesoBibliotecaId = "proc_coordinacion_produccion_proyecto",
                Nombre = "Coordinación de producción",
                TipoProceso = TipoProcesoProductivo.GestionCoordinacion,
                AlcanceTemporal = AlcanceTemporalProceso.ProyectoCompleto,
                EtapasCubiertas = { "preproduccion", "produccion", "postproduccion" },
                SemanasActivas = 6,
                HorasPorSemana = 4,
                PorcentajeDedicacion = 100,
                CargoId = "cargo_coordinador_produccion",
                HorasCalculadas = 24,
                HorasAsignadas = 24,
                OrigenHoras = "Prueba funcional",
                ReglaProrrateo = ReglaProrrateoProceso.SinProrratear
            });

            direccion.Items.Add(new ServicioProyecto
            {
                Id = "srv_direccion_gestion",
                BibliotecaId = "servicio_direccion_gestion",
                Nombre = "Dirección y gestión transversal",
                Cantidad = 1,
                Unidad = "proyecto",
                Snapshot = new SnapshotItem
                {
                    NombreBiblioteca = "Dirección y gestión transversal",
                    CategoriaBiblioteca = "Gestión"
                }
            });

            ProductoProyecto personajes = new ProductoProyecto
            {
                Id = "prod_diseno_personajes",
                BibliotecaId = "producto_diseno_personajes",
                Nombre = "Diseño de personajes",
                Cantidad = 1,
                Unidad = "pack",
                Snapshot = new SnapshotItem
                {
                    NombreBiblioteca = "Diseño de personajes",
                    CategoriaBiblioteca = "Preproducción",
                    UnidadBase = "personajes"
                }
            };

            SubproductoProyecto principales = new SubproductoProyecto
            {
                Id = "sub_personajes_principales",
                ProductoProyectoId = personajes.Id,
                SubproductoBibliotecaId = "personajes_principales",
                Nombre = "Personajes principales",
                Cantidad = 3,
                Unidad = "personajes",
                ModoCantidad = ModoCantidadSubproducto.InstanciasIndividuales
            };
            principales.Instancias.Add(CrearInstancia(principales.Id, "ins_hanna", "Hanna", 0));
            principales.Instancias.Add(CrearInstancia(principales.Id, "ins_mat", "Mat", 1));
            principales.Instancias.Add(CrearInstancia(principales.Id, "ins_joey", "Joey", 2));
            personajes.Subproductos.Add(principales);
            pre.Items.Add(personajes);

            ProductoProyecto animacion = new ProductoProyecto
            {
                Id = "prod_animacion_2d",
                BibliotecaId = "producto_animacion_2d",
                Nombre = "Animación 2D",
                Cantidad = 1,
                Unidad = "proyecto",
                Snapshot = new SnapshotItem
                {
                    NombreBiblioteca = "Animación 2D",
                    CategoriaBiblioteca = "Producción",
                    UnidadBase = "segundos"
                }
            };

            animacion.Subproductos.Add(CrearSubAnimacion(animacion.Id, "sub_rough", "Rough animation", 60, "segundos", "proc_rough", "proc_revision_rough", "proc_correccion_rough", 100m, 10m, 15m));
            animacion.Subproductos.Add(CrearSubAnimacion(animacion.Id, "sub_clean_up", "Clean up", 60, "segundos", "proc_clean_up", "proc_revision_clean_up", "proc_correccion_clean_up", 120m, 12m, 18m));
            prod.Items.Add(animacion);

            ProyectoCotizacionJsonService.Normalizar(proyecto);
            return proyecto;
        }

        private static InstanciaSubproducto CrearInstancia(string subId, string id, string nombre, int orden)
        {
            return new InstanciaSubproducto
            {
                Id = id,
                SubproductoProyectoId = subId,
                Nombre = nombre,
                CantidadEquivalente = 1,
                Orden = orden,
                Estado = "Pendiente"
            };
        }

        private static SubproductoProyecto CrearSubAnimacion(
            string productoId,
            string id,
            string nombre,
            decimal cantidad,
            string unidad,
            string procProduccion,
            string procRevision,
            string procCorreccion,
            decimal horasProduccion,
            decimal horasRevision,
            decimal horasCorreccion
        )
        {
            SubproductoProyecto sub = new SubproductoProyecto
            {
                Id = id,
                ProductoProyectoId = productoId,
                SubproductoBibliotecaId = id.Replace("sub_", ""),
                Nombre = nombre,
                Cantidad = cantidad,
                Unidad = unidad,
                ModoCantidad = ModoCantidadSubproducto.Homogeneo
            };

            sub.Procesos.Add(CrearProceso("prj_" + procProduccion, procProduccion, nombre, TipoProcesoProductivo.ProduccionDirecta, "", horasProduccion));
            sub.Procesos.Add(CrearProceso("prj_" + procRevision, procRevision, "Revisión de " + nombre, TipoProcesoProductivo.RevisionControl, "prj_" + procProduccion, horasRevision));
            sub.Procesos.Add(CrearProceso("prj_" + procCorreccion, procCorreccion, "Correcciones de " + nombre, TipoProcesoProductivo.CorreccionRetrabajo, "prj_" + procProduccion, horasCorreccion));
            return sub;
        }

        private static ProcesoProyecto CrearProceso(
            string id,
            string bibliotecaId,
            string nombre,
            TipoProcesoProductivo tipo,
            string dependencia = "",
            decimal horas = 0m
        )
        {
            ProcesoProyecto proceso = new ProcesoProyecto
            {
                Id = id,
                ProcesoBibliotecaId = bibliotecaId,
                Nombre = nombre,
                TipoProceso = tipo,
                MetodoCalculo = tipo == TipoProcesoProductivo.ProduccionDirecta
                    ? MetodoCalculoProceso.PorDuracionEntregable
                    : MetodoCalculoProceso.PorPorcentajeProduccion,
                AlcanceTemporal = AlcanceTemporalProceso.Subproducto,
                EtapaId = "produccion",
                Paralelo = false
            };
            proceso.Resultado.HorasCalculadas = horas;
            proceso.Resultado.HorasAsignadas = horas;
            proceso.Resultado.Diagnostico = "Prueba funcional";

            if (!string.IsNullOrWhiteSpace(dependencia))
            {
                proceso.Dependencias.Add(dependencia);
            }

            proceso.Asignaciones.Add(new AsignacionProductiva
            {
                Id = "asg_" + id,
                ProcesoProyectoId = id,
                CargoId = tipo == TipoProcesoProductivo.RevisionControl
                    ? "cargo_supervisor_animacion"
                    : "cargo_animador_2d",
                HorasCalculadas = horas,
                HorasAsignadas = horas,
                OrigenHoras = OrigenHorasProductivas.Calculado,
                DedicacionPorcentaje = 100
            });

            return proceso;
        }
    }
}
