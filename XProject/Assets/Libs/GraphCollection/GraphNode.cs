﻿using System.Collections.Generic;

namespace GraphCollection
{
    public class GraphNode<T>
    {
        public readonly List<Neighbour> Neighbours;
        public bool Visited = false;
        public T Value;
        public int TentativeDistance;

        public GraphNode(T value)
        {
            Value = value;
            Neighbours = new List<Neighbour>();
        }

        public void AddNeighbour(GraphNode<T> graphNode, int distance)
        {
            for (int i = 0; i < Neighbours.Count; ++i)
            {
                if (Neighbours[i].GraphNode.Equals(graphNode))
                    return;
            }
            Neighbours.Add(new Neighbour(graphNode, distance));
            graphNode.Neighbours.Add(new Neighbour(this, distance));
        }

        public bool Equals(GraphNode<T> compareNode)
        {
            return Value.Equals(compareNode.Value);
        }

        public struct Neighbour
        {
            public int Distance;
            public GraphNode<T> GraphNode;

            public Neighbour(GraphNode<T> graphNode, int distance)
            {
                GraphNode = graphNode;
                Distance = distance;
            }
        }
    }
}