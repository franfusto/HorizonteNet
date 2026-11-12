using OpenDataAemet.Models;

namespace OpenDataAemet.Interfaces;

/// <summary>
/// Servicio de Avisos de Fenómenos Meteorológicos Adversos (Avisos CAP).
/// </summary>
public interface IAvisosCapService
{
    /// <summary>
    /// Obtiene los últimos avisos de fenómenos meteorológicos adversos elaborados para el área seleccionada.
    /// </summary>
    /// <param name="area">Código del área (ej: 'esp', '61' para Andalucía, '72' para Madrid, etc.).</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la información de avisos.</returns>
    Task<RespuestaAemet> GetUltimoElaboradoAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el archivo histórico de avisos de fenómenos meteorológicos adversos para el rango de fechas en formato texto.
    /// </summary>
    /// <param name="fechaIniStr">Fecha inicial en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="fechaFinStr">Fecha final en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los avisos del periodo solicitado.</returns>
    Task<RespuestaAemet> GetArchivoAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el archivo histórico de avisos de fenómenos meteorológicos adversos para el rango de fechas especificado.
    /// </summary>
    /// <param name="fechaInicio">Fecha y hora inicial del periodo.</param>
    /// <param name="fechaFin">Fecha y hora final del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los avisos del periodo solicitado.</returns>
    Task<RespuestaAemet> GetArchivoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Índices de Incendios y Mapas de Riesgo.
/// </summary>
public interface IIndicesIncendiosService
{
    /// <summary>
    /// Obtiene el mapa de niveles de riesgo de incendio meteorológico estimado para el área seleccionada.
    /// </summary>
    /// <param name="area">'p' Península, 'b' Baleares, 'c' Canarias.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el mapa de riesgo estimado.</returns>
    Task<RespuestaAemet> GetMapaRiesgoEstimadoAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el mapa de niveles de riesgo de incendio meteorológico previsto (d+1, d+2 o d+3) para el área seleccionada.
    /// </summary>
    /// <param name="dia">'1' para d+1, '2' para d+2, '3' para d+3.</param>
    /// <param name="area">'p' Península, 'b' Baleares, 'c' Canarias.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el mapa de riesgo previsto.</returns>
    Task<RespuestaAemet> GetMapaRiesgoPrevistoAsync(string dia, string area, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de datos maestros (municipios).
/// </summary>
public interface IMaestroService
{
    /// <summary>
    /// Obtiene la información sobre un municipio específico por su código INE.
    /// </summary>
    /// <param name="municipio">Código INE de 5 dígitos del municipio (ej: '28079' para Madrid).</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los datos del municipio.</returns>
    Task<RespuestaAemet> GetMunicipioAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el listado de todos los municipios de España.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la lista completa de municipios.</returns>
    Task<RespuestaAemet> GetMunicipiosAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Mapas y Gráficos (análisis y mapas significativos).
/// </summary>
public interface IMapasYGraficosService
{
    /// <summary>
    /// Obtiene el mapa de análisis de superficie.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la URL de la imagen del análisis.</returns>
    Task<RespuestaAemet> GetAnalisisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los mapas significativos de tiempo previsto.
    /// </summary>
    /// <param name="fecha">Fecha en formato AAAA-MM-DD.</param>
    /// <param name="ambito">'esp' para España, 'eur' para Europa.</param>
    /// <param name="dia">'a' para D+0 (0-12), 'b' para D+0 (12-24), 'c' para D+1 (0-12), 'd' para D+1 (12-24), 'e' para D+2 (0-12), 'f' para D+2 (12-24).</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los mapas significativos.</returns>
    Task<RespuestaAemet> GetMapasSignificativosAsync(string fecha, string ambito, string dia, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Observación Convencional (estaciones y mensajes meteorológicos).
/// </summary>
public interface IObservacionConvencionalService
{
    /// <summary>
    /// Obtiene los datos de observación de todas las estaciones convencionales disponibles para las últimas 24 horas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los datos de todas las estaciones.</returns>
    Task<RespuestaAemet> GetTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los datos de observación para una estación específica en las últimas 24 horas.
    /// </summary>
    /// <param name="idema">Indicativo climatológico de la estación (ej: '3195').</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los datos de observación de la estación.</returns>
    Task<RespuestaAemet> GetDatosEstacionAsync(string idema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene mensajes de observación meteorológica por tipo (SYNOP, METAR, etc.).
    /// </summary>
    /// <param name="tipoMensaje">Tipo de mensaje ('synop', 'metar', etc.).</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los mensajes correspondientes.</returns>
    Task<RespuestaAemet> GetMensajesTipoAsync(string tipoMensaje, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicciones Específicas (montaña, nivológica, municipio diaria y horaria, playa, UVI).
/// </summary>
public interface IPrediccionesEspecificasService
{
    /// <summary>
    /// Obtiene la predicción de montaña para el área indicada.
    /// </summary>
    /// <param name="area">Código de área montañosa.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción de montaña.</returns>
    Task<RespuestaAemet> GetMontanaPasadaAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción de montaña para el área y día indicados.
    /// </summary>
    /// <param name="area">Código de área montañosa.</param>
    /// <param name="dia">Día consultado.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción de montaña para el día seleccionado.</returns>
    Task<RespuestaAemet> GetMontanaPasadaDiaAsync(string area, string dia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la información nivológica para el área seleccionada.
    /// </summary>
    /// <param name="area">Código del área.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el informe nivológico.</returns>
    Task<RespuestaAemet> GetNivologicaAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción diaria por municipio (hasta 7 días).
    /// </summary>
    /// <param name="municipio">Código INE de 5 dígitos del municipio (ej: '28079').</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el pronóstico diario del municipio.</returns>
    Task<RespuestaAemet> GetMunicipioDiariaAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción diaria para todos los municipios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción diaria de todos los municipios.</returns>
    Task<RespuestaAemet> GetMunicipioDiariaTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción horaria por municipio (hasta 48 horas).
    /// </summary>
    /// <param name="municipio">Código INE de 5 dígitos del municipio.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el pronóstico horario del municipio.</returns>
    Task<RespuestaAemet> GetMunicipioHorariaAsync(string municipio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción horaria para todos los municipios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción horaria de todos los municipios.</returns>
    Task<RespuestaAemet> GetMunicipioHorariaTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción específica para la playa seleccionada.
    /// </summary>
    /// <param name="playa">Código identificador de la playa.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción marítima de la playa.</returns>
    Task<RespuestaAemet> GetPlayaAsync(string playa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el índice de Radiación Ultravioleta (UVI) máximo previsto.
    /// </summary>
    /// <param name="dia">'0' para hoy, '1' para mañana, '2' para pasado mañana, '3' para d+3, '4' para d+4.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con los valores de índice UVI previstos.</returns>
    Task<RespuestaAemet> GetUviAsync(string dia, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicciones Normalizadas en Texto (CCAA, Nacional y Provincias).
/// </summary>
public interface IPrediccionesTextoService
{
    // CCAA
    /// <summary>Obtiene la predicción de hoy para la comunidad autónoma especificada.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaHoyAsync(string ccaa, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción de hoy para la CCAA y fecha de elaboración especificadas.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaHoyElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción de mañana para la comunidad autónoma especificada.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaMananaAsync(string ccaa, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción de mañana para la CCAA y fecha de elaboración especificadas.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción a medio plazo para la comunidad autónoma especificada.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaMedioPlazoAsync(string ccaa, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción a medio plazo para la CCAA y fecha de elaboración especificadas.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaMedioPlazoElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción de pasado mañana para la comunidad autónoma especificada.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaPasadoMananaAsync(string ccaa, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción de pasado mañana para la CCAA y fecha de elaboración especificadas.</summary>
    /// <param name="ccaa">Código de la CCAA.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetCcaaPasadoMananaElaboracionAsync(string ccaa, string fecha, CancellationToken cancellationToken = default);

    // Nacional
    /// <summary>Obtiene la predicción meteorológica nacional para hoy.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalHoyAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional de hoy para la fecha de elaboración indicada.</summary>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalHoyElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional para mañana.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalMananaAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional de mañana para la fecha de elaboración indicada.</summary>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional a medio plazo.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalMedioPlazoAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional a medio plazo para la fecha de elaboración indicada.</summary>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalMedioPlazoElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional para pasado mañana.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalPasadoMananaAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción meteorológica nacional de pasado mañana para la fecha de elaboración indicada.</summary>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalPasadoMananaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la tendencia meteorológica nacional.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalTendenciaAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la tendencia meteorológica nacional para la fecha de elaboración indicada.</summary>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNacionalTendenciaElaboracionAsync(string fecha, CancellationToken cancellationToken = default);

    // Provincia
    /// <summary>Obtiene la predicción provincial de hoy para la provincia especificada.</summary>
    /// <param name="provincia">Código de provincia.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProvinciaHoyAsync(string provincia, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción provincial de hoy para la provincia y fecha de elaboración especificadas.</summary>
    /// <param name="provincia">Código de provincia.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProvinciaHoyElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción provincial de mañana para la provincia especificada.</summary>
    /// <param name="provincia">Código de provincia.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProvinciaMananaAsync(string provincia, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la predicción provincial de mañana para la provincia y fecha de elaboración especificadas.</summary>
    /// <param name="provincia">Código de provincia.</param>
    /// <param name="fecha">Fecha de elaboración en formato AAAA-MM-DD.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProvinciaMananaElaboracionAsync(string provincia, string fecha, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Predicción Marítima (altamar y zonas costeras).
/// </summary>
public interface IPrediccionMaritimaService
{
    /// <summary>
    /// Obtiene la predicción marítima en alta mar para el área seleccionada.
    /// </summary>
    /// <param name="area">Código del área de alta mar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción de alta mar.</returns>
    Task<RespuestaAemet> GetAltamarAsync(string area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la predicción marítima costera para la zona costera seleccionada.
    /// </summary>
    /// <param name="costa">Código de la zona costera.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la predicción costera.</returns>
    Task<RespuestaAemet> GetCosteraAsync(string costa, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Productos Climatológicos (balance hídrico, resumen climatológico nacional, capas shape).
/// </summary>
public interface IProductosClimatologicosService
{
    /// <summary>
    /// Obtiene el balance hídrico para el año y decena seleccionados.
    /// </summary>
    /// <param name="anio">Año de consulta (ej: "2023").</param>
    /// <param name="decena">Decena del mes ("1", "2" o "3").</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el informe del balance hídrico.</returns>
    Task<RespuestaAemet> GetBalanceHidricoAsync(string anio, string decena, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el resumen climatológico nacional mensual.
    /// </summary>
    /// <param name="anio">Año de consulta.</param>
    /// <param name="mes">Mes de consulta ("01" a "12").</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con el resumen mensual.</returns>
    Task<RespuestaAemet> GetResumenClimatologicoNacionalAsync(string anio, string mes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las capas Shapefile de estaciones climatológicas según el tipo.
    /// </summary>
    /// <param name="tipoEstacion">'automaticas', 'completas', 'pluviometricas', 'secundarias', 'termoradiometricas'.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API con la URL de descarga de las capas shapefile.</returns>
    Task<RespuestaAemet> GetCapasShapeAsync(string tipoEstacion, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Redes Especiales (contaminación de fondo, ozono, perfil de ozono y radiación).
/// </summary>
public interface IRedesEspecialesService
{
    /// <summary>Obtiene los datos de contaminación de fondo para una estación específica.</summary>
    /// <param name="nombreEstacion">Nombre o identificador de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetContaminacionFondoAsync(string nombreEstacion, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los datos de ozono.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetOzonoAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene el perfil de ozono para la estación especificada.</summary>
    /// <param name="estacion">Nombre o identificador de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetPerfilOzonoAsync(string estacion, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los datos de radiación.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetRadiacionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Red de Radares (nacional y regional).
/// </summary>
public interface IRedRadaresService
{
    /// <summary>Obtiene la imagen o datos del radar nacional.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetRadarNacionalAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene la imagen o datos del radar regional especificado.</summary>
    /// <param name="radar">Código del radar regional.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetRadarRegionalAsync(string radar, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Red de Detección de Rayos.
/// </summary>
public interface IRedRayosService
{
    /// <summary>Obtiene el mapa de detección de rayos.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetMapaRayosAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Información Satelital (NVDI, SST).
/// </summary>
public interface IInformacionSateliteService
{
    /// <summary>Obtiene el producto satelital NVDI (Índice de Vegetación de Diferencia Normalizada).</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProductoNvdiAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene el producto satelital SST (Temperatura Superficial del Mar).</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetProductoSstAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de Valores Climatológicos (diarios, mensuales/anuales, normales, extremos e inventario de estaciones).
/// </summary>
public interface IValoresClimatologicosService
{
    /// <summary>Obtiene los valores climatológicos diarios para una estación en el rango de fechas en formato texto.</summary>
    /// <param name="fechaIniStr">Fecha inicial en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="fechaFinStr">Fecha final en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="idema">Indicativo climatológico de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDiariosEstacionAsync(string fechaIniStr, string fechaFinStr, string idema, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores climatológicos diarios para una estación en el rango de fechas especificado.</summary>
    /// <param name="fechaInicio">Fecha inicial.</param>
    /// <param name="fechaFin">Fecha final.</param>
    /// <param name="idema">Indicativo climatológico de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDiariosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string idema, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores climatológicos diarios para todas las estaciones en el rango de fechas en formato texto.</summary>
    /// <param name="fechaIniStr">Fecha inicial en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="fechaFinStr">Fecha final en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(string fechaIniStr, string fechaFinStr, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores climatológicos diarios para todas las estaciones en el rango de fechas especificado.</summary>
    /// <param name="fechaInicio">Fecha inicial.</param>
    /// <param name="fechaFin">Fecha final.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDiariosTodasEstacionesAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);

    /// <summary>Obtiene el inventario de las estaciones climatológicas especificadas.</summary>
    /// <param name="estaciones">Cadena de estaciones separadas por comas o criterio de búsqueda.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetInventarioEstacionesAsync(string estaciones, CancellationToken cancellationToken = default);

    /// <summary>Obtiene el inventario de todas las estaciones climatológicas.</summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetInventarioTodasEstacionesAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores climatológicos mensuales y anuales para una estación específica en el rango de años.</summary>
    /// <param name="anioIniStr">Año inicial.</param>
    /// <param name="anioFinStr">Año final.</param>
    /// <param name="idema">Indicativo climatológico de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetMensualesAnualesEstacionAsync(string anioIniStr, string anioFinStr, string idema, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores climatológicos normales (periodo estándar) para una estación.</summary>
    /// <param name="idema">Indicativo climatológico de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetNormalesEstacionAsync(string idema, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los valores extremos registrados para un parámetro meteorológico y estación específica.</summary>
    /// <param name="parametro">Parámetro analizado (ej: "T" temperatura, "P" precipitación).</param>
    /// <param name="idema">Indicativo climatológico de la estación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetValoresExtremosAsync(string parametro, string idema, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de datos meteorológicos de la Antártida.
/// </summary>
public interface IAntartidaService
{
    /// <summary>Obtiene datos meteorológicos de la estación antártica en el rango de fechas en formato texto.</summary>
    /// <param name="fechaIniStr">Fecha inicial en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="fechaFinStr">Fecha final en formato AAAA-MM-DDTHH:MM:SSUTC.</param>
    /// <param name="identificacion">Identificador de la estación antártica.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDatosEstacionAsync(string fechaIniStr, string fechaFinStr, string identificacion, CancellationToken cancellationToken = default);

    /// <summary>Obtiene datos meteorológicos de la estación antártica en el rango de fechas especificado.</summary>
    /// <param name="fechaInicio">Fecha inicial.</param>
    /// <param name="fechaFin">Fecha final.</param>
    /// <param name="identificacion">Identificador de la estación antártica.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta de la API.</returns>
    Task<RespuestaAemet> GetDatosEstacionAsync(DateTime fechaInicio, DateTime fechaFin, string identificacion, CancellationToken cancellationToken = default);
}
