namespace Horizonte.Extension.AspNetCore;

public static class Extensions
{
    public static IApplicationBuilder UseHorizonteStaticFiles(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<HorizonteStaticFileMiddelware>();
    }

    public static IServiceCollection AddHorizonteLegacyServices(
        this IServiceCollection services, IServiceProvider legacyServiceProvider)
        
    {

        var context = legacyServiceProvider. GetService<IHContext>();
        if (context != null) services.AddSingleton<IHContext>(context);
        
        var trans = legacyServiceProvider.GetService<IHtrans>();
        if (trans != null) services.AddSingleton<IHtrans>(trans);        
        
        var gescom = legacyServiceProvider.GetService<IHGesCom>();
        if (gescom != null) services.AddSingleton<IHGesCom>(gescom); 
        
        var symLinkScafolder = legacyServiceProvider.GetService<ISymLinkScafolder>();
        if (symLinkScafolder != null) services.AddSingleton<ISymLinkScafolder>(symLinkScafolder);   
        
        /*
        var modmanager = instanceEnv.GetService<IHModManager>();
        if (modmanager != null) services.AddSingleton<IHModManager>(modmanager);
        */
        
        return services;
    }
}