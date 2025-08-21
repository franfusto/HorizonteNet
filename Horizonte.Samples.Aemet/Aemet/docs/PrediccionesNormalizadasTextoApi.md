# Org.OpenAPITools.Api.PrediccionesNormalizadasTextoApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**PrediccinCCAAHoyArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinccaahoyarchivo) | **GET** /api/prediccion/ccaa/hoy/{ccaa}/elaboracion/{fecha} | Predicción CCAA hoy. Archivo. |
| [**PrediccinCCAAHoyTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinccaahoytiempoactual) | **GET** /api/prediccion/ccaa/hoy/{ccaa} | Predicción CCAA hoy. Tiempo actual. |
| [**PrediccinCCAAMaanaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinccaamaanaarchivo) | **GET** /api/prediccion/ccaa/manana/{ccaa}/elaboracion/{fecha} | Predicción CCAA mañana. Archivo. |
| [**PrediccinCCAAMaanaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinccaamaanatiempoactual) | **GET** /api/prediccion/ccaa/manana/{ccaa} | Predicción CCAA mañana. Tiempo actual. |
| [**PrediccinCCAAMedioPlazoArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinccaamedioplazoarchivo) | **GET** /api/prediccion/ccaa/medioplazo/{ccaa}/elaboracion/{fecha} | Predicción CCAA medio plazo. Archivo. |
| [**PrediccinCCAAMedioPlazoTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinccaamedioplazotiempoactual) | **GET** /api/prediccion/ccaa/medioplazo/{ccaa} | Predicción CCAA medio plazo. Tiempo actual. |
| [**PrediccinCCAAPasadoMaanaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinccaapasadomaanaarchivo) | **GET** /api/prediccion/ccaa/pasadomanana/{ccaa}/elaboracion/{fecha} | Predicción CCAA pasado mañana. Archivo. |
| [**PrediccinCCAAPasadoMaanaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinccaapasadomaanatiempoactual) | **GET** /api/prediccion/ccaa/pasadomanana/{ccaa} | Predicción CCAA pasado mañana. Tiempo actual. |
| [**PrediccinNacionalHoyArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalhoyarchivo) | **GET** /api/prediccion/nacional/hoy/elaboracion/{fecha} | Predicción nacional hoy. Archivo. |
| [**PrediccinNacionalHoyTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalhoytiempoactual) | **GET** /api/prediccion/nacional/hoy | Predicción nacional hoy. Última elaborada. |
| [**PrediccinNacionalMaanaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalmaanaarchivo) | **GET** /api/prediccion/nacional/manana/elaboracion/{fecha} | Predicción nacional mañana. Archivo. |
| [**PrediccinNacionalMaanaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalmaanatiempoactual) | **GET** /api/prediccion/nacional/manana | Predicción nacional mañana. Tiempo actual. |
| [**PrediccinNacionalMedioPlazoArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalmedioplazoarchivo) | **GET** /api/prediccion/nacional/medioplazo/elaboracion/{fecha} | Predicción nacional medio plazo. Archivo. |
| [**PrediccinNacionalMedioPlazoTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalmedioplazotiempoactual) | **GET** /api/prediccion/nacional/medioplazo | Predicción nacional medio plazo. Tiempo actual. |
| [**PrediccinNacionalPasadoMaanaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalpasadomaanaarchivo) | **GET** /api/prediccion/nacional/pasadomanana/elaboracion/{fecha} | Predicción nacional pasado mañana. Archivo. |
| [**PrediccinNacionalPasadoMaanaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinnacionalpasadomaanatiempoactual) | **GET** /api/prediccion/nacional/pasadomanana | Predicción nacional pasado mañana. Tiempo actual. |
| [**PrediccinNacionalTendenciaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinnacionaltendenciaarchivo) | **GET** /api/prediccion/nacional/tendencia/elaboracion/{fecha} | Predicción nacional tendencia. Archivo. |
| [**PrediccinNacionalTendenciaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinnacionaltendenciatiempoactual) | **GET** /api/prediccion/nacional/tendencia | Predicción nacional tendencia. Tiempo actual. |
| [**PrediccinProvincialEInsularHoyArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinprovincialeinsularhoyarchivo) | **GET** /api/prediccion/provincia/hoy/{provincia}/elaboracion/{fecha} | Predicción provincial e insular hoy. Archivo. |
| [**PrediccinProvincialEInsularHoyTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinprovincialeinsularhoytiempoactual) | **GET** /api/prediccion/provincia/hoy/{provincia} | Predicción provincial e insular hoy. Tiempo actual. |
| [**PrediccinProvincialEInsularMaanaTiempoActual**](PrediccionesNormalizadasTextoApi.md#prediccinprovincialeinsularmaanatiempoactual) | **GET** /api/prediccion/provincia/manana/{provincia} | Predicción provincial e insular mañana. Tiempo actual. |
| [**PrediccinProvincialOInsularMaanaArchivo**](PrediccionesNormalizadasTextoApi.md#prediccinprovincialoinsularmaanaarchivo) | **GET** /api/prediccion/provincia/manana/{provincia}/elaboracion/{fecha} | Predicción provincial e insular mañana. Archivo. |

<a id="prediccinccaahoyarchivo"></a>
# **PrediccinCCAAHoyArchivo**
> Model200 PrediccinCCAAHoyArchivo (string ccaa, string fecha)

Predicción CCAA hoy. Archivo.

Predicción para la comunidad autónoma que se pasa como parámetro (ccaa) con validez para el día de fecha de elaboración que se pasa como parámetro (fecha). Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAHoyArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción CCAA hoy. Archivo.
                Model200 result = apiInstance.PrediccinCCAAHoyArchivo(ccaa, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAHoyArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAHoyArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA hoy. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAHoyArchivoWithHttpInfo(ccaa, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAHoyArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

<a id="prediccinccaahoytiempoactual"></a>
# **PrediccinCCAAHoyTiempoActual**
> Model200 PrediccinCCAAHoyTiempoActual (string ccaa)

Predicción CCAA hoy. Tiempo actual.

Predicción para la CCAA que se pasa como parámetro con validez para mismo día que la fecha de petición. En el caso de que en la fecha de petición este producto todavía no se hubiera elaborado, se retornará el último elaborado. Actualización continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAHoyTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La

            try
            {
                // Predicción CCAA hoy. Tiempo actual.
                Model200 result = apiInstance.PrediccinCCAAHoyTiempoActual(ccaa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAHoyTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAHoyTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA hoy. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAHoyTiempoActualWithHttpInfo(ccaa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAHoyTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La |  |

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

<a id="prediccinccaamaanaarchivo"></a>
# **PrediccinCCAAMaanaArchivo**
> Model200 PrediccinCCAAMaanaArchivo (string ccaa, string fecha)

Predicción CCAA mañana. Archivo.

Predicción para la comunidad autónoma que se pasa como parámetro (ccaa) con validez para el día siguiente a la fecha de elaboración que se pasa como parámetro (fecha). Periodicidad de actualización. continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAMaanaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción CCAA mañana. Archivo.
                Model200 result = apiInstance.PrediccinCCAAMaanaArchivo(ccaa, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMaanaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAMaanaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA mañana. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAMaanaArchivoWithHttpInfo(ccaa, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMaanaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

<a id="prediccinccaamaanatiempoactual"></a>
# **PrediccinCCAAMaanaTiempoActual**
> Model200 PrediccinCCAAMaanaTiempoActual (string ccaa)

Predicción CCAA mañana. Tiempo actual.

Predicción para la comunidad autónoma que se pasa como parámetro para el día siguiente a la fecha de la petición. En el caso de el producto no se hubiera elaborado todavía en la fecha de petición se retornará el último producto elaborado. Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAMaanaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   

            try
            {
                // Predicción CCAA mañana. Tiempo actual.
                Model200 result = apiInstance.PrediccinCCAAMaanaTiempoActual(ccaa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMaanaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAMaanaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA mañana. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAMaanaTiempoActualWithHttpInfo(ccaa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMaanaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |

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

<a id="prediccinccaamedioplazoarchivo"></a>
# **PrediccinCCAAMedioPlazoArchivo**
> Model200 PrediccinCCAAMedioPlazoArchivo (string ccaa, string fecha)

Predicción CCAA medio plazo. Archivo.

Predicción de mediio plazo para la comunidad autónoma que se pasa como parámetro (ccaa) a partir de la fecha de elaboración que se pasa como parámetro (fecha). Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAMedioPlazoArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción CCAA medio plazo. Archivo.
                Model200 result = apiInstance.PrediccinCCAAMedioPlazoArchivo(ccaa, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMedioPlazoArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAMedioPlazoArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA medio plazo. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAMedioPlazoArchivoWithHttpInfo(ccaa, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMedioPlazoArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

<a id="prediccinccaamedioplazotiempoactual"></a>
# **PrediccinCCAAMedioPlazoTiempoActual**
> Model200 PrediccinCCAAMedioPlazoTiempoActual (string ccaa)

Predicción CCAA medio plazo. Tiempo actual.

Predicción para la comunidad autónoma que se pasa como parámetro (ccaa) y con validez para el medio plazo a partir de la fecha de petición. En el caso de que en el fecha de la petición no se hubiera generado aún el producto, se retornará el última elaborado. Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAMedioPlazoTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   

            try
            {
                // Predicción CCAA medio plazo. Tiempo actual.
                Model200 result = apiInstance.PrediccinCCAAMedioPlazoTiempoActual(ccaa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMedioPlazoTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAMedioPlazoTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA medio plazo. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAMedioPlazoTiempoActualWithHttpInfo(ccaa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAMedioPlazoTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |

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

<a id="prediccinccaapasadomaanaarchivo"></a>
# **PrediccinCCAAPasadoMaanaArchivo**
> Model200 PrediccinCCAAPasadoMaanaArchivo (string ccaa, string fecha)

Predicción CCAA pasado mañana. Archivo.

Predicción para la comunidad autónoma que se pasa como parámetro (ccaa) y validez para pasado mañana a partir de la fecha de elaboración que se pasa como parámetro (fecha). Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAPasadoMaanaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción CCAA pasado mañana. Archivo.
                Model200 result = apiInstance.PrediccinCCAAPasadoMaanaArchivo(ccaa, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAPasadoMaanaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAPasadoMaanaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA pasado mañana. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAPasadoMaanaArchivoWithHttpInfo(ccaa, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAPasadoMaanaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

<a id="prediccinccaapasadomaanatiempoactual"></a>
# **PrediccinCCAAPasadoMaanaTiempoActual**
> Model200 PrediccinCCAAPasadoMaanaTiempoActual (string ccaa)

Predicción CCAA pasado mañana. Tiempo actual.

Predicción para la comunidad autónoma que se pasa como parámetro (ccaa) y validez para el medio plazo a partir de la fecha de la petición. En el caso de que en la fecha de la petición dicho producto aún no se hubiera generado retornará el último de este tipo que se hubiera generado.  Periodicidad de actualización: continuamente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinCCAAPasadoMaanaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var ccaa = "ccaa_example";  // string |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La   

            try
            {
                // Predicción CCAA pasado mañana. Tiempo actual.
                Model200 result = apiInstance.PrediccinCCAAPasadoMaanaTiempoActual(ccaa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAPasadoMaanaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinCCAAPasadoMaanaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción CCAA pasado mañana. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinCCAAPasadoMaanaTiempoActualWithHttpInfo(ccaa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinCCAAPasadoMaanaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **ccaa** | **string** |  | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Astrrias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La    |  |

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

<a id="prediccinnacionalhoyarchivo"></a>
# **PrediccinNacionalHoyArchivo**
> Model200 PrediccinNacionalHoyArchivo (string fecha)

Predicción nacional hoy. Archivo.

Predicción nacional para el día correspondiente a la fecha que se pasa como parámetro en en formato texto. Actualización diaria. Hay días en los que este producto no se realiza. En ese caso se devuelve un 404 producto no existente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalHoyArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var fecha = "fecha_example";  // string | Fecha en formato (AAAA-MM-DD)

            try
            {
                // Predicción nacional hoy. Archivo.
                Model200 result = apiInstance.PrediccinNacionalHoyArchivo(fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalHoyArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalHoyArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional hoy. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalHoyArchivoWithHttpInfo(fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalHoyArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Fecha en formato (AAAA-MM-DD) |  |

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

<a id="prediccinnacionalhoytiempoactual"></a>
# **PrediccinNacionalHoyTiempoActual**
> Model200 PrediccinNacionalHoyTiempoActual ()

Predicción nacional hoy. Última elaborada.

Predicción nacional para el día actual a la fecha de elaboración en formato texto. Actualización diaria. Hay días en los que este producto no se realiza. En ese caso se devuelve la predicción nacional última que se elaboró.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalHoyTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);

            try
            {
                // Predicción nacional hoy. Última elaborada.
                Model200 result = apiInstance.PrediccinNacionalHoyTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalHoyTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalHoyTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional hoy. Última elaborada.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalHoyTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalHoyTiempoActualWithHttpInfo: " + e.Message);
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

<a id="prediccinnacionalmaanaarchivo"></a>
# **PrediccinNacionalMaanaArchivo**
> Model200 PrediccinNacionalMaanaArchivo (string fecha)

Predicción nacional mañana. Archivo.

Predicción nacional para el día siguiente a la fecha de elaboración. En este caso la fecha de elaboración es la fecha que se pasa como parámetro. Actualización diaria.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalMaanaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var fecha = "fecha_example";  // string | Día (AAAA-MM-DD)

            try
            {
                // Predicción nacional mañana. Archivo.
                Model200 result = apiInstance.PrediccinNacionalMaanaArchivo(fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMaanaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalMaanaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional mañana. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalMaanaArchivoWithHttpInfo(fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMaanaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Día (AAAA-MM-DD) |  |

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

<a id="prediccinnacionalmaanatiempoactual"></a>
# **PrediccinNacionalMaanaTiempoActual**
> Model200 PrediccinNacionalMaanaTiempoActual ()

Predicción nacional mañana. Tiempo actual.

Predicción nacional para el día siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual  todavía no se haya elaborado se devolverá el último producto de predicción nacional para mañana elaborado.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalMaanaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);

            try
            {
                // Predicción nacional mañana. Tiempo actual.
                Model200 result = apiInstance.PrediccinNacionalMaanaTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMaanaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalMaanaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional mañana. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalMaanaTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMaanaTiempoActualWithHttpInfo: " + e.Message);
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

<a id="prediccinnacionalmedioplazoarchivo"></a>
# **PrediccinNacionalMedioPlazoArchivo**
> Model200 PrediccinNacionalMedioPlazoArchivo (string fecha)

Predicción nacional medio plazo. Archivo.

Predicción nacional para el medio plazo siguiente a la fecha de elaboración. En este caso, la fecha de elaboración es la fecha que se pasa como parámetro. Actualización diaria.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalMedioPlazoArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var fecha = "fecha_example";  // string | Día (AAAA-MM-DD)

            try
            {
                // Predicción nacional medio plazo. Archivo.
                Model200 result = apiInstance.PrediccinNacionalMedioPlazoArchivo(fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMedioPlazoArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalMedioPlazoArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional medio plazo. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalMedioPlazoArchivoWithHttpInfo(fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMedioPlazoArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Día (AAAA-MM-DD) |  |

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

<a id="prediccinnacionalmedioplazotiempoactual"></a>
# **PrediccinNacionalMedioPlazoTiempoActual**
> Model200 PrediccinNacionalMedioPlazoTiempoActual ()

Predicción nacional medio plazo. Tiempo actual.

Predicción nacional para medio plazo siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual  todavía no se haya elaborado se devolverá el último producto de predicción nacional para medio plazo elaborado.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalMedioPlazoTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);

            try
            {
                // Predicción nacional medio plazo. Tiempo actual.
                Model200 result = apiInstance.PrediccinNacionalMedioPlazoTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMedioPlazoTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalMedioPlazoTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional medio plazo. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalMedioPlazoTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalMedioPlazoTiempoActualWithHttpInfo: " + e.Message);
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

<a id="prediccinnacionalpasadomaanaarchivo"></a>
# **PrediccinNacionalPasadoMaanaArchivo**
> Model200 PrediccinNacionalPasadoMaanaArchivo (string fecha)

Predicción nacional pasado mañana. Archivo.

Predicción nacional para pasado mañana siguiente a la fecha de elaboración. En este caso, la fecha de elaboración es la fecha que se pasa como parámetro. Actualización diaria.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalPasadoMaanaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var fecha = "fecha_example";  // string | Día (AAAA-MM-DD)

            try
            {
                // Predicción nacional pasado mañana. Archivo.
                Model200 result = apiInstance.PrediccinNacionalPasadoMaanaArchivo(fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalPasadoMaanaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalPasadoMaanaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional pasado mañana. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalPasadoMaanaArchivoWithHttpInfo(fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalPasadoMaanaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Día (AAAA-MM-DD) |  |

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

<a id="prediccinnacionalpasadomaanatiempoactual"></a>
# **PrediccinNacionalPasadoMaanaTiempoActual**
> Model200 PrediccinNacionalPasadoMaanaTiempoActual ()

Predicción nacional pasado mañana. Tiempo actual.

Predicción nacional para pasado mañana siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual  todavía no se haya elaborado se devolverá el último producto de predicción nacional para pasado mañana elaborado.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalPasadoMaanaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);

            try
            {
                // Predicción nacional pasado mañana. Tiempo actual.
                Model200 result = apiInstance.PrediccinNacionalPasadoMaanaTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalPasadoMaanaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalPasadoMaanaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional pasado mañana. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalPasadoMaanaTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalPasadoMaanaTiempoActualWithHttpInfo: " + e.Message);
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

<a id="prediccinnacionaltendenciaarchivo"></a>
# **PrediccinNacionalTendenciaArchivo**
> Model200 PrediccinNacionalTendenciaArchivo (string fecha)

Predicción nacional tendencia. Archivo.

Predicción nacional para tendencia siguiente a la fecha de elaboración. En este caso, la fecha de elaboración es la fecha que se pasa como parámetro. Actualización diaria.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalTendenciaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var fecha = "fecha_example";  // string | Día (AAAA-MM-DD)

            try
            {
                // Predicción nacional tendencia. Archivo.
                Model200 result = apiInstance.PrediccinNacionalTendenciaArchivo(fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalTendenciaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalTendenciaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional tendencia. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalTendenciaArchivoWithHttpInfo(fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalTendenciaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Día (AAAA-MM-DD) |  |

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

<a id="prediccinnacionaltendenciatiempoactual"></a>
# **PrediccinNacionalTendenciaTiempoActual**
> Model200 PrediccinNacionalTendenciaTiempoActual ()

Predicción nacional tendencia. Tiempo actual.

Predicción nacional para tendencia siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual  todavía no se haya elaborado se devolverá el último producto de predicción nacional para tendencia elaborado.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinNacionalTendenciaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);

            try
            {
                // Predicción nacional tendencia. Tiempo actual.
                Model200 result = apiInstance.PrediccinNacionalTendenciaTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalTendenciaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinNacionalTendenciaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción nacional tendencia. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinNacionalTendenciaTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinNacionalTendenciaTiempoActualWithHttpInfo: " + e.Message);
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

<a id="prediccinprovincialeinsularhoyarchivo"></a>
# **PrediccinProvincialEInsularHoyArchivo**
> Model200 PrediccinProvincialEInsularHoyArchivo (string provincia, string fecha)

Predicción provincial e insular hoy. Archivo.

Predicción del día siguiente a la fecha que se pasa como parámetro para la provincia o isla que se pasa como parámetro. Actualización continua y fija a las 14:00 Hora Oficial Peninsular del día que se pasa como parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinProvincialEInsularHoyArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var provincia = "provincia_example";  // string |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | | 
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción provincial e insular hoy. Archivo.
                Model200 result = apiInstance.PrediccinProvincialEInsularHoyArchivo(provincia, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularHoyArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinProvincialEInsularHoyArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción provincial e insular hoy. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinProvincialEInsularHoyArchivoWithHttpInfo(provincia, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularHoyArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **provincia** | **string** |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | |  |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

<a id="prediccinprovincialeinsularhoytiempoactual"></a>
# **PrediccinProvincialEInsularHoyTiempoActual**
> Model200 PrediccinProvincialEInsularHoyTiempoActual (string provincia)

Predicción provincial e insular hoy. Tiempo actual.

Predicción del día actual para la provincia o isla que se pasa como parámetro. En el caso de que este producto no se haya elaborado todavía en el día actual, se retorna el último elaborado. Actualización continua y fija a las 14:00 Hora Oficial Peninsular.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinProvincialEInsularHoyTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var provincia = "provincia_example";  // string |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | | 

            try
            {
                // Predicción provincial e insular hoy. Tiempo actual.
                Model200 result = apiInstance.PrediccinProvincialEInsularHoyTiempoActual(provincia);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularHoyTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinProvincialEInsularHoyTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción provincial e insular hoy. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinProvincialEInsularHoyTiempoActualWithHttpInfo(provincia);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularHoyTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **provincia** | **string** |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | |  |  |

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

<a id="prediccinprovincialeinsularmaanatiempoactual"></a>
# **PrediccinProvincialEInsularMaanaTiempoActual**
> Model200 PrediccinProvincialEInsularMaanaTiempoActual (string provincia)

Predicción provincial e insular mañana. Tiempo actual.

Predicción del día siguiente para la provincia o isla que se pasa como parámetro. En el caso de que este producto no se haya elaborado todavía en el día actual, se retorna el último elaborado. Actualización continua y fija a las 14:00 Hora Oficial Peninsular.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinProvincialEInsularMaanaTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var provincia = "provincia_example";  // string |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | | 

            try
            {
                // Predicción provincial e insular mañana. Tiempo actual.
                Model200 result = apiInstance.PrediccinProvincialEInsularMaanaTiempoActual(provincia);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularMaanaTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinProvincialEInsularMaanaTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción provincial e insular mañana. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.PrediccinProvincialEInsularMaanaTiempoActualWithHttpInfo(provincia);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialEInsularMaanaTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **provincia** | **string** |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | |  |  |

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

<a id="prediccinprovincialoinsularmaanaarchivo"></a>
# **PrediccinProvincialOInsularMaanaArchivo**
> Model200 PrediccinProvincialOInsularMaanaArchivo (string provincia, string fecha)

Predicción provincial e insular mañana. Archivo.

Predicción del día siguiente a la fecha que se pasa como parámetro para la provincia o isla que se pasa como parámetro. Actualización continua y fija a las 14:00 Hora Oficial Peninsular del día que se pasa como parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinProvincialOInsularMaanaArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionesNormalizadasTextoApi(config);
            var provincia = "provincia_example";  // string |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | | 
            var fecha = "fecha_example";  // string | Día de elaboración (AAAA-MM-DD)

            try
            {
                // Predicción provincial e insular mañana. Archivo.
                Model200 result = apiInstance.PrediccinProvincialOInsularMaanaArchivo(provincia, fecha);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialOInsularMaanaArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinProvincialOInsularMaanaArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción provincial e insular mañana. Archivo.
    ApiResponse<Model200> response = apiInstance.PrediccinProvincialOInsularMaanaArchivoWithHttpInfo(provincia, fecha);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionesNormalizadasTextoApi.PrediccinProvincialOInsularMaanaArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **provincia** | **string** |  | Código  | Provincia / Isla | |- -- -- -- -- -|- -- -- -- -- -| | 01  | Araba/Álaba   | | 01  | Araba/Álava   | | 02  | Albacete   | | 03  | Alacant/Alicante  | | 04  | Almería   | | 33  | Asturias   | | 05  | Ávila   | | 06  | Badajoz   | |  08  | Barcelona   | | 48  | Bizkaia   | | 09  | Burgos   | | 10  | Cáceres   | | 11  | Cádiz   | | 39  | Cantabria   | | 12  | Castelló/Castellón   | | 51  | Ceuta   | | 13  | Ciudad Real   | | 14  | Córdoba   | | 15  | A Coruña   | | 16  | Cuenca   | | 17  | Girona   | | 18  | Granada   | | 19  | Guadalajara   | | 20  | Gipuzkoa   | | 21  | Huelva   | | 22  | Huesca   | | 071  | Isla de Menorca   | | 072  | Isla de Mallorca   | | 073  | Islas de Ibiza y Formentera   | |  351  | Isla de Lanzarote  | | 352  | Isla de Fuerteventura  | | 353  | Isla de Gran Canaria   |  | 381  | Isla de Tenerife   | | 382  | Isla de La Gomera   | | 383  | Isla de La Palma   | | 384  | Isla de El Hierro   | | 23  | Jaén   | | 24  | León   | | 25  | Lleida   | | 27  | Lugo   | | 28  | Madrid   | | 29  | Málaga   | | 52  | Melilla   | | 30  | Murcia   | | 31  | Navarra   | | 32  | Ourense   | | 34  | Palencia   | |  36  | Pontevedra   | | 26  | La Rioja   | | 37  | Salamanca   | |  40  | Segovia   | | 41  | Sevilla   | | 42  | Soria   | | 43  | Tarragona   | | 44  | Teruel   | | 45  | Toledo   | | 46  | València/Valencia   | | 47  | Valladolid   | | 49  | Zamora   | | 50  | Zaragoza   | |  |  |
| **fecha** | **string** | Día de elaboración (AAAA-MM-DD) |  |

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

