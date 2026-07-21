using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace OthalartSoftware.Tests;

public class CalculoProductivoResolverServiceTests
{
    [Fact]
    public void Tiempo_asignado_conserva_horas_y_recalcula_costo()
    {
        RequerimientoProduccionInterna requerimiento = new RequerimientoProduccionInterna
        {
            ModoCalculoProductivo = ModosCalculoProductivo.TiempoAsignado,
            HorasEstandar = 3.5,
            TarifaDiaCargoCLP = 80000
        };

        bool aplicado = CalculoProductivoResolverService.Aplicar(requerimiento, 5);

        Assert.True(aplicado);
        Assert.Equal(3.5, requerimiento.HorasEstandar, 6);
        Assert.Equal(3.5 / 8.0, requerimiento.DiasPersonaStd, 6);
        Assert.Equal(35000, requerimiento.CostoEstandarCLP, 2);
        Assert.Equal("Tiempo asignado", requerimiento.OrigenHoras);
    }

    [Fact]
    public void Tiempo_asignado_recupera_horas_desde_dias_legacy()
    {
        RequerimientoProduccionInterna requerimiento = new RequerimientoProduccionInterna
        {
            ModoCalculoProductivo = ModosCalculoProductivo.TiempoAsignado,
            DiasPersonaStd = 0.625,
            TarifaDiaCargoCLP = 80000
        };

        bool aplicado = CalculoProductivoResolverService.AplicarTiempoAsignado(requerimiento);

        Assert.True(aplicado);
        Assert.Equal(5, requerimiento.HorasEstandar, 6);
        Assert.Equal(50000, requerimiento.CostoEstandarCLP, 2);
    }

    [Fact]
    public void Tiempo_asignado_negativo_se_normaliza_y_no_se_aplica()
    {
        RequerimientoProduccionInterna requerimiento = new RequerimientoProduccionInterna
        {
            ModoCalculoProductivo = ModosCalculoProductivo.TiempoAsignado,
            HorasEstandar = -5
        };

        bool aplicado = CalculoProductivoResolverService.AplicarTiempoAsignado(requerimiento);

        Assert.False(aplicado);
        Assert.Equal(0, requerimiento.HorasEstandar);
    }
}
