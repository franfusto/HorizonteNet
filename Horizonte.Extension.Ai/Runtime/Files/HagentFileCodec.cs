using System.Text.Json;
using Horizonte.Extension.Ai.Definitions;

namespace Horizonte.Extension.Ai.Runtime.Files;

internal sealed class HagentFileCodec
{
    private const string SupportedEncoding = "base64";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyList<HagentFile> DecodeInputFiles(
        string[]? files,
        AgentFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (files is null || files.Length == 0)
        {
            return [];
        }

        if (!options.AllowInputFiles)
        {
            throw new InvalidOperationException(
                "El agente no permite archivos de entrada.");
        }

        var decodedFiles = new List<HagentFile>(files.Length);

        foreach (var fileJson in files)
        {
            if (string.IsNullOrWhiteSpace(fileJson))
            {
                throw new InvalidOperationException(
                    "La lista de archivos de entrada contiene un elemento vacío.");
            }

            HagentFileEnvelope envelope;

            try
            {
                envelope = JsonSerializer.Deserialize<HagentFileEnvelope>(
                    fileJson,
                    JsonOptions) ?? throw new InvalidOperationException(
                    "No se pudo deserializar el archivo de entrada.");
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    "El archivo de entrada no tiene un formato JSON válido.",
                    exception);
            }

            ValidateEnvelope(envelope);

            if (!IsSupportedEncoding(envelope.Encoding))
            {
                throw new InvalidOperationException(
                    $"Encoding no soportado para archivo de entrada '{envelope.FileName}': '{envelope.Encoding}'. Solo se soporta '{SupportedEncoding}'.");
            }

            byte[] content;

            try
            {
                content = Convert.FromBase64String(envelope.Content);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    $"El contenido del archivo de entrada '{envelope.FileName}' no es base64 válido.",
                    exception);
            }

            var file = new HagentFile
            {
                FileName = envelope.FileName,
                MimeType = envelope.MimeType,
                Encoding = SupportedEncoding,
                Content = content
            };

            ValidateInputFile(file, options);
            decodedFiles.Add(file);
        }

        return decodedFiles;
    }

    public string[] EncodeOutputFiles(
        IReadOnlyList<HagentFile> files,
        AgentFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(options);

        if (files.Count == 0)
        {
            return [];
        }

        if (!options.AllowOutputFiles)
        {
            throw new InvalidOperationException(
                "El agente no permite archivos de salida.");
        }

        var encodedFiles = new string[files.Count];

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];

            ValidateOutputFile(file, options);

            var envelope = new HagentFileEnvelope
            {
                FileName = file.FileName,
                MimeType = file.MimeType,
                Encoding = SupportedEncoding,
                Content = Convert.ToBase64String(file.Content)
            };

            encodedFiles[index] = JsonSerializer.Serialize(envelope, JsonOptions);
        }

        return encodedFiles;
    }

    public void ValidateInputFile(
        HagentFile file,
        AgentFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(options);

        ValidateFile(file);

        if (file.Content.Length > options.MaxInputFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"El archivo de entrada '{file.FileName}' supera el tamaño máximo permitido de {options.MaxInputFileSizeBytes} bytes.");
        }

        ValidateMimeType(
            file,
            options.AllowedInputMimeTypes,
            "entrada");
    }

    public void ValidateOutputFile(
        HagentFile file,
        AgentFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(options);

        ValidateFile(file);

        if (file.Content.Length > options.MaxOutputFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"El archivo de salida '{file.FileName}' supera el tamaño máximo permitido de {options.MaxOutputFileSizeBytes} bytes.");
        }

        ValidateMimeType(
            file,
            options.AllowedOutputMimeTypes,
            "salida");
    }

    private static void ValidateEnvelope(HagentFileEnvelope envelope)
    {
        if (string.IsNullOrWhiteSpace(envelope.FileName))
        {
            throw new InvalidOperationException(
                "El envelope del archivo no define 'fileName'.");
        }

        if (string.IsNullOrWhiteSpace(envelope.MimeType))
        {
            throw new InvalidOperationException(
                $"El archivo '{envelope.FileName}' no define 'mimeType'.");
        }

        if (string.IsNullOrWhiteSpace(envelope.Encoding))
        {
            throw new InvalidOperationException(
                $"El archivo '{envelope.FileName}' no define 'encoding'.");
        }

        if (string.IsNullOrWhiteSpace(envelope.Content))
        {
            throw new InvalidOperationException(
                $"El archivo '{envelope.FileName}' no define 'content'.");
        }
    }

    private static void ValidateFile(HagentFile file)
    {
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            throw new InvalidOperationException(
                "El archivo no define FileName.");
        }

        if (string.IsNullOrWhiteSpace(file.MimeType))
        {
            throw new InvalidOperationException(
                $"El archivo '{file.FileName}' no define MimeType.");
        }

        if (string.IsNullOrWhiteSpace(file.Encoding))
        {
            throw new InvalidOperationException(
                $"El archivo '{file.FileName}' no define Encoding.");
        }

        if (!IsSupportedEncoding(file.Encoding))
        {
            throw new InvalidOperationException(
                $"Encoding no soportado para archivo '{file.FileName}': '{file.Encoding}'. Solo se soporta '{SupportedEncoding}'.");
        }

        if (file.Content is null)
        {
            throw new InvalidOperationException(
                $"El archivo '{file.FileName}' no define Content.");
        }
    }

    private static void ValidateMimeType(
        HagentFile file,
        IReadOnlyCollection<string> allowedMimeTypes,
        string direction)
    {
        if (allowedMimeTypes.Count == 0)
        {
            return;
        }

        var isAllowed = allowedMimeTypes.Contains(
            file.MimeType,
            StringComparer.OrdinalIgnoreCase);

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"El MIME type '{file.MimeType}' del archivo de {direction} '{file.FileName}' no está permitido.");
        }
    }

    private static bool IsSupportedEncoding(string encoding)
    {
        return string.Equals(
            encoding,
            SupportedEncoding,
            StringComparison.OrdinalIgnoreCase);
    }
}