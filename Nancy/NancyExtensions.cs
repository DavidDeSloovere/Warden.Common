﻿using Autofac;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Warden.Common.Security;

namespace Warden.Common.Nancy
{
    public static class NancyExtensions
    {
        public static object ToExceptionData(this NancyContext context)
            => new
            {
                userId = context.CurrentUser?.Identity?.Name ?? string.Empty,
                path = context.Request.Path,
                method = context.Request.Method,
                ipAddress = context.Request.UserHostAddress,
                headers = context.Request.Headers,
            };

        public static void SetupTokenAuthentication(this IPipelines pipelines, ILifetimeScope container)
        {
            var jwtTokenHandler = container.Resolve<IJwtTokenHandler>();
            var statelessAuthConfiguration = new StatelessAuthenticationConfiguration(ctx =>
                {
                    var token = jwtTokenHandler.GetFromAuthorizationHeader(ctx.Request.Headers.Authorization);
                    var isValid = jwtTokenHandler.IsValid(token);

                    return isValid ? new WardenIdentity(token.Sub) : null;
                });
            StatelessAuthentication.Enable(pipelines, statelessAuthConfiguration);            
        }
    }
}