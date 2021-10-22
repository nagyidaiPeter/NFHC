using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Core
{
    public interface IAutoFacRegistrar
    {
        void RegisterDependencies(ContainerBuilder containerBuilder);
    }
}
