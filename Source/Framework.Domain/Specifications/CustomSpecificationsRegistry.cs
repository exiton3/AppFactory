using System;
using System.Collections.Generic;
using System.Linq;
using AppFactory.Framework.Domain.Paging;

namespace AppFactory.Framework.Domain.Specifications
{

    //TODO Delete if will be not used
    public class CustomSpecificationsRegistry : ICustomSpecificationsRegistry
    {
        private readonly Dictionary<Type, Func<object[], ISpecification>> _specifications =
            new Dictionary<Type, Func<object[], ISpecification>>();

        public ISpecification<T> GetSpecification<T>(Filter filter) where T : class
        {
            var modelType = typeof(T);
            if (!_specifications.ContainsKey(modelType))
                throw new KeyNotFoundException();

            var func = _specifications[modelType];
            var spec = func(filter.Values.Select(x => x.Value).ToArray());
            return (ISpecification<T>)spec;
        }
    }
}
