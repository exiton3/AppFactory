using System.Collections.Generic;

namespace AppFactory.Framework.Domain.Infrastructure.Graph
{
    public class Node<T>
    {
        public Node(T elements, string name)
        {
            Elements = new List<T> {elements};
            Name = name;
        }
        public List<T> Elements { get; private set; }
        public string Name { get; private set; }

        public List<Edge<T>> Edges { get; set; }
        public bool Marked { get; protected set; }

        public void Mark()
        {
            Marked = true;
        }
        public void Unmark()
        {
            Marked = false;
        }
    }
}