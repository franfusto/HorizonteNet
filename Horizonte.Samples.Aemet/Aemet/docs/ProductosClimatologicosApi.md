# Org.OpenAPITools.Api.ProductosClimatologicosApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**BalanceHdricoNacionalDocumento**](ProductosClimatologicosApi.md#balancehdriconacionaldocumento) | **GET** /api/productos/climatologicos/balancehidrico/{anio}/{decena} | Balance hídrico nacional (documento). |
| [**CapasSHAPEDeEstacionesClimatolgicas**](ProductosClimatologicosApi.md#capasshapedeestacionesclimatolgicas) | **GET** /api/productos/climatologicos/capasshape/{tipoestacion} | Capas SHAPE de estaciones climatológicas de AEMET. |
| [**ResumenMensualClimatolgicoNacionalDocumento**](ProductosClimatologicosApi.md#resumenmensualclimatolgiconacionaldocumento) | **GET** /api/productos/climatologicos/resumenclimatologico/nacional/{anio}/{mes} | Resumen mensual climatológico nacional (documento). |

<a id="balancehdriconacionaldocumento"></a>
# **BalanceHdricoNacionalDocumento**
> Model200 BalanceHdricoNacionalDocumento (string anio, string decena)

Balance hídrico nacional (documento).

Se obtiene, para la decema y el año pasados por parámetro, el Boletín Hídrico Nacional que se elabora cada diez días. Se presenta información resumida de forma distribuida para todo el territorio nacional de diferentes variables, en las que se incluye informaciones de la precipitación y la evapotranspiración potencial acumuladas desde el 1 de septiembre.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class BalanceHdricoNacionalDocumentoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ProductosClimatologicosApi(config);
            var anio = "anio_example";  // string | Año (AAAA)
            var decena = "decena_example";  // string | Decena de 01 (primera decena) a 36 (última decena)

            try
            {
                // Balance hídrico nacional (documento).
                Model200 result = apiInstance.BalanceHdricoNacionalDocumento(anio, decena);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProductosClimatologicosApi.BalanceHdricoNacionalDocumento: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the BalanceHdricoNacionalDocumentoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Balance hídrico nacional (documento).
    ApiResponse<Model200> response = apiInstance.BalanceHdricoNacionalDocumentoWithHttpInfo(anio, decena);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProductosClimatologicosApi.BalanceHdricoNacionalDocumentoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **anio** | **string** | Año (AAAA) |  |
| **decena** | **string** | Decena de 01 (primera decena) a 36 (última decena) |  |

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

<a id="capasshapedeestacionesclimatolgicas"></a>
# **CapasSHAPEDeEstacionesClimatolgicas**
> Model200 CapasSHAPEDeEstacionesClimatolgicas (string tipoestacion)

Capas SHAPE de estaciones climatológicas de AEMET.

Capas SHAPE de las distintas estaciones climatológicas de AEMET: automáticas, completas, pluviométricas y termométricas.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class CapasSHAPEDeEstacionesClimatolgicasExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ProductosClimatologicosApi(config);
            var tipoestacion = "tipoestacion_example";  // string |  | Código | Tipo de Estación | |- -- -- -- -- -|- -- -- -- -- -| | automaticas  | Estaciones Automáticas   | | completas  | Estaciones Completas   | | pluviometricas  | Estaciones Pluviométricas   | | termometricas  | Estaciones Termométricas   

            try
            {
                // Capas SHAPE de estaciones climatológicas de AEMET.
                Model200 result = apiInstance.CapasSHAPEDeEstacionesClimatolgicas(tipoestacion);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProductosClimatologicosApi.CapasSHAPEDeEstacionesClimatolgicas: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the CapasSHAPEDeEstacionesClimatolgicasWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Capas SHAPE de estaciones climatológicas de AEMET.
    ApiResponse<Model200> response = apiInstance.CapasSHAPEDeEstacionesClimatolgicasWithHttpInfo(tipoestacion);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProductosClimatologicosApi.CapasSHAPEDeEstacionesClimatolgicasWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tipoestacion** | **string** |  | Código | Tipo de Estación | |- -- -- -- -- -|- -- -- -- -- -| | automaticas  | Estaciones Automáticas   | | completas  | Estaciones Completas   | | pluviometricas  | Estaciones Pluviométricas   | | termometricas  | Estaciones Termométricas    |  |

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

<a id="resumenmensualclimatolgiconacionaldocumento"></a>
# **ResumenMensualClimatolgicoNacionalDocumento**
> Model200 ResumenMensualClimatolgicoNacionalDocumento (string anio, string mes)

Resumen mensual climatológico nacional (documento).

Resumen climatológico nacional, para el año y mes pasado por parámetro, sobre el estado del clima y la evolución de las principales variables climáticas, en especial temperatura y precipitación, a nivel mensual, estacional y anual.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ResumenMensualClimatolgicoNacionalDocumentoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ProductosClimatologicosApi(config);
            var anio = "anio_example";  // string | Año (AAAA)
            var mes = "mes_example";  // string | Mes (mm)

            try
            {
                // Resumen mensual climatológico nacional (documento).
                Model200 result = apiInstance.ResumenMensualClimatolgicoNacionalDocumento(anio, mes);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProductosClimatologicosApi.ResumenMensualClimatolgicoNacionalDocumento: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ResumenMensualClimatolgicoNacionalDocumentoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Resumen mensual climatológico nacional (documento).
    ApiResponse<Model200> response = apiInstance.ResumenMensualClimatolgicoNacionalDocumentoWithHttpInfo(anio, mes);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProductosClimatologicosApi.ResumenMensualClimatolgicoNacionalDocumentoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **anio** | **string** | Año (AAAA) |  |
| **mes** | **string** | Mes (mm) |  |

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

