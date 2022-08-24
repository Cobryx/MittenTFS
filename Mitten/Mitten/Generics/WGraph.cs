using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    public class WGraph
    {
        List<Waypoint> vertices = new List<Waypoint>();
        List<Edge> edges = new List<Edge>();
        double[,] adjacencyMatrix;
        bool graphFinalized = false;


        public WGraph()
        {
            
        }

        public int AddVertex(Waypoint wp)
        {
            vertices.Add(wp);
            return vertices.Count - 1;
        }

        public int AddEdge(Edge e)
        {
            edges.Add(e);
            return edges.Count - 1;
        }

        public void RemoveEdge(int edgeNumber)
        {
            edges.RemoveAt(edgeNumber);
        }

        public void RemoveEdge(Edge e)
        {
            edges.Remove(e);
        }

        public void RemoveEdgeByVertex(int vertexNumber)
        {
            edges.RemoveAll(x => x.from == vertexNumber || x.to == vertexNumber);
        }

        public void RemoveEdgeByVertex(Waypoint w)
        {
            int vertexNumber = vertices.FindIndex(x => x == w);
            edges.RemoveAll(x => x.from == vertexNumber || x.to == vertexNumber);
        }

        public void RemoveEdgeBetweenVertices(int v1, int v2)
        {
            edges.RemoveAll(x => (x.from == v1 && x.to == v2) || (x.from == v2 && x.to == v1));
        }

        public void RemoveVertex(int vertexNumber)
        {
            vertices.RemoveAt(vertexNumber);
            edges.RemoveAll(x => x.from == vertexNumber || x.to == vertexNumber);
            foreach (Edge e in edges)
            {
                if (e.to > vertexNumber)
                {
                    e.to--;
                }
                if (e.from > vertexNumber)
                {
                    e.from--;
                }
            }
        }

        public Waypoint getVertex(int vertexNumber)
        {
            return vertices[vertexNumber];
        }

        public Edge getEdge(int edgeNumber)
        {
            return edges[edgeNumber];
        }

        public List<Waypoint> Vertices
        {
            get { return vertices; }
        }

        public List<Edge> Edges
        {
            get { return edges; }
        }

        /*
        public void AddEdge(int v1, int v2, Double weight)
        {
            
            adjacencyMatrix[v1, v2] = weight;
            adjacencyMatrix[v2, v1] = weight;
        }
        */

        public double[,] BuildAdjacencyMatrix()
        {
            if (!graphFinalized)
            {
                adjacencyMatrix = new double[vertices.Count, vertices.Count];

                for (int i = 0; i < vertices.Count; i++)
                {
                    for (int j = 0; j < vertices.Count; j++)
                    {
                        adjacencyMatrix[i, j] = 0;
                    }
                }

                foreach (Edge e in edges)
                {
                    adjacencyMatrix[e.from, e.to] = e.weight;
                    adjacencyMatrix[e.to, e.from] = e.weight;
                }

                graphFinalized = true;
            }
            return adjacencyMatrix;
        }
    }
}

public class Edge
{
    public int from { get; set; }
    public int to { get; set; }
    public Double weight { get; set; }

    public Edge(int from, int to, Double weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
    }
}

public class Dijkstra
{
   
    public double[] dist { get; private set; }
    public int[] path { get; private set; }

    private List<int> queue = new List<int>();

    private void Initialize(int s, int len)
    {
        dist = new double[len];
        path = new int[len];

        for (int i = 0; i < len; i++)
        {
            dist[i] = Double.PositiveInfinity;

            queue.Add(i);
        }

        dist[s] = 0;
        path[s] = -1;
    }

    private int GetNextVertex()
    {
        double min = Double.PositiveInfinity;
        int Vertex = -1;

        foreach (int j in queue)
        {
            if (dist[j] <= min)
            {
                min = dist[j];
                Vertex = j;
            }
        }

        queue.Remove(Vertex);

        return Vertex;

    }

    public Dijkstra(double[,] G, int s)
    {
        if (G.GetLength(0) < 1 || G.GetLength(0) != G.GetLength(1))
        {
            throw new ArgumentException("Graph error, wrong format or no nodes to compute");
        }

        int len = G.GetLength(0);

        Initialize(s, len);

        while (queue.Count > 0)
        {
            int u = GetNextVertex();

            for (int v = 0; v < len; v++)
            {
                if (G[u, v] < 0)
                {
                    throw new ArgumentException("Graph contains negative edge(s)");
                }

                if (G[u, v] > 0)
                {
                    if (dist[v] > dist[u] + G[u, v])
                    {
                        dist[v] = dist[u] + G[u, v];
                        path[v] = u;
                    }
                }
            }
        }
    }
}