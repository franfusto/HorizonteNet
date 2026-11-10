using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenDataAemet.Interfaces;
using OpenDataAemet.Models;

namespace OpenDataAemet.Services;

/// <summary>
/// Implementación de <see cref="IAemetClient"/> para interactuar con la API REST de AEMET OpenData.
/// </summary>
public class AemetClient : IAemetClient, IDisposable
{
    static AemetClient()
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch
        {
            // Ignorar si el proveedor ya fue registrado o no está soportado en este entorno
        }
    }

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AemetOptions Options { get; }

    public IAvisosCapService AvisosCap { get; }
    public IIndicesIncendiosService IndicesIncendios { get; }
    public IMaestroService Maestro { get; }
    public IMapasYGraficosService MapasYGraficos { get; }
    public IObservacionConvencionalService ObservacionConvencional { get; }
    public IPrediccionesEspecificasService PrediccionesEspecificas { get; }
    public IPrediccionesTextoService PrediccionesTexto { get; }
    public IPrediccionMaritimaService PrediccionMaritima { get; }
    public IProductosClimatologicosService ProductosClimatologicos { get; }
    public IRedesEspecialesService RedesEspeciales { get; }
    public IRedRadaresService RedRadares { get; }
    public IRedRayosService RedRayos { get; }
    public IInformacionSateliteService InformacionSatelite { get; }
    public IValoresClimatologicosService ValoresClimatologicos { get; }
    public IAntartidaService Antartida { get; }

    /// <summary>
    /// Crea una nueva instancia de <see cref="AemetClient"/> con la clave de API indicada.
    /// </summary>
    public AemetClient(string apiKey) : this(new AemetOptions { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Crea una nueva instancia de <see cref="AemetClient"/> con las opciones indicadas.
    /// </summary>
    public AemetClient(AemetOptions options) : this(new HttpClient(), options, disposeHttpClient: true)
    {
    }

    /// <summary>
    /// Crea una nueva instancia de <see cref="AemetClient"/> usando un <see cref="HttpClient"/> personalizado.
    /// </summary>
    public AemetClient(HttpClient httpClient, AemetOptions options, bool disposeHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _disposeHttpClient = disposeHttpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(Options.BaseUrl))
        {
            try
            {
                _httpClient.BaseAddress = new Uri(Options.BaseUrl.TrimEnd('/') + "/");
            }
            catch (InvalidOperationException)
            {
                // HttpClient ya ha iniciado peticiones, se construirá la URI completa en cada llamada si es necesario
            }
        }

        if (Options.Timeout > TimeSpan.Zero)
        {
            try
            {
                _httpClient.Timeout = Options.Timeout;
            }
            catch (InvalidOperationException)
            {
                // Ignorar si HttpClient ya fue iniciado
            }
        }

        // Inicializar los subservicios
        AvisosCap = new AvisosCapService(this);
        IndicesIncendios = new IndicesIncendiosService(this);
        Maestro = new MaestroService(this);
        MapasYGraficos = new MapasYGraficosService(this);
        ObservacionConvencional = new ObservacionConvencionalService(this);
        PrediccionesEspecificas = new PrediccionesEspecificasService(this);
        PrediccionesTexto = new PrediccionesTextoService(this);
        PrediccionMaritima = new PrediccionMaritimaService(this);
        ProductosClimatologicos = new ProductosClimatologicosService(this);
        RedesEspeciales = new RedesEspecialesService(this);
        RedRadares = new RedRadaresService(this);
        RedRayos = new RedRayosService(this);
        InformacionSatelite = new InformacionSateliteService(this);
        ValoresClimatologicos = new ValoresClimatologicosService(this);
        Antartida = new AntartidaService(this);
    }

    /// <inheritdoc/>
    public async Task<RespuestaAemet> EnviarPeticionAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            throw new ArgumentException("La URL relativa no puede ser nula ni vacía.", nameof(relativeUrl));

        string url = relativeUrl.TrimStart('/');

        // Construir la URL si la API Key va por QueryString
        if (Options.UbicacionApiKey == ApiKeyLocation.QueryString && !string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            char separator = url.Contains('?') ? '&' : '?';
            url = $"{url}{separator}api_key={Uri.EscapeDataString(Options.ApiKey)}";
        }

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(Options.BaseUrl))
        {
            url = $"{Options.BaseUrl.TrimEnd('/')}/{url}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Añadir cabecera si la API Key va por Header
        if (Options.UbicacionApiKey == ApiKeyLocation.Header && !string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation("api_key", Options.ApiKey);
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AemetException($"Error de red al consultar el endpoint '{relativeUrl}': {ex.Message}", ex);
        }

        string responseBody = await LeerContenidoTextoAsync(response.Content, cancellationToken).ConfigureAwait(false);

        RespuestaAemet? respuesta = null;
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                respuesta = JsonSerializer.Deserialize<RespuestaAemet>(responseBody, _jsonOptions);
            }
            catch (JsonException)
            {
                // Si la respuesta no es JSON válido
            }
        }

        if (respuesta == null)
        {
            respuesta = new RespuestaAemet
            {
                Estado = (int)response.StatusCode,
                Descripcion = response.ReasonPhrase ?? responseBody
            };
        }

        if (!response.IsSuccessStatusCode && respuesta.Estado == 200)
        {
            respuesta.Estado = (int)response.StatusCode;
        }

        return respuesta;
    }

    /// <inheritdoc/>
    public async Task<string> ObtenerDatosTextoAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(respuesta);

        if (string.IsNullOrWhiteSpace(respuesta.Datos))
        {
            throw new AemetException("La respuesta no contiene una URL de datos válida.", respuesta.Estado, respuesta.Descripcion);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, respuesta.Datos);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await LeerContenidoTextoAsync(response.Content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && ex is not AemetException)
        {
            throw new AemetException($"Error al descargar los datos desde '{respuesta.Datos}': {ex.Message}", ex, respuesta.Estado, respuesta.Descripcion);
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> ObtenerDatosBytesAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(respuesta);

        if (string.IsNullOrWhiteSpace(respuesta.Datos))
        {
            throw new AemetException("La respuesta no contiene una URL de datos válida.", respuesta.Estado, respuesta.Descripcion);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, respuesta.Datos);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && ex is not AemetException)
        {
            throw new AemetException($"Error al descargar los bytes desde '{respuesta.Datos}': {ex.Message}", ex, respuesta.Estado, respuesta.Descripcion);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> ObtenerDatosJsonAsync<T>(RespuestaAemet respuesta, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        string jsonText = await ObtenerDatosTextoAsync(respuesta, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(jsonText, options ?? _jsonOptions);
    }

    /// <inheritdoc/>
    public async Task<string?> ObtenerMetadatosTextoAsync(RespuestaAemet respuesta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(respuesta);

        if (string.IsNullOrWhiteSpace(respuesta.Metadatos))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, respuesta.Metadatos);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await LeerContenidoTextoAsync(response.Content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AemetException($"Error al descargar los metadatos desde '{respuesta.Metadatos}': {ex.Message}", ex, respuesta.Estado, respuesta.Descripcion);
        }
    }

    private static async Task<string> LeerContenidoTextoAsync(HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            return await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            byte[] bytes = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            string? charset = content.Headers.ContentType?.CharSet;
            if (!string.IsNullOrWhiteSpace(charset))
            {
                try
                {
                    var encoding = Encoding.GetEncoding(charset.Trim('"', '\''));
                    return encoding.GetString(bytes);
                }
                catch
                {
                    // Fallback
                }
            }

            try
            {
                var iso885915 = Encoding.GetEncoding("ISO-8859-15");
                return iso885915.GetString(bytes);
            }
            catch
            {
                try
                {
                    return Encoding.Latin1.GetString(bytes);
                }
                catch
                {
                    return Encoding.UTF8.GetString(bytes);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
