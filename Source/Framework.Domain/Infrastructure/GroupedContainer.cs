using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Framework.Domain.Infrastructure
{
    public class GroupedContainer<TKey, TValue> : ConcurrentDictionary<TKey, BlockingCollection<TValue>>
        where TKey : class
        where TValue : class
    {
        private readonly object _sync = new object();
        public readonly Expression<Func<TValue, TKey>> KeyExtractor;
        private readonly Func<TValue, TKey> compiledExtractor;

        public GroupedContainer(Expression<Func<TValue, TKey>> keyExtractor)
        {
            KeyExtractor = keyExtractor;
            compiledExtractor = keyExtractor.Compile();
        }

        public GroupedContainer<TKey, TValue> Add(TValue item)
        {
            lock (_sync)
            {
                BlockingCollection<TValue> bag;
                var key = compiledExtractor(item);
                if (TryGetValue(key, out bag))
                {
                    bag.Add(item);
                }
                else
                {
                    bag = new BlockingCollection<TValue> { item };
                    this[key] = bag;
                }
                return this;
            }
        }

        public GroupedContainer<TKey, TValue> AddAll(IEnumerable<TValue> list)
        {
            lock (_sync)
            {
                foreach (var item in list)
                {
                    Add(item);
                }
                return this;
            }
        }

    }
}
