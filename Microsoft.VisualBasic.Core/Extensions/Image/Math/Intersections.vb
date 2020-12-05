Namespace Imaging.Math2D

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