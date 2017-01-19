using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Class that handles map to mesh conversion using the marching squares algorithm
/// </summary>
public class MeshGenerator : MonoBehaviour {
    public float wallHeight = 5;

    private MeshFilter walls;
    private SquareGrid squareGrid;
    private List<Vector3> vertices;
    private List<int> triangles;

    private Dictionary<int, List<Triangle>> triangleDic = new Dictionary<int, List<Triangle>>();
    private HashSet<int> checkedVertices = new HashSet<int>();
    private List<List<int>> outlines = new List<List<int>>();

    private void Start() {
        walls = transform.GetChild(0).GetComponent<MeshFilter>();
    }

    public void GenerateMesh(Map<int> map, float squareSize) {
        triangleDic.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        squareGrid = new SquareGrid(map, squareSize);

        foreach(Square sqr in squareGrid) {
            Triangulate(sqr);
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }

    /// <summary>
    /// Create the wall for the generated cave mesh
    /// </summary>
    void CreateWallMesh() {
        CalculateOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();

        foreach(List<int> outline in outlines) {
            for(int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);
                wallVertices.Add(vertices[outline[i + 1]]);
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
        walls.gameObject.GetComponent<MeshCollider>().sharedMesh = wallMesh;
    }

    void Triangulate(Square square) {
        switch(square.configuration) {
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vIndex);
                checkedVertices.Add(square.topRight.vIndex);
                checkedVertices.Add(square.bottomRight.vIndex);
                checkedVertices.Add(square.bottomLeft.vIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        VertAssign(points);

        if(points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if(points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if(points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if(points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void VertAssign(Node[] points) {
        for(int i = 0; i < points.Length; i++) {
            if(points[i].vIndex == -1) {
                points[i].vIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vIndex);
        triangles.Add(b.vIndex);
        triangles.Add(c.vIndex);

        Triangle triangle = new Triangle(a.vIndex, b.vIndex, c.vIndex);
        AddToDic(triangle.a, triangle);
        AddToDic(triangle.b, triangle);
        AddToDic(triangle.c, triangle);
    }

    void AddToDic(int vertexIndexKey, Triangle triangle) {
        if(triangleDic.ContainsKey(vertexIndexKey)) {
            triangleDic[vertexIndexKey].Add(triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDic.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateOutlines() {
        for(int vIndex = 0; vIndex < vertices.Count; vIndex++) {
            if(!checkedVertices.Contains(vIndex)) {
                int newOutlineVertex = GetConnectedOutlineVertex(vIndex);
                if(newOutlineVertex != -1) {
                    checkedVertices.Add(vIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex) {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if(nextVertexIndex != -1) {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vIndex) {
        List<Triangle> trianglesWithVertex = triangleDic[vIndex];

        for(int i = 0; i < trianglesWithVertex.Count; i++) {
            Triangle triangle = trianglesWithVertex[i];

            for(int j = 0; j < 3; j++) {
                int v = triangle[j];
                if(v != vIndex && !checkedVertices.Contains(v)) {
                    if(IsOutlineEdge(vIndex, v)) {
                        return v;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB) {
        List<Triangle> trianglesContainingVertexA = triangleDic[vertexA];
        int sharedTriangleCounter = 0;

        for(int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if(trianglesContainingVertexA[i].Contains(vertexB)) {
                if(sharedTriangleCounter > 1) {
                    break;
                }
                sharedTriangleCounter++;
            }
        }
        return sharedTriangleCounter == 1;
    }

    struct Triangle {
        public int a;
        public int b;
        public int c;
        int[] vertices;

        public Triangle(int a, int b, int c) {
            this.a = a;
            this.b = b;
            this.c = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i] {
            get {
                return vertices[i];
            }
        }

        public bool Contains(int v) {
            return v == a || v == b || v == c;
        }
    }

    public class SquareGrid : IEnumerable<Square> {
        Square[,] squares;

        public int width { get { return squares.GetLength(0); } }
        public int height { get { return squares.GetLength(1); } }

        public Square this[int i, int j] {
            get {
                return squares[i, j];
            }
            set {
                squares[i, j] = value;
            }
        }

        public SquareGrid(Map<int> map, float squareSize) {
            int nodeCountX = map.width;
            int nodeCountY = map.width;
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int x = 0; x < nodeCountX; x++) {
                for(int y = 0; y < nodeCountY; y++) {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for(int x = 0; x < nodeCountX - 1; x++) {
                for(int y = 0; y < nodeCountY - 1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }

        public IEnumerator<Square> GetEnumerator() {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    yield return squares[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    yield return squares[x, y];
                }
            }
        }
    }

    public class Square {

        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft) {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            centreTop = this.topLeft.right;
            centreRight = this.bottomRight.above;
            centreBottom = this.bottomLeft.right;
            centreLeft = this.bottomLeft.above;

            if(this.topLeft.active)
                configuration += 8;
            if(this.topRight.active)
                configuration += 4;
            if(this.bottomRight.active)
                configuration += 2;
            if(this.bottomLeft.active)
                configuration += 1;
        }
    }

    public class Node {
        public Vector3 position;
        public int vIndex = -1;

        public Node(Vector3 pos) {
            position = pos;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos) {
            this.active = active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
