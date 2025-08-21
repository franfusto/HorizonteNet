# Org.OpenAPITools.Api.PrediccionesEspecificasApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**InformacionNivologica**](PrediccionesEspecificasApi.md#informacionnivologica) | **GET** /api/prediccion/especifica/nivologica/{area} | Información nivológica. |
| [**PrediccinDeMontaaTiempoActual**](PrediccionesEspecificasApi.md#prediccindemontaatiempoactual) | **GET** /api/prediccion/especifica/montaña/pasada/area/{area}/dia/{dia} | Predicción de montaña. Tiempo actual. |
| [**PrediccinDeMontaaTiempoPasado**](PrediccionesEspecificasApi.md#prediccindemontaatiempopasado) | **GET** /api/prediccion/especifica/montaña/pasada/area/{area} | Predicción de montaña. Tiempo pasado. |
| [**PrediccinDeRadiacinUltravioletaUVI**](PrediccionesEspecificasApi.md#prediccinderadiacinultravioletauvi) | **GET** /api/prediccion/especifica/uvi/{dia} | Predicción de radiación ultravioleta (UVI). |
| [**PrediccinParaLasPlayasTiempoActual**](PrediccionesEspecificasApi.md#prediccinparalasplayastiempoactual) | **GET** /api/prediccion/especifica/playa/{playa} | Predicción para las playas. Tiempo actual. |
| [**PrediccinPorMunicipiosDiariaTiempoActual**](PrediccionesEspecificasApi.md#prediccinpormunicipiosdiariatiempoactual) | **GET** /api/prediccion/especifica/municipio/diaria/{municipio} | Predicción por municipios diaria. Tiempo actual. |
| [**PrediccinPorMunicipiosHorariaTiempoActual**](PrediccionesEspecificasApi.md#prediccinpormunicipioshorariatiempoactual) | **GET** /api/prediccion/especifica/municipio/horaria/{municipio} | Predicción por municipios horaria. Tiempo actual. |

<a id="informacionnivologica"></a>
# **InformacionNivologica**
> Model200 InformacionNivologica (string area)

Información nivológica.

Información nivológica para la zona montañosa que se pasa como parámetro (area).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class InformacionNivologicaExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var area = "area_example";  // string |  | Código de  Área Montañosa |  Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | 0 | Pirineo Catalán  | | 1  | Pirineo Navarro y Aragonés

            try
            {
                // Información nivológica.
                Model200 result = apiInstance.InformacionNivologica(area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.InformacionNivologica: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the InformacionNivologicaWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Información nivológica.
    ApiResponse<Model200> response = apiInstance.InformacionNivologicaWithHttpInfo(area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.InformacionNivologicaWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código de  Área Montañosa |  Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | 0 | Pirineo Catalán  | | 1  | Pirineo Navarro y Aragonés |  |

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

<a id="prediccindemontaatiempoactual"></a>
# **PrediccinDeMontaaTiempoActual**
> Model200 PrediccinDeMontaaTiempoActual (string area, string dia)

Predicción de montaña. Tiempo actual.

Predicción meteorológica para la zona montañosa que se pasa como parámetro (area) con validez para el día (día).  Periodicidad de actualización: continuamente.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción montaña todos</li>         <li>Predicción montaña actual</li>         <li>Predicción montaña área</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinDeMontaaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var area = "area_example";  // string |  | Código de Área Montañosa | Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | peu1 | Picos de Europa   | | nav1  | Pirineo Navarro   | | arn1  | Pirineo Aragonés  | | cat1  | Pirineo Catalán   | | rio1  | Ibérica Riojana   | | arn2  | Ibérica Aragonesa   | | mad2  | Sierras de Guadarrama y Somosierra  | | gre1  | Sierra de Gredos   | | nev1  | Sierra Nevada
            var dia = "dia_example";  // string |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | 0 | día actual  | | 1  | d+1 (mañana)   | | 2  | d+2 (pasado mañana)  | | 3  | d+3 (siguente a pasado mañana)

            try
            {
                // Predicción de montaña. Tiempo actual.
                Model200 result = apiInstance.PrediccinDeMontaaTiempoActual(area, dia);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeMontaaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinDeMontaaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción de montaña. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinDeMontaaTiempoActualWithHttpInfo(area, dia);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeMontaaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código de Área Montañosa | Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | peu1 | Picos de Europa   | | nav1  | Pirineo Navarro   | | arn1  | Pirineo Aragonés  | | cat1  | Pirineo Catalán   | | rio1  | Ibérica Riojana   | | arn2  | Ibérica Aragonesa   | | mad2  | Sierras de Guadarrama y Somosierra  | | gre1  | Sierra de Gredos   | | nev1  | Sierra Nevada |  |
| **dia** | **string** |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | 0 | día actual  | | 1  | d+1 (mañana)   | | 2  | d+2 (pasado mañana)  | | 3  | d+3 (siguente a pasado mañana) |  |

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

<a id="prediccindemontaatiempopasado"></a>
# **PrediccinDeMontaaTiempoPasado**
> Model200 PrediccinDeMontaaTiempoPasado (string area)

Predicción de montaña. Tiempo pasado.

Breve resumen con lo más significativo de las condiciones meteorológicas registradas en la zona de montaña que se pasa como parámetro (area) en las últimas 24-36 horas.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción montaña todos</li>         <li>Predicción montaña pasado</li>         <li>Predicción montaña área</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinDeMontaaTiempoPasadoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var area = "area_example";  // string |  | Código de Área Montañosa | Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | peu1 | Picos de Europa   | | nav1  | Pirineo Navarro   | | arn1  | Pirineo Aragonés  | | cat1  | Pirineo Catalán   | | rio1  | Ibérica Riojana   | | arn2  | Ibérica Aragonesa   | | mad2  | Sierras de Guadarrama y Somosierra  | | gre1  | Sierra de Gredos   | | nev1  | Sierra Nevada

            try
            {
                // Predicción de montaña. Tiempo pasado.
                Model200 result = apiInstance.PrediccinDeMontaaTiempoPasado(area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeMontaaTiempoPasado: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinDeMontaaTiempoPasadoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción de montaña. Tiempo pasado.
    ApiResponse<Model200> response = apiInstance.PrediccinDeMontaaTiempoPasadoWithHttpInfo(area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeMontaaTiempoPasadoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código de Área Montañosa | Área Montañosa | |- -- -- -- -- -|- -- -- -- -- -| | peu1 | Picos de Europa   | | nav1  | Pirineo Navarro   | | arn1  | Pirineo Aragonés  | | cat1  | Pirineo Catalán   | | rio1  | Ibérica Riojana   | | arn2  | Ibérica Aragonesa   | | mad2  | Sierras de Guadarrama y Somosierra  | | gre1  | Sierra de Gredos   | | nev1  | Sierra Nevada |  |

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

<a id="prediccinderadiacinultravioletauvi"></a>
# **PrediccinDeRadiacinUltravioletaUVI**
> Model200 PrediccinDeRadiacinUltravioletaUVI (string dia)

Predicción de radiación ultravioleta (UVI).

Predicción de Índice de radiación UV máximo en condiciones de cielo despejado para el día seleccionado.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción de radiación ultravioleta (UVI)</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinDeRadiacinUltravioletaUVIExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var dia = "dia_example";  // string |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | 0 | día actual  | | 1  | d+1 (mañana)   | | 2  | d+2 (pasado mañana)  | | 3  | d+3 (dentro de 3 días) | | 4  | d+4 (dentro de 4 días)

            try
            {
                // Predicción de radiación ultravioleta (UVI).
                Model200 result = apiInstance.PrediccinDeRadiacinUltravioletaUVI(dia);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeRadiacinUltravioletaUVI: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinDeRadiacinUltravioletaUVIWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción de radiación ultravioleta (UVI).
    ApiResponse<Model200> response = apiInstance.PrediccinDeRadiacinUltravioletaUVIWithHttpInfo(dia);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinDeRadiacinUltravioletaUVIWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **dia** | **string** |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | 0 | día actual  | | 1  | d+1 (mañana)   | | 2  | d+2 (pasado mañana)  | | 3  | d+3 (dentro de 3 días) | | 4  | d+4 (dentro de 4 días) |  |

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

<a id="prediccinparalasplayastiempoactual"></a>
# **PrediccinParaLasPlayasTiempoActual**
> Model200 PrediccinParaLasPlayasTiempoActual (string playa)

Predicción para las playas. Tiempo actual.

La predicción diaria de la playa que se pasa como parámetro. Establece el estado de nubosidad para unas horas determinadas, las 11 y las 17 hora oficial. Se analiza también si se espera precipitación en el entorno de esas horas, entre las 08 y las 14 horas y entre las 14 y 20 horas.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción playa</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinParaLasPlayasTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var playa = "playa_example";  // string | <a href='https://www.aemet.es/documentos/es/eltiempo/prediccion/playas/Playas_codigos.csv' target='_blank'>Código de playa</a>

            try
            {
                // Predicción para las playas. Tiempo actual.
                Model200 result = apiInstance.PrediccinParaLasPlayasTiempoActual(playa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinParaLasPlayasTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinParaLasPlayasTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción para las playas. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinParaLasPlayasTiempoActualWithHttpInfo(playa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinParaLasPlayasTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **playa** | **string** | &lt;a href&#x3D;&#39;https://www.aemet.es/documentos/es/eltiempo/prediccion/playas/Playas_codigos.csv&#39; target&#x3D;&#39;_blank&#39;&gt;Código de playa&lt;/a&gt; |  |

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

<a id="prediccinpormunicipiosdiariatiempoactual"></a>
# **PrediccinPorMunicipiosDiariaTiempoActual**
> Model200 PrediccinPorMunicipiosDiariaTiempoActual (string municipio)

Predicción por municipios diaria. Tiempo actual.

Predicción diaria para el municipio que se pasa como parámetro (municipio). Periodicidad de actualización: continuamente.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción municipio</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinPorMunicipiosDiariaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var municipio = "municipio_example";  // string | <a href='https://www.ine.es/daco/daco42/codmun/diccionario24.xlsx' target='_blank'>Código de municipio</a>

            try
            {
                // Predicción por municipios diaria. Tiempo actual.
                Model200 result = apiInstance.PrediccinPorMunicipiosDiariaTiempoActual(municipio);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinPorMunicipiosDiariaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinPorMunicipiosDiariaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción por municipios diaria. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinPorMunicipiosDiariaTiempoActualWithHttpInfo(municipio);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinPorMunicipiosDiariaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **municipio** | **string** | &lt;a href&#x3D;&#39;https://www.ine.es/daco/daco42/codmun/diccionario24.xlsx&#39; target&#x3D;&#39;_blank&#39;&gt;Código de municipio&lt;/a&gt; |  |

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

<a id="prediccinpormunicipioshorariatiempoactual"></a>
# **PrediccinPorMunicipiosHorariaTiempoActual**
> Model200 PrediccinPorMunicipiosHorariaTiempoActual (string municipio)

Predicción por municipios horaria. Tiempo actual.

Predicción horaria para el municipio que se pasa como parámetro (municipio). Presenta la información de hora en hora hasta 48 horas.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Predicción municipio</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinPorMunicipiosHorariaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesEspecificasApi(config);
            var municipio = "municipio_example";  // string | <a href='https://www.ine.es/daco/daco42/codmun/diccionario24.xlsx' target='_blank'>Código de municipio</a>

            try
            {
                // Predicción por municipios horaria. Tiempo actual.
                Model200 result = apiInstance.PrediccinPorMunicipiosHorariaTiempoActual(municipio);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinPorMunicipiosHorariaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinPorMunicipiosHorariaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción por municipios horaria. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinPorMunicipiosHorariaTiempoActualWithHttpInfo(municipio);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesEspecificasApi.PrediccinPorMunicipiosHorariaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **municipio** | **string** | &lt;a href&#x3D;&#39;https://www.ine.es/daco/daco42/codmun/diccionario24.xlsx&#39; target&#x3D;&#39;_blank&#39;&gt;Código de municipio&lt;/a&gt; |  |

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

