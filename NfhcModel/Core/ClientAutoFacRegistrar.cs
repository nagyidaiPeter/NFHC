using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NfhcModel.Debuggers;
using NfhcModel.Helpers;
using NfhcModel.Network;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.ClientModul.ClientProcessors;
using NfhcModel.Network.ServerModul.ServerProcessors;

namespace NfhcModel.Core
{
    public class ClientAutoFacRegistrar : IAutoFacRegistrar
    {
        private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();
        private readonly IModule[] modules;

        public ClientAutoFacRegistrar(params IModule[] modules)
        {
            this.modules = modules;
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            foreach (IModule module in modules)
            {
                containerBuilder.RegisterModule(module);
            }

            RegisterCoreDependencies(containerBuilder);
            RegisterPacketProcessors(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<BaseDebugger>()
                            .As<BaseDebugger>()
                            .AsSelf()
                            .SingleInstance();

            containerBuilder.RegisterType<ClientConfig>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<Client>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<PlayerManager>().SingleInstance();

            //containerBuilder.RegisterType<MultiplayerSessionManager>()
            //                .As<IMultiplayerSession>()
            //                .As<IPacketSender>()
            //                .InstancePerLifetimeScope();

        }

        private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
        {
            //containerBuilder
            //    .RegisterAssemblyTypes(currentAssembly)
            //    .AsClosedTypesOf(typeof(ClientPacketProcessor<>))
            //    .InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientChatProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientPlayerDataProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientSceneSyncProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientPlayerPosProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientLevelDetailsSyncProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientEnemyPosProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ClientGameEntityProcessor>().InstancePerLifetimeScope();

        }

    }
}
