# Org.OpenAPITools.Api.IndicesIncendiosApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestales**](IndicesIncendiosApi.md#mapadenivelesderiesgoestimadometeorolgicodeincendiosforestales) | **GET** /api/incendios/mapasriesgo/estimado/area/{area} | Mapa de niveles de riesgo estimado meteorológico de incendios forestales. |
| [**MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestales**](IndicesIncendiosApi.md#mapadenivelesderiesgoprevistometeorolgicodeincendiosforestales) | **GET** /api/incendios/mapasriesgo/previsto/dia/{dia}/area/{area} | Mapa de niveles de riesgo previsto meteorológico de incendios forestales. |

<a id="mapadenivelesderiesgoestimadometeorolgicodeincendiosforestales"></a>
# **MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestales**
> Model200 MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestales (string area)

Mapa de niveles de riesgo estimado meteorológico de incendios forestales.

Último mapa elaborado de niveles de riesgo estimado meteorológico de incendios forestales para el área pasada por parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestalesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new IndicesIncendiosApi(config);
            var area = "area_example";  // string |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | p  | Península y Baleares  | | c  | Canarias   

            try
            {
                // Mapa de niveles de riesgo estimado meteorológico de incendios forestales.
                Model200 result = apiInstance.MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestales(area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling IndicesIncendiosApi.MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestales: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestalesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mapa de niveles de riesgo estimado meteorológico de incendios forestales.
    ApiResponse<Model200> response = apiInstance.MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestalesWithHttpInfo(area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling IndicesIncendiosApi.MapaDeNivelesDeRiesgoEstimadoMeteorolgicoDeIncendiosForestalesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | p  | Península y Baleares  | | c  | Canarias    |  |

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

<a id="mapadenivelesderiesgoprevistometeorolgicodeincendiosforestales"></a>
# **MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestales**
> Model200 MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestales (string dia, string area)

Mapa de niveles de riesgo previsto meteorológico de incendios forestales.

Mapa elaborado de niveles de riesgo estimado meteorológico de incendios forestales para el día y el área pasados por parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestalesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new IndicesIncendiosApi(config);
            var dia = "dia_example";  // string |  | Código | Día | |- -- -- -- -- -|- -- -- -- -- -| | 1  | Mañana   | | 2  | Pasado Mañana   | | 3  | Dentro de 3 días   | | 4  | Dentro de 4 días   | | 5  | Dentro de 5 días   | | 6  | Dentro de 6 días   | | 7  | Dentro de 7 días   
            var area = "area_example";  // string |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | p  | Península y Baleares  | | c  | Canarias   

            try
            {
                // Mapa de niveles de riesgo previsto meteorológico de incendios forestales.
                Model200 result = apiInstance.MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestales(dia, area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling IndicesIncendiosApi.MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestales: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestalesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mapa de niveles de riesgo previsto meteorológico de incendios forestales.
    ApiResponse<Model200> response = apiInstance.MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestalesWithHttpInfo(dia, area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling IndicesIncendiosApi.MapaDeNivelesDeRiesgoPrevistoMeteorolgicoDeIncendiosForestalesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **dia** | **string** |  | Código | Día | |- -- -- -- -- -|- -- -- -- -- -| | 1  | Mañana   | | 2  | Pasado Mañana   | | 3  | Dentro de 3 días   | | 4  | Dentro de 4 días   | | 5  | Dentro de 5 días   | | 6  | Dentro de 6 días   | | 7  | Dentro de 7 días    |  |
| **area** | **string** |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | p  | Península y Baleares  | | c  | Canarias    |  |

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

