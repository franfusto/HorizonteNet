using System.Text.Json;
using OpenDataAemet.Models;

namespace OpenDataAemet.Interfaces;

/// <summary>
/// Cliente principal para la API de AEMET OpenData.
/// </summary>
public interface IAemetClient
{
    /// <summary>
    /// Opciones de configuración del cliente.
    /// </summary>
    AemetOptions Options { get; }

    /// <summary>
    /// Servicio de Avisos de Fenómenos Meteorológicos Adversos (Avisos CAP).
    /// </summary>
    IAvisosCapService AvisosCap { get; }

    /// <summary>
    /// Servicio de Índices de Incendios y Mapas de Riesgo.
    /// </summary>
    IIndicesIncendiosService IndicesIncendios { get; }

    /// <summary>
    /// Servicio de datos maestros (municipios).
    /// </summary>
    IMaestroService Maestro { get; }

    /// <summary>
    /// Servicio de Mapas y Gráficos.
    /// </summary>
    IMapasYGraficosService MapasYGraficos { get; }

    /// <summary>
    /// Servicio de Observación Convencional.
    /// </summary>
    IObservacionConvencionalService ObservacionConvencional { get; }

    /// <summary>
    /// Servicio de Predicciones Específicas.
    /// </summary>
    IPrediccionesEspecificasService PrediccionesEspecificas { get; }

    /// <summary>
    /// Servicio de Predicciones Normalizadas en Texto.
    /// </summary>
    IPrediccionesTextoService PrediccionesTexto { get; }

    /// <summary>
    /// Servicio de Predicción Marítima.
    /// </summary>
    IPrediccionMaritimaService PrediccionMaritima { get; }

    /// <summary>
    /// Servicio de Productos Climatológicos.
    /// </summary>
    IProductosClimatologicosService ProductosClimatologicos { get; }

    /// <summary>
    /// Servicio de Redes Especiales.
    /// </summary>
    IRedesEspecialesService RedesEspeciales { get; }

    /// <summary>
    /// Servicio de Red de Radares.
    /// </summary>
    IRedRadaresService RedRadares { get; }

    /// <summary>
    /// Servicio de Red de Rayos.
    /// </summary>
    IRedRayosService RedRayos { get; }

    /// <summary>
    /// Servicio de Información Satelital.
    /// </summary>
    IInformacionSateliteService InformacionSatelite { get; }

    /// <summary>
    /// Servicio de Valores Climatológicos.
    /// </summary>
    IValoresClimatologicosService ValoresClimatologicos { get; }

    /// <summary>
    /// Servicio de datos de la Antártida.
    /// </summary>
    IAntartidaService Antartida { get; }

    /// <summary>
    /// Ejecuta una petición GET a un endpoint relativo de la API de AEMET OpenData y deserializa la respuesta estándar.
    /// </summary>
    /// <param name="relativeUrl">Ruta relativa del endpoint (ej: "/api/maestro/municipios").</param>
    /// <param name="cancellationToken">Token de cancelación opcional para abortar la operación asíncrona.</param>
    /// <returns>Una instancia de <see cref="RespuestaAemet"/> con el estado y las URLs de datos y metadatos.</returns>
    Task<RespuestaAemet> EnviarPeticionAsync(string relativeUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga el contenido de texto plano / JSON / XML desde la URL especificada en el campo 'Datos' de la respuesta.
    /// </summary>
    /// <param name="respuesta">Respuesta devuelta previamente por la API de AEMET.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>El contenido de texto descargado.</returns>
    Task<string> ObtenerDatosTextoAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga los bytes sin procesar desde la URL especificada en el campo 'Datos' de la respuesta (útil para imágenes o archivos binarios).
    /// </summary>
    /// <param name="respuesta">Respuesta devuelta previamente por la API de AEMET.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Matriz de bytes con los datos binarios descargados.</returns>
    Task<byte[]> ObtenerDatosBytesAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga y deserializa el objeto JSON desde la URL indicada en el campo 'Datos' de la respuesta.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto al que se deserializarán los datos JSON.</typeparam>
    /// <param name="respuesta">Respuesta devuelta previamente por la API de AEMET.</param>
    /// <param name="options">Opciones opcionales de serialización JSON.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>La instancia deserializada de tipo <typeparamref name="T"/>, o <c>null</c> si el contenido está vacío.</returns>
    Task<T?> ObtenerDatosJsonAsync<T>(RespuestaAemet respuesta, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga el contenido de metadatos desde la URL especificada en el campo 'Metadatos' de la respuesta.
    /// </summary>
    /// <param name="respuesta">Respuesta devuelta previamente por la API de AEMET.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>El contenido de texto de los metadatos descargados, o <c>null</c> si no se especificaron metadatos.</returns>
    Task<string?> ObtenerMetadatosTextoAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default);
}
