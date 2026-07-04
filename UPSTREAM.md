# Upstream Kitsu/Zou

Este archivo registra la auditoria inicial de Kitsu y Zou como posibles bases open source.

## Repos Revisados

| Proyecto | URL | Rol | Stack | Licencia observada |
| --- | --- | --- | --- | --- |
| Kitsu | https://github.com/cgwire/kitsu | UI de gestion de produccion | Vue/Vite | AGPL-3.0 |
| Zou | https://github.com/cgwire/zou | API/backend de Kitsu | Python/Flask/SQLAlchemy | AGPL-3.0 |

Clones locales de auditoria:

- `.codex-audit/kitsu`
- `.codex-audit/zou`

Estos clones no son parte del producto y quedan ignorados por Git.

## Piezas Reutilizables

### Kitsu

Kitsu ya contiene modulos UI para:

- Producciones/proyectos.
- Assets, shots, entities y breakdown.
- Tasks, task types y task status.
- People, departments y asignaciones.
- Schedule.
- Comments, previews, playlists y revision.
- Export/import y vistas de produccion.

Directorios relevantes vistos:

- `src/store/modules/tasks.js`
- `src/store/modules/people.js`
- `src/store/modules/departments.js`
- `src/store/modules/schedule.js`
- `src/store/modules/assets.js`
- `src/store/modules/shots.js`
- `src/store/modules/entities.js`
- `src/store/modules/breakdown.js`

### Zou

Zou ya contiene modelos y endpoints para:

- `Project`
- `Entity`
- `Task`
- `TaskType`
- `TaskStatus`
- `Person`
- `Department`
- `ScheduleItem`
- `TimeSpent`
- `Comment`
- `PreviewFile`
- Assign/unassign de tareas.

Archivos relevantes vistos:

- `zou/app/models/project.py`
- `zou/app/models/entity.py`
- `zou/app/models/task.py`
- `zou/app/models/task_type.py`
- `zou/app/models/task_status.py`
- `zou/app/models/person.py`
- `zou/app/models/department.py`
- `zou/app/models/schedule_item.py`
- `zou/app/blueprints/tasks/resources.py`

## Matriz Reusar / Adaptar / Mantener

| Capacidad | Decision |
| --- | --- |
| Gestion de proyectos | Reusar Kitsu/Zou |
| Personas/equipo | Reusar Kitsu/Zou, extender con datos Othalart |
| Tareas/estados/asignaciones | Reusar Kitsu/Zou |
| Comments/previews/revision | Reusar Kitsu/Zou |
| Gantt/seguimiento operativo | Reusar/adaptar Kitsu/Zou |
| Productos cotizables | Mantener Othalart y mapear a templates |
| Ecuaciones productivas | Mantener Othalart |
| Rendimientos/costos | Mantener Othalart |
| Precio, margen, informe | Mantener Othalart |
| Export comercial | Mantener Othalart |
| UI WinForms completa | Mantener como beta/prototipo mientras se migra |

## Estrategia De Fork

1. Mantener los repos upstream limpios.
2. Crear fork propio solo cuando exista decision legal/producto.
3. No copiar codigo AGPL dentro de WinForms sin revisar licencia.
4. Encapsular integracion con API/adaptador primero.
5. Registrar cambios propios como capa Othalart sobre entidades upstream.

## Riesgos

- AGPL-3.0 afecta distribucion y uso en red.
- Kitsu/Zou resuelven produccion, no cotizacion economica.
- Mapear productos Othalart a entities/tasks requiere IDs estables.
- Si se intenta migrar UI antes del motor, se duplican bugs.
- Si se copia codigo sin frontera, se pierde claridad de licencia.

## Proxima Auditoria Tecnica

Antes de fork real:

1. Levantar Kitsu/Zou localmente.
2. Crear un proyecto de prueba con assets/tasks/persons.
3. Probar API de creacion de tasks y assignment.
4. Definir extension de metadata Othalart por task/entity.
5. Probar export/import desde `productos2d.json`.
