namespace AppFactory.Framework.Domain.Infrastructure.Graph
{
    public class Edge<T>
    {
        public Edge(Node<T> from, Node<T> to)
        {
            From = from;
            To = to;
        }

        public Node<T> From { get; private set; }
        public Node<T> To { get; private set; }

    }
}