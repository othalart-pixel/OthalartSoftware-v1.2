using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace OthalartSoftware.Tests;

public class ReglaCalculoProcesoServiceTests
{
    [Theory]
    [InlineData("horas = horas_produccion * 0.125", 0.125)]
    [InlineData("horas = horas_produccion x 0,275", 0.275)]
    [InlineData("horas = 12.5% de produccion", 0.125)]
    [InlineData("horas = 7,5 % de produccion", 0.075)]
    public void Porcentaje_acepta_valores_arbitrarios(string formula, double esperado)
    {
        EcuacionProductivaDefinicion proceso = new EcuacionProductivaDefinicion
        {
            FormulaReferencia = formula,
            TipoProceso = TipoProcesoProductivo.CorreccionRetrabajo
        };

        double resultado = ReglaCalculoProcesoService.ObtenerFactorPorcentaje(proceso);

        Assert.Equal(esperado, resultado, 6);
    }

    [Fact]
    public void Porcentaje_sin_valor_usa_fallback_segun_tipo()
    {
        EcuacionProductivaDefinicion correccion = new EcuacionProductivaDefinicion
        {
            TipoProceso = TipoProcesoProductivo.CorreccionRetrabajo
        };
        EcuacionProductivaDefinicion revision = new EcuacionProductivaDefinicion
        {
            TipoProceso = TipoProcesoProductivo.RevisionControl
        };

        Assert.Equal(0.15, ReglaCalculoProcesoService.ObtenerFactorPorcentaje(correccion), 6);
        Assert.Equal(0.10, ReglaCalculoProcesoService.ObtenerFactorPorcentaje(revision), 6);
    }

    [Theory]
    [InlineData("horas = semanas_activas * 3.5", 3.5)]
    [InlineData("horas = semanas_activas x 6,25", 6.25)]
    public void Horas_semanales_aceptan_decimales(string formula, double esperado)
    {
        EcuacionProductivaDefinicion proceso = new EcuacionProductivaDefinicion
        {
            FormulaReferencia = formula,
            TipoProceso = TipoProcesoProductivo.GestionCoordinacion
        };

        Assert.Equal(
            esperado,
            ReglaCalculoProcesoService.ObtenerHorasPorSemana(proceso),
            6);
    }

    [Fact]
    public void Dependencias_hacen_round_trip_y_eliminan_duplicados()
    {
        string json = ReglaCalculoProcesoService.CrearDependenciasJson(new[]
        {
            " proc_rough ",
            "PROC_ROUGH",
            "",
            "proc_clean_up"
        });

        List<string> resultado = ReglaCalculoProcesoService.LeerDependenciasJson(json);

        Assert.Equal(new[] { "proc_rough", "proc_clean_up" }, resultado);
    }

    [Fact]
    public void Dependencias_invalidas_no_hacen_fallar_el_motor()
    {
        Assert.Empty(ReglaCalculoProcesoService.LeerDependenciasJson("{no-es-json"));
    }
}
