using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;
using Ui.Core.Repositories;
using Ui.Core.Services;
using Ui.Data.Context;
using Ui.Data.Entities;
using Unity;
using Unity.Injection;
using Unity.Mvc5;

namespace Ui.Client
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

            #region Identity

            container.RegisterType<ApplicationSignInManager>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<ApplicationRoleManager>();

            container.RegisterType<IAuthenticationManager>(
            new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(
            new InjectionConstructor(typeof(ApplicationDbContext)));
            container.RegisterType<IRoleStore<IdentityRole, string>, RoleStore<IdentityRole>>(new InjectionConstructor(new ApplicationDbContext()));

            #endregion


            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}