namespace Horizonte.Extension.AspNetCore;

public static class Extensions
{
    public static IApplicationBuilder UseHorizonteStaticFiles(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<HorizonteStaticFileMiddelware>();
    }

    public static IServiceCollection AddHorizonteLegacyServices(
        this IServiceCollection services, IHorizonteEnv instanceEnv)
        
    {
        services.AddSingleton<IHorizonteEnv>( instanceEnv);

        var context = instanceEnv.GetService<IHContext>();
        if (context != null) services.AddSingleton<IHContext>(context);
        
        var trans = instanceEnv.GetService<IHtrans>();
        if (trans != null) services.AddSingleton<IHtrans>(trans);        
        
        var gescom = instanceEnv.GetService<IHGesCom>();
        if (gescom != null) services.AddSingleton<IHGesCom>(gescom); 
        
        var symLinkScafolder = instanceEnv.GetService<ISymLinkScafolder>();
        if (symLinkScafolder != null) services.AddSingleton<ISymLinkScafolder>(symLinkScafolder);   
        
        /*
        var modmanager = instanceEnv.GetService<IHModManager>();
        if (modmanager != null) services.AddSingleton<IHModManager>(modmanager);
        */
        
        return services;
    }
}