using Autofac;
using NfhcModel.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Module = Autofac.Module;

namespace NfhcModel.Modules
{
    class NfhcPatchesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IPersistentPatch>()
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IDynamicPatch>()
                .AsImplementedInterfaces();
        }
    }
}
