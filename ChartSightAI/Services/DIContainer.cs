using ChartSightAI.MVVM.ViewModels;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Services.Interfaces;
using ChartSightAI.Services.Repos;
using ChartSightAI.Utility;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace ChartSightAI.Services
{
    public static class DIContainer
    {
        public static IServiceCollection RegisterSupabase(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                };
                return new Client(AppConfig.SupabaseUrl, AppConfig.SupabaseAnonKey, options);
            });
            return services;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<AnalysisSessionRepo>();
            services.AddTransient<PresetRepo>();
            services.AddTransient<UserPreferenceRepo>();
            return services;
        }

        public static IServiceCollection RegisterAuthServices(this IServiceCollection services)
        {
            services.AddSingleton<AuthService>();
            return services;
        }

        public static IServiceCollection RegisterViewModels(this IServiceCollection services)
        {
            services.AddTransient<HomeVM>();
            services.AddTransient<NewPredictionVM>();
            services.AddTransient<PresetsVM>();
            services.AddTransient<NewPresetVM>();
            services.AddTransient<HistoryVM>();
            services.AddTransient<AnalyticsVM>();
            services.AddTransient<SettingsVM>();
            services.AddTransient<LoginVM>();
            services.AddTransient<LoadingVM>();
            services.AddTransient<SignUpVM>();
            return services;
        }

        public static IServiceCollection RegisterViews(this IServiceCollection services)
        {
            services.AddTransient<HomePage>();
            services.AddTransient<NewPrediction>();
            services.AddTransient<Presets>();
            services.AddTransient<NewPreset>();
            services.AddTransient<History>();
            services.AddTransient<AnalysisDetails>();
            services.AddTransient<Analytics>();
            services.AddTransient<Settings>();
            services.AddTransient<Profile>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LoadingPage>();
            services.AddTransient<SignUpPage>();
            return services;
        }

        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            return services
                 .RegisterSupabase()
                    .RegisterRepositories()
                    .RegisterAuthServices()
                    .RegisterViews()
                    .RegisterViewModels();
        }
    }
}
