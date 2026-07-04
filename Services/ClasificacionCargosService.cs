using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ClasificacionCargosService
    {
        public const string TipoProductivo = "Productivo";
        public const string TipoGestion = "Gestion";
        public const string TipoApoyo = "Apoyo";

        private static readonly string[] CargosGestionOApoyo =
        {
            "director",
            "direccion",
            "productor",
            "producer",
            "project manager",
            "coordinador",
            "coordinacion",
            "supervisor",
            "supervision",
            "revisor",
            "revision",
            "control de calidad",
            "quality",
            "ejecutivo",
            "manager",
            "lead",
            "jefe",
            "cliente",
            "soporte"
        };

        private static readonly string[] SenalesProductivas =
        {
            "animador",
            "animation",
            "rough",
            "clean",
            "cleanup",
            "colorista",
            "color",
            "compositor",
            "composicion",
            "comp",
            "editor",
            "edicion",
            "postproductor",
            "render",
            "export",
            "guionista",
            "storyboard",
            "dibujante",
            "ilustrador",
            "disenador",
            "artista",
            "sonidista",
            "musico",
            "audio",
            "locutor",
            "entintado",
            "shading",
            "fondo",
            "background",
            "pagina"
        };

        public static bool EsCargoGestionOApoyo(string cargo)
        {
            string cargoNorm = NormalizarCargo(cargo);
            return !string.IsNullOrWhiteSpace(cargoNorm) &&
                   CargosGestionOApoyo.Any(cargoNorm.Contains);
        }

        public static bool EsCargoGestionOApoyo(CategoriaTrabajador cargo)
        {
            string tipo = ObtenerTipoCargo(cargo);
            return tipo == TipoGestion || tipo == TipoApoyo;
        }

        public static bool EsCargoProductivo(CategoriaTrabajador cargo)
        {
            return ObtenerTipoCargo(cargo) == TipoProductivo;
        }

        public static string ObtenerTipoCargo(CategoriaTrabajador cargo)
        {
            if (cargo == null)
            {
                return TipoApoyo;
            }

            string tipoExplicito = NormalizarTipoCargo(cargo.TipoCargo);
            if (!string.IsNullOrWhiteSpace(tipoExplicito))
            {
                return tipoExplicito;
            }

            return InferirTipoCargo(cargo.NombreCompleto);
        }

        public static string InferirTipoCargo(string cargo)
        {
            string cargoNorm = NormalizarCargo(cargo);
            if (string.IsNullOrWhiteSpace(cargoNorm))
            {
                return TipoApoyo;
            }

            if (EsCargoGestionOApoyo(cargoNorm))
            {
                if (cargoNorm.Contains("supervisor") ||
                    cargoNorm.Contains("supervision") ||
                    cargoNorm.Contains("revisor") ||
                    cargoNorm.Contains("revision") ||
                    cargoNorm.Contains("control de calidad") ||
                    cargoNorm.Contains("quality") ||
                    cargoNorm.Contains("soporte"))
                {
                    return TipoApoyo;
                }

                return TipoGestion;
            }

            return SenalesProductivas.Any(cargoNorm.Contains)
                ? TipoProductivo
                : TipoApoyo;
        }

        public static bool RequiereRendimientoProductivo(
            CategoriaTrabajador cargo,
            string pieza,
            string proceso,
            string etapa,
            string variables
        )
        {
            if (cargo == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(cargo.TipoCargo))
            {
                return ObtenerTipoCargo(cargo) == TipoProductivo;
            }

            return RequiereRendimientoProductivo(
                cargo.NombreCompleto,
                pieza,
                proceso,
                etapa,
                variables
            );
        }

        public static bool RequiereRendimientoProductivo(
            string cargo,
            string pieza,
            string proceso,
            string etapa,
            string variables
        )
        {
            string cargoNorm = NormalizarCargo(cargo);
            if (string.IsNullOrWhiteSpace(cargoNorm))
            {
                return false;
            }

            if (EsCargoGestionOApoyo(cargoNorm))
            {
                return false;
            }

            string contexto = Normalizar(cargo + ";" + pieza + ";" + proceso + ";" + etapa + ";" + variables);
            return SenalesProductivas.Any(t => cargoNorm.Contains(t) || contexto.Contains(t));
        }

        public static string DescribirTipoCargo(string cargo)
        {
            return DescribirTipoCargoDesdeTipo(InferirTipoCargo(cargo));
        }

        public static string DescribirTipoCargo(CategoriaTrabajador cargo)
        {
            return DescribirTipoCargoDesdeTipo(ObtenerTipoCargo(cargo));
        }

        public static string DescribirTipoCargoDesdeTipo(string tipoCargo)
        {
            string tipo = NormalizarTipoCargo(tipoCargo);
            if (tipo == TipoGestion)
            {
                return "Gestion: direccion, produccion o coordinacion; calcula dedicacion/costo, no rendimiento.";
            }

            if (tipo == TipoApoyo)
            {
                return "Apoyo: supervision, revision o soporte; calcula dedicacion/costo, no rendimiento.";
            }

            return "Productivo: ejecuta trabajo medible y usa rendimiento/capacidad.";
        }

        public static string NormalizarTipoCargo(string tipoCargo)
        {
            string tipo = Normalizar(tipoCargo);
            if (tipo == "productivo" || tipo == "produccion" || tipo == "operativo" || tipo == "ejecutor")
            {
                return TipoProductivo;
            }

            if (tipo == "gestion" || tipo == "direccion" || tipo == "ejecutivo" || tipo == "administrativo")
            {
                return TipoGestion;
            }

            if (tipo == "apoyo" || tipo == "soporte" || tipo == "supervision" || tipo == "revision" ||
                tipo == "qa" || tipo == "control")
            {
                return TipoApoyo;
            }

            return "";
        }

        public static string NormalizarCargo(string texto)
        {
            string normalizado = Normalizar(texto);
            int parentesis = normalizado.IndexOf('(');
            if (parentesis >= 0)
            {
                normalizado = normalizado.Substring(0, parentesis).Trim();
            }

            return normalizado;
        }

        public static string Normalizar(string texto)
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
                .Replace("ñ", "n")
                .Replace("Ã¡", "a")
                .Replace("Ã©", "e")
                .Replace("Ã­", "i")
                .Replace("Ã³", "o")
                .Replace("Ãº", "u")
                .Replace("Ã±", "n")
                .Replace("ÃƒÂ¡", "a")
                .Replace("ÃƒÂ©", "e")
                .Replace("ÃƒÂ­", "i")
                .Replace("ÃƒÂ³", "o")
                .Replace("ÃƒÂº", "u")
                .Replace("ÃƒÂ±", "n");
        }
    }
}
