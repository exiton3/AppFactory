using System.Collections.Generic;
using System.Linq;
using AppFactory.Framework.Domain.Infrastructure.Extensions;

namespace AppFactory.Framework.Domain.Infrastructure.Graph
{
    public class Path<T>
    {
        public Path(LinkedList<Node<T>> elements)
        {
            Elements = elements;
        }

        public LinkedList<Node<T>> Elements { get; private set; }
        public string GetTrace()
        {
            return Elements.Select(el => el.Name).JoinBySeparator('-');
        }
        public bool IsEmpty()
        {
            return Elements.Count == 0;
        }
    }
}