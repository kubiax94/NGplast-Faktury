using Microsoft.Extensions.DependencyInjection;
using Services.FilesService;
using Services.OCRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Builder
{
    public static class BackendServiceBuilder
    {
        public static IServiceCollection BuildServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IFilesReaderService, FilesReaderService>( x => new FilesReaderService());
            services.AddSingleton<InnvoiceOCR>(x => new InnvoiceOCR(x.GetRequiredService<IFilesReaderService>()));

            return services;
            
        }
    }
}
