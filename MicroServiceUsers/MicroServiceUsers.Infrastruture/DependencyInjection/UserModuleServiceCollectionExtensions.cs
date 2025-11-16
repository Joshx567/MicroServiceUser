using Microsoft.Extensions.DependencyInjection;
using ServiceUser.Application.Interfaces;
using ServiceUser.Domain.Ports;
using ServiceUser.Infrastructure.Persistence;
using ServiceUser.Infrastructure.Provider;

namespace ServiceUser.Infrastructure.DependencyInjection
{
    public static class UserModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddUserModule<TProvider>(this IServiceCollection services)
            where TProvider : class, IUserConnectionProvider
        {
            // Registra el provider que la aplicación principal nos da.
            services.AddSingleton<IUserConnectionProvider, TProvider>();

            return services.AddUserCore();
        }

        // Permite registrar el módulo pasando una función que sabe cómo obtener la connection string.
        public static IServiceCollection AddUserModule(this IServiceCollection services, Func<IServiceProvider, string> connectionStringFactory)
        {
            ArgumentNullException.ThrowIfNull(connectionStringFactory, nameof(connectionStringFactory));

            services.AddSingleton<IUserConnectionProvider>(sp =>
            {
                var connectionString = connectionStringFactory(sp);
                return new DelegatedUserConnectionProvider(connectionString);
            });

            return services.AddUserCore();
        }

        private static IServiceCollection AddUserCore(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        private sealed class DelegatedUserConnectionProvider : IUserConnectionProvider
        {
            private readonly string _connectionString;

            public DelegatedUserConnectionProvider(string connectionString)
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentException("La cadena de conexión no puede ser nula ni estar vacía.", nameof(connectionString));
                }
                _connectionString = connectionString;
            }

            public string GetConnectionString() => _connectionString;
        }
    }
}
