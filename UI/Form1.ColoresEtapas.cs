using System;
using System.Drawing;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private string NormalizarNombreEtapaColor(string? nombreEtapa)
        {
            if (string.IsNullOrWhiteSpace(nombreEtapa))
            {
                return "";
            }

            return nombreEtapa
                .Trim()
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

        private Color ObtenerColorBaseEtapa(string? nombreEtapa)
        {
            EtapaDefinicion definicion = ObtenerDefinicionEtapa(nombreEtapa);

            if (definicion != null)
            {
                return Color.FromArgb(definicion.ColorArgb);
            }

            string nombre = NormalizarNombreEtapaColor(nombreEtapa);

            if (nombre == "desarrollo")
            {
                return Color.FromArgb(76, 175, 80);
            }

            if (nombre == "preproduccion")
            {
                return Color.FromArgb(255, 193, 7);
            }

            if (nombre == "produccion")
            {
                return Color.FromArgb(244, 67, 54);
            }

            if (nombre == "postproduccion")
            {
                return Color.FromArgb(33, 150, 243);
            }

            if (nombre == "transversal" || nombre == "general")
            {
                return Color.FromArgb(156, 39, 176);
            }

            return Color.FromArgb(180, 180, 180);
        }

        private Color MezclarConBlanco(Color colorBase, double factorBlanco)
        {
            factorBlanco = Math.Max(0.0, Math.Min(1.0, factorBlanco));

            int r = (int)(colorBase.R + (255 - colorBase.R) * factorBlanco);
            int g = (int)(colorBase.G + (255 - colorBase.G) * factorBlanco);
            int b = (int)(colorBase.B + (255 - colorBase.B) * factorBlanco);

            return Color.FromArgb(r, g, b);
        }

        private Color MezclarConNegro(Color colorBase, double factorNegro)
        {
            factorNegro = Math.Max(0.0, Math.Min(1.0, factorNegro));

            int r = (int)(colorBase.R * (1.0 - factorNegro));
            int g = (int)(colorBase.G * (1.0 - factorNegro));
            int b = (int)(colorBase.B * (1.0 - factorNegro));

            return Color.FromArgb(r, g, b);
        }

        private Color ObtenerColorFilaEtapa(string? nombreEtapa)
        {
            Color baseColor = ObtenerColorBaseEtapa(nombreEtapa);
            return MezclarConBlanco(baseColor, 0.78);
        }

        private Color ObtenerColorBarraEtapa(string? nombreEtapa)
        {
            Color baseColor = ObtenerColorBaseEtapa(nombreEtapa);
            return MezclarConBlanco(baseColor, 0.10);
        }

        private Color ObtenerColorBordeEtapa(string? nombreEtapa)
        {
            Color baseColor = ObtenerColorBaseEtapa(nombreEtapa);
            return MezclarConNegro(baseColor, 0.25);
        }

        private Color ObtenerColorTextoEtapa(string? nombreEtapa)
        {
            return Color.Black;
        }

        private string ObtenerNombreVisibleEtapa(string? nombreEtapa)
        {
            EtapaDefinicion definicion = ObtenerDefinicionEtapa(nombreEtapa);

            if (definicion != null)
            {
                return definicion.Nombre;
            }

            string nombre = NormalizarNombreEtapaColor(nombreEtapa);

            if (nombre == "desarrollo")
            {
                return "Desarrollo";
            }

            if (nombre == "preproduccion")
            {
                return "Preproducción";
            }

            if (nombre == "produccion")
            {
                return "Producción";
            }

            if (nombre == "postproduccion")
            {
                return "Postproducción";
            }

            if (nombre == "transversal")
            {
                return "Transversal";
            }

            if (nombre == "general")
            {
                return "General";
            }

            return string.IsNullOrWhiteSpace(nombreEtapa)
                ? "Sin etapa"
                : nombreEtapa;
        }

        private void AplicarColorFilaDesglose(DataGridViewRow row, string etapa)
        {
            if (row == null)
            {
                return;
            }

            Color colorFila = ObtenerColorFilaEtapa(etapa);
            Color colorSeleccion = ObtenerColorBarraEtapa(etapa);

            row.DefaultCellStyle.BackColor = colorFila;
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.SelectionBackColor = colorSeleccion;
            row.DefaultCellStyle.SelectionForeColor = Color.Black;

            if (dgvDesgloseProductivo.Columns.Contains("EtapaSugerida"))
            {
                row.Cells["EtapaSugerida"].Style.BackColor =
                    MezclarConBlanco(ObtenerColorBaseEtapa(etapa), 0.35);

                row.Cells["EtapaSugerida"].Style.ForeColor = Color.Black;
                row.Cells["EtapaSugerida"].Style.Font =
                    new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
        }

        private int ObtenerOrdenEtapaGeneral(string? nombreEtapa)
        {
            EtapaDefinicion definicion = ObtenerDefinicionEtapa(nombreEtapa);

            if (definicion != null)
            {
                return definicion.Orden;
            }

            string nombre = NormalizarNombreEtapaColor(nombreEtapa);

            if (nombre == "desarrollo")
            {
                return 10;
            }

            if (nombre == "preproduccion")
            {
                return 20;
            }

            if (nombre == "produccion")
            {
                return 30;
            }

            if (nombre == "postproduccion")
            {
                return 40;
            }

            if (nombre == "transversal" || nombre == "general")
            {
                return 50;
            }

            return 90;
        }

        private EtapaDefinicion ObtenerDefinicionEtapa(string? nombreEtapa)
        {
            if (bibliotecaEtapas == null || bibliotecaEtapas.Count == 0)
            {
                return null;
            }

            string nombre = NormalizarNombreEtapaColor(nombreEtapa);

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            return bibliotecaEtapas.FirstOrDefault(e =>
                e != null &&
                (
                    NormalizarNombreEtapaColor(e.Clave) == nombre ||
                    NormalizarNombreEtapaColor(e.Nombre) == nombre
                )
            );
        }

        private string ObtenerClaveEtapaConfigurada(string? nombreEtapa)
        {
            EtapaDefinicion definicion = ObtenerDefinicionEtapa(nombreEtapa);

            if (definicion != null && !string.IsNullOrWhiteSpace(definicion.Clave))
            {
                return NormalizarNombreEtapaColor(definicion.Clave);
            }

            return NormalizarNombreEtapaColor(nombreEtapa);
        }
    }
}
