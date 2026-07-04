using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaOpcionesProducto2DJsonService
    {
        private const string NombreArchivo = "opciones_productos2d.json";

        public static string ObtenerRutaOpciones()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivo);
        }

        public static OpcionesProducto2D CargarOpciones()
        {
            try
            {
                AsegurarArchivo();
                string json = File.ReadAllText(ObtenerRutaOpciones());
                OpcionesProducto2D opciones = JsonSerializer.Deserialize<OpcionesProducto2D>(
                    json,
                    CrearOpcionesJson()
                ) ?? CrearBase();

                NormalizarOpciones(opciones);
                return opciones;
            }
            catch
            {
                return CrearBase();
            }
        }

        public static void GuardarOpciones(OpcionesProducto2D opciones)
        {
            opciones = opciones ?? CrearBase();
            NormalizarOpciones(opciones);

            string ruta = ObtenerRutaOpciones();
            string carpeta = Path.GetDirectoryName(ruta) ?? "";

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            File.WriteAllText(ruta, JsonSerializer.Serialize(opciones, CrearOpcionesJson()));
        }

        public static OpcionesProducto2D CargarOpcionesConProductos(
            IEnumerable<Producto2DDefinicion> productos
        )
        {
            OpcionesProducto2D opciones = CargarOpciones();

            foreach (Producto2DDefinicion producto in productos ?? new List<Producto2DDefinicion>())
            {
                AgregarSiFalta(opciones.Industrias, producto?.Industria);
                AgregarSiFalta(opciones.Categorias, producto?.Categoria);
                AgregarSiFalta(opciones.UnidadesCantidad, producto?.UnidadCantidadSugerida);
                AgregarSiFalta(opciones.UnidadesDuracion, producto?.UnidadDuracionSugerida);
            }

            NormalizarOpciones(opciones);
            return opciones;
        }

        public static OpcionesProducto2D CrearBase()
        {
            return new OpcionesProducto2D
            {
                Industrias = new List<string>
                {
                    "General",
                    "Animacion",
                    "Diseno",
                    "Audiovisual",
                    "Publicidad / marketing",
                    "Videojuegos"
                },
                Categorias = new List<string>
                {
                    "Animacion",
                    "Preproduccion",
                    "Produccion",
                    "Postproduccion",
                    "Diseno",
                    "Audiovisual",
                    "Audio",
                    "Assets",
                    "Narrativa visual",
                    "Producto final"
                },
                UnidadesCantidad = new List<string>
                {
                    "piezas",
                    "personajes",
                    "fondos",
                    "assets",
                    "sets",
                    "paginas",
                    "cinematicas",
                    "loops",
                    "unidades"
                },
                UnidadesDuracion = new List<string>
                {
                    "segundos",
                    "minutos",
                    "horas",
                    "dias",
                    "no aplica"
                }
            };
        }

        private static void AsegurarArchivo()
        {
            if (!File.Exists(ObtenerRutaOpciones()))
            {
                GuardarOpciones(CrearBase());
            }
        }

        private static void NormalizarOpciones(OpcionesProducto2D opciones)
        {
            opciones.Industrias = NormalizarLista(opciones.Industrias, CrearBase().Industrias);
            opciones.Categorias = NormalizarLista(opciones.Categorias, CrearBase().Categorias);
            opciones.UnidadesCantidad = NormalizarLista(
                opciones.UnidadesCantidad,
                CrearBase().UnidadesCantidad
            );
            opciones.UnidadesDuracion = NormalizarLista(
                opciones.UnidadesDuracion,
                CrearBase().UnidadesDuracion
            );
        }

        private static List<string> NormalizarLista(List<string> valores, List<string> fallback)
        {
            List<string> resultado = new List<string>();

            foreach (string valor in valores ?? fallback ?? new List<string>())
            {
                AgregarSiFalta(resultado, valor);
            }

            if (resultado.Count == 0)
            {
                foreach (string valor in fallback ?? new List<string>())
                {
                    AgregarSiFalta(resultado, valor);
                }
            }

            return resultado;
        }

        private static void AgregarSiFalta(List<string> lista, string valor)
        {
            valor = (valor ?? "").Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            if (!lista.Any(v => string.Equals(v, valor, StringComparison.OrdinalIgnoreCase)))
            {
                lista.Add(valor);
            }
        }

        private static string ObtenerCarpetaBiblioteca()
        {
            string carpeta = Path.Combine(ObtenerRaizAplicacion(), "Bibliotecas", "default");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            return carpeta;
        }

        private static string ObtenerRaizAplicacion()
        {
            string raizConfigurada = Environment.GetEnvironmentVariable("OTHALART_PROJECT_ROOT") ?? "";
            if (!string.IsNullOrWhiteSpace(raizConfigurada) &&
                Directory.Exists(Path.Combine(raizConfigurada, "Bibliotecas")))
            {
                return raizConfigurada;
            }

            string directorioTrabajo = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(directorioTrabajo) &&
                Directory.Exists(Path.Combine(directorioTrabajo, "Bibliotecas")))
            {
                return directorioTrabajo;
            }

            DirectoryInfo actual = new DirectoryInfo(AppContext.BaseDirectory);

            while (actual != null)
            {
                bool tieneCsproj = actual.GetFiles("*.csproj").Length > 0;
                bool tieneCarpetaData = Directory.Exists(Path.Combine(actual.FullName, "Data"));

                if (tieneCsproj || tieneCarpetaData)
                {
                    return actual.FullName;
                }

                actual = actual.Parent;
            }

            return AppContext.BaseDirectory;
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
