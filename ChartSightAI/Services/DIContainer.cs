using ChartSightAI.MVVM.Views;
using ChartSightAI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Services
{
    public static class DIContainer
    {

        public static IServiceCollection RegisterViewModels(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection RegisterViews(this IServiceCollection services)
        {

            services.AddTransient<HomePage>();
            return services;
        }
        public static IServiceCollection RegisterAuthServices(this IServiceCollection services)
        {

            services.AddSingleton<IAuthService, AuthService>();
            return services;
        }
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            return services
                .RegisterAuthServices()
                .RegisterViews()
                .RegisterViewModels();

        }

    }
}
