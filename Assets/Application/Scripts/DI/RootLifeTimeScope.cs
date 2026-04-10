using VContainer;
using VContainer.Unity;

namespace Application
{
    public sealed class RootLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CheckerboardFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PawnFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LineFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ConnectionFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ConnectorFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<DragSystem>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<CheckerboardRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PawnRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ConnectorsRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ConnectorRecolorSystem>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<SelectionSystem>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SelectableRegistry>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterEntryPoint<CameraController>();
            builder.RegisterEntryPoint<AppController>();
        }
    }
}