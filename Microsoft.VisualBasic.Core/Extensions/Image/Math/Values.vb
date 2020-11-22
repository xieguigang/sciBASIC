Namespace Imaging.Math2D

    ''' <summary>
    ''' 请注意，视图上面的象限的位置和计算机之中的象限是反过来的
    ''' </summary>
    Public Enum QuadrantRegions

        ''' <summary>
        ''' 重叠在一起
        ''' </summary>
        Origin = 0

        ''' <summary>
        ''' quadrant 1 = 0,90 ~ -90,0 ~ 270,360
        ''' </summary>
        RightTop
        YTop
        ''' <summary>
        ''' quadrant 2 = 90,180 ~ -180,-90 ~ 180,270
        ''' </summary>
        LeftTop
        XLeft
        ''' <summary>
        ''' quadrant 3 = 180,270 ~ -270,-180 ~ 90,180 
        ''' </summary>
        LeftBottom
        YBottom
        ''' <summary>
        ''' quadrant 4 = 270,360 ~ -270, -360 ~ 0, 90
        ''' </summary>
        RightBottom
        XRight
    End Enum

    ''' <summary>
    ''' 几何体之间的关系类型
    ''' </summary>
    Public Enum Intersections As Byte
        None
        ''' <summary>
        ''' 正切
        ''' </summary>
        Tangent
        ''' <summary>
        ''' 相交
        ''' </summary>
        Intersection
        ''' <summary>
        ''' 包围
        ''' </summary>
        Containment
    End Enum
End Namespace