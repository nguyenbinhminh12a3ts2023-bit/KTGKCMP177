using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace aspnetboilerplate.com.Web.Startup;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    internal static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Limits.MaxRequestHeadersTotalSize = 1048576; // 1 MB
                    serverOptions.Limits.MaxRequestBufferSize = 1048576;
                });
                webBuilder.UseStartup<Startup>();
            })
            .UseCastleWindsor(IocManager.Instance.IocContainer);
}
