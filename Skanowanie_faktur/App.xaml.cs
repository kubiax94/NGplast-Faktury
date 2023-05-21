using Microsoft.Extensions.DependencyInjection;
using Services.Builder;
using Services.FilesService;
using Skanowanie_faktur.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Skanowanie_faktur
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IServiceCollection services = BackendServiceBuilder.BuildServices();


            services.AddSingleton<MainViewModel>(s => new MainViewModel(s.GetRequiredService<IFilesReaderService>()));
            services.AddSingleton<MainWindow>(s => new MainWindow(s.GetRequiredService<MainViewModel>()));

            var provider = services.BuildServiceProvider();

            Window main = provider.GetRequiredService<MainWindow>();
            main.Show();
        }

    }
}
