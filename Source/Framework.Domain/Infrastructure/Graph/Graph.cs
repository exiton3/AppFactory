using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Domain.Infrastructure.Graph
{
    //Tests coverage note: covered by functional tests in BomLoopCheckingServiceFuncTests
    public class Graph<T> where T : class
    {
        public Graph(List<T> elements, Func<T, string> childKeyExtractor, Func<T, string> keyExtractor)
        {
            Nodes = new List<Node<T>>();
            foreach (var element in elements)
            {
                var key = keyExtractor(element);
                var existed = Nodes.FirstOrDefault(n => n.Name == key);
                if (existed != null)
                {
                    existed.Elements.Add(element);
                }
                else
                {
                    Nodes.Add(new Node<T>(element, key));
                }
            }

            foreach (var node in Nodes)
            {
                node.Edges = new List<Edge<T>>();
                foreach (var element in node.Elements)
                {
                    var childName = childKeyExtractor(element);
                    if (childName == null) continue;

                    var childNode = Nodes.FirstOrDefault(n => n.Name == childName);
                    if (childNode != null)
                    {
                        node.Edges.Add(new Edge<T>(node, childNode));
                    }
                }
            }
        }

        private LinkedList<Path<T>> _cycles;
        public LinkedList<Path<T>> FindCycles()
        {
            _cycles = new LinkedList<Path<T>>();
            foreach (var node in Nodes)
            {
                ProceedNode(node, new LinkedList<Node<T>>(), false);
            }
            Nodes.ForEach(n => n.Unmark());
            return _cycles;
        }

        private void ProceedNode(Node<T> node, LinkedList<Node<T>> currentPath, bool cameFromMarked)
        {
            if (currentPath.Contains(node))
            {
                while (currentPath.First() != node)
                {
                    currentPath.RemoveFirst();
                }
                currentPath.AddLast(node);
                _cycles.AddLast(new Path<T>(currentPath));
            }

            if (cameFromMarked && node.Marked) return;

            node.Mark();
            currentPath.AddLast(node);
            if (node.Edges == null) return;

            foreach (var edge in node.Edges)
            {
                ProceedNode(edge.To, new LinkedList<Node<T>>(currentPath), node.Marked);
            }
        }

        public List<Node<T>> Nodes { get; private set; }

    }
}