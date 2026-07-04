using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaPersonalEmpresaJsonService
    {
        private const string NombreArchivoPersonal = "personal_empresa.json";

        public static string ObtenerRutaPersonal()
        {
            return Path.Combine(
                BibliotecaCargosJsonService.ObtenerCarpetaBiblioteca(),
                NombreArchivoPersonal
            );
        }

        public static void CrearPersonalJsonSiNoExiste()
        {
            string ruta = ObtenerRutaPersonal();

            if (File.Exists(ruta))
            {
                return;
            }

            GuardarPersonal(new List<PersonaEquipo>());
        }

        public static List<PersonaEquipo> CargarPersonal()
        {
            CrearPersonalJsonSiNoExiste();

            try
            {
                string json = File.ReadAllText(ObtenerRutaPersonal());
                List<PersonaEquipo> personas = JsonSerializer.Deserialize<List<PersonaEquipo>>(
                    json,
                    CrearOpcionesJson()
                ) ?? new List<PersonaEquipo>();

                NormalizarPersonas(personas);
                return personas;
            }
            catch
            {
                return new List<PersonaEquipo>();
            }
        }

        public static void GuardarPersonal(List<PersonaEquipo> personas)
        {
            string ruta = ObtenerRutaPersonal();
            string carpeta = Path.GetDirectoryName(ruta) ?? "";

            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            personas = personas ?? new List<PersonaEquipo>();
            NormalizarPersonas(personas);

            string json = JsonSerializer.Serialize(personas, CrearOpcionesJson());
            File.WriteAllText(ruta, json);
        }

        public static string CrearIdUnico(List<PersonaEquipo> personas)
        {
            personas = personas ?? new List<PersonaEquipo>();

            for (int i = 1; i < 100000; i++)
            {
                string id = "persona_" + i.ToString("000");

                if (!personas.Any(p => string.Equals(p?.Id, id, StringComparison.OrdinalIgnoreCase)))
                {
                    return id;
                }
            }

            return "persona_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private static void NormalizarPersonas(List<PersonaEquipo> personas)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (PersonaEquipo persona in personas)
            {
                if (persona == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(persona.Id) || ids.Contains(persona.Id))
                {
                    persona.Id = CrearIdUnico(personas);
                }

                ids.Add(persona.Id);

                if (persona.CargosPosibles == null)
                {
                    persona.CargosPosibles = new List<string>();
                }

                persona.CargosPosibles = persona.CargosPosibles
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();

                if (persona.TrabajosPosibles == null)
                {
                    persona.TrabajosPosibles = new List<string>();
                }

                persona.TrabajosPosibles = persona.TrabajosPosibles
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(t => t)
                    .ToList();

                if (string.IsNullOrWhiteSpace(persona.PeriodoPago))
                {
                    persona.PeriodoPago = "Mensual";
                }

                if (persona.HorasTrabajoSemana <= 0)
                {
                    persona.HorasTrabajoSemana = 42m;
                }

                if (persona.PagoInterno <= 0 && persona.CostoHora > 0)
                {
                    persona.PagoInterno = persona.CostoHora * ObtenerHorasPeriodo(persona.PeriodoPago, persona.HorasTrabajoSemana);
                }

                if (persona.CostoHora <= 0 && persona.PagoInterno > 0)
                {
                    decimal horasPeriodo = ObtenerHorasPeriodo(persona.PeriodoPago, persona.HorasTrabajoSemana);
                    persona.CostoHora = horasPeriodo <= 0 ? 0 : Math.Round(persona.PagoInterno / horasPeriodo, 2);
                }

                if (persona.HorasMaximasPorTanda <= 0)
                {
                    persona.HorasMaximasPorTanda = 42m;
                }
            }
        }

        private static decimal ObtenerHorasPeriodo(string periodo, decimal horasSemana)
        {
            horasSemana = horasSemana <= 0 ? 42m : horasSemana;
            string valor = (periodo ?? "").Trim().ToLowerInvariant();

            if (valor == "semanal" || valor == "semana")
            {
                return horasSemana;
            }

            if (valor == "quincenal" || valor == "quincena")
            {
                return horasSemana * 2m;
            }

            if (valor == "diario" || valor == "dia" || valor == "día")
            {
                return Math.Max(1m, horasSemana / 5m);
            }

            return horasSemana * 4m;
        }

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
    }
}
