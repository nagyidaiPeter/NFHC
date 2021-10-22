using Autofac;
using Autofac.Core;
using NfhcModel.Debuggers;
using NfhcModel.Network;
using NfhcModel.Network.ServerModul;
using NfhcModel.Network.ServerModul.ServerProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Core
{
    public class ServerAutoFacRegistrar : IAutoFacRegistrar
    {

        public virtual void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ServerConfig>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Server>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<PlayerManager>().SingleInstance();

            containerBuilder.RegisterType<ServerChatProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerPlayerDataProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerSceneSyncProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerPlayerPosProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerLevelDetailsSyncProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerEnemyPosProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ServerGameEntityProcessor>().InstancePerLifetimeScope();

            //containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            //    .AsClosedTypesOf(typeof(IProcessor))
            //    .AsImplementedInterfaces();

            //containerBuilder.RegisterType<LiteNetLibServer>()
            //                .As<PoCoopServer>()
            //                .SingleInstance();
        }
    }
}
