using ChartSightAI.MVVM.Views;
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
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            return services
                .RegisterViews()
                .RegisterViewModels();

        }

    }
}
