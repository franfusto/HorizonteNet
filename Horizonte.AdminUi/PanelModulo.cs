using System.Diagnostics;
using Horizonte;
using Horizonte.AdminUi.Components;
using Horizonte.AdminUi.Widgets;
using Horizonte.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Horizonte.AdminUi;

[HorizonteModule("Horizonte.AdminUi")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private HGuiSettings? _settings;

    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("HorizonteGui_Init")]
    public bool Init()
    {
        _logger.LogInformation("Horizonte.AdminUi Inciciado");
        _settings = _context.Get<HGuiSettings>();
        return true;
    }

    [HorizonteCommand("HorizonteGui_ConfigPage", "Aplicación")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AppPanel), Parameters = null };
    
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_MenuEditor", "Menu Editor")]
    public WidgetDef MenuEditor() => new WidgetDef() { Type = typeof(MenuEditor), Parameters = null };

    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_NetConfig", "")]
    public WidgetDef NetConfig() => new WidgetDef() { Type = typeof(NetConfig), Parameters = null };

    [HorizonteCommand("HorizonteGui_OpenGui")]
    public void OpenGui()
    {
        try
        {
            if (!string.IsNullOrEmpty(_settings?.AppUrl))
                Process.Start(new ProcessStartInfo(_settings.AppUrl) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }

    [HorizonteCommand("HorizonteGui_RenderWidget",
        description: "Renderiza un widget en base a un WindgetDef proporcionado")]
    public RenderFragment RenderWidget(WidgetDef widgetDef) => (builder) =>
    {
        builder.OpenComponent<Widget>(0);
        builder.AddAttribute(1, "WidgetDef", widgetDef);
        builder.CloseComponent();
    };




    [HorizonteCommand("HorizonteGui_Test", description: "Crear configuración de pruebas")]
    public void Test()
    {
        try
        {
            HGuiMenu menu = new HGuiMenu();
            menu.Caption = "Test";
            menu.Items.Add(new() { Caption = "Módulos", Icon = "view_module", Id = 1, ParentId = 0 });
            menu.Items.Add(new()
            {
                Caption = "Módulos instalados", Component = "HorizonteConfig_Modules", Icon = "", Id = 2, ParentId = 1
            });
            menu.Items.Add(new()
                { Caption = "Repositorio", Component = "HorizonteConfig_Repository", Icon = "", Id = 3, ParentId = 1 });
            menu.Items.Add(new()
                { Caption = "Crear Módulo", Component = "HorizonteConfig_NewModule", Icon = "", Id = 4, ParentId = 1 });
            menu.Items.Add(new()
            {
                Caption = "Comandos", Component = "HorizonteConfig_Commands", Icon = "tab_move", Id = 5, ParentId = 0
            });
            menu.Items.Add(new()
                { Caption = "Service Workers", Component = "Command_Name", Icon = "home", Id = 6, ParentId = 0 });
            menu.Items.Add(new()
            {
                Caption = "Registro", Component = "HorizonteConfig_Log", Icon = "settings_system_daydream", Id = 7,
                ParentId = 0
            });
            menu.Items.Add(new()
            {
                Caption = "Contexto", Component = "HorizonteConfig_ViewContext", Icon = "settings_system_daydream",
                Id = 8, ParentId = 0
            });
            menu.Items.Add(new()
                { Caption = "Configuración", Component = "Command_Name", Icon = "home", Id = 9, ParentId = 0 });
            menu.Items.Add(new()
                { Caption = "Agente", Component = "AgentH_Agent", Icon = "smart_toy", Id = 10, ParentId = 0 });


            _context.Update(menu);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}