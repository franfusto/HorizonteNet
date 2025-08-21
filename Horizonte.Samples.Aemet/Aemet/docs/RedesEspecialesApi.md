# Org.OpenAPITools.Api.RedesEspecialesApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ContenidoTotalDeOzonoTiempoActual**](RedesEspecialesApi.md#contenidototaldeozonotiempoactual) | **GET** /api/red/especial/ozono | Contenido total de ozono. Tiempo actual. |
| [**DatosDeContaminacinDeFondoTiempoActual**](RedesEspecialesApi.md#datosdecontaminacindefondotiempoactual) | **GET** /api/red/especial/contaminacionfondo/estacion/{nombre_estacion} | Datos de contaminación de fondo. Tiempo actual. |
| [**DatosDeRadiacinGlobalDirectaODifusaTiempoActual**](RedesEspecialesApi.md#datosderadiacinglobaldirectaodifusatiempoactual) | **GET** /api/red/especial/radiacion | Datos de radiación global, directa o difusa. Tiempo actual. |
| [**PerfilesVerticalesDeOzonoTiempoActual**](RedesEspecialesApi.md#perfilesverticalesdeozonotiempoactual) | **GET** /api/red/especial/perfilozono/estacion/{estacion} | Perfiles verticales de ozono. Tiempo actual. |

<a id="contenidototaldeozonotiempoactual"></a>
# **ContenidoTotalDeOzonoTiempoActual**
> Model200 ContenidoTotalDeOzonoTiempoActual ()

Contenido total de ozono. Tiempo actual.

Dato medio diario de contenido total de ozono. Cada 24 h (actualmente, en fines de semana, festivos y vacaciones no se genera por la falta de personal en el Centro Radiométrico Nacional).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ContenidoTotalDeOzonoTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedesEspecialesApi(config);

            try
            {
                // Contenido total de ozono. Tiempo actual.
                Model200 result = apiInstance.ContenidoTotalDeOzonoTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedesEspecialesApi.ContenidoTotalDeOzonoTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ContenidoTotalDeOzonoTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Contenido total de ozono. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.ContenidoTotalDeOzonoTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedesEspecialesApi.ContenidoTotalDeOzonoTiempoActualWithHttpInfo: " + e.Message);
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

<a id="datosdecontaminacindefondotiempoactual"></a>
# **DatosDeContaminacinDeFondoTiempoActual**
> Model200 DatosDeContaminacinDeFondoTiempoActual (string nombreEstacion)

Datos de contaminación de fondo. Tiempo actual.

Ficheros diarios con datos diezminutales de la estación de la red de contaminación de fondo EMEP/VAG/CAMP pasada por parámetro, de temperatura, presión, humedad, viento (dirección y velocidad), radiación global, precipitación y 4 componentes químicos: O3,SO2,NO,NO2 y PM10. Los datos se encuentran en formato FINN (propio del Ministerio de Medio Ambiente). Periodicidad: cada hora.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class DatosDeContaminacinDeFondoTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedesEspecialesApi(config);
            var nombreEstacion = "nombreEstacion_example";  // string |  | Código | Estación de la Red EMEP | |- -- -- -- -- -|- -- -- -- -- -| | 11  | Barcarrota (Badajoz)   | | 10  | Cabo de Creus (Girona)   | | 09  | Campisábalos (Guadalajara)   | | 17  | Doñana (Huelva)  | | 14  | Els Torms (Lleida)   | | 06  | Mahón (Illes Balears)   | | 08  | Niembro-Llanes (Asturias)   | | 05  | Noia (A Coruña)   | | 16  | O Saviñao (Lugo)   | | 13  | Peñausende (Zamora)   | | 01  | San Pablo de los Montes (Toledo)   | | 07  | Víznar (Granada)   | | 12  | Zarra (Valencia) 

            try
            {
                // Datos de contaminación de fondo. Tiempo actual.
                Model200 result = apiInstance.DatosDeContaminacinDeFondoTiempoActual(nombreEstacion);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedesEspecialesApi.DatosDeContaminacinDeFondoTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DatosDeContaminacinDeFondoTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Datos de contaminación de fondo. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.DatosDeContaminacinDeFondoTiempoActualWithHttpInfo(nombreEstacion);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedesEspecialesApi.DatosDeContaminacinDeFondoTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **nombreEstacion** | **string** |  | Código | Estación de la Red EMEP | |- -- -- -- -- -|- -- -- -- -- -| | 11  | Barcarrota (Badajoz)   | | 10  | Cabo de Creus (Girona)   | | 09  | Campisábalos (Guadalajara)   | | 17  | Doñana (Huelva)  | | 14  | Els Torms (Lleida)   | | 06  | Mahón (Illes Balears)   | | 08  | Niembro-Llanes (Asturias)   | | 05  | Noia (A Coruña)   | | 16  | O Saviñao (Lugo)   | | 13  | Peñausende (Zamora)   | | 01  | San Pablo de los Montes (Toledo)   | | 07  | Víznar (Granada)   | | 12  | Zarra (Valencia)  |  |

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

<a id="datosderadiacinglobaldirectaodifusatiempoactual"></a>
# **DatosDeRadiacinGlobalDirectaODifusaTiempoActual**
> Model200 DatosDeRadiacinGlobalDirectaODifusaTiempoActual ()

Datos de radiación global, directa o difusa. Tiempo actual.

Datos horarios (HORA SOLAR VERDADERA) acumulados de radiación  global, directa, difusa e infrarroja, y datos semihorarios  (HORA SOLAR VERDADERA) acumulados de radiación ultravioleta eritemática.Datos diarios acumulados  de radiación global, directa, difusa, ultravioleta eritemática e infrarroja. Periodicidad: Cada 24h (actualmente en fines de semana, festivos y vacaciones, no se genera por la ausencia de personal en el Centro Radiométrico Nacional).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class DatosDeRadiacinGlobalDirectaODifusaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedesEspecialesApi(config);

            try
            {
                // Datos de radiación global, directa o difusa. Tiempo actual.
                Model200 result = apiInstance.DatosDeRadiacinGlobalDirectaODifusaTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedesEspecialesApi.DatosDeRadiacinGlobalDirectaODifusaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DatosDeRadiacinGlobalDirectaODifusaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Datos de radiación global, directa o difusa. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.DatosDeRadiacinGlobalDirectaODifusaTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedesEspecialesApi.DatosDeRadiacinGlobalDirectaODifusaTiempoActualWithHttpInfo: " + e.Message);
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

<a id="perfilesverticalesdeozonotiempoactual"></a>
# **PerfilesVerticalesDeOzonoTiempoActual**
> Model200 PerfilesVerticalesDeOzonoTiempoActual (string estacion)

Perfiles verticales de ozono. Tiempo actual.

Perfil Vertical de Ozono de la estación pasada por parámetro. Periodicidad: cada 7 días.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PerfilesVerticalesDeOzonoTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedesEspecialesApi(config);
            var estacion = "estacion_example";  // string |  | Código | Estación | |- -- -- -- -- -|- -- -- -- -- -| | canarias  | Izaña   | | peninsula  | Madrid   

            try
            {
                // Perfiles verticales de ozono. Tiempo actual.
                Model200 result = apiInstance.PerfilesVerticalesDeOzonoTiempoActual(estacion);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedesEspecialesApi.PerfilesVerticalesDeOzonoTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PerfilesVerticalesDeOzonoTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Perfiles verticales de ozono. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PerfilesVerticalesDeOzonoTiempoActualWithHttpInfo(estacion);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedesEspecialesApi.PerfilesVerticalesDeOzonoTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **estacion** | **string** |  | Código | Estación | |- -- -- -- -- -|- -- -- -- -- -| | canarias  | Izaña   | | peninsula  | Madrid    |  |

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

