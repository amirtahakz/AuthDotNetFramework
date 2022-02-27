using System.Web.Http;
using Ui.Core.Repositories;
using Ui.Core.Services;
using Unity;
using Unity.WebApi;

namespace Ui.Api
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<Ui.Core.Repositories.IEmailService, Ui.Core.Services.EmailService>();
            container.RegisterType<IExampleService, ExampleService>();
            container.RegisterType<ITokenGeneratorService, TokenGeneratorService>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}