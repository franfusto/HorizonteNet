# Org.OpenAPITools.Api.ValoresClimatologicosApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ClimatologasDiarias**](ValoresClimatologicosApi.md#climatologasdiarias) | **GET** /api/valores/climatologicos/diarios/datos/fechaini/{fechaIniStr}/fechafin/{fechaFinStr}/estacion/{idema} | Climatologías diarias. |
| [**ClimatologasDiarias1**](ValoresClimatologicosApi.md#climatologasdiarias1) | **GET** /api/valores/climatologicos/diarios/datos/fechaini/{fechaIniStr}/fechafin/{fechaFinStr}/todasestaciones | Climatologías diarias. |
| [**ClimatologasMensualesAnuales**](ValoresClimatologicosApi.md#climatologasmensualesanuales) | **GET** /api/valores/climatologicos/mensualesanuales/datos/anioini/{anioIniStr}/aniofin/{anioFinStr}/estacion/{idema} | Climatologías mensuales anuales. |
| [**ClimatologasNormales19912020**](ValoresClimatologicosApi.md#climatologasnormales19912020) | **GET** /api/valores/climatologicos/normales/estacion/{idema} | Climatologías normales (1991-2020). |
| [**EstacionesPorIndicativo**](ValoresClimatologicosApi.md#estacionesporindicativo) | **GET** /api/valores/climatologicos/inventarioestaciones/estaciones/{estaciones} | Estaciones por indicativo. |
| [**InventarioDeEstacionesValoresClimatolgicos**](ValoresClimatologicosApi.md#inventariodeestacionesvaloresclimatolgicos) | **GET** /api/valores/climatologicos/inventarioestaciones/todasestaciones | Inventario de estaciones (valores climatológicos). |
| [**ValoresExtremos**](ValoresClimatologicosApi.md#valoresextremos) | **GET** /api/valores/climatologicos/valoresextremos/parametro/{parametro}/estacion/{idema} | Valores extremos. |

<a id="climatologasdiarias"></a>
# **ClimatologasDiarias**
> Model200 ClimatologasDiarias (string fechaIniStr, string fechaFinStr, string idema)

Climatologías diarias.

Valores climatológicos para el rango de fechas y la estación seleccionada. Periodicidad: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ClimatologasDiariasExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var fechaIniStr = "fechaIniStr_example";  // string | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC)
            var fechaFinStr = "fechaFinStr_example";  // string | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC)
            var idema = "idema_example";  // string | Indicativo climatológico de la EMA. Puede introducir varios indicativos separados por comas (,)

            try
            {
                // Climatologías diarias.
                Model200 result = apiInstance.ClimatologasDiarias(fechaIniStr, fechaFinStr, idema);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasDiarias: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ClimatologasDiariasWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Climatologías diarias.
    ApiResponse<Model200> response = apiInstance.ClimatologasDiariasWithHttpInfo(fechaIniStr, fechaFinStr, idema);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasDiariasWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fechaIniStr** | **string** | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **fechaFinStr** | **string** | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **idema** | **string** | Indicativo climatológico de la EMA. Puede introducir varios indicativos separados por comas (,) |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="climatologasdiarias1"></a>
# **ClimatologasDiarias1**
> Model200 ClimatologasDiarias1 (string fechaIniStr, string fechaFinStr)

Climatologías diarias.

Valores climatológicos de todas las estaciones para el rango de fechas seleccionado. Periodicidad: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ClimatologasDiarias1Example
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var fechaIniStr = "fechaIniStr_example";  // string | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC)
            var fechaFinStr = "fechaFinStr_example";  // string | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC)

            try
            {
                // Climatologías diarias.
                Model200 result = apiInstance.ClimatologasDiarias1(fechaIniStr, fechaFinStr);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasDiarias1: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ClimatologasDiarias1WithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Climatologías diarias.
    ApiResponse<Model200> response = apiInstance.ClimatologasDiarias1WithHttpInfo(fechaIniStr, fechaFinStr);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasDiarias1WithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fechaIniStr** | **string** | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **fechaFinStr** | **string** | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC) |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="climatologasmensualesanuales"></a>
# **ClimatologasMensualesAnuales**
> Model200 ClimatologasMensualesAnuales (string anioIniStr, string anioFinStr, string idema)

Climatologías mensuales anuales.

Valores medios mensuales y anuales de los datos climatológicos para la estación y el periodo de años pasados por parámetro. Periodicidad de actualización: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ClimatologasMensualesAnualesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var anioIniStr = "anioIniStr_example";  // string | Año Inicial (AAAA)
            var anioFinStr = "anioFinStr_example";  // string | Año Final (AAAA)
            var idema = "idema_example";  // string | Indicativo climatológico de la EMA

            try
            {
                // Climatologías mensuales anuales.
                Model200 result = apiInstance.ClimatologasMensualesAnuales(anioIniStr, anioFinStr, idema);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasMensualesAnuales: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ClimatologasMensualesAnualesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Climatologías mensuales anuales.
    ApiResponse<Model200> response = apiInstance.ClimatologasMensualesAnualesWithHttpInfo(anioIniStr, anioFinStr, idema);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasMensualesAnualesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **anioIniStr** | **string** | Año Inicial (AAAA) |  |
| **anioFinStr** | **string** | Año Final (AAAA) |  |
| **idema** | **string** | Indicativo climatológico de la EMA |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="climatologasnormales19912020"></a>
# **ClimatologasNormales19912020**
> Model200 ClimatologasNormales19912020 (string idema)

Climatologías normales (1991-2020).

Valores climatológicos normales (periodo 1991-2020) para la estación pasada por parámetro. Periodicidad: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ClimatologasNormales19912020Example
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var idema = "idema_example";  // string | Indicativo climatológico de la EMA

            try
            {
                // Climatologías normales (1991-2020).
                Model200 result = apiInstance.ClimatologasNormales19912020(idema);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasNormales19912020: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ClimatologasNormales19912020WithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Climatologías normales (1991-2020).
    ApiResponse<Model200> response = apiInstance.ClimatologasNormales19912020WithHttpInfo(idema);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.ClimatologasNormales19912020WithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **idema** | **string** | Indicativo climatológico de la EMA |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="estacionesporindicativo"></a>
# **EstacionesPorIndicativo**
> Model200 EstacionesPorIndicativo (string estaciones)

Estaciones por indicativo.

Características de la estación climatológica pasada por parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class EstacionesPorIndicativoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var estaciones = "estaciones_example";  // string | Listado de indicativos climatológicos (id1,id2,id3,...,idn)

            try
            {
                // Estaciones por indicativo.
                Model200 result = apiInstance.EstacionesPorIndicativo(estaciones);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.EstacionesPorIndicativo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EstacionesPorIndicativoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Estaciones por indicativo.
    ApiResponse<Model200> response = apiInstance.EstacionesPorIndicativoWithHttpInfo(estaciones);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.EstacionesPorIndicativoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **estaciones** | **string** | Listado de indicativos climatológicos (id1,id2,id3,...,idn) |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="inventariodeestacionesvaloresclimatolgicos"></a>
# **InventarioDeEstacionesValoresClimatolgicos**
> Model200 InventarioDeEstacionesValoresClimatolgicos ()

Inventario de estaciones (valores climatológicos).

Inventario con las características de todas las estaciones climatológicas. Periodicidad: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class InventarioDeEstacionesValoresClimatolgicosExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);

            try
            {
                // Inventario de estaciones (valores climatológicos).
                Model200 result = apiInstance.InventarioDeEstacionesValoresClimatolgicos();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.InventarioDeEstacionesValoresClimatolgicos: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the InventarioDeEstacionesValoresClimatolgicosWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Inventario de estaciones (valores climatológicos).
    ApiResponse<Model200> response = apiInstance.InventarioDeEstacionesValoresClimatolgicosWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.InventarioDeEstacionesValoresClimatolgicosWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="valoresextremos"></a>
# **ValoresExtremos**
> Model200 ValoresExtremos (string parametro, string idema)

Valores extremos.

Valores extremos para la estación y la variable (precipitación, temperatura y viento) pasadas por parámetro. Periodicidad: 1 vez al día.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ValoresExtremosExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ValoresClimatologicosApi(config);
            var parametro = "parametro_example";  // string |  | Código | Parámetro Meteorológico | |- -- -- -- -- -|- -- -- -- -- -| | P  | Precipitación   | | T  | Temperatura   | | V  | Viento 
            var idema = "idema_example";  // string | Indicativo climatológico de la EMA

            try
            {
                // Valores extremos.
                Model200 result = apiInstance.ValoresExtremos(parametro, idema);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ValoresClimatologicosApi.ValoresExtremos: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ValoresExtremosWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Valores extremos.
    ApiResponse<Model200> response = apiInstance.ValoresExtremosWithHttpInfo(parametro, idema);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ValoresClimatologicosApi.ValoresExtremosWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **parametro** | **string** |  | Código | Parámetro Meteorológico | |- -- -- -- -- -|- -- -- -- -- -| | P  | Precipitación   | | T  | Temperatura   | | V  | Viento  |  |
| **idema** | **string** | Indicativo climatológico de la EMA |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

