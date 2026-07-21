# Pruebas automatizadas de Othalart

## Ejecutar toda la suite

Desde la carpeta raíz del proyecto:

```powershell
dotnet test "OthalartSoftware v1.0.slnx" -c Debug
```

## Cobertura inicial

La primera suite protege:

- porcentajes arbitrarios con punto, coma o símbolo `%`;
- horas semanales decimales;
- dependencias de procesos y eliminación de duplicados;
- edición mediante tiempo asignado;
- conversión entre horas y días;
- recálculo de costos después de modificar horas;
- guardado y carga del JSON de proyecto;
- conservación de cargos, horas y modificaciones locales;
- aislamiento entre archivos de proyectos distintos;
- rechazo controlado de archivos vacíos o inválidos.

Las pruebas de persistencia crean una carpeta única dentro del directorio temporal del
sistema y la eliminan al terminar. No escriben en `Bibliotecas` ni en los proyectos
reales del usuario.

## Organización

- `ReglaCalculoProcesoServiceTests.cs`: ecuaciones, porcentajes y dependencias.
- `CalculoProductivoResolverServiceTests.cs`: horas asignadas, días y costos.
- `ProyectoOthalartArchivoServiceTests.cs`: round trip e independencia de proyectos.

## Próximos escenarios recomendados

1. Snapshot local frente a proceso local sin snapshot.
2. Distribución de horas entre varios cargos.
3. Consolidación de horas y costos de un proyecto completo.
4. Migración de proyectos de versiones anteriores.
5. Pruebas mínimas de navegación e inspector WinForms.
