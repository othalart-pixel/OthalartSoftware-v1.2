using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private SplitContainer splitEtapasYGanttGrande;
        private Panel panelScrollGanttGrandeEtapas;
        private Panel panelCanvasGanttGrandeEtapas;
        private List<GanttGrandeItem> itemsGanttGrandeEtapas = new List<GanttGrandeItem>();
        private List<GanttGrandeHitBox> hitBoxesGanttGrandeEtapas = new List<GanttGrandeHitBox>();
        private string claveDetalleGanttGrandeSeleccionada = "";

        private const int GanttGrandeSemanasPorMes = 4;

        private const int GanttGrandeAltoHeaderMes = 24;
        private const int GanttGrandeAltoHeaderSemana = 24;
        private const int GanttGrandeAltoHeader = GanttGrandeAltoHeaderMes + GanttGrandeAltoHeaderSemana;

        private const int GanttGrandeAltoFilaEtapa = 32;
        private const int GanttGrandeAltoFilaSub = 26;

        private const int GanttGrandeAnchoNombre = 280;
        private const int GanttGrandeAnchoEstado = 165;
        private const int GanttGrandeAnchoSemana = 38;

        private const int GanttGrandeMesesMinimos = 6;
        private const int GanttGrandeSemanasMinimas = GanttGrandeMesesMinimos * GanttGrandeSemanasPorMes;
        private const int GanttGrandeMargen = 8;

        private class GanttGrandeItem
        {
            public bool EsEtapa { get; set; }
            public bool EsGrupoProducto { get; set; }
            public bool TieneDetalleInterno { get; set; }
            public string NombreEtapa { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Estado { get; set; } = "";
            public string ClaveDetalle { get; set; } = "";
            public string Unidad { get; set; } = "";
            public int TotalProductos { get; set; } = 1;
            public bool Activo { get; set; }

            // Ahora la Gantt trabaja directamente en semanas.
            public int InicioSemana { get; set; }
            public int FinSemanaExclusiva { get; set; }

            public int AltoFila
            {
                get
                {
                    return EsEtapa
                        ? GanttGrandeAltoFilaEtapa
                        : EsGrupoProducto
                            ? GanttGrandeAltoFilaEtapa
                            : GanttGrandeAltoFilaSub;
                }
            }
        }

        private class GanttReqPlan
        {
            public string Etapa { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string ClaveDetalle { get; set; } = "";
            public string DependeDe { get; set; } = "";
            public string ModoPlanificacion { get; set; } = "";
            public string CargoSugerido { get; set; } = "";
            public double Cantidad { get; set; }
            public string Unidad { get; set; } = "";
            public int IndiceProducto { get; set; } = 1;
            public int TotalProductos { get; set; } = 1;
            public double DiasStd { get; set; }
            public int InstanciasVisuales { get; set; } = 1;
            public int InicioSemana { get; set; }
            public int FinSemanaExclusiva { get; set; }
        }

        private class GanttGrandeHitBox
        {
            public Rectangle Bounds { get; set; }
            public GanttGrandeItem Item { get; set; }
        }

        // Puedes llamar este método desde Form1.TabEtapas.cs.
        // Internamente usa el instalador principal seguro.
        private void InstalarGanttGrandeEnTabEtapas(TabPage tab)
        {
            InstalarGanttGrandeEnTabPrincipal(tab);
        }

        // Puedes llamar este método desde Form1.TabSubEtapas.cs.
        // Este es el instalador seguro real.
        private void InstalarGanttGrandeEnTabPrincipal(TabPage tab)
        {
            if (tab == null)
            {
                return;
            }

            foreach (Control control in tab.Controls)
            {
                if (control.Name == "splitPrincipalConGanttGrande")
                {
                    return;
                }
            }

            List<Control> controlesOriginales = tab.Controls
                .Cast<Control>()
                .ToList();

            tab.Controls.Clear();

            splitEtapasYGanttGrande = new SplitContainer();
            splitEtapasYGanttGrande.Name = "splitPrincipalConGanttGrande";
            splitEtapasYGanttGrande.Dock = DockStyle.Fill;
            splitEtapasYGanttGrande.Orientation = Orientation.Horizontal;
            splitEtapasYGanttGrande.SplitterWidth = 7;

            // Bajos para no reventar al construir la UI.
            // Se ajusta después cuando WinForms ya conoce el tamaño real.
            splitEtapasYGanttGrande.Panel1MinSize = 80;
            splitEtapasYGanttGrande.Panel2MinSize = 80;

            foreach (Control control in controlesOriginales)
            {
                splitEtapasYGanttGrande.Panel1.Controls.Add(control);
            }

            Panel panelTitulo = new Panel();
            panelTitulo.Dock = DockStyle.Top;
            panelTitulo.Height = 34;
            panelTitulo.BackColor = Color.White;

            Label lblTitulo = new Label();
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Text = "Gantt de etapas y subprocesos";
            lblTitulo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblTitulo.Padding = new Padding(8, 0, 0, 0);

            panelTitulo.Controls.Add(lblTitulo);

            panelScrollGanttGrandeEtapas = new Panel();
            panelScrollGanttGrandeEtapas.Dock = DockStyle.Fill;
            panelScrollGanttGrandeEtapas.AutoScroll = true;
            panelScrollGanttGrandeEtapas.BackColor = Color.White;

            panelCanvasGanttGrandeEtapas = new Panel();
            panelCanvasGanttGrandeEtapas.Location = new Point(0, 0);
            panelCanvasGanttGrandeEtapas.BackColor = Color.White;
            panelCanvasGanttGrandeEtapas.Paint -= PanelCanvasGanttGrandeEtapas_Paint;
            panelCanvasGanttGrandeEtapas.Paint += PanelCanvasGanttGrandeEtapas_Paint;
            panelCanvasGanttGrandeEtapas.MouseClick -= PanelCanvasGanttGrandeEtapas_MouseClick;
            panelCanvasGanttGrandeEtapas.MouseClick += PanelCanvasGanttGrandeEtapas_MouseClick;
            panelCanvasGanttGrandeEtapas.Cursor = Cursors.Hand;

            panelScrollGanttGrandeEtapas.Controls.Add(panelCanvasGanttGrandeEtapas);

            splitEtapasYGanttGrande.Panel2.Controls.Add(panelScrollGanttGrandeEtapas);
            splitEtapasYGanttGrande.Panel2.Controls.Add(panelTitulo);

            tab.Controls.Add(splitEtapasYGanttGrande);

            splitEtapasYGanttGrande.HandleCreated += (s, e) =>
            {
                AjustarSplitterGanttGrandeSeguro(splitEtapasYGanttGrande);
                RefrescarGanttGrandeEtapas();
            };

            splitEtapasYGanttGrande.Resize += (s, e) =>
            {
                AjustarSplitterGanttGrandeSeguro(splitEtapasYGanttGrande);
                RefrescarGanttGrandeEtapas();
            };

            tab.VisibleChanged += (s, e) =>
            {
                if (tab.Visible)
                {
                    AjustarSplitterGanttGrandeSeguro(splitEtapasYGanttGrande);
                    RefrescarGanttGrandeEtapas();
                }
            };

            if (splitEtapasYGanttGrande.IsHandleCreated)
            {
                AjustarSplitterGanttGrandeSeguro(splitEtapasYGanttGrande);
                RefrescarGanttGrandeEtapas();
            }
        }

        private void AjustarSplitterGanttGrandeSeguro(SplitContainer split)
        {
            if (split == null)
            {
                return;
            }

            int altoTotal = split.Height;

            if (altoTotal <= 0)
            {
                return;
            }

            int minimoPanel1 = split.Panel1MinSize;
            int minimoPanel2 = split.Panel2MinSize;
            int anchoSplitter = split.SplitterWidth;

            int maximoPermitido = altoTotal - minimoPanel2 - anchoSplitter;

            if (maximoPermitido <= minimoPanel1)
            {
                return;
            }

            int distanciaDeseada = (int)(altoTotal * 0.50);

            if (distanciaDeseada < minimoPanel1)
            {
                distanciaDeseada = minimoPanel1;
            }

            if (distanciaDeseada > maximoPermitido)
            {
                distanciaDeseada = maximoPermitido;
            }

            if (split.SplitterDistance == distanciaDeseada)
            {
                return;
            }

            try
            {
                split.SplitterDistance = distanciaDeseada;
            }
            catch
            {
                // WinForms puede estar recalculando tamaño.
            }
        }

        private void RefrescarGanttGrandeEtapas()
        {
            if (panelCanvasGanttGrandeEtapas == null)
            {
                return;
            }

            itemsGanttGrandeEtapas = ConstruirItemsGanttGrandeEtapas();

            int semanas = ObtenerSemanasTotalesGanttGrande(itemsGanttGrandeEtapas);

            int alto = GanttGrandeAltoHeader + GanttGrandeMargen;

            foreach (GanttGrandeItem item in itemsGanttGrandeEtapas)
            {
                alto += item.AltoFila;
            }

            alto += CalcularAltoDetalleMicroGantt();
            alto += 40;

            int ancho =
                GanttGrandeMargen +
                GanttGrandeAnchoNombre +
                GanttGrandeAnchoEstado +
                semanas * GanttGrandeAnchoSemana +
                40;

            int altoDisponible = 400;

            if (panelScrollGanttGrandeEtapas != null && panelScrollGanttGrandeEtapas.Height > 0)
            {
                altoDisponible = panelScrollGanttGrandeEtapas.Height - 5;
            }

            panelCanvasGanttGrandeEtapas.Size = new Size(
                ancho,
                Math.Max(alto, altoDisponible)
            );

            panelCanvasGanttGrandeEtapas.Invalidate();
        }

        private List<GanttGrandeItem> ConstruirItemsGanttGrandeEtapas()
        {
            if (TieneDesgloseProductivoParaGanttGrande())
            {
                return ConstruirItemsGanttGrandeDesdeDesglose();
            }

            List<GanttGrandeItem> items = new List<GanttGrandeItem>();

            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return items;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapa(e)))
            {
                if (etapa == null)
                {
                    continue;
                }

                bool etapaActiva =
                    etapa.Seleccionada ||
                    TieneSubEtapasActivasDesdeTablaPrincipal(etapa);

                int inicioSemanaEtapa = ConvertirMesInicioASemanaGanttGrande(etapa.InicioMes);
                int finSemanaEtapa = ConvertirMesFinASemanaExclusivaGanttGrande(etapa.FinMes);

                if (finSemanaEtapa <= inicioSemanaEtapa)
                {
                    double duracionMeses = etapa.DuracionMeses;

                    if (duracionMeses <= 0.0)
                    {
                        duracionMeses = 1.0;
                    }

                    finSemanaEtapa = inicioSemanaEtapa +
                        (int)Math.Ceiling(duracionMeses * GanttGrandeSemanasPorMes);
                }

                items.Add(new GanttGrandeItem
                {
                    EsEtapa = true,
                    NombreEtapa = etapa.Nombre,
                    Nombre = etapa.Nombre,
                    Estado = etapaActiva ? "Activa" : "Pendiente / sin definir",
                    Activo = etapaActiva,
                    InicioSemana = inicioSemanaEtapa,
                    FinSemanaExclusiva = Math.Max(inicioSemanaEtapa + 1, finSemanaEtapa)
                });

                if (bibliotecaSubEtapas == null)
                {
                    continue;
                }

                List<SubEtapaProyecto> subEtapas = bibliotecaSubEtapas
                    .Where(s =>
                        s != null &&
                        NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .OrderBy(s => s.Orden)
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapas)
                {
                    bool activa = sub.Activa || sub.Requerida;

                    /*
                     * IMPORTANTE:
                     * SubEtapaProyecto ahora calcula internamente con double:
                     * InicioSemana = 1.25
                     * DuracionSemanas = 0.40
                     * FinSemana = 1.65
                     *
                     * Pero el Gantt grande dibuja por columnas semanales enteras.
                     * Por eso aquí recién convertimos a int.
                     */

                    int inicioSemana = ObtenerSemanaVisualInicioGantt(sub.InicioSemana);
                    int finSemanaExclusiva = ObtenerSemanaVisualFinExclusivaGantt(sub.FinSemana);

                    if (finSemanaExclusiva <= inicioSemana)
                    {
                        finSemanaExclusiva = inicioSemana + 1;
                    }

                    string estado;

                    if (!activa)
                    {
                        estado = "Excluido";
                    }
                    else
                    {
                        estado = sub.Requerida
                            ? "Objetivo requerido"
                            : "Objetivo opcional";
                    }

                    items.Add(new GanttGrandeItem
                    {
                        EsEtapa = false,
                        NombreEtapa = etapa.Nombre,
                        Nombre = sub.Nombre,
                        Estado = estado,
                        Activo = activa,
                        InicioSemana = inicioSemana,
                        FinSemanaExclusiva = Math.Max(inicioSemana + 1, finSemanaExclusiva)
                    });
                }
            }

            return items;
        }

        private bool TieneDesgloseProductivoParaGanttGrande()
        {
            return cotizacion != null &&
                cotizacion.DesgloseProductivo != null &&
                cotizacion.DesgloseProductivo.Requerimientos != null &&
                cotizacion.DesgloseProductivo.Requerimientos.Any(r => r != null);
        }

        private List<GanttGrandeItem> ConstruirItemsGanttGrandeDesdeDesglose()
        {
            List<GanttGrandeItem> items = new List<GanttGrandeItem>();

            List<GanttReqPlan> requerimientos = ConstruirRequerimientosVisualesGanttDesdeDesglose();

            if (requerimientos.Count == 0 || cotizacion == null || cotizacion.Etapas == null)
            {
                return items;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapa(e)))
            {
                if (etapa == null)
                {
                    continue;
                }

                string nombreEtapa = ObtenerNombreVisibleEtapaGanttGrande(etapa.Nombre);

                List<GanttReqPlan> reqEtapa = requerimientos
                    .Where(r => NormalizarNombreEtapa(r.Etapa) == NormalizarNombreEtapa(nombreEtapa))
                    .OrderBy(r => r.InicioSemana)
                    .ThenBy(r => r.Nombre)
                    .ToList();

                bool etapaActiva = etapa.Seleccionada || reqEtapa.Count > 0;

                int inicioSemanaEtapa = reqEtapa.Count > 0
                    ? reqEtapa.Min(r => r.InicioSemana)
                    : ConvertirMesInicioASemanaGanttGrande(etapa.InicioMes);

                int finSemanaEtapa = reqEtapa.Count > 0
                    ? reqEtapa.Max(r => r.FinSemanaExclusiva)
                    : ConvertirMesFinASemanaExclusivaGanttGrande(etapa.FinMes);

                if (finSemanaEtapa <= inicioSemanaEtapa)
                {
                    finSemanaEtapa = inicioSemanaEtapa + 1;
                }

                items.Add(new GanttGrandeItem
                {
                    EsEtapa = true,
                    NombreEtapa = nombreEtapa,
                    Nombre = nombreEtapa,
                    Estado = etapaActiva ? "Activa" : "Pendiente / sin definir",
                    Activo = etapaActiva,
                    InicioSemana = inicioSemanaEtapa,
                    FinSemanaExclusiva = finSemanaEtapa
                });

                foreach (var grupoDetalle in reqEtapa.GroupBy(r => r.ClaveDetalle)
                    .OrderBy(g => g.Min(r => r.InicioSemana))
                    .ThenBy(g => g.First().Nombre))
                {
                    List<GanttReqPlan> reqDetalle = grupoDetalle.ToList();

                    if (reqDetalle.Count == 0)
                    {
                        continue;
                    }

                    GanttReqPlan primero = reqDetalle[0];
                    int totalUnidades = reqDetalle.Max(r => r.TotalProductos);

                    items.Add(new GanttGrandeItem
                    {
                        EsEtapa = false,
                        NombreEtapa = nombreEtapa,
                        Nombre = primero.Nombre,
                        Estado = ConstruirEstadoRequerimientoGantt(primero),
                        Activo = true,
                        TieneDetalleInterno = totalUnidades > 1,
                        ClaveDetalle = primero.ClaveDetalle,
                        Unidad = primero.Unidad,
                        TotalProductos = totalUnidades,
                        InicioSemana = reqDetalle.Min(r => r.InicioSemana),
                        FinSemanaExclusiva = Math.Max(
                            reqDetalle.Min(r => r.InicioSemana) + 1,
                            reqDetalle.Max(r => r.FinSemanaExclusiva)
                        )
                    });
                }
            }

            return items;
        }

        private List<GanttReqPlan> ConstruirRequerimientosVisualesGanttDesdeDesglose()
        {
            List<GanttReqPlan> planes = new List<GanttReqPlan>();

            if (!TieneDesgloseProductivoParaGanttGrande())
            {
                return planes;
            }

            var gruposBase = cotizacion.DesgloseProductivo.Requerimientos
                .Where(r => r != null)
                .GroupBy(r => ConstruirClaveRequerimientoGantt(r))
                .Select(g =>
                {
                    RequerimientoProduccionInterna primero = g.First();

                    return new GanttReqPlan
                    {
                        Etapa = ObtenerNombreVisibleEtapaGanttGrande(primero.EtapaSugerida),
                        Nombre = ConstruirNombreRequerimientoGantt(primero),
                        ClaveDetalle = ConstruirClaveDetalleMicroGantt(primero),
                        DependeDe = primero.DependeDe ?? "",
                        ModoPlanificacion = primero.ModoPlanificacion ?? "",
                        CargoSugerido = string.Join("; ",
                            g.Select(r => r.CargoSugerido ?? "")
                                .Where(c => !string.IsNullOrWhiteSpace(c))
                                .Distinct()),
                        Cantidad = Math.Max(1.0, g.Max(r => r.Cantidad)),
                        Unidad = primero.Unidad ?? "",
                        DiasStd = Math.Max(0.1, g.Max(r => r.DiasPersonaStd))
                    };
                })
                .ToList();

            foreach (GanttReqPlan plan in gruposBase)
            {
                /*
                 * La cantidad del producto ya viene incorporada en los dias-persona del
                 * desglose. Antes se clonaba el requerimiento por cantidad global y se
                 * mostraba como "N unidades en paralelo"; eso hacia que la Gantt pareciera
                 * asumir personal paralelo sin que Mano de obra lo hubiera definido.
                 */
                int instancias = 1;
                double diasCalendarioPlan = CalcularDiasCalendarioRequerimientoSegunManoObra(
                    plan.Etapa,
                    plan.CargoSugerido,
                    plan.DiasStd
                );
                double diasPorInstancia = Math.Max(0.1, diasCalendarioPlan / Math.Max(1, instancias));

                for (int i = 1; i <= instancias; i++)
                {
                    planes.Add(new GanttReqPlan
                    {
                        Etapa = plan.Etapa,
                        Nombre = plan.Nombre,
                        ClaveDetalle = plan.ClaveDetalle,
                        DependeDe = plan.DependeDe,
                        ModoPlanificacion = plan.ModoPlanificacion,
                        CargoSugerido = plan.CargoSugerido,
                        Cantidad = instancias > 1 ? 1.0 : plan.Cantidad,
                        Unidad = plan.Unidad,
                        IndiceProducto = i,
                        TotalProductos = 1,
                        DiasStd = diasPorInstancia,
                        InstanciasVisuales = instancias
                    });
                }
            }

            ProgramarRequerimientosGanttDesdeDesglose(planes);

            return planes;
        }

        private void ProgramarRequerimientosGanttDesdeDesglose(List<GanttReqPlan> planes)
        {
            if (planes == null || planes.Count == 0 || cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            Dictionary<string, double> finPorNombre = new Dictionary<string, double>();

            foreach (var grupoEtapa in planes.GroupBy(p => p.Etapa).OrderBy(g => ObtenerOrdenEtapaNombre(g.Key)))
            {
                EtapaProyecto etapa = cotizacion.Etapas.FirstOrDefault(e =>
                    e != null &&
                    NormalizarNombreEtapa(e.Nombre) == NormalizarNombreEtapa(grupoEtapa.Key));

                double inicioEtapa = etapa == null
                    ? 0.0
                    : Math.Max(0.0, etapa.InicioMes * GanttGrandeSemanasPorMes);

                List<GanttReqPlan> planesEtapa = grupoEtapa.ToList();
                Dictionary<string, List<double>> carrilesPorCargo =
                    CrearCarrilesManoObraParaEtapa(grupoEtapa.Key, planesEtapa);
                Dictionary<string, int> indiceParaleloPorProducto = new Dictionary<string, int>();
                HashSet<GanttReqPlan> pendientes = new HashSet<GanttReqPlan>(planesEtapa);
                int guardia = 0;

                while (pendientes.Count > 0 && guardia < 5000)
                {
                    guardia++;

                    List<GanttReqPlan> ordenables = pendientes
                        .Where(p => DependenciasListasGantt(p, finPorNombre, inicioEtapa))
                        .OrderBy(p => CalcularNivelDependenciaGantt(
                            p,
                            planesEtapa,
                            new Dictionary<string, int>(),
                            new HashSet<string>()))
                        .ThenBy(p => string.IsNullOrWhiteSpace(p.DependeDe) ? 0 : 1)
                        .ThenBy(p => p.DependeDe)
                        .ThenBy(p => p.Nombre)
                        .ToList();

                    if (ordenables.Count == 0)
                    {
                        ordenables = pendientes
                            .OrderBy(p => p.Nombre)
                            .ToList();
                    }

                    foreach (GanttReqPlan plan in ordenables)
                    {
                        double inicio = Math.Max(
                            inicioEtapa,
                            ObtenerFinDependenciaGantt(plan, finPorNombre, inicioEtapa)
                        );

                        if (string.IsNullOrWhiteSpace(plan.DependeDe) &&
                            EsModoPlanificacionParalelo(plan.ModoPlanificacion))
                        {
                            string claveProducto = plan.Etapa + "#" + plan.IndiceProducto.ToString();
                            int indiceParalelo = indiceParaleloPorProducto.ContainsKey(claveProducto)
                                ? indiceParaleloPorProducto[claveProducto]
                                : 0;

                            inicio += indiceParalelo * ObtenerDesfaseParaleloSemanas();
                            indiceParaleloPorProducto[claveProducto] = indiceParalelo + 1;
                        }

                        double duracionSemanas = Math.Max(
                            0.1,
                            plan.DiasStd / ObtenerDiasHabilesPorSemanaGantt()
                        );

                        string claveCargo = ObtenerClaveCargoPlanGantt(plan);
                        List<double> carrilesCargo =
                            ObtenerOCrearCarrilesCargoGantt(carrilesPorCargo, plan.Etapa, claveCargo);
                        int carril = ObtenerCarrilDisponibleGantt(carrilesCargo, inicio);

                        inicio = Math.Max(inicio, carrilesCargo[carril]);

                        double fin = inicio + duracionSemanas;
                        carrilesCargo[carril] = fin;

                        plan.InicioSemana = Math.Max(0, (int)Math.Floor(inicio));
                        plan.FinSemanaExclusiva = Math.Max(
                            plan.InicioSemana + 1,
                            (int)Math.Ceiling(fin)
                        );

                        foreach (string clavePlan in ObtenerClavesPlanGantt(plan))
                        {
                            if (!finPorNombre.ContainsKey(clavePlan) || finPorNombre[clavePlan] < fin)
                            {
                                finPorNombre[clavePlan] = fin;
                            }
                        }

                        pendientes.Remove(plan);
                    }
                }
            }
        }

        private List<string> SepararDependenciasGantt(string texto)
        {
            return (texto ?? "")
                .Replace(" y ", ";")
                .Split(new[] { ';', '|', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }

        private Dictionary<string, List<double>> CrearCarrilesManoObraParaEtapa(
            string etapa,
            List<GanttReqPlan> planesEtapa
        )
        {
            Dictionary<string, List<double>> carriles = new Dictionary<string, List<double>>();

            foreach (GanttReqPlan plan in planesEtapa)
            {
                string claveCargo = ObtenerClaveCargoPlanGantt(plan);

                if (carriles.ContainsKey(claveCargo))
                {
                    continue;
                }

                /*
                 * La duración del plan ya viene ajustada por la dotación de mano de obra.
                 * El carril evita montar dos trabajos del mismo cargo encima sin contar dos
                 * veces a las mismas personas.
                 */
                int cantidadCarriles = 1;

                carriles[claveCargo] = Enumerable.Repeat(0.0, cantidadCarriles).ToList();
            }

            return carriles;
        }

        private List<double> ObtenerOCrearCarrilesCargoGantt(
            Dictionary<string, List<double>> carrilesPorCargo,
            string etapa,
            string claveCargo
        )
        {
            if (carrilesPorCargo.ContainsKey(claveCargo))
            {
                return carrilesPorCargo[claveCargo];
            }

            int cantidadCarriles = 1;

            carrilesPorCargo[claveCargo] = Enumerable.Repeat(0.0, cantidadCarriles).ToList();
            return carrilesPorCargo[claveCargo];
        }

        private int ObtenerCarrilDisponibleGantt(List<double> carriles, double inicioMinimo)
        {
            if (carriles == null || carriles.Count == 0)
            {
                return 0;
            }

            int mejorIndice = 0;
            double mejorInicio = double.MaxValue;

            for (int i = 0; i < carriles.Count; i++)
            {
                double inicioCarril = Math.Max(inicioMinimo, carriles[i]);

                if (inicioCarril < mejorInicio)
                {
                    mejorInicio = inicioCarril;
                    mejorIndice = i;
                }
            }

            return mejorIndice;
        }

        private bool DependenciasListasGantt(
            GanttReqPlan plan,
            Dictionary<string, double> finPorNombre,
            double inicioEtapa
        )
        {
            if (plan == null || string.IsNullOrWhiteSpace(plan.DependeDe))
            {
                return true;
            }

            foreach (string dependencia in SepararDependenciasGantt(plan.DependeDe))
            {
                if (!ExisteFinDependenciaGantt(plan, dependencia, finPorNombre))
                {
                    return false;
                }
            }

            return true;
        }

        private double ObtenerFinDependenciaGantt(
            GanttReqPlan plan,
            Dictionary<string, double> finPorNombre,
            double inicioEtapa
        )
        {
            if (plan == null || string.IsNullOrWhiteSpace(plan.DependeDe))
            {
                return inicioEtapa;
            }

            double finDependencia = inicioEtapa;

            foreach (string dependencia in SepararDependenciasGantt(plan.DependeDe))
            {
                string claveDependencia = NormalizarClaveGantt(dependencia);
                string sufijoProducto = "#" + plan.IndiceProducto.ToString();

                double finParcial = finPorNombre
                    .Where(kv =>
                        kv.Key.EndsWith(sufijoProducto, StringComparison.Ordinal) &&
                        (kv.Key.Contains(claveDependencia) || claveDependencia.Contains(kv.Key)))
                    .Select(kv => kv.Value)
                    .DefaultIfEmpty(inicioEtapa)
                    .Max();

                finDependencia = Math.Max(finDependencia, finParcial);
            }

            return finDependencia;
        }

        private bool ExisteFinDependenciaGantt(
            GanttReqPlan plan,
            string dependencia,
            Dictionary<string, double> finPorNombre
        )
        {
            if (plan == null || string.IsNullOrWhiteSpace(dependencia))
            {
                return true;
            }

            string claveDependencia = NormalizarClaveGantt(dependencia);
            string sufijoProducto = "#" + plan.IndiceProducto.ToString();

            return finPorNombre.Any(kv =>
                kv.Key.EndsWith(sufijoProducto, StringComparison.Ordinal) &&
                (kv.Key.Contains(claveDependencia) || claveDependencia.Contains(kv.Key)));
        }

        private List<string> ObtenerClavesPlanGantt(GanttReqPlan plan)
        {
            List<string> claves = new List<string>();

            if (plan == null)
            {
                return claves;
            }

            string sufijoProducto = "#" + plan.IndiceProducto.ToString();

            claves.Add(NormalizarClaveGantt(plan.Nombre) + sufijoProducto);
            claves.Add(NormalizarClaveGantt(plan.ClaveDetalle) + sufijoProducto);

            if (!string.IsNullOrWhiteSpace(plan.Unidad))
            {
                claves.Add(NormalizarClaveGantt(plan.Nombre + "|" + plan.Unidad) + sufijoProducto);
            }

            return claves
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
        }

        private string ObtenerClaveCargoPlanGantt(GanttReqPlan plan)
        {
            if (plan == null || string.IsNullOrWhiteSpace(plan.CargoSugerido))
            {
                return "Cargo no definido";
            }

            return plan.CargoSugerido.Trim();
        }

        private double ObtenerDiasHabilesPorSemanaGantt()
        {
            if (cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0)
            {
                return cotizacion.DiasHabilesEstudioPorSemana;
            }

            return 5.0;
        }

        private int CalcularNivelDependenciaGantt(
            GanttReqPlan plan,
            List<GanttReqPlan> planes,
            Dictionary<string, int> cache,
            HashSet<string> visitados
        )
        {
            if (plan == null)
            {
                return 0;
            }

            string clave = NormalizarClaveGantt(plan.Nombre + "#" + plan.IndiceProducto.ToString());

            if (cache.ContainsKey(clave))
            {
                return cache[clave];
            }

            if (visitados.Contains(clave))
            {
                return 0;
            }

            visitados.Add(clave);

            List<string> dependencias = SepararDependenciasGantt(plan.DependeDe);

            if (dependencias.Count == 0)
            {
                cache[clave] = 0;
                return 0;
            }

            int nivelMaximo = 0;

            foreach (string dependencia in dependencias)
            {
                string claveDependencia = NormalizarClaveGantt(dependencia);

                GanttReqPlan previo = planes.FirstOrDefault(p =>
                    p != null &&
                    p.IndiceProducto == plan.IndiceProducto &&
                    (NormalizarClaveGantt(p.Nombre).Contains(claveDependencia) ||
                     claveDependencia.Contains(NormalizarClaveGantt(p.Nombre))));

                if (previo == null)
                {
                    continue;
                }

                int nivel = CalcularNivelDependenciaGantt(
                    previo,
                    planes,
                    cache,
                    new HashSet<string>(visitados)
                );

                nivelMaximo = Math.Max(nivelMaximo, nivel);
            }

            cache[clave] = nivelMaximo + 1;
            return cache[clave];
        }

        private string ConstruirClaveRequerimientoGantt(RequerimientoProduccionInterna req)
        {
            return string.Join("|",
                ObtenerNombreVisibleEtapaGanttGrande(req.EtapaSugerida),
                req.EntregableCliente ?? "",
                req.TipoInterno ?? "",
                req.NombreRequerimiento ?? "",
                req.Unidad ?? "",
                req.DependeDe ?? "",
                req.ModoPlanificacion ?? "");
        }

        private string ConstruirNombreRequerimientoGantt(RequerimientoProduccionInterna req)
        {
            string nombre = string.IsNullOrWhiteSpace(req.NombreRequerimiento)
                ? req.TipoInterno
                : req.NombreRequerimiento;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                nombre = req.EntregableCliente;
            }

            return string.IsNullOrWhiteSpace(nombre)
                ? "Requerimiento productivo"
                : nombre.Trim();
        }

        private string ConstruirClaveDetalleMicroGantt(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "";
            }

            return NormalizarClaveGantt(
                ObtenerNombreVisibleEtapaGanttGrande(req.EtapaSugerida) + "|" +
                ConstruirNombreRequerimientoGantt(req) + "|" +
                (req.Unidad ?? "") + "|" +
                (req.DependeDe ?? "")
            );
        }

        private int ObtenerCantidadGlobalGanttDesdeBrief()
        {
            if (cotizacion == null || cotizacion.BriefProducto == null)
            {
                return 1;
            }

            if (cotizacion.BriefProducto.CantidadGlobalProducto > 1)
            {
                return Math.Min(cotizacion.BriefProducto.CantidadGlobalProducto, 40);
            }

            if (cotizacion.BriefProducto.CantidadPiezas > 1)
            {
                return Math.Min(cotizacion.BriefProducto.CantidadPiezas, 40);
            }

            return 1;
        }

        private string ObtenerNombreGrupoProductoGantt(GanttReqPlan req)
        {
            string unidad = NormalizarClaveGantt(req == null ? "" : req.Unidad);
            string baseNombre = "Unidad";

            if (unidad.Contains("personaje"))
            {
                baseNombre = "Personaje";
            }
            else if (unidad.Contains("fondo"))
            {
                baseNombre = "Fondo";
            }
            else if (unidad.Contains("prop"))
            {
                baseNombre = "Prop";
            }
            else if (unidad.Contains("asset"))
            {
                baseNombre = "Asset";
            }
            else if (unidad.Contains("pieza"))
            {
                baseNombre = "Pieza";
            }

            int indice = req == null ? 1 : req.IndiceProducto;

            return baseNombre + " " + indice.ToString();
        }

        private int CalcularInstanciasVisualesRequerimiento(GanttReqPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            if (!EsUnidadConteableParaGantt(plan.Unidad))
            {
                return 1;
            }

            int cantidad = (int)Math.Round(plan.Cantidad);

            if (cantidad < 1)
            {
                return 1;
            }

            if (Math.Abs(plan.Cantidad - cantidad) > 0.001)
            {
                return 1;
            }

            return Math.Min(cantidad, 40);
        }

        private bool EsUnidadConteableParaGantt(string unidad)
        {
            string u = NormalizarClaveGantt(unidad);

            if (string.IsNullOrWhiteSpace(u))
            {
                return true;
            }

            if (u.Contains("segundo") || u.Contains("minuto") || u.Contains("hora") || u.Contains("frame"))
            {
                return false;
            }

            return true;
        }

        private string ConstruirEstadoRequerimientoGantt(GanttReqPlan req)
        {
            if (req == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(req.DependeDe))
            {
                return "Después de " + req.DependeDe;
            }

            return "Sin dependencia definida";
        }

        private string ObtenerNombreVisibleEtapaGanttGrande(string etapa)
        {
            string e = NormalizarNombreEtapa(etapa);

            if (e.Contains("desarrollo"))
            {
                return "Desarrollo";
            }

            if (e.Contains("pre"))
            {
                return "Preproduccion";
            }

            if (e.Contains("post"))
            {
                return "Postproduccion";
            }

            if (e.Contains("prod"))
            {
                return "Produccion";
            }

            return string.IsNullOrWhiteSpace(etapa) ? "Desarrollo" : etapa;
        }

        private int ObtenerOrdenEtapaNombre(string etapa)
        {
            string e = NormalizarNombreEtapa(etapa);

            if (e.Contains("desarrollo")) return 10;
            if (e.Contains("pre")) return 20;
            if (e.Contains("produccion")) return 30;
            if (e.Contains("post")) return 40;

            return 99;
        }

        private string NormalizarClaveGantt(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto.Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }

        private int ObtenerSemanaVisualInicioGantt(double semana)
        {
            if (semana < 0.0)
            {
                semana = 0.0;
            }

            return (int)Math.Floor(semana);
        }

        private int ObtenerSemanaVisualFinExclusivaGantt(double semanaFin)
        {
            if (semanaFin < 0.0)
            {
                semanaFin = 0.0;
            }

            int semanaVisual = (int)Math.Ceiling(semanaFin);

            if (semanaVisual < 0)
            {
                semanaVisual = 0;
            }

            return semanaVisual;
        }

        private int ConvertirMesInicioASemanaGanttGrande(double mes)
        {
            if (mes < 0.0)
            {
                mes = 0.0;
            }

            return (int)Math.Round(mes * GanttGrandeSemanasPorMes);
        }

        private int ConvertirMesFinASemanaExclusivaGanttGrande(double mesFin)
        {
            if (mesFin <= 0.0)
            {
                return 1;
            }

            return (int)Math.Ceiling(mesFin * GanttGrandeSemanasPorMes);
        }

        private int ObtenerSemanasTotalesGanttGrande(List<GanttGrandeItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return GanttGrandeSemanasMinimas;
            }

            int maxFin = items
                .Where(i => i != null)
                .Select(i => i.FinSemanaExclusiva)
                .DefaultIfEmpty(GanttGrandeSemanasMinimas)
                .Max();

            if (maxFin < GanttGrandeSemanasMinimas)
            {
                maxFin = GanttGrandeSemanasMinimas;
            }

            return maxFin;
        }

        private void PanelCanvasGanttGrandeEtapas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            hitBoxesGanttGrandeEtapas.Clear();

            int semanas = ObtenerSemanasTotalesGanttGrande(itemsGanttGrandeEtapas);

            int xNombre = GanttGrandeMargen;
            int xEstado = xNombre + GanttGrandeAnchoNombre;
            int xTimeline = xEstado + GanttGrandeAnchoEstado;

            int y = GanttGrandeMargen;

            DibujarHeaderGanttGrande(g, xNombre, xEstado, xTimeline, y, semanas);

            y += GanttGrandeAltoHeader;

            foreach (GanttGrandeItem item in itemsGanttGrandeEtapas)
            {
                DibujarFilaGanttGrande(g, item, xNombre, xEstado, xTimeline, y, semanas);
                y += item.AltoFila;
            }

            DibujarDetalleMicroGantt(g, xNombre, xEstado, xTimeline, y + 14, semanas);
        }

        private void DibujarHeaderGanttGrande(
            Graphics g,
            int xNombre,
            int xEstado,
            int xTimeline,
            int y,
            int semanas
        )
        {
            int meses = (int)Math.Ceiling(semanas / (double)GanttGrandeSemanasPorMes);

            using (Brush brushHeader = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (Pen pen = new Pen(Color.FromArgb(190, 190, 190)))
            using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                Rectangle rNombre = new Rectangle(
                    xNombre,
                    y,
                    GanttGrandeAnchoNombre,
                    GanttGrandeAltoHeader
                );

                Rectangle rEstado = new Rectangle(
                    xEstado,
                    y,
                    GanttGrandeAnchoEstado,
                    GanttGrandeAltoHeader
                );

                g.FillRectangle(brushHeader, rNombre);
                g.DrawRectangle(pen, rNombre);
                g.DrawString("Etapa / subetapa", font, Brushes.Black, rNombre, sf);

                g.FillRectangle(brushHeader, rEstado);
                g.DrawRectangle(pen, rEstado);
                g.DrawString("Estado", font, Brushes.Black, rEstado, sf);

                for (int m = 0; m < meses; m++)
                {
                    int semanaInicioMes = m * GanttGrandeSemanasPorMes;
                    int semanasEnMes = Math.Min(
                        GanttGrandeSemanasPorMes,
                        semanas - semanaInicioMes
                    );

                    Rectangle rMes = new Rectangle(
                        xTimeline + semanaInicioMes * GanttGrandeAnchoSemana,
                        y,
                        semanasEnMes * GanttGrandeAnchoSemana,
                        GanttGrandeAltoHeaderMes
                    );

                    g.FillRectangle(brushHeader, rMes);
                    g.DrawRectangle(pen, rMes);
                    g.DrawString("Mes " + (m + 1).ToString(), font, Brushes.Black, rMes, sf);
                }

                for (int s = 0; s < semanas; s++)
                {
                    Rectangle rSemana = new Rectangle(
                        xTimeline + s * GanttGrandeAnchoSemana,
                        y + GanttGrandeAltoHeaderMes,
                        GanttGrandeAnchoSemana,
                        GanttGrandeAltoHeaderSemana
                    );

                    g.FillRectangle(brushHeader, rSemana);
                    g.DrawRectangle(pen, rSemana);
                    g.DrawString("S" + s.ToString(), font, Brushes.Black, rSemana, sf);
                }
            }
        }

        private void DibujarFilaGanttGrande(
            Graphics g,
            GanttGrandeItem item,
            int xNombre,
            int xEstado,
            int xTimeline,
            int y,
            int semanas
        )
        {
            int alto = item.AltoFila;

            Color colorFila = item.EsEtapa
                ? ObtenerColorFilaEtapa(item.NombreEtapa)
                : item.EsGrupoProducto
                    ? MezclarConBlanco(ObtenerColorFilaEtapa(item.NombreEtapa), 0.18)
                    : MezclarConBlanco(ObtenerColorFilaEtapa(item.NombreEtapa), 0.30);

            if (!item.Activo)
            {
                colorFila = MezclarConBlanco(colorFila, 0.55);
            }

            Rectangle rNombre = new Rectangle(xNombre, y, GanttGrandeAnchoNombre, alto);
            Rectangle rEstado = new Rectangle(xEstado, y, GanttGrandeAnchoEstado, alto);
            Rectangle rTimeline = new Rectangle(
                xTimeline,
                y,
                semanas * GanttGrandeAnchoSemana,
                alto
            );

            using (Brush brushFila = new SolidBrush(colorFila))
            using (Pen penGrid = new Pen(Color.FromArgb(220, 220, 220)))
            using (Pen penMes = new Pen(Color.FromArgb(185, 185, 185)))
            {
                g.FillRectangle(brushFila, rNombre);
                g.FillRectangle(brushFila, rEstado);
                g.FillRectangle(Brushes.White, rTimeline);

                g.DrawRectangle(penGrid, rNombre);
                g.DrawRectangle(penGrid, rEstado);
                g.DrawRectangle(penGrid, rTimeline);

                for (int s = 0; s <= semanas; s++)
                {
                    int x = xTimeline + s * GanttGrandeAnchoSemana;

                    Pen penActual = (s % GanttGrandeSemanasPorMes == 0)
                        ? penMes
                        : penGrid;

                    g.DrawLine(penActual, x, y, x, y + alto);
                }
            }

            DibujarTextoFilaGanttGrande(g, item, rNombre, rEstado);
            DibujarBarraFilaGanttGrande(g, item, xTimeline, y, alto);

            if (item.TieneDetalleInterno && !string.IsNullOrWhiteSpace(item.ClaveDetalle))
            {
                hitBoxesGanttGrandeEtapas.Add(new GanttGrandeHitBox
                {
                    Bounds = new Rectangle(rNombre.X, rNombre.Y, rNombre.Width + rEstado.Width + rTimeline.Width, alto),
                    Item = item
                });
            }
        }

        private void DibujarTextoFilaGanttGrande(
            Graphics g,
            GanttGrandeItem item,
            Rectangle rNombre,
            Rectangle rEstado
        )
        {
            string prefijo = item.EsEtapa
                ? "▼ "
                : item.EsGrupoProducto
                    ? "  ▣ "
                    : "      ↳ ";
            string nombre = prefijo + item.Nombre;

            Color colorTexto = item.Activo
                ? Color.Black
                : Color.FromArgb(120, 120, 120);

            Color colorEstado = item.Activo
                ? ObtenerColorBordeEtapa(item.NombreEtapa)
                : Color.FromArgb(130, 130, 130);

            Font fuenteNombre = item.EsEtapa
                ? new Font("Segoe UI", 9, FontStyle.Bold)
                : item.EsGrupoProducto
                    ? new Font("Segoe UI", 8.5f, FontStyle.Bold)
                    : new Font("Segoe UI", 8, FontStyle.Regular);

            Font fuenteEstado = item.EsEtapa
                ? new Font("Segoe UI", 8, FontStyle.Bold)
                : new Font("Segoe UI", 8, FontStyle.Italic);

            using (fuenteNombre)
            using (fuenteEstado)
            using (Brush brushNombre = new SolidBrush(colorTexto))
            using (Brush brushEstado = new SolidBrush(colorEstado))
            using (StringFormat sfNombre = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            using (StringFormat sfEstado = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                Rectangle rNombreTexto = new Rectangle(
                    rNombre.X + 6,
                    rNombre.Y,
                    rNombre.Width - 10,
                    rNombre.Height
                );

                g.DrawString(nombre, fuenteNombre, brushNombre, rNombreTexto, sfNombre);
                g.DrawString(item.Estado, fuenteEstado, brushEstado, rEstado, sfEstado);
            }
        }

        private void DibujarBarraFilaGanttGrande(
            Graphics g,
            GanttGrandeItem item,
            int xTimeline,
            int y,
            int altoFila
        )
        {
            if (!item.Activo)
            {
                return;
            }

            int inicioSemana = item.InicioSemana;

            if (inicioSemana < 0)
            {
                inicioSemana = 0;
            }

            int finSemanaExclusiva = item.FinSemanaExclusiva;

            if (finSemanaExclusiva <= inicioSemana)
            {
                finSemanaExclusiva = inicioSemana + 1;
            }

            int indiceInicio = inicioSemana;
            int cantidadSemanas = finSemanaExclusiva - inicioSemana;

            int x = xTimeline + indiceInicio * GanttGrandeAnchoSemana;
            int ancho = Math.Max(8, cantidadSemanas * GanttGrandeAnchoSemana);

            int altoBarra = item.EsEtapa ? 18 : item.EsGrupoProducto ? 14 : 12;
            int yBarra = y + (altoFila - altoBarra) / 2;

            Rectangle rBarra = new Rectangle(
                x + 3,
                yBarra,
                Math.Max(6, ancho - 6),
                altoBarra
            );

            Color colorBarra = item.EsEtapa
                ? ObtenerColorBarraEtapa(item.NombreEtapa)
                : item.EsGrupoProducto
                    ? MezclarConBlanco(ObtenerColorBarraEtapa(item.NombreEtapa), 0.10)
                    : MezclarConBlanco(ObtenerColorBarraEtapa(item.NombreEtapa), 0.20);

            Color colorBorde = ObtenerColorBordeEtapa(item.NombreEtapa);

            using (Brush brush = new SolidBrush(colorBarra))
            using (Pen pen = new Pen(colorBorde, item.EsEtapa ? 2 : 1))
            {
                g.FillRectangle(brush, rBarra);
                g.DrawRectangle(pen, rBarra);
            }

            if (item.EsEtapa)
            {
                string texto =
                    "S" + inicioSemana.ToString() +
                    " - S" + finSemanaExclusiva.ToString();

                using (Font fuente = new Font("Segoe UI", 8, FontStyle.Bold))
                using (Brush brushTexto = new SolidBrush(Color.Black))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                })
                {
                    Rectangle rTexto = new Rectangle(
                        rBarra.X + 5,
                        rBarra.Y,
                        rBarra.Width - 6,
                        rBarra.Height
                    );

                    g.DrawString(texto, fuente, brushTexto, rTexto, sf);
                }
            }
        }

        private int CalcularAltoDetalleMicroGantt()
        {
            return 0;
        }

        private void DibujarDetalleMicroGantt(
            Graphics g,
            int xNombre,
            int xEstado,
            int xTimeline,
            int y,
            int semanas
        )
        {
            // Pendiente: detalle interno por subproducto/unidad.
        }

        private void PanelCanvasGanttGrandeEtapas_MouseClick(object sender, MouseEventArgs e)
        {
            GanttGrandeHitBox hit = hitBoxesGanttGrandeEtapas
                .FirstOrDefault(h => h.Bounds.Contains(e.Location));

            if (hit == null || hit.Item == null)
            {
                return;
            }

            claveDetalleGanttGrandeSeleccionada =
                claveDetalleGanttGrandeSeleccionada == hit.Item.ClaveDetalle
                    ? ""
                    : hit.Item.ClaveDetalle;

            panelCanvasGanttGrandeEtapas?.Invalidate();
        }
    }
}
