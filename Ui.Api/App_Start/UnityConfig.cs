using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Http;
using Ui.Core.Repositories;
using Ui.Core.Services;
using Ui.Data.Context;
using Ui.Data.Entities;
using Unity;
using Unity.Injection;
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

            #region Identity

            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<ApplicationRoleManager>();

            container.RegisterType<IAuthenticationManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new InjectionConstructor(typeof(ApplicationDbContext)));
            container.RegisterType<IRoleStore<IdentityRole, string>, RoleStore<IdentityRole>>(new InjectionConstructor(new ApplicationDbContext()));

            #endregion

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}