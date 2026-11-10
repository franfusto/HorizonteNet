using OpenDataAemet.Models;

namespace OpenDataAemet.Interfaces;

/// <summary>
/// Servicio de Avisos de Fenómenos Meteorológicos Adversos (Avisos CAP).
/// </summary>
public interface IAvisosCapService
{
    /// <summary>
    /// Últimos avisos de fenómenos meteorológicos adversos elaborado para el área seleccionada.
    /// </summary>
    /// <param name="area">Código del área (ej: 'esp', '61' para Andalucía, '72' para Madrid, etc.).</param>
    Task<RespuestaAemet> GetUltimoElaboradoAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Avisos de fenómenos meteorológicos adversos para el rango de fechas seleccionado.
    /// </summary>
    /// <param name="fechaIniStr">Fecha inicial en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="fechaFinStr">Fecha final en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    Task<RespuestaAemet> GetArchivoAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default);

    /// <summary>
    /// Avisos de fenómenos meteorológicos adversos para el rango de fechas seleccionado.
    /// </summary>
    Task<RespuestaAemet> GetArchivoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Índices de Incendios y Mapas de Riesgo.
/// </summary>
public interface IIndicesIncendiosService
{
    /// <summary>
    /// Mapa de niveles de riesgo de incendio meteorológico estimado para el área seleccionada.
    /// </summary>
    /// <param name="area">'p' Península, 'b' Baleares, 'c' Canarias.</param>
    Task<RespuestaAemet> GetMapaRiesgoEstimadoAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mapa de niveles de riesgo de incendio meteorológico previsto (d+1, d+2 o d+3) para el área seleccionada.
    /// </summary>
    /// <param name="dia">'1' para d+1, '2' para d+2, '3' para d+3.</param>
    /// <param name="area">'p' Península, 'b' Baleares, 'c' Canarias.</param>
    Task<RespuestaAemet> GetMapaRiesgoPrevistoAsync(string dia, string area, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de datos maestros (municipios).
/// </summary>
public interface IMaestroService
{
    /// <summary>
    /// Información sobre un municipio específico por su código INE (ej: '28079' para Madrid).
    /// </summary>
    Task<RespuestaAemet> GetMunicipioAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Listado de todos los municipios.
    /// </summary>
    Task<RespuestaAemet> GetMunicipiosAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Mapas y Gráficos (análisis y mapas significativos).
/// </summary>
public interface IMapasYGraficosService
{
    /// <summary>
    /// Mapa de análisis de superficie.
    /// </summary>
    Task<RespuestaAemet> GetAnalisisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mapas significativos de tiempo previsto.
    /// </summary>
    /// <param name="fecha">Fecha en formato AAAA-MM-DD.</param>
    /// <param name="ambito">'esp' para España, 'eur' para Europa.</param>
    /// <param name="dia">'a' para D+0 (0-12), 'b' para D+0 (12-24), 'c' para D+1 (0-12), 'd' para D+1 (12-24), 'e' para D+2 (0-12), 'f' para D+2 (12-24).</param>
    Task<RespuestaAemet> GetMapasSignificativosAsync(string fecha, string ambito, string dia, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Observación Convencional (estaciones y mensajes meteorológicos).
/// </summary>
public interface IObservacionConvencionalService
{
    /// <summary>
    /// Datos de observación de todas las estaciones convencionales disponibles para las últimas 24 horas.
    /// </summary>
    Task<RespuestaAemet> GetTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Datos de observación para una estación específica en las últimas 24 horas.
    /// </summary>
    /// <param name="idema">Indicativo climatológico de la estación (ej: '3195').</param>
    Task<RespuestaAemet> GetDatosEstacionAsync(string idema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mensajes de observación (SYNOP, METAR, etc.).
    /// </summary>
    /// <param name="tipoMensaje">Tipo de mensaje ('synop', 'metar', etc.).</param>
    Task<RespuestaAemet> GetMensajesTipoAsync(string tipoMensaje, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicciones Específicas (montaña, nivológica, municipio diaria y horaria, playa, UVI).
/// </summary>
public interface IPrediccionesEspecificasService
{
    /// <summary>
    /// Predicción de montaña para el área indicada.
    /// </summary>
    Task<RespuestaAemet> GetMontanaPasadaAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción de montaña para el área y día indicados.
    /// </summary>
    Task<RespuestaAemet> GetMontanaPasadaDiaAsync(string area, string dia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Información nivológica para el área seleccionada.
    /// </summary>
    Task<RespuestaAemet> GetNivologicaAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción diaria por municipio (hasta 7 días).
    /// </summary>
    /// <param name="municipio">Código INE de 5 dígitos del municipio (ej: '28079').</param>
    Task<RespuestaAemet> GetMunicipioDiariaAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción diaria para todos los municipios.
    /// </summary>
    Task<RespuestaAemet> GetMunicipioDiariaTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción horaria por municipio (hasta 48 horas).
    /// </summary>
    /// <param name="municipio">Código INE de 5 dígitos del municipio.</param>
    Task<RespuestaAemet> GetMunicipioHorariaAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción horaria para todos los municipios.
    /// </summary>
    Task<RespuestaAemet> GetMunicipioHorariaTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción específica para la playa seleccionada.
    /// </summary>
    /// <param name="playa">Código de playa.</param>
    Task<RespuestaAemet> GetPlayaAsync(string playa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Índice de Radiación Ultravioleta (UVI) máximo previsto.
    /// </summary>
    /// <param name="dia">'0' para hoy, '1' para mañana, '2' para pasado mañana, '3' para d+3, '4' para d+4.</param>
    Task<RespuestaAemet> GetUviAsync(string dia, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicciones Normalizadas en Texto (CCAA, Nacional y Provincias).
/// </summary>
public interface IPrediccionesTextoService
{
    // CCAA
    Task<RespuestaAemet> GetCcaaHoyAsync(string ccaa, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaHoyElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaMananaAsync(string ccaa, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaMedioPlazoAsync(string ccaa, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaMedioPlazoElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaPasadoMananaAsync(string ccaa, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetCcaaPasadoMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);

    // Nacional
    Task<RespuestaAemet> GetNacionalHoyAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalHoyElaboracionAsync(string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalMananaAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalMedioPlazoAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalMedioPlazoElaboracionAsync(string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalPasadoMananaAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalPasadoMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalTendenciaAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNacionalTendenciaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    // Provincia
    Task<RespuestaAemet> GetProvinciaHoyAsync(string provincia, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetProvinciaHoyElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetProvinciaMananaAsync(string provincia, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetProvinciaMananaElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicción Marítima (altamar y zonas costeras).
/// </summary>
public interface IPrediccionMaritimaService
{
    /// <summary>
    /// Predicción marítima en alta mar para el área seleccionada.
    /// </summary>
    Task<RespuestaAemet> GetAltamarAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicción marítima costera para la zona costera seleccionada.
    /// </summary>
    Task<RespuestaAemet> GetCosteraAsync(string costa, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Productos Climatológicos (balance hídrico, resumen climatológico nacional, capas shape).
/// </summary>
public interface IProductosClimatologicosService
{
    /// <summary>
    /// Balance hídrico para el año y decena seleccionados.
    /// </summary>
    Task<RespuestaAemet> GetBalanceHidricoAsync(string anio, string decena, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumen climatológico nacional mensual.
    /// </summary>
    Task<RespuestaAemet> GetResumenClimatologicoNacionalAsync(string anio, string mes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Capas Shapefile de estaciones climatológicas.
    /// </summary>
    /// <param name="tipoEstacion">'automaticas', 'completas', 'pluviometricas', 'secundarias', 'termoradiometricas'.</param>
    Task<RespuestaAemet> GetCapasShapeAsync(string tipoEstacion, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Redes Especiales (contaminación de fondo, ozono, perfil de ozono y radiación).
/// </summary>
public interface IRedesEspecialesService
{
    Task<RespuestaAemet> GetContaminacionFondoAsync(string nombreEstacion, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetOzonoAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetPerfilOzonoAsync(string estacion, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetRadiacionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Red de Radares (nacional y regional).
/// </summary>
public interface IRedRadaresService
{
    Task<RespuestaAemet> GetRadarNacionalAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetRadarRegionalAsync(string radar, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Red de Detección de Rayos.
/// </summary>
public interface IRedRayosService
{
    Task<RespuestaAemet> GetMapaRayosAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Información Satelital (NVDI, SST).
/// </summary>
public interface IInformacionSateliteService
{
    Task<RespuestaAemet> GetProductoNvdiAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetProductoSstAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Valores Climatológicos (diarios, mensuales/anuales, normales, extremos e inventario de estaciones).
/// </summary>
public interface IValoresClimatologicosService
{
    Task<RespuestaAemet> GetDiariosEstacionAsync(string fechaIniStr, string fechaFinStr, string idema, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetDiariosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string idema, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetInventarioEstacionesAsync(string estaciones, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetInventarioTodasEstacionesAsync(CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetMensualesAnualesEstacionAsync(string anioIniStr, string anioFinStr, string idema, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetNormalesEstacionAsync(string idema, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetValoresExtremosAsync(string parametro, string idema, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de datos meteorológicos de la Antártida.
/// </summary>
public interface IAntartidaService
{
    Task<RespuestaAemet> GetDatosEstacionAsync(string fechaIniStr, string fechaFinStr, string identificacion, CancellationToken cancellationToken = default);
    Task<RespuestaAemet> GetDatosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string identificacion, CancellationToken cancellationToken = default);
}
