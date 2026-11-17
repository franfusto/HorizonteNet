// ==============================================================================
// ARGUMENTS & PARAMETERS
// ==============================================================================
var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");
var runtime = Argument("runtime", "linux-x64");

// Docker Configuration
var dockerUsername = Argument("dockerUsername", EnvironmentVariable("DOCKER_USERNAME") ?? "franfusto");
var promptDockerUsername = Argument("promptDockerUsername", true);

var dockerPassword = Argument("dockerPassword", EnvironmentVariable("DOCKER_PASSWORD") ?? "");
var promptDockerPassword = Argument("promptDockerPassword", true);

var imageName = Argument("imageName", EnvironmentVariable("DOCKER_IMAGE_NAME") ?? "horizontenet");
var promptImageName = Argument("promptImageName", true);

var imageTag = Argument("imageTag", EnvironmentVariable("DOCKER_IMAGE_TAG") ?? "latest");
var promptImageTag = Argument("promptImageTag", true);

// ==============================================================================
// PROMPT HELPERS & INTERACTIVE INPUT
// ==============================================================================
bool ShouldPromptForDockerImage(string targetName)
{
    return targetName.Equals("Build", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Publish", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Default", StringComparison.OrdinalIgnoreCase);
}

bool ShouldPromptForDockerCredentials(string targetName)
{
    return targetName.Equals("Publish", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Docker-Login", StringComparison.OrdinalIgnoreCase);
}

string ReadMaskedPassword()
{
    var pass = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (pass.Length > 0)
            {
                pass.Remove(pass.Length - 1, 1);
                Console.Write("\b \b");
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            pass.Append(key.KeyChar);
            Console.Write("*");
        }
    }

    return pass.ToString().Trim();
}

string PromptForImageName(string currentImageName)
{
    Console.WriteLine($"ImageName actual: {currentImageName}");
    Console.Write("Nuevo ImageName, Enter para mantener el actual: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine($"Se mantiene ImageName: {currentImageName}");
        return currentImageName;
    }

    Console.WriteLine($"Nuevo ImageName: {input}");
    return input;
}

string PromptForImageTag(string currentImageTag)
{
    Console.WriteLine($"ImageTag actual: {currentImageTag}");
    Console.Write("Nuevo ImageTag, Enter para mantener el actual: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine($"Se mantiene ImageTag: {currentImageTag}");
        return currentImageTag;
    }

    Console.WriteLine($"Nuevo ImageTag: {input}");
    return input;
}

string PromptForDockerUsername(string currentUsername)
{
    Console.WriteLine($"DockerUsername actual: {currentUsername}");
    Console.Write("Nuevo DockerUsername, Enter para mantener el actual: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine($"Se mantiene DockerUsername: {currentUsername}");
        return currentUsername;
    }

    Console.WriteLine($"Nuevo DockerUsername: {input}");
    return input;
}

string PromptForDockerPassword(string currentPassword)
{
    Console.WriteLine();
    if (!string.IsNullOrWhiteSpace(currentPassword))
    {
        Console.WriteLine("DockerPassword actual: [configurada]");
        Console.Write("Nueva DockerPassword, Enter para mantener la actual: ");
    }
    else
    {
        Console.Write("Introduce DockerPassword: ");
    }

    var input = ReadMaskedPassword();
    if (string.IsNullOrWhiteSpace(input))
    {
        if (!string.IsNullOrWhiteSpace(currentPassword))
        {
            Console.WriteLine("Se mantiene la DockerPassword actual.");
        }
        return currentPassword;
    }

    Console.WriteLine("DockerPassword actualizada.");
    return input;
}

// Ejecutar prompts interactivos condicionalmente
if (promptImageName && !HasArgument("imageName") && ShouldPromptForDockerImage(target))
{
    imageName = PromptForImageName(imageName);
}

if (promptImageTag && !HasArgument("imageTag") && ShouldPromptForDockerImage(target))
{
    imageTag = PromptForImageTag(imageTag);
}

if (promptDockerUsername && !HasArgument("dockerUsername") && (ShouldPromptForDockerImage(target) || ShouldPromptForDockerCredentials(target)))
{
    dockerUsername = PromptForDockerUsername(dockerUsername);
}

if (promptDockerPassword && !HasArgument("dockerPassword") && ShouldPromptForDockerCredentials(target))
{
    dockerPassword = PromptForDockerPassword(dockerPassword);
}

// Paths
var projectPath = "../Launchers/ConsoleApp/ConsoleApp.csproj";
var outputDir = "./AppFiles";

// Full image name for Docker Hub (e.g., username/repository:tag)
var fullImageTag = string.IsNullOrWhiteSpace(dockerUsername)
    ? $"{imageName}:{imageTag}"
    : $"{dockerUsername}/{imageName}:{imageTag}";

// ==============================================================================
// SETUP / TEARDOWN
// ==============================================================================
Setup(context =>
{
    Information("========================================");
    Information("Iniciando Cake Build Script (Docker)");
    Information($"Target:         {target}");
    Information($"Configuración:  {configuration}");
    Information($"Runtime:        {runtime}");
    Information($"Imagen Docker:  {fullImageTag}");
    Information("========================================");
});

Teardown(context =>
{
    Information("Cake Build Script finalizado con éxito.");
});

// ==============================================================================
// TASKS
// ==============================================================================

// Target: Clean-App
Task("Clean-App")
    .Description("Limpia la carpeta de salida de la aplicación previa a la publicación.")
    .Does(() =>
{
    if (DirectoryExists(outputDir))
    {
        Information($"Limpiando directorio de salida: {outputDir}...");
        CleanDirectory(outputDir);
    }
    else
    {
        CreateDirectory(outputDir);
    }
});

// Target: Publish-App
Task("Publish-App")
    .IsDependentOn("Clean-App")
    .Description("Compila y publica la aplicación ConsoleApp para Linux en AppFiles.")
    .Does(() =>
{
    Information($"Publicando {projectPath} ({configuration}, {runtime})...");

    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        Runtime = runtime,
        SelfContained = false,
        OutputDirectory = outputDir
    };

    DotNetPublish(projectPath, settings);
    Information($"Publicación completada en: {outputDir}");

    var configFile = "./horizonte.json";
    if (FileExists(configFile))
    {
        Information($"Copiando {configFile} a {outputDir}...");
        CopyFileToDirectory(configFile, outputDir);
    }
});

// Target: Build
Task("Build")
    .IsDependentOn("Publish-App")
    .Description("Construye la imagen de Docker a partir del Dockerfile local tras publicar ConsoleApp.")
    .Does(() =>
{
    Information($"Construyendo imagen Docker: {fullImageTag}...");

    var exitCode = StartProcess("docker", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder()
            .Append("build")
            .AppendSwitchQuoted("-t", fullImageTag)
            .Append(".")
    });

    if (exitCode != 0)
    {
        throw new Exception($"Error al construir la imagen Docker. Código de salida: {exitCode}");
    }

    Information("Imagen Docker construida exitosamente.");
});

// Target: Docker-Login (Interno o previo a Publish)
Task("Docker-Login")
    .Description("Inicia sesión en Docker Hub si se proporcionaron credenciales de forma segura vía stdin.")
    .Does(() =>
{
    if (!string.IsNullOrWhiteSpace(dockerUsername) && !string.IsNullOrWhiteSpace(dockerPassword))
    {
        Information($"Iniciando sesión en Docker Hub como '{dockerUsername}'...");

        using (var process = new System.Diagnostics.Process())
        {
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"login -u \"{dockerUsername}\" --password-stdin",
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            using (var writer = process.StandardInput)
            {
                writer.WriteLine(dockerPassword);
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Fallo al autenticar en Docker Hub. Código de salida: {process.ExitCode}");
            }
        }

        Information("Autenticación en Docker Hub completada exitosamente.");
    }
    else
    {
        Information("No se pasaron credenciales explícitas; se usará la sesión de Docker existente.");
    }
});

// Target: Publish
Task("Publish")
    .IsDependentOn("Docker-Login")
    .IsDependentOn("Build")
    .Description("Publica la imagen construida en Docker Hub.")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(dockerUsername))
    {
        Warning("ADVERTENCIA: No se especificó 'dockerUsername'. Para publicar en Docker Hub se recomienda definir el repositorio 'usuario/imagen'.");
    }

    Information($"Publicando imagen en Docker Hub: {fullImageTag}...");

    var pushExitCode = StartProcess("docker", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder()
            .Append("push")
            .Append(fullImageTag)
    });

    if (pushExitCode != 0)
    {
        throw new Exception($"Error al publicar la imagen en Docker Hub. Código de salida: {pushExitCode}");
    }

    Information($"Imagen {fullImageTag} publicada con éxito en Docker Hub.");
});

// Target por defecto
Task("Default")
    .IsDependentOn("Build");

// ==============================================================================
// EJECUCIÓN
// ==============================================================================
RunTarget(target);
