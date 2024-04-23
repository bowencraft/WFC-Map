using System;
using UnityEngine;

public class MultipleTile : MonoBehaviour
{
    // 使用 [Flags] 属性定义 EdgeType 为位域，允许组合多个值
    [Flags]
    public enum EdgeType
    {
        None = 0,
        E01 = 1 << 0,
        E10 = 1 << 1,
        E11 = 1 << 2,
        E00 = 1 << 3,
        T0 = 1 << 4,
        T1 = 1 << 5,
        T2 = 1 << 6,
    }

    // 定义 Edge 作为存储边缘类型和期望类型的结构
    [Serializable]
    public struct Edge
    {
        public EdgeType OwnType; // 本边的类型
        public EdgeType DesiredType; // 与之相邻边的期望类型

        public Edge(EdgeType own, EdgeType desired)
        {
            OwnType = own;
            DesiredType = desired;
        }
    }

    // 为每个方向使用 Edge 结构而不是单一的 EdgeType
    public Edge LeftEdge;
    public Edge RightEdge;
    public Edge FrontEdge; // New edge for 3D
    public Edge BackEdge; // New edge for 3D
    public Edge UpEdge;
    public Edge DownEdge;

    public int RotationVariant = 0; // 新增：旋转变种值，范围从0到3，每个数值代表旋转90度

    // 旋转边缘，获取实际用于比较的边缘
    private Edge GetRotatedEdge(Edge[] edges, int rotation, Vector3Int direction)
    {
        // 将方向旋转对应次数
        for(int i = 0; i < rotation; i++)
        {
            direction = new Vector3Int(-direction.z, direction.y, direction.x);
        }
        if (direction == Vector3Int.left) return edges[0];
        if (direction == Vector3Int.right) return edges[1];
        if (direction == Vector3Int.up) return edges[2];
        if (direction == Vector3Int.down) return edges[3];
        if (direction == Vector3Int.forward) return edges[4]; // New direction for 3D
        if (direction == Vector3Int.back) return edges[5]; // New direction for 3D

        throw new ArgumentException("Invalid direction");
    }


    public static bool IsCompatible(MultipleTile a, MultipleTile b, Vector3Int direction)
    {
        Edge[] aEdges = {a.LeftEdge, a.RightEdge, a.UpEdge, a.DownEdge, a.FrontEdge, a.BackEdge};
        Edge[] bEdges = {b.LeftEdge, b.RightEdge, b.UpEdge, b.DownEdge, b.FrontEdge, b.BackEdge};

        Edge aEdge = a.GetRotatedEdge(aEdges, a.RotationVariant, direction);
        Edge bEdge = b.GetRotatedEdge(bEdges, b.RotationVariant, -direction);

        return (aEdge.OwnType & bEdge.DesiredType) != 0 && (bEdge.OwnType & aEdge.DesiredType) != 0;
    }
    //
    // // 使用此方法初始化示例（只是一个示例，实际使用中可能有所不同）
    // void Start()
    // {
    //     // 初始化边缘类型和期望类型
    //     LeftEdge = new Edge(EdgeType.Road, EdgeType.Building | EdgeType.Grass);
    //     RightEdge = new Edge(EdgeType.Building, EdgeType.Road | EdgeType.HighRoad);
    // }
}