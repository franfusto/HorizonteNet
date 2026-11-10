using OpenDataAemet.Interfaces;
using OpenDataAemet.Models;

namespace OpenDataAemet.Services;

internal class AvisosCapService : IAvisosCapService
{
    private readonly IAemetClient _client;
    public AvisosCapService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetUltimoElaboradoAsync(string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/avisos_cap/ultimoelaborado/area/{Uri.EscapeDataString(area)}", cancellationToken);

    public Task<RespuestaAemet> GetArchivoAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/avisos_cap/archivo/fechaini/{Uri.EscapeDataString(fechaIniStr)}/fechafin/{Uri.EscapeDataString(fechaFinStr)}", cancellationToken);

    public Task<RespuestaAemet> GetArchivoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        => GetArchivoAsync(FormatearFechaAemet(fechaInicio), FormatearFechaAemet(fechaFin), cancellationToken);

    private static string FormatearFechaAemet(DateTime fecha) => fecha.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssUTC");
}

internal class IndicesIncendiosService : IIndicesIncendiosService
{
    private readonly IAemetClient _client;
    public IndicesIncendiosService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetMapaRiesgoEstimadoAsync(string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/incendios/mapasriesgo/estimado/area/{Uri.EscapeDataString(area)}", cancellationToken);

    public Task<RespuestaAemet> GetMapaRiesgoPrevistoAsync(string dia, string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/incendios/mapasriesgo/previsto/dia/{Uri.EscapeDataString(dia)}/area/{Uri.EscapeDataString(area)}", cancellationToken);
}

internal class MaestroService : IMaestroService
{
    private readonly IAemetClient _client;
    public MaestroService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetMunicipioAsync(string municipio, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/maestro/municipio/{Uri.EscapeDataString(municipio)}", cancellationToken);

    public Task<RespuestaAemet> GetMunicipiosAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/maestro/municipios", cancellationToken);
}

internal class MapasYGraficosService : IMapasYGraficosService
{
    private readonly IAemetClient _client;
    public MapasYGraficosService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetAnalisisAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/mapasygraficos/analisis", cancellationToken);

    public Task<RespuestaAemet> GetMapasSignificativosAsync(string fecha, string ambito, string dia, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/mapasygraficos/mapassignificativos/fecha/{Uri.EscapeDataString(fecha)}/{Uri.EscapeDataString(ambito)}/{Uri.EscapeDataString(dia)}", cancellationToken);
}

internal class ObservacionConvencionalService : IObservacionConvencionalService
{
    private readonly IAemetClient _client;
    public ObservacionConvencionalService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetTodasAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/observacion/convencional/todas", cancellationToken);

    public Task<RespuestaAemet> GetDatosEstacionAsync(string idema, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/observacion/convencional/datos/estacion/{Uri.EscapeDataString(idema)}", cancellationToken);

    public Task<RespuestaAemet> GetMensajesTipoAsync(string tipoMensaje, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/observacion/convencional/mensajes/tipomensaje/{Uri.EscapeDataString(tipoMensaje)}", cancellationToken);
}

internal class PrediccionesEspecificasService : IPrediccionesEspecificasService
{
    private readonly IAemetClient _client;
    public PrediccionesEspecificasService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetMontanaPasadaAsync(string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/montaña/pasada/area/{Uri.EscapeDataString(area)}", cancellationToken);

    public Task<RespuestaAemet> GetMontanaPasadaDiaAsync(string area, string dia, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/montaña/pasada/area/{Uri.EscapeDataString(area)}/dia/{Uri.EscapeDataString(dia)}", cancellationToken);

    public Task<RespuestaAemet> GetNivologicaAsync(string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/nivologica/{Uri.EscapeDataString(area)}", cancellationToken);

    public Task<RespuestaAemet> GetMunicipioDiariaAsync(string municipio, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/municipio/diaria/{Uri.EscapeDataString(municipio)}", cancellationToken);

    public Task<RespuestaAemet> GetMunicipioDiariaTodosAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/especifica/municipio/diaria/todos", cancellationToken);

    public Task<RespuestaAemet> GetMunicipioHorariaAsync(string municipio, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/municipio/horaria/{Uri.EscapeDataString(municipio)}", cancellationToken);

    public Task<RespuestaAemet> GetMunicipioHorariaTodosAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/especifica/municipio/horaria/todos", cancellationToken);

    public Task<RespuestaAemet> GetPlayaAsync(string playa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/playa/{Uri.EscapeDataString(playa)}", cancellationToken);

    public Task<RespuestaAemet> GetUviAsync(string dia, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/especifica/uvi/{Uri.EscapeDataString(dia)}", cancellationToken);
}

internal class PrediccionesTextoService : IPrediccionesTextoService
{
    private readonly IAemetClient _client;
    public PrediccionesTextoService(IAemetClient client) => _client = client;

    // CCAA
    public Task<RespuestaAemet> GetCcaaHoyAsync(string ccaa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/hoy/{Uri.EscapeDataString(ccaa)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaHoyElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/hoy/{Uri.EscapeDataString(ccaa)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaMananaAsync(string ccaa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/manana/{Uri.EscapeDataString(ccaa)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/manana/{Uri.EscapeDataString(ccaa)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaMedioPlazoAsync(string ccaa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/medioplazo/{Uri.EscapeDataString(ccaa)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaMedioPlazoElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/medioplazo/{Uri.EscapeDataString(ccaa)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaPasadoMananaAsync(string ccaa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/pasadomanana/{Uri.EscapeDataString(ccaa)}", cancellationToken);

    public Task<RespuestaAemet> GetCcaaPasadoMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/ccaa/pasadomanana/{Uri.EscapeDataString(ccaa)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    // Nacional
    public Task<RespuestaAemet> GetNacionalHoyAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/nacional/hoy", cancellationToken);

    public Task<RespuestaAemet> GetNacionalHoyElaboracionAsync(string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/nacional/hoy/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetNacionalMananaAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/nacional/manana", cancellationToken);

    public Task<RespuestaAemet> GetNacionalMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/nacional/manana/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetNacionalMedioPlazoAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/nacional/medioplazo", cancellationToken);

    public Task<RespuestaAemet> GetNacionalMedioPlazoElaboracionAsync(string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/nacional/medioplazo/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetNacionalPasadoMananaAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/nacional/pasadomanana", cancellationToken);

    public Task<RespuestaAemet> GetNacionalPasadoMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/nacional/pasadomanana/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetNacionalTendenciaAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/prediccion/nacional/tendencia", cancellationToken);

    public Task<RespuestaAemet> GetNacionalTendenciaElaboracionAsync(string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/nacional/tendencia/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    // Provincia
    public Task<RespuestaAemet> GetProvinciaHoyAsync(string provincia, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/provincia/hoy/{Uri.EscapeDataString(provincia)}", cancellationToken);

    public Task<RespuestaAemet> GetProvinciaHoyElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/provincia/hoy/{Uri.EscapeDataString(provincia)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);

    public Task<RespuestaAemet> GetProvinciaMananaAsync(string provincia, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/provincia/manana/{Uri.EscapeDataString(provincia)}", cancellationToken);

    public Task<RespuestaAemet> GetProvinciaMananaElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/provincia/manana/{Uri.EscapeDataString(provincia)}/elaboracion/{Uri.EscapeDataString(fecha)}", cancellationToken);
}

internal class PrediccionMaritimaService : IPrediccionMaritimaService
{
    private readonly IAemetClient _client;
    public PrediccionMaritimaService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetAltamarAsync(string area, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/maritima/altamar/area/{Uri.EscapeDataString(area)}", cancellationToken);

    public Task<RespuestaAemet> GetCosteraAsync(string costa, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/prediccion/maritima/costera/costa/{Uri.EscapeDataString(costa)}", cancellationToken);
}

internal class ProductosClimatologicosService : IProductosClimatologicosService
{
    private readonly IAemetClient _client;
    public ProductosClimatologicosService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetBalanceHidricoAsync(string anio, string decena, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/productos/climatologicos/balancehidrico/{Uri.EscapeDataString(anio)}/{Uri.EscapeDataString(decena)}", cancellationToken);

    public Task<RespuestaAemet> GetResumenClimatologicoNacionalAsync(string anio, string mes, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/productos/climatologicos/resumenclimatologico/nacional/{Uri.EscapeDataString(anio)}/{Uri.EscapeDataString(mes)}", cancellationToken);

    public Task<RespuestaAemet> GetCapasShapeAsync(string tipoEstacion, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/productos/climatologicos/capasshape/{Uri.EscapeDataString(tipoEstacion)}", cancellationToken);
}

internal class RedesEspecialesService : IRedesEspecialesService
{
    private readonly IAemetClient _client;
    public RedesEspecialesService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetContaminacionFondoAsync(string nombreEstacion, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/red/especial/contaminacionfondo/estacion/{Uri.EscapeDataString(nombreEstacion)}", cancellationToken);

    public Task<RespuestaAemet> GetOzonoAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/red/especial/ozono", cancellationToken);

    public Task<RespuestaAemet> GetPerfilOzonoAsync(string estacion, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/red/especial/perfilozono/estacion/{Uri.EscapeDataString(estacion)}", cancellationToken);

    public Task<RespuestaAemet> GetRadiacionAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/red/especial/radiacion", cancellationToken);
}

internal class RedRadaresService : IRedRadaresService
{
    private readonly IAemetClient _client;
    public RedRadaresService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetRadarNacionalAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/red/radar/nacional", cancellationToken);

    public Task<RespuestaAemet> GetRadarRegionalAsync(string radar, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/red/radar/regional/{Uri.EscapeDataString(radar)}", cancellationToken);
}

internal class RedRayosService : IRedRayosService
{
    private readonly IAemetClient _client;
    public RedRayosService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetMapaRayosAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/red/rayos/mapa", cancellationToken);
}

internal class InformacionSateliteService : IInformacionSateliteService
{
    private readonly IAemetClient _client;
    public InformacionSateliteService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetProductoNvdiAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/satelites/producto/nvdi", cancellationToken);

    public Task<RespuestaAemet> GetProductoSstAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/satelites/producto/sst", cancellationToken);
}

internal class ValoresClimatologicosService : IValoresClimatologicosService
{
    private readonly IAemetClient _client;
    public ValoresClimatologicosService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetDiariosEstacionAsync(string fechaIniStr, string fechaFinStr, string idema, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/diarios/datos/fechaini/{Uri.EscapeDataString(fechaIniStr)}/fechafin/{Uri.EscapeDataString(fechaFinStr)}/estacion/{Uri.EscapeDataString(idema)}", cancellationToken);

    public Task<RespuestaAemet> GetDiariosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string idema, CancellationToken cancellationToken = default)
        => GetDiariosEstacionAsync(FormatearFechaAemet(fechaInicio), FormatearFechaAemet(fechaFin), idema, cancellationToken);

    public Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/diarios/datos/fechaini/{Uri.EscapeDataString(fechaIniStr)}/fechafin/{Uri.EscapeDataString(fechaFinStr)}/todasestaciones", cancellationToken);

    public Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        => GetDiariosTodasEstacionesAsync(FormatearFechaAemet(fechaInicio), FormatearFechaAemet(fechaFin), cancellationToken);

    public Task<RespuestaAemet> GetInventarioEstacionesAsync(string estaciones, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/inventarioestaciones/estaciones/{Uri.EscapeDataString(estaciones)}", cancellationToken);

    public Task<RespuestaAemet> GetInventarioTodasEstacionesAsync(CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync("/api/valores/climatologicos/inventarioestaciones/todasestaciones", cancellationToken);

    public Task<RespuestaAemet> GetMensualesAnualesEstacionAsync(string anioIniStr, string anioFinStr, string idema, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/mensualesanuales/datos/anioini/{Uri.EscapeDataString(anioIniStr)}/aniofin/{Uri.EscapeDataString(anioFinStr)}/estacion/{Uri.EscapeDataString(idema)}", cancellationToken);

    public Task<RespuestaAemet> GetNormalesEstacionAsync(string idema, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/normales/estacion/{Uri.EscapeDataString(idema)}", cancellationToken);

    public Task<RespuestaAemet> GetValoresExtremosAsync(string parametro, string idema, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/valores/climatologicos/valoresextremos/parametro/{Uri.EscapeDataString(parametro)}/estacion/{Uri.EscapeDataString(idema)}", cancellationToken);

    private static string FormatearFechaAemet(DateTime fecha) => fecha.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssUTC");
}

internal class AntartidaService : IAntartidaService
{
    private readonly IAemetClient _client;
    public AntartidaService(IAemetClient client) => _client = client;

    public Task<RespuestaAemet> GetDatosEstacionAsync(string fechaIniStr, string fechaFinStr, string identificacion, CancellationToken cancellationToken = default)
        => _client.EnviarPeticionAsync($"/api/antartida/datos/fechaini/{Uri.EscapeDataString(fechaIniStr)}/fechafin/{Uri.EscapeDataString(fechaFinStr)}/estacion/{Uri.EscapeDataString(identificacion)}", cancellationToken);

    public Task<RespuestaAemet> GetDatosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string identificacion, CancellationToken cancellationToken = default)
        => GetDatosEstacionAsync(FormatearFechaAemet(fechaInicio), FormatearFechaAemet(fechaFin), identificacion, cancellationToken);

    private static string FormatearFechaAemet(DateTime fecha) => fecha.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssUTC");
}
