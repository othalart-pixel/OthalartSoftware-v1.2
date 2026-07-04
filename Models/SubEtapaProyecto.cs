using System;

namespace Cotizador_animacion_Othalart.Models
{
    public class SubEtapaProyecto
    {
        public int Id { get; set; } = 0;

        public string EtapaPadre { get; set; } = "";

        public string Nombre { get; set; } = "";

        public int Orden { get; set; } = 0;

        public bool Activa { get; set; } = true;

        public bool Requerida { get; set; } = true;

        public bool Editable { get; set; } = true;

        public string Requiere { get; set; } = "";

        public string Entrega { get; set; } = "";

        public string CargosSugeridos { get; set; } = "";

        public double PorcentajeMinimoEtapa { get; set; } = 0.0;

        public double PorcentajeRecomendadoEtapa { get; set; } = 0.0;

        public double PorcentajeMaximoEtapa { get; set; } = 0.0;

       

        public double InicioSemana { get; set; } = 0.0;

        public double DuracionSemanas { get; set; } = 1.0;

        // =========================
        // CONEXIÓN CON DESGLOSE PRODUCTIVO
        // =========================

        public string TiposInternosQueProduce { get; set; } = "";

        public string TiposInternosQueConsume { get; set; } = "";

        public string PalabrasClaveActivacion { get; set; } = "";

        /*
         * FinSemana es fin exclusivo.
         *
         * Si Inicio = 1 y Duración = 1,
         * entonces Fin = 2.
         *
         * Visualmente eso se lee como S1 → S2,
         * pero productivamente equivale a 1 semana.
         */
        public double FinSemana
        {
            get
            {
                return InicioSemana + DuracionSemanas;
            }
            set
            {
                double nuevaDuracion = value - InicioSemana;

                if (nuevaDuracion <= 0.0)
                {
                    nuevaDuracion = 0.1;
                }

                DuracionSemanas = nuevaDuracion;
            }
        }

        public SubEtapaProyecto()
        {
        }

        public SubEtapaProyecto(
            int id,
            string etapaPadre,
            string nombre,
            int orden,
            bool activa,
            bool requerida
        )
        {
            Id = id;
            EtapaPadre = etapaPadre;
            Nombre = nombre;
            Orden = orden;
            Activa = activa;
            Requerida = requerida;

            InicioSemana = orden <= 0 ? 0.0 : Convert.ToDouble(orden - 1);
            DuracionSemanas = 1.0;

            TiposInternosQueProduce = "";
            TiposInternosQueConsume = "";
            PalabrasClaveActivacion = "";
        }

        public int InicioSemanaVisual
        {
            get
            {
                if (InicioSemana < 0.0)
                {
                    return 0;
                }

                return (int)Math.Floor(InicioSemana);
            }
        }

        /*
         * Fin visual exclusivo.
         *
         * Si FinSemana = 2.0, el Gantt termina en la frontera de S2.
         * Si FinSemana = 2.25, visualmente se extiende hasta S3.
         */
        public int FinSemanaVisualExclusiva
        {
            get
            {
                double fin = FinSemana;

                if (fin < 0.0)
                {
                    return 1;
                }

                int visual = (int)Math.Ceiling(fin);

                if (visual <= InicioSemanaVisual)
                {
                    visual = InicioSemanaVisual + 1;
                }

                return visual;
            }
        }

        public int DuracionSemanasVisual
        {
            get
            {
                int duracion = FinSemanaVisualExclusiva - InicioSemanaVisual;

                if (duracion < 1)
                {
                    duracion = 1;
                }

                return duracion;
            }
        }

        public string InicioSemanaTexto
        {
            get
            {
                return InicioSemana.ToString("0.##");
            }
        }

        public string FinSemanaTexto
        {
            get
            {
                return FinSemana.ToString("0.##");
            }
        }

        public string DuracionSemanasTexto
        {
            get
            {
                return DuracionSemanas.ToString("0.##");
            }
        }
    }
}