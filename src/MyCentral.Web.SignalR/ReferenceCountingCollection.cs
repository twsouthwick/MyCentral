using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MyCentral.Web.Hubs
{
    public class ReferenceCountingCollection<TKey> : IEnumerable<TKey>
         where TKey : notnull
    {
        private ImmutableDictionary<TKey, int> _cache = ImmutableDictionary.Create<TKey, int>();

        public void Add(TKey key)
        {
            ImmutableInterlocked.AddOrUpdate(
                ref _cache, key, static h => 1, static (_, v) => v + 1);
        }

        public bool Remove(TKey key)
        {
            if (!_cache.TryGetValue(key, out var current))
            {
                return false;
            }

            var newValue = current - 1;

            if (newValue == 0)
            {
                ImmutableInterlocked.TryRemove(ref _cache, key, out var result);

                if (result > 1)
                {
                    Add(key);
                    return false;
                }

                return true;
            }
            else
            {
                if (!ImmutableInterlocked.TryUpdate(ref _cache, key, newValue, current))
                {
                    return Remove(key);
                }

                return true;
            }
        }

        public IEnumerator<TKey> GetEnumerator()
            => _cache.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
