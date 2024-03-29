using System;
using UnityEngine;

public class MultipleTile : MonoBehaviour
{
    // 使用 [Flags] 属性定义 EdgeType 为位域，允许组合多个值
    [Flags]
    public enum EdgeType
    {
        None = 0,
        Road = 1 << 0,
        HighRoad = 1 << 1,
        Building = 1 << 2,
        Condo = 1 << 3,
        Grass = 1 << 4,
        RoadBarrier = 1 << 5
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
    public Edge UpEdge;
    public Edge DownEdge;

    public int RotationVariant = 0; // 新增：旋转变种值，范围从0到3，每个数值代表旋转90度

    // 旋转边缘，获取实际用于比较的边缘
    private Edge GetRotatedEdge(Edge[] edges, int rotation, Vector2Int direction)
    {
        // 将方向旋转对应次数
        for(int i = 0; i < rotation; i++)
        {
            direction = new Vector2Int(-direction.y, direction.x);
        }
        if (direction == Vector2Int.left) return edges[0];
        if (direction == Vector2Int.right) return edges[1];
        if (direction == Vector2Int.up) return edges[2];
        if (direction == Vector2Int.down) return edges[3];
        throw new ArgumentException("Invalid direction");
    }

    public static bool  IsCompatible(MultipleTile a, MultipleTile b, Vector2Int direction)
    {
        // 首先基于旋转变种值调整边缘
        Edge[] aEdges = {a.LeftEdge, a.RightEdge, a.UpEdge, a.DownEdge};
        Edge[] bEdges = {b.LeftEdge, b.RightEdge, b.UpEdge, b.DownEdge};

        Edge aEdge = a.GetRotatedEdge(aEdges, a.RotationVariant, direction);
        Edge bEdge = b.GetRotatedEdge(bEdges, b.RotationVariant, -direction); // 注意，b的方向是相反的

        // 现在aEdge和bEdge是考虑旋转后的实际边缘，进行兼容性比较
        return (aEdge.OwnType & bEdge.DesiredType) != 0 && (bEdge.OwnType & aEdge.DesiredType) != 0;
    }
    //
    // public static bool IsCompatible(MultipleTile a, MultipleTile b, Vector2Int direction)
    // {
    //     // 比较两个瓦片的边缘是否兼容，考虑期望的类型
    //     if (direction == Vector2Int.right)
    //     {
    //         return (a.RightEdge.OwnType & b.LeftEdge.DesiredType) != 0 && (b.LeftEdge.OwnType & a.RightEdge.DesiredType) != 0;
    //     }
    //     else if (direction == Vector2Int.left)
    //     {
    //         return (a.LeftEdge.OwnType & b.RightEdge.DesiredType) != 0 && (b.RightEdge.OwnType & a.LeftEdge.DesiredType) != 0;
    //     }
    //     else if (direction == Vector2Int.up)
    //     {
    //         return (a.UpEdge.OwnType & b.DownEdge.DesiredType) != 0 && (b.DownEdge.OwnType & a.UpEdge.DesiredType) != 0;
    //     }
    //     else if (direction == Vector2Int.down)
    //     {
    //         return (a.DownEdge.OwnType & b.UpEdge.DesiredType) != 0 && (b.UpEdge.OwnType & a.DownEdge.DesiredType) != 0;
    //     }
    //
    //     return false;
    // }

    // 使用此方法初始化示例（只是一个示例，实际使用中可能有所不同）
    void Start()
    {
        // 初始化边缘类型和期望类型
        LeftEdge = new Edge(EdgeType.Road, EdgeType.Building | EdgeType.Grass);
        RightEdge = new Edge(EdgeType.Building, EdgeType.Road | EdgeType.HighRoad);
        // 初始化 UpEdge 和 DownEdge...
    }
}