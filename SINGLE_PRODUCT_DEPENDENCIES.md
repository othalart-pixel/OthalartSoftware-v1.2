# SINGLE PRODUCT DEPENDENCIES

Auditoria inicial de dependencias donde el sistema aun asume que una cotizacion equivale a un unico producto seleccionado desde `Datos`.

Estado:
- `pendiente`: aun depende del flujo monoproducto.
- `parcial`: existe adaptador o puente, pero la pantalla aun no consume `ProyectoCotizacion` como fuente exclusiva.
- `migrado`: consume `ProyectoCotizacion`/`ProyectoProductivoExpansionService`.

| Archivo | Metodo / campo | Dependencia | Reemplazo propuesto | Estado |
| --- | --- | --- | --- | --- |
| `UI/Form1.TabDatos.cs` | `cmbTipoProductoServicio`, `cmbProductoServicio` | Selecciona un unico producto/servicio desde Datos. | Mover selector a `Productos y servicios`; Datos solo debe editar datos generales del proyecto. | pendiente |
| `UI/Form1.TabDatosLogic.cs` | `CargarDatosEnPantalla`, `AplicarDatosDesdePantalla` | Lee/escribe `BriefProducto.ProductoServicioSeleccionado`, `TipoProducto`, `TipoPiezaPrincipal`. | Escribir datos generales en `ProyectoCotizacion` y mantener brief solo como compatibilidad. | pendiente |
| `UI/Form1.TabDatosLogic.cs` | `GuardarEntregablesSeleccionadosDesdePantalla` | Crea seleccion unica de piezas/subproductos desde grilla de Datos. | Crear/editar `SubproductoProyecto` dentro del item seleccionado en `Productos y servicios`. | pendiente |
| `UI/Form1.TabDatosLogic.cs` | `InvalidarDesgloseProductivoPorCambioDeBrief` | Invalida `cotizacion.DesgloseProductivo` cuando cambia el producto unico. | Invalidar/recalcular por item o proyecto mediante `ProyectoProductivoExpansionService`. | pendiente |
| `UI/Form1.TabDatosLogic.cs` | `GenerarDesgloseProductivoDesdeEcuaciones` invocado desde Datos | Genera desglose desde el brief monoproducto. | Generar filas normalizadas desde `ProyectoCotizacion -> Grupos -> Items -> Subproductos -> Procesos`. | pendiente |
| `Services/DesgloseProductivoService.cs` | `Generar(Cotizacion)` | Recorre `cotizacion.BriefProducto.EntregablesSeleccionados`. | Nuevo camino `GenerarDesdeProyecto(ProyectoCotizacion)` o adaptador desde `ProyectoProductivoExpansionService`. | pendiente |
| `Services/DesgloseProductivoService.cs` | `AsegurarRequerimientosDesdePipelineSeleccionado` | Usa pipeline del producto seleccionado en el brief. | Instanciar procesos al agregar producto/servicio y expandir desde el proyecto. | pendiente |
| `UI/Form1.TabDesgloseProductivo.cs` | `TabDesgloseProductivo_Enter` | Si no hay `cotizacion.DesgloseProductivo`, lo genera desde ecuaciones/brief. | Cargar desglose desde filas del `ProyectoProductivoExpansionService`. | pendiente |
| `UI/Form1.TabDesgloseProductivo.cs` | `CargarDesgloseProductivoEnPantalla` / `GuardarDesgloseProductivoDesdePantalla` | Tabla opera sobre un unico `cotizacion.DesgloseProductivo.Requerimientos`. | Operar sobre filas del proyecto y guardar overrides en item/subproducto/proceso/asignacion. | pendiente |
| `UI/Form1.TabManoObra.cs` | `RellenarManoObraDesdeDesgloseProductivo` | Rellena equipo desde `cotizacion.DesgloseProductivo`. | Consolidar asignaciones por proyecto desde filas normalizadas. | pendiente |
| `UI/Form1.EtapasDesdeDesglose.cs` | `ObtenerRequerimientosProductivosComoObjetos` | Obtiene requerimientos solo desde `cotizacion.DesgloseProductivo`. | Obtener procesos/filas desde `ProyectoCotizacion`. | pendiente |
| `UI/Form1.TabCostos.cs` | calculos ligados a `cotizacion.DesgloseProductivo` | Costos productivos dependen del desglose unico. | Usar consolidado de proyecto y `CostoProyectoProductivoCLP`. | parcial |
| `Services/PlanCapacidadDesdeDesgloseService.cs` | entrada `Cotizacion` | Calcula capacidad desde `cotizacion.DesgloseProductivo.Requerimientos`. | Entrada nueva con filas normalizadas del proyecto. | pendiente |
| `Services/PropuestaPlanificacionDesgloseService.cs` | entrada `Cotizacion` | Propone planificacion desde desglose unico. | Proponer desde procesos del proyecto consolidado. | pendiente |
| `Services/EvaluadorPlazoProyectoService.cs` | `DesgloseProductivoService.Generar(cotizacion)` | Evalua plazo generando desde brief monoproducto. | Evaluar plazo desde `ProyectoConsolidado` y procesos/depencias del proyecto. | pendiente |
| `Services/ExpansorBriefProduccionService.cs` | `brief.EntregablesSeleccionados` | Expande brief monoproducto. | Mantener solo como compatibilidad/importador a `ProyectoCotizacion`. | pendiente |
| `Services/Integrations/OthalartKitsuZouMappingService.cs` | `BriefProducto.ProductoServicioSeleccionado` | Exporta un producto principal. | Exportar todos los items del proyecto. | pendiente |
| `UI/Form1.TabGuardar.cs` | `TipoSolicitudDatosGuardado`, `ProductoServicioDatosGuardado`, `Piezas2DDatosGuardadas` | Persiste respaldo del flujo monoproducto. | Mantener por migracion, pero guardar/cargar `ProyectoProductivo` como fuente operativa. | parcial |
| `UI/Form1.Visuals.cs` | resumen lateral de entregables | Resume `BriefProducto.EntregablesSeleccionados`. | Resumen visual desde `ProyectoConsolidado`. | pendiente |
| `UI/DatosEntregables.cs` | grilla de productos/entregables | Seleccion de entregables desde producto unico. | Reubicar como compatibilidad o asistente de item. | pendiente |

Notas de migracion:
- `ProyectoProductivoExpansionService` ya existe y debe convertirse en el unico servicio de expansion operativa.
- `Cotizacion.DesgloseProductivo` debe quedar como cache/compatibilidad temporal, no como fuente primaria.
- Las pantallas principales no se consideran migradas mientras lean `cmbProductoServicio` o `BriefProducto.EntregablesSeleccionados` para calcular.
