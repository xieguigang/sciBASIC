Namespace Drawing3D

    ''' <summary>
    ''' A 3D spatial heatmap point, 3d point [x,y,z] combine with the color scaler(or heatmap) data
    ''' </summary>
    Public Interface IPointCloud : Inherits PointF3D

        Property intensity As Double

    End Interface
End Namespace