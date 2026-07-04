# Plan De Migracion Othalart

Este plan evita reescribir todo de golpe. La app actual sigue viva mientras se extrae el motor y se evalua Kitsu/Zou.

## Fase 0 - Congelar Reglas

- Documentar fuentes de verdad.
- Mantener WinForms funcionando.
- Evitar cambios grandes de UI que mezclen logica.
- Agregar diagnosticos cuando falten datos.

Salida esperada:

- `ARCHITECTURE.md`
- `UPSTREAM.md`
- `THIRD_PARTY_NOTICES.md`
- Build verde.

## Fase 1 - Contratos Del Motor

Extraer interfaces puras para:

- Productos y subproductos.
- Ecuaciones productivas.
- Rendimientos.
- Cargos participantes.
- Personal.
- Desglose productivo.
- Mano de obra.
- Resultados economicos.

Regla: WinForms debe llamar contratos, no manipular reglas internas.

## Fase 2 - Validacion De Bibliotecas

Crear una validacion unica que detecte:

- Producto sin subproducto.
- Subproducto sin ecuacion.
- Ecuacion sin formula madre valida.
- Cargo participante sin tarifa.
- Cargo productivo sin rendimiento.
- Cargo ejecutivo tratado como productor.
- Dependencia inexistente.
- Proyecto con seleccion guardada que no existe en biblioteca.

## Fase 3 - Adaptador Kitsu/Zou De Lectura

Sin modificar Kitsu aun:

- Leer projects, persons, departments, tasks.
- Crear DTOs Othalart equivalentes.
- Mapear Person -> PersonaEquipo.
- Mapear TaskType -> Subetapa/Proceso.
- Mapear Task -> Labor productiva.

## Fase 4 - Exportador Othalart A Kitsu

Crear un exportador que tome:

- Producto cotizado.
- Subproductos seleccionados.
- Procesos productivos.
- Dependencias explicitas.
- Personas/asignaciones.

Y proponga:

- Kitsu Project.
- Entities.
- Tasks.
- Assignments.
- Metadata Othalart.

## Fase 5 - Sincronizacion Bidireccional Controlada

Solo despues de probar Fase 4:

- Kitsu/Zou manda estado operativo.
- Othalart manda costo, presupuesto, propuesta y metadata productiva.
- Cambios conflictivos se muestran como advertencia, no se sobreescriben silenciosamente.

## Fase 6 - UI Nueva

La UI nueva puede ser:

- Fork Kitsu con modulo Othalart.
- Aplicacion web separada que use Zou.
- Hibrido: WinForms solo para beta y export.

No decidir UI definitiva hasta tener el adaptador probado.

## Riesgos De Compatibilidad

- Proyectos guardados dependen de JSON embebido.
- Nombres visibles han sido usados como IDs en varias pantallas.
- Cambiar cargos/productos puede dejar selecciones huerfanas.
- La Gantt actual mezcla propuesta, etapa y calculo.
- Mano de obra historica puede no tener trazabilidad a subproducto/labor.

## Regla De Migracion De Datos

Cada migracion debe:

1. Leer formato antiguo.
2. Completar IDs estables cuando falten.
3. Mantener nombres visibles.
4. Registrar warnings.
5. Guardar en formato nuevo solo si el usuario confirma.

## Primer Corte Recomendado

Antes de tocar Kitsu:

1. Crear IDs estables en todas las bibliotecas Othalart.
2. Separar cargos productivos, cargos de gestion y cargos comerciales.
3. Asegurar que cada subproducto tenga:
   - proceso,
   - ecuacion,
   - cargos participantes,
   - dependencia explicita,
   - diagnostico si falta algo.
4. Hacer que Mano de obra consuma solo outputs del desglose.
5. Recién ahi probar export a Kitsu/Zou.
