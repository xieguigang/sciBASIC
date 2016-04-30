Imports System.Drawing

Namespace Drawing3D

    ''' <summary>
    ''' 标傲世一个3D物体的模型
    ''' </summary>
    Public MustInherit Class Model

        ''' <summary>
        ''' 顶点
        ''' </summary>
        Protected _vertices(8) As Point3D
        ''' <summary>
        ''' 表面
        ''' </summary>
        Protected _faces(6, 4) As Integer
        ''' <summary>
        ''' 表面的颜色
        ''' </summary>
        Protected _colors(6) As Color
        ''' <summary>
        ''' 颜色刷子的缓存
        ''' </summary>
        Protected _brushes(6) As Brush

        Public ReadOnly Property Length As Integer
            Get
                Return _vertices.Length
            End Get
        End Property

        Public MustOverride Sub Draw(gdi As Graphics)

        Public Function RotateX(angle As Integer, clientSize As Size) As Point3D()
            Dim t(Length - 1) As Point3D
        End Function
    End Class

    Public Class Cube : Inherits Model

        Public Overrides Sub Draw(gdi As Graphics)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace