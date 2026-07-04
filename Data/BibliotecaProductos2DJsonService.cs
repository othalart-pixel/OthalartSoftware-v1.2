using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaProductos2DJsonService
    {
        private const string NombreArchivoProductos = "productos2d.json";

        public static string ObtenerRutaProductos()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivoProductos);
        }

        public static List<Producto2DDefinicion> CargarProductos()
        {
            try
            {
                AsegurarArchivo();

                string json = File.ReadAllText(ObtenerRutaProductos());
                bool requiereMigracionEcuaciones = !json.Contains("\"EcuacionProductiva\"");
                List<Producto2DDefinicion> productos =
                    JsonSerializer.Deserialize<List<Producto2DDefinicion>>(
                        json,
                        CrearOpcionesJson()
                    ) ?? CrearProductosBase();

                if (productos.Count == 0)
                {
                    productos = CrearProductosBase();
                    GuardarProductos(productos);
                }

                bool cambioNormalizacion = NormalizarProductos(productos);
                if (requiereMigracionEcuaciones || cambioNormalizacion)
                {
                    GuardarProductos(productos);
                }

                return productos;
            }
            catch
            {
                return CrearProductosBase();
            }
        }

        public static void GuardarProductos(List<Producto2DDefinicion> productos)
        {
            if (productos == null)
            {
                productos = CrearProductosBase();
            }

            NormalizarProductos(productos);

            string ruta = ObtenerRutaProductos();
            string carpeta = Path.GetDirectoryName(ruta) ?? "";

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string json = JsonSerializer.Serialize(productos, CrearOpcionesJson());
            File.WriteAllText(ruta, json);
        }

        public static void RegenerarProductosBase()
        {
            GuardarProductos(CrearProductosBase());
        }

        public static List<Producto2DDefinicion> CrearProductosBase()
        {
            List<Producto2DDefinicion> productos = BibliotecaProductos2D.CrearBaseDesdeCodigo();
            NormalizarProductos(productos);
            return productos;
        }

        private static void AsegurarArchivo()
        {
            if (File.Exists(ObtenerRutaProductos()))
            {
                return;
            }

            GuardarProductos(CrearProductosBase());
        }

        private static bool NormalizarProductos(List<Producto2DDefinicion> productos)
        {
            bool cambio = false;

            foreach (Producto2DDefinicion producto in productos)
            {
                if (producto == null)
                {
                    continue;
                }

                if (producto.Subproductos == null)
                {
                    producto.Subproductos = new List<Subproducto2D>();
                    cambio = true;
                }

                if (producto.Etapas == null || producto.Etapas.Count == 0)
                {
                    producto.Etapas = CrearEtapasDesdeSubproductos(producto.Subproductos);
                    cambio = true;
                }

                cambio |= NormalizarSubproductosProducto(producto);
                cambio |= NormalizarEtapasProducto(producto);
            }

            return cambio;
        }

        private static bool NormalizarSubproductosProducto(Producto2DDefinicion producto)
        {
            bool cambio = false;
            int orden = 10;

            foreach (Subproducto2D subproducto in producto.Subproductos)
            {
                if (subproducto == null)
                {
                    continue;
                }

                if (subproducto.Orden <= 0)
                {
                    subproducto.Orden = orden;
                    cambio = true;
                }

                if (string.IsNullOrWhiteSpace(subproducto.Categoria))
                {
                    subproducto.Categoria = ObtenerNombreVisibleEtapa(subproducto.EtapaSugerida);
                    cambio = true;
                }

                if (subproducto.DependeDe == null)
                {
                    subproducto.DependeDe = "";
                    cambio = true;
                }

                if (string.IsNullOrWhiteSpace(subproducto.Resolucion))
                {
                    subproducto.Resolucion = "Interno";
                    cambio = true;
                }

                cambio |= VincularSubproductoConEcuacionJson(subproducto);

                orden += 10;
            }

            return cambio;
        }

        private static List<ProductoEtapaDefinicion> CrearEtapasDesdeSubproductos(
            List<Subproducto2D> subproductos
        )
        {
            List<string> etapas = subproductos
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.EtapaSugerida))
                .Select(s => s.EtapaSugerida)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(ObtenerOrdenEtapaBase)
                .ToList();

            List<ProductoEtapaDefinicion> resultado = new List<ProductoEtapaDefinicion>();
            string etapaAnterior = "";

            foreach (string etapa in etapas)
            {
                resultado.Add(new ProductoEtapaDefinicion
                {
                    ClaveEtapa = etapa,
                    NombreVisible = ObtenerNombreVisibleEtapa(etapa),
                    Orden = ObtenerOrdenEtapaBase(etapa),
                    Activa = true,
                    DependeDe = etapaAnterior
                });

                etapaAnterior = etapa;
            }

            return resultado;
        }

        private static bool VincularSubproductoConEcuacionJson(Subproducto2D subproducto)
        {
            if (subproducto == null)
            {
                return false;
            }

            bool cambio = false;

            EcuacionProductivaDefinicion ecuacion =
                BibliotecaEcuacionesProductivasJsonService.BuscarMejorPara(
                    subproducto.EtapaSugerida,
                    subproducto.SubEtapaSugerida,
                    subproducto.Nombre,
                    subproducto.CargosSugeridos
                );

            if (ecuacion == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(subproducto.EtapaSugerida) &&
                !string.IsNullOrWhiteSpace(ecuacion.Etapa))
            {
                subproducto.EtapaSugerida = ecuacion.Etapa;
                cambio = true;
            }

            if (string.IsNullOrWhiteSpace(subproducto.SubEtapaSugerida) &&
                !string.IsNullOrWhiteSpace(ecuacion.SubEtapa))
            {
                subproducto.SubEtapaSugerida = ecuacion.SubEtapa;
                cambio = true;
            }

            string textoEcuacion = FormatearEcuacionProducto(ecuacion);
            if (string.IsNullOrWhiteSpace(subproducto.EcuacionProductiva) &&
                !string.IsNullOrWhiteSpace(textoEcuacion))
            {
                subproducto.EcuacionProductiva = textoEcuacion;
                cambio = true;
            }

            if (string.IsNullOrWhiteSpace(subproducto.VariablesEcuacion) &&
                !string.IsNullOrWhiteSpace(ecuacion.Variables))
            {
                subproducto.VariablesEcuacion = ecuacion.Variables;
                cambio = true;
            }

            if (string.IsNullOrWhiteSpace(subproducto.ImpactoEcuacion) &&
                !string.IsNullOrWhiteSpace(ecuacion.Impacto))
            {
                subproducto.ImpactoEcuacion = ecuacion.Impacto;
                cambio = true;
            }

            if (string.IsNullOrWhiteSpace(subproducto.CargosSugeridos) &&
                !string.IsNullOrWhiteSpace(ecuacion.CargosPermitidos))
            {
                subproducto.CargosSugeridos = ecuacion.CargosPermitidos;
                cambio = true;
            }

            if (string.IsNullOrWhiteSpace(subproducto.Categoria))
            {
                subproducto.Categoria = ObtenerNombreVisibleEtapa(subproducto.EtapaSugerida);
                cambio = true;
            }

            return cambio;
        }

        private static string FormatearEcuacionProducto(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(ecuacion.Clave))
            {
                return ecuacion.NombreVisible ?? "";
            }

            if (string.IsNullOrWhiteSpace(ecuacion.NombreVisible))
            {
                return ecuacion.Clave;
            }

            return ecuacion.Clave + " | " + ecuacion.NombreVisible;
        }

        private static bool NormalizarEtapasProducto(Producto2DDefinicion producto)
        {
            bool cambio = false;
            int ordenFallback = 10;

            foreach (ProductoEtapaDefinicion etapa in producto.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(etapa.ClaveEtapa))
                {
                    etapa.ClaveEtapa = etapa.NombreVisible;
                    cambio = true;
                }

                if (string.IsNullOrWhiteSpace(etapa.NombreVisible))
                {
                    etapa.NombreVisible = ObtenerNombreVisibleEtapa(etapa.ClaveEtapa);
                    cambio = true;
                }

                if (etapa.Orden <= 0)
                {
                    etapa.Orden = ordenFallback;
                    cambio = true;
                }

                if (etapa.DependeDe == null)
                {
                    etapa.DependeDe = "";
                    cambio = true;
                }

                ordenFallback += 10;
            }

            return cambio;
        }

        private static int ObtenerOrdenEtapaBase(string etapa)
        {
            string n = Normalizar(etapa);

            if (n.Contains("desarrollo")) return 10;
            if (n.Contains("preproduccion")) return 20;
            if (n.Contains("produccion") && !n.Contains("post")) return 30;
            if (n.Contains("postproduccion")) return 40;

            return 90;
        }

        private static string ObtenerNombreVisibleEtapa(string etapa)
        {
            string n = Normalizar(etapa);

            if (n.Contains("desarrollo")) return "Desarrollo";
            if (n.Contains("preproduccion")) return "Preproducción";
            if (n.Contains("produccion") && !n.Contains("post")) return "Producción";
            if (n.Contains("postproduccion")) return "Postproducción";

            return string.IsNullOrWhiteSpace(etapa) ? "Sin etapa" : etapa;
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

        private static string Normalizar(string texto)
        {
            return (texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n");
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
