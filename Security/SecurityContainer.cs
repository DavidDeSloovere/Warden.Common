using Autofac;
using Warden.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace Warden.Common.Security
{
    public static class SecurityContainer
    {
        public static void Register(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<JwtTokenHandler>()
                .As<IJwtTokenHandler>()
                .SingleInstance();

            builder.RegisterType<ServiceAuthenticatorClient>()
                .As<IServiceAuthenticatorClient>()
                .SingleInstance();
            
            builder.RegisterType<ServiceAuthenticatorHost>()
                .As<IServiceAuthenticatorHost>()
                .SingleInstance();

            builder.RegisterInstance(configuration.GetSettings<JwtTokenSettings>())
                .SingleInstance();

            builder.RegisterInstance(configuration.GetSettings<ServiceSettings>())
                .SingleInstance();

            builder.RegisterInstance(configuration.GetSettings<ServicesSettings>())
                .SingleInstance();
        }
    }
}