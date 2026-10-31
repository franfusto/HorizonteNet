# HorizonteNet

[Español](README_es.md) | [English](README.md)

---

**HorizonteNet** is an advanced modular environment built on **.NET 10**, designed to simplify the creation of extensible and distributed applications through NuGet packages.

---

## Modular architecture

HorizonteNet is based on a microservices and decoupled modules architecture:

- **Core (`Horizonte`)**  
  The system core responsible for managing the context (`IhContext`) and the lifecycle of workers (`HWorkersManager`).

- **Admin UI (`Horizonte.AdminUi`)**  
  A modern and reactive administration interface built with **Blazor**.

- **Core Extensions (`Horizonte.Extension`)**  
  An extension ecosystem for working with C# scripts, notebooks, workflows, and AI agents.

- **Modules (`Horizonte.Modules`)**  
  A set of modules that extend HorizonteNet functionality with predefined and customizable capabilities:

  - **Admin UI:** administration panel for managing extensions and modules.
  - **Scripts:** execution and management of C# scripts and Jupyter Notebooks.
  - **Workflows:** orchestration of customizable workflows.
  - **Agents:** definition and execution of autonomous agents.
  - **MCP Server:** service based on the Model Context Protocol that integrates module functionality.

---

## Main features

- **Flexible modular architecture management:** JSON-based configuration system with support for local and user overrides.
- **Module loading in ALC:** module isolation in different application domains.
- **Hot Reload** support for changes in services, modules, and scripts.
- **Background services:** simplified orchestration of services, workers, and scheduled tasks.
- **Assembly management:** management of NuGet packages, unmanaged code, and associated web content.
- **Extensibility:** designed to be extended through new extensions, modules, and workflows.
- **Cross-platform:** compatible with Linux, Windows, and macOS.

---

## Preview 10.0.3-beta

HorizonteNet can be run directly from the compiled binaries available in the project release:

- [Windows executable](https://github.com/franfusto/HorizonteNet/releases/download/10.0.3-beta/ConsoleApp.exe)
- [Linux executable](https://github.com/franfusto/HorizonteNet/releases/download/10.0.3-beta/ConsoleApp)

### Running from binaries

1. Download the executable corresponding to your operating system.
2. Run the downloaded file.
3. Open the application in your browser:

   ```text
   http://localhost:5000
   ```

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later.
- A compatible development environment:
  - JetBrains Rider
  - Visual Studio
  - Visual Studio Code

### Installation from source code

1. Clone the repository:

   ```bash
   git clone https://github.com/franfusto/HorizonteNet.git
   ```

2. Go to the project directory:

   ```bash
   cd HorizonteNet
   ```

3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Build the project:

   ```bash
   dotnet build
   ```

5. Run the `ConsoleApp` project:

   ```bash
   dotnet run --project Launchers/ConsoleApp
   ```

6. Open the application in your browser:

   ```text
   http://localhost:5000
   ```

---

## Explore

You can download a sample context file to explore the initial configuration and structure of HorizonteNet:

- [Download sample context](githubfiles/horizonte_sample.json)

This file can be used as a starting point to review how a context is defined, how modules are organized, and how extensions are configured within the environment.

<table>
  <tr>
    <td align="center">
      <strong>Jupyter Notebooks integration</strong><br>
      <img src="githubfiles/img/readme1.png" alt="HorizonteNet preview 1" width="300">
    </td>
    <td align="center">
      <strong>Embedded scripting system</strong><br>
      <img src="githubfiles/img/readme2.png" alt="HorizonteNet preview 2" width="300">
    </td>
  </tr>
  <tr>
    <td align="center">
      <strong>Service orchestration</strong><br>
      <img src="githubfiles/img/readme3.png" alt="HorizonteNet preview 3" width="300">
    </td>
    <td align="center">
      <strong>Workflows with Microsoft.Agents.AI.Workflows</strong><br>
      <img src="githubfiles/img/readme4.png" alt="HorizonteNet preview 4" width="300">
    </td>
  </tr>
</table>

---

## Project status

HorizonteNet is currently in **Beta**.

The project is under active development, so any feedback, bug reports, or improvement proposals are welcome.

You can participate through:

- [GitHub Issues](https://github.com/franfusto/HorizonteNet/issues)
- [Discord](https://discord.com/channels/1538980325953249290/1538980326527995960)

---

## Project information

- **Repository:** [github.com/franfusto/HorizonteNet](https://github.com/franfusto/HorizonteNet)
- **Author:** franfusto
- **License:** [MIT](LICENSE)

---

*HorizonteNet — elevating modular development in .NET.*
