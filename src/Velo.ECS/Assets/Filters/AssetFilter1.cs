using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velo.Collections.Enumerators;
using Velo.ECS.Components;

namespace Velo.ECS.Assets.Filters
{
    public interface IAssetFilter<TComponent> : IAssetFilter, IEnumerable<Asset<TComponent>>
        where TComponent : IComponent
    {
        bool TryGet(int assetId, out Asset<TComponent> asset);

        IEnumerable<Asset<TComponent>> Where<TArg>(Func<Asset<TComponent>, TArg, bool> filter, TArg arg);
    }

    [DebuggerTypeProxy(typeof(DebuggerVisualizer<>))]
    [DebuggerDisplay("Length = {" + nameof(Length) + "}")]
    internal sealed class AssetFilter<TComponent> : IAssetFilter<TComponent>
        where TComponent : IComponent
    {
        public int Length => _assets.Length;

        private readonly Asset<TComponent>[] _assets;

        public AssetFilter(Asset[] assets)
        {
            var buffer = new List<Asset<TComponent>>();
            foreach (var asset in assets)
            {
                if (asset.TryGetComponent<TComponent>(out var component))
                {
                    buffer.Add(new Asset<TComponent>(asset, component));
                }
            }

            _assets = buffer.ToArray();
        }

        public bool Contains(int assetId) => TryGet(assetId, out _);

        public bool TryGet(int assetId, out Asset<TComponent> asset)
        {
            foreach (var exists in _assets)
            {
                if (exists.Entity.Id != assetId) continue;

                asset = exists;
                return true;
            }

            asset = default;
            return false;
        }

        public IEnumerable<Asset<TComponent>> Where<TArg>(Func<Asset<TComponent>, TArg, bool> filter, TArg arg)
        {
            return new ArrayWhereEnumerator<Asset<TComponent>, TArg>(_assets, filter, arg);
        }

        public IEnumerator<Asset<TComponent>> GetEnumerator()
        {
            return new ArrayEnumerator<Asset<TComponent>>(_assets);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}