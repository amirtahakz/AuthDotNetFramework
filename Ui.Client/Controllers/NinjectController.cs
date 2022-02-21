using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ui.Core.Repositories;
using Ui.Core.Services;

namespace Ui.Client.Controllers
{
    public class NinjectController : DefaultControllerFactory
    {
        //before evry thing install ninject pack 
        //and add under line to end of line in Global File
        //ControllerBuilder.Current.SetControllerFactory(new $safeitemname$());
        private IKernel ninjectKernel;
        public NinjectController()
        {
            ninjectKernel = new StandardKernel();
            AddBinding();
        }
        void AddBinding()
        {

            //----Add Services----//
            ninjectKernel.Bind<IExampleService>().To<ExampleService>();

        }
        protected override IController GetControllerInstance(RequestContext requestContext, Type ControllerType)
        {
            return ControllerType == null ? null : (IController)ninjectKernel.Get(ControllerType);
        }
    }
}