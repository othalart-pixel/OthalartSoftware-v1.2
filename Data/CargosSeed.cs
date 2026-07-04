using System.Collections.Generic;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Data
{
    public static class CargosSeed
    {
        public static CategoriaTrabajador CrearCargo(
            int id,
            string nombre,
            string nivel,
            double clpMin,
            double clpTipico,
            double clpMax
        )
        {
            return new CategoriaTrabajador
            {
                Id = id,
                Nombre = nombre,
                Nivel = nivel,
                TipoCargo = ClasificacionCargosService.InferirTipoCargo($"{nombre} ({nivel})"),

                SueldoMensualCLPMin = clpMin,
                SueldoMensualCLPTipico = clpTipico,
                SueldoMensualCLPMax = clpMax,

                /*
                 * Base interna oficial: CLP.
                 *
                 * No guardamos USD como valor base del cargo.
                 * La conversión a USD/EUR/JPY/UF debe hacerla el módulo de moneda
                 * usando el tipo de cambio actual.
                 */
                SueldoMensualUSDMin = 0,
                SueldoMensualUSDTipico = 0,
                SueldoMensualUSDMax = 0
            };
        }

        // =========================================================
        // CARGOS TRANSVERSALES
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaGenerales()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(1, "Productor / Project manager", "junior", 700000, 950000, 1300000),
                CrearCargo(2, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(3, "Productor / Project manager", "senior", 1800000, 2600000, 4200000),

                CrearCargo(4, "Coordinador/a de producción", "junior", 600000, 800000, 1050000),
                CrearCargo(5, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),
                CrearCargo(18, "Coordinador/a de producción", "senior", 1300000, 1800000, 2800000),

                CrearCargo(6, "Director creativo", "típico", 1400000, 2000000, 3200000),
                CrearCargo(7, "Director creativo", "senior", 2200000, 3200000, 5200000),

                CrearCargo(8, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(9, "Director de arte", "senior", 2000000, 3000000, 4800000),

                CrearCargo(10, "Supervisor de arte", "típico", 1200000, 1700000, 2600000),
                CrearCargo(11, "Supervisor de arte", "senior", 1900000, 2800000, 4400000),

                CrearCargo(12, "Director/a de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(13, "Director/a de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(14, "Supervisor de animación", "típico", 1300000, 1800000, 2800000),
                CrearCargo(15, "Supervisor de animación", "senior", 2000000, 3000000, 5000000),

                CrearCargo(16, "Director técnico 2D", "típico", 1500000, 2200000, 3500000),
                CrearCargo(17, "Director técnico 2D", "senior", 2400000, 3600000, 6000000),

                CrearCargo(19, "Productor ejecutivo", "típico", 1800000, 2600000, 4200000),
                CrearCargo(20, "Productor ejecutivo", "senior", 2800000, 4200000, 7000000),

                CrearCargo(99, "Otro cargo general", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "General");
            return cargos;
        }

        // =========================================================
        // DESARROLLO / ESTRATEGIA / NARRATIVA
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaDesarrollo()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(101, "Guionista", "junior", 600000, 800000, 1100000),
                CrearCargo(102, "Guionista", "típico", 900000, 1250000, 1800000),
                CrearCargo(103, "Guionista", "senior", 1500000, 2200000, 3600000),

                CrearCargo(104, "Script doctor", "típico", 1100000, 1600000, 2500000),
                CrearCargo(105, "Script doctor", "senior", 1800000, 2800000, 4500000),

                CrearCargo(106, "Investigador de referencias", "típico", 600000, 850000, 1200000),

                CrearCargo(107, "Desarrollador creativo", "típico", 950000, 1350000, 2100000),
                CrearCargo(108, "Desarrollador creativo", "senior", 1600000, 2400000, 4000000),

                CrearCargo(109, "Consultor creativo", "típico", 1200000, 1800000, 3000000),
                CrearCargo(110, "Consultor creativo", "senior", 2200000, 3400000, 5600000),

                CrearCargo(111, "Diseñador narrativo", "típico", 1000000, 1400000, 2200000),
                CrearCargo(112, "Diseñador narrativo", "senior", 1700000, 2600000, 4300000),

                CrearCargo(113, "Director narrativo", "típico", 1500000, 2200000, 3500000),
                CrearCargo(114, "Director narrativo", "senior", 2400000, 3600000, 6000000),

                CrearCargo(115, "Productor / Project manager", "junior", 700000, 950000, 1300000),
                CrearCargo(116, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(117, "Productor / Project manager", "senior", 1800000, 2600000, 4200000),

                CrearCargo(118, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(119, "Director de arte", "senior", 2000000, 3000000, 4800000),

                CrearCargo(120, "Director creativo", "típico", 1400000, 2000000, 3200000),
                CrearCargo(121, "Director creativo", "senior", 2200000, 3200000, 5200000),

                CrearCargo(122, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),

                CrearCargo(199, "Otro cargo de desarrollo", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "Desarrollo");
            return cargos;
        }

        // =========================================================
        // PREPRODUCCIÓN / DISEÑO VISUAL / ILUSTRACIÓN
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaPreproduccion()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(201, "Storyboard artist", "junior", 700000, 950000, 1300000),
                CrearCargo(202, "Storyboard artist", "típico", 1000000, 1400000, 2200000),
                CrearCargo(203, "Storyboard artist", "senior", 1600000, 2500000, 4200000),

                CrearCargo(204, "Animatic editor", "junior", 700000, 900000, 1250000),
                CrearCargo(241, "Animatic editor", "típico", 950000, 1300000, 2000000),
                CrearCargo(242, "Animatic editor", "senior", 1500000, 2200000, 3500000),

                CrearCargo(205, "Layout artist", "típico", 900000, 1250000, 1900000),
                CrearCargo(206, "Layout artist", "senior", 1500000, 2300000, 3800000),

                CrearCargo(207, "Concept artist", "junior", 750000, 1000000, 1400000),
                CrearCargo(208, "Concept artist", "típico", 1100000, 1600000, 2600000),
                CrearCargo(209, "Concept artist", "senior", 1900000, 3000000, 5200000),

                CrearCargo(210, "Visual development artist", "típico", 1200000, 1700000, 2800000),
                CrearCargo(211, "Visual development artist", "senior", 2000000, 3200000, 5400000),

                CrearCargo(212, "Diseñador visual", "junior", 700000, 950000, 1300000),
                CrearCargo(213, "Diseñador visual", "típico", 1000000, 1400000, 2200000),
                CrearCargo(214, "Diseñador visual", "senior", 1700000, 2600000, 4300000),

                CrearCargo(215, "Diseñador de personajes", "junior", 750000, 1000000, 1400000),
                CrearCargo(216, "Diseñador de personajes", "típico", 1100000, 1600000, 2600000),
                CrearCargo(217, "Diseñador de personajes", "senior", 1900000, 3000000, 5200000),

                CrearCargo(218, "Diseñador de fondos", "junior", 700000, 950000, 1300000),
                CrearCargo(219, "Diseñador de fondos", "típico", 1000000, 1450000, 2300000),
                CrearCargo(220, "Diseñador de fondos", "senior", 1800000, 2800000, 4600000),

                CrearCargo(221, "Diseñador de props", "junior", 650000, 850000, 1150000),
                CrearCargo(222, "Diseñador de props", "típico", 900000, 1250000, 1900000),
                CrearCargo(223, "Diseñador de props", "senior", 1500000, 2300000, 3800000),

                CrearCargo(224, "Ilustrador", "junior", 700000, 950000, 1300000),
                CrearCargo(225, "Ilustrador", "típico", 1000000, 1500000, 2500000),
                CrearCargo(226, "Ilustrador", "senior", 1900000, 3200000, 5600000),

                CrearCargo(227, "Color script artist", "típico", 1100000, 1600000, 2600000),
                CrearCargo(228, "Color script artist", "senior", 1900000, 3000000, 5000000),

                CrearCargo(229, "Background painter", "junior", 700000, 950000, 1300000),
                CrearCargo(230, "Background painter", "típico", 1000000, 1500000, 2500000),
                CrearCargo(231, "Background painter", "senior", 1900000, 3200000, 5600000),

                CrearCargo(232, "Matte painter 2D", "típico", 1300000, 1900000, 3200000),
                CrearCargo(233, "Matte painter 2D", "senior", 2300000, 3600000, 6500000),

                CrearCargo(234, "Diseñador gráfico", "junior", 650000, 850000, 1150000),
                CrearCargo(235, "Diseñador gráfico", "típico", 900000, 1250000, 1900000),
                CrearCargo(236, "Diseñador gráfico", "senior", 1500000, 2300000, 3800000),

                CrearCargo(237, "Diseñador editorial / layout gráfico", "típico", 900000, 1300000, 2000000),
                CrearCargo(238, "Diseñador editorial / layout gráfico", "senior", 1600000, 2500000, 4200000),

                CrearCargo(239, "Tipógrafo / lettering artist", "típico", 900000, 1300000, 2100000),
                CrearCargo(240, "Tipógrafo / lettering artist", "senior", 1600000, 2600000, 4600000),

                CrearCargo(243, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(244, "Director de arte", "senior", 2000000, 3000000, 4800000),

                CrearCargo(245, "Productor / Project manager", "junior", 700000, 950000, 1300000),
                CrearCargo(246, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(247, "Productor / Project manager", "senior", 1800000, 2600000, 4200000),

                CrearCargo(248, "Director/a de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(249, "Director/a de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(250, "Supervisor de animación", "típico", 1300000, 1800000, 2800000),
                CrearCargo(251, "Supervisor de animación", "senior", 2000000, 3000000, 5000000),

                CrearCargo(252, "Supervisor de arte", "típico", 1200000, 1700000, 2600000),
                CrearCargo(253, "Supervisor de arte", "senior", 1900000, 2800000, 4400000),

                CrearCargo(254, "Director creativo", "típico", 1400000, 2000000, 3200000),
                CrearCargo(255, "Director creativo", "senior", 2200000, 3200000, 5200000),

                CrearCargo(256, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),

                CrearCargo(257, "Director de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(258, "Director de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(299, "Otro cargo de preproducción / diseño", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "Preproduccion");
            return cargos;
        }

        // =========================================================
        // PRODUCCIÓN 2D / ANIMACIÓN
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaProduccion()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(301, "Rough animator", "junior", 700000, 950000, 1300000),
                CrearCargo(302, "Rough animator", "típico", 1100000, 1600000, 2600000),
                CrearCargo(303, "Rough animator", "senior", 1900000, 3000000, 5200000),

                CrearCargo(304, "Key animator", "típico", 1200000, 1800000, 3000000),
                CrearCargo(305, "Key animator", "senior", 2200000, 3600000, 6500000),

                CrearCargo(306, "Inbetween artist", "junior", 600000, 800000, 1050000),
                CrearCargo(307, "Inbetween artist", "típico", 800000, 1100000, 1600000),
                CrearCargo(340, "Inbetween artist", "senior", 1200000, 1800000, 3000000),

                CrearCargo(308, "Clean up artist", "junior", 600000, 800000, 1050000),
                CrearCargo(309, "Clean up artist", "típico", 800000, 1100000, 1600000),
                CrearCargo(310, "Clean up artist", "senior", 1300000, 2000000, 3400000),

                CrearCargo(311, "Colorista", "junior", 550000, 750000, 1000000),
                CrearCargo(312, "Colorista", "típico", 750000, 1000000, 1500000),
                CrearCargo(313, "Colorista", "senior", 1200000, 1800000, 3000000),

                CrearCargo(314, "Animador cut-out", "junior", 700000, 950000, 1300000),
                CrearCargo(315, "Animador cut-out", "típico", 1100000, 1600000, 2600000),
                CrearCargo(316, "Animador cut-out", "senior", 1900000, 3000000, 5200000),

                CrearCargo(317, "Animador frame by frame", "junior", 750000, 1000000, 1400000),
                CrearCargo(318, "Animador frame by frame", "típico", 1200000, 1800000, 3000000),
                CrearCargo(319, "Animador frame by frame", "senior", 2200000, 3600000, 6500000),

                CrearCargo(320, "FX animator 2D", "típico", 1100000, 1600000, 2600000),
                CrearCargo(321, "FX animator 2D", "senior", 1900000, 3000000, 5200000),

                CrearCargo(322, "Lip sync artist", "típico", 800000, 1100000, 1600000),
                CrearCargo(323, "Pose artist", "típico", 850000, 1200000, 1800000),

                CrearCargo(324, "Artista de fondos", "junior", 700000, 950000, 1300000),
                CrearCargo(325, "Artista de fondos", "típico", 1000000, 1500000, 2500000),
                CrearCargo(326, "Artista de fondos", "senior", 1900000, 3200000, 5600000),

                CrearCargo(327, "Asistente de producción", "junior", 550000, 700000, 900000),

                CrearCargo(328, "Animador 2D", "junior", 700000, 950000, 1300000),
                CrearCargo(329, "Animador 2D", "típico", 1100000, 1600000, 2600000),
                CrearCargo(330, "Animador 2D", "senior", 1900000, 3000000, 5200000),

                CrearCargo(331, "Background artist", "junior", 700000, 950000, 1300000),
                CrearCargo(332, "Background artist", "típico", 1000000, 1500000, 2500000),
                CrearCargo(333, "Background artist", "senior", 1900000, 3200000, 5600000),

                CrearCargo(334, "Artista clean up", "junior", 600000, 800000, 1050000),
                CrearCargo(335, "Artista clean up", "típico", 800000, 1100000, 1600000),
                CrearCargo(336, "Artista clean up", "senior", 1300000, 2000000, 3400000),

                CrearCargo(337, "Asistente de color", "junior", 550000, 750000, 1000000),
                CrearCargo(338, "Artista de color", "típico", 750000, 1000000, 1500000),
                CrearCargo(339, "Artista de color", "senior", 1200000, 1800000, 3000000),

                CrearCargo(341, "Productor / Project manager", "junior", 700000, 950000, 1300000),
                CrearCargo(342, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(343, "Productor / Project manager", "senior", 1800000, 2600000, 4200000),

                CrearCargo(344, "Director/a de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(345, "Director/a de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(346, "Director de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(347, "Director de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(348, "Supervisor de animación", "típico", 1300000, 1800000, 2800000),
                CrearCargo(349, "Supervisor de animación", "senior", 2000000, 3000000, 5000000),

                CrearCargo(350, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(351, "Director de arte", "senior", 2000000, 3000000, 4800000),

                CrearCargo(352, "Supervisor de arte", "típico", 1200000, 1700000, 2600000),
                CrearCargo(353, "Supervisor de arte", "senior", 1900000, 2800000, 4400000),

                CrearCargo(354, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),

                // Audio como producción: creación sonora, música, diseño sonoro y sincronización.
                CrearCargo(355, "Diseñador sonoro", "junior", 650000, 850000, 1150000),
                CrearCargo(356, "Diseñador sonoro", "típico", 950000, 1400000, 2300000),
                CrearCargo(357, "Diseñador sonoro", "senior", 1700000, 2700000, 4600000),

                CrearCargo(358, "Sound designer", "junior", 650000, 850000, 1150000),
                CrearCargo(359, "Sound designer", "típico", 950000, 1400000, 2300000),
                CrearCargo(360, "Sound designer", "senior", 1700000, 2700000, 4600000),

                CrearCargo(361, "Mezclador de audio", "típico", 950000, 1400000, 2300000),
                CrearCargo(362, "Músico / compositor musical", "típico", 1100000, 1700000, 3000000),
                CrearCargo(363, "Editor de audio", "típico", 850000, 1200000, 1900000),

                CrearCargo(399, "Otro cargo de producción", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "Produccion");
            return cargos;
        }

        // =========================================================
        // VIDEOJUEGOS / ASSETS / UI / INTERACTIVO
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaVideojuegosAssets()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(501, "Game artist 2D", "junior", 700000, 950000, 1300000),
                CrearCargo(502, "Game artist 2D", "típico", 1000000, 1500000, 2500000),
                CrearCargo(503, "Game artist 2D", "senior", 1900000, 3200000, 5600000),

                CrearCargo(504, "Pixel artist", "junior", 650000, 850000, 1150000),
                CrearCargo(505, "Pixel artist", "típico", 900000, 1300000, 2100000),
                CrearCargo(506, "Pixel artist", "senior", 1600000, 2600000, 4600000),

                CrearCargo(507, "Sprite artist", "junior", 650000, 850000, 1150000),
                CrearCargo(508, "Sprite artist", "típico", 900000, 1300000, 2100000),
                CrearCargo(509, "Sprite artist", "senior", 1600000, 2600000, 4600000),

                CrearCargo(510, "Animador de sprites", "junior", 650000, 850000, 1150000),
                CrearCargo(511, "Animador de sprites", "típico", 900000, 1300000, 2100000),
                CrearCargo(512, "Animador de sprites", "senior", 1600000, 2600000, 4600000),

                CrearCargo(513, "UI artist 2D", "junior", 700000, 950000, 1300000),
                CrearCargo(514, "UI artist 2D", "típico", 1000000, 1500000, 2500000),
                CrearCargo(515, "UI artist 2D", "senior", 1900000, 3200000, 5600000),

                CrearCargo(516, "UX/UI designer", "típico", 1200000, 1800000, 3000000),
                CrearCargo(517, "UX/UI designer", "senior", 2200000, 3600000, 6500000),

                CrearCargo(518, "Icon artist", "típico", 850000, 1200000, 1800000),
                CrearCargo(519, "Tileset artist", "típico", 900000, 1300000, 2100000),

                CrearCargo(520, "Technical artist 2D", "típico", 1400000, 2200000, 3600000),
                CrearCargo(521, "Technical artist 2D", "senior", 2600000, 4200000, 7200000),

                CrearCargo(522, "Artista de implementación 2D", "típico", 1000000, 1500000, 2500000),
                CrearCargo(523, "Artista de implementación 2D", "senior", 1900000, 3200000, 5600000),

                CrearCargo(524, "Game animator 2D", "junior", 700000, 950000, 1300000),
                CrearCargo(525, "Game animator 2D", "típico", 1000000, 1500000, 2500000),
                CrearCargo(526, "Game animator 2D", "senior", 1900000, 3200000, 5600000),

                CrearCargo(527, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(528, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(529, "Supervisor de arte", "típico", 1200000, 1700000, 2600000),

                CrearCargo(599, "Otro cargo de videojuego / assets", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "VideojuegosAssets");
            return cargos;
        }

        // =========================================================
        // POSTPRODUCCIÓN / AUDIO
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaPostproduccion()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(401, "Editor", "junior", 650000, 850000, 1150000),
                CrearCargo(402, "Editor", "típico", 950000, 1350000, 2100000),
                CrearCargo(403, "Editor", "senior", 1600000, 2600000, 4600000),

                CrearCargo(404, "Compositor", "junior", 700000, 950000, 1300000),
                CrearCargo(405, "Compositor", "típico", 1100000, 1700000, 2800000),
                CrearCargo(406, "Compositor", "senior", 2200000, 3600000, 6500000),

                CrearCargo(407, "Motion graphics artist", "junior", 700000, 950000, 1300000),
                CrearCargo(408, "Motion graphics artist", "típico", 1100000, 1600000, 2600000),
                CrearCargo(409, "Motion graphics artist", "senior", 1900000, 3200000, 5600000),

                // En post se conservan cargos de cierre sonoro: mezcla, master, export y revisión.
                CrearCargo(410, "Sound designer", "junior", 650000, 850000, 1150000),
                CrearCargo(411, "Sound designer", "típico", 950000, 1400000, 2300000),
                CrearCargo(412, "Sound designer", "senior", 1700000, 2700000, 4600000),

                CrearCargo(413, "Mezclador de audio", "típico", 950000, 1400000, 2300000),
                CrearCargo(414, "Músico / compositor musical", "típico", 1100000, 1700000, 3000000),
                CrearCargo(415, "Corrección de color", "típico", 950000, 1400000, 2300000),

                CrearCargo(416, "Diseñador sonoro", "junior", 650000, 850000, 1150000),
                CrearCargo(417, "Diseñador sonoro", "típico", 950000, 1400000, 2300000),
                CrearCargo(418, "Diseñador sonoro", "senior", 1700000, 2700000, 4600000),

                CrearCargo(419, "Render / export manager", "junior", 600000, 750000, 1000000),
                CrearCargo(420, "Render / export manager", "típico", 750000, 1000000, 1500000),
                CrearCargo(421, "Render / export manager", "senior", 1200000, 1800000, 3000000),

                CrearCargo(422, "Postproductor", "junior", 700000, 950000, 1300000),
                CrearCargo(423, "Postproductor", "típico", 1100000, 1600000, 2600000),
                CrearCargo(424, "Postproductor", "senior", 1900000, 3000000, 5200000),

                CrearCargo(425, "Asistente de postproducción", "junior", 550000, 700000, 900000),
                CrearCargo(426, "Asistente de postproducción", "típico", 700000, 900000, 1200000),

                CrearCargo(427, "Productor / Project manager", "junior", 700000, 950000, 1300000),
                CrearCargo(428, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(429, "Productor / Project manager", "senior", 1800000, 2600000, 4200000),

                CrearCargo(430, "Director de arte", "típico", 1300000, 1800000, 2800000),
                CrearCargo(431, "Director de arte", "senior", 2000000, 3000000, 4800000),

                CrearCargo(432, "Director/a de animación", "típico", 1400000, 2000000, 3200000),
                CrearCargo(433, "Director/a de animación", "senior", 2200000, 3300000, 5400000),

                CrearCargo(434, "Control de calidad técnico", "junior", 550000, 700000, 900000),
                CrearCargo(435, "Control de calidad técnico", "típico", 700000, 950000, 1300000),

                CrearCargo(436, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),

                CrearCargo(499, "Otro cargo de postproducción", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "Postproduccion");
            return cargos;
        }

        // =========================================================
        // TAREAS TÉCNICAS INTERNAS
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaTecnicaInterna()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>
            {
                CrearCargo(601, "Control de calidad técnico", "junior", 550000, 700000, 900000),
                CrearCargo(602, "Control de calidad técnico", "típico", 700000, 950000, 1300000),

                CrearCargo(603, "Preparación de archivos", "junior", 500000, 650000, 850000),
                CrearCargo(604, "Preparación de archivos", "típico", 650000, 850000, 1150000),

                CrearCargo(605, "Gestión de entrega / transferencia", "junior", 500000, 650000, 850000),
                CrearCargo(606, "Gestión de entrega / transferencia", "típico", 650000, 850000, 1150000),

                CrearCargo(607, "Soporte técnico de pipeline", "típico", 1100000, 1600000, 2600000),
                CrearCargo(608, "Soporte técnico de pipeline", "senior", 1900000, 3000000, 5200000),

                CrearCargo(609, "Render / export manager", "típico", 750000, 1000000, 1500000),
                CrearCargo(610, "Render / export manager", "senior", 1200000, 1800000, 3000000),

                CrearCargo(611, "Pipeline assistant", "junior", 550000, 700000, 900000),
                CrearCargo(612, "Pipeline assistant", "típico", 700000, 950000, 1300000),

                CrearCargo(613, "Productor / Project manager", "típico", 1100000, 1500000, 2200000),
                CrearCargo(614, "Coordinador/a de producción", "típico", 850000, 1150000, 1600000),

                CrearCargo(699, "Otra tarea técnica interna", "personalizado", 0, 0, 0)
            };

            AsignarBloque(cargos, "TecnicaInterna");
            return cargos;
        }

        // =========================================================
        // BIBLIOTECA COMPLETA
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaCompleta()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>();

            cargos.AddRange(CrearBibliotecaGenerales());
            cargos.AddRange(CrearBibliotecaDesarrollo());
            cargos.AddRange(CrearBibliotecaPreproduccion());
            cargos.AddRange(CrearBibliotecaProduccion());
            cargos.AddRange(CrearBibliotecaVideojuegosAssets());
            cargos.AddRange(CrearBibliotecaPostproduccion());
            cargos.AddRange(CrearBibliotecaTecnicaInterna());

            return cargos;
        }

        // =========================================================
        // BIBLIOTECA SEGÚN BLOQUE / ETAPA
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaPorBloque(string bloque)
        {
            string b = Normalizar(bloque);

            if (b.Contains("general"))
            {
                return CrearBibliotecaGenerales();
            }

            if (b.Contains("desarrollo"))
            {
                return CrearBibliotecaDesarrollo();
            }

            if (b.Contains("preproduccion") || b.Contains("pre"))
            {
                return CrearBibliotecaPreproduccion();
            }

            if (b.Contains("videojuego") ||
                b.Contains("asset") ||
                b.Contains("juego") ||
                b.Contains("interactivo") ||
                b.Contains("ui") ||
                b.Contains("sprite"))
            {
                return CrearBibliotecaVideojuegosAssets();
            }

            if (b.Contains("post"))
            {
                return CrearBibliotecaPostproduccion();
            }

            if (b.Contains("tecnica") || b.Contains("interno"))
            {
                return CrearBibliotecaTecnicaInterna();
            }

            /*
             * IMPORTANTE:
             * "postproducción" contiene la palabra "producción".
             * Por eso producción se evalúa después de post.
             */
            if (b.Contains("produccion"))
            {
                return CrearBibliotecaProduccion();
            }

            return CrearBibliotecaCompleta();
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static void AsignarBloque(List<CategoriaTrabajador> cargos, string bloque)
        {
            if (cargos == null)
            {
                return;
            }

            foreach (CategoriaTrabajador cargo in cargos)
            {
                if (cargo != null)
                {
                    cargo.Bloque = bloque;
                }
            }
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
                .Replace("ñ", "n")
                .Replace(" ", "")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "");
        }
    }
}
