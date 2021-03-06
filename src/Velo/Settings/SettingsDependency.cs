using System;
using Velo.DependencyInjection;
using Velo.DependencyInjection.Dependencies;
using Velo.Settings.Provider;
using Velo.Utils;

namespace Velo.Settings
{
    internal sealed partial class SettingsFactory
    {
        private sealed class SettingsDependency<TSettings> : IDependency
            where TSettings : class
        {
            public Type[] Contracts => _contracts ??= new[] {Implementation};

            public Type Implementation { get; }

            public DependencyLifetime Lifetime => DependencyLifetime.Singleton;

            private Type[]? _contracts;
            private readonly string _path;

            public SettingsDependency(string path)
            {
                _path = path;

                Implementation = Typeof<TSettings>.Raw;
            }

            public bool Applicable(Type contract)
            {
                return contract == Implementation;
            }

            public object GetInstance(Type contract, IServiceProvider services)
            {
                var settingsProvider = services.GetRequired<ISettingsProvider>();
                return settingsProvider.Get<TSettings>(_path);
            }

            #region Interfaces

            void IDependency.Init(IDependencyEngine engine)
            {
            }

            void IDisposable.Dispose()
            {
            }

            #endregion
        }
    }
}