Imports System.Drawing

Namespace Drawing3D

    ''' <summary>
    ''' 标傲世一个3D物体的模型
    ''' </summary>
    Public MustInherit Class Model : Inherits ModelData

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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="angle"></param>
        ''' <param name="clientSize"></param>
        ''' <param name="aixs"></param>
        ''' <returns></returns>
        ''' <remarks>http://codentronix.com/2011/05/25/rotating-solid-cube-using-vb-net-and-gdi/</remarks>
        Public Function Rotate(angle As Integer, clientSize As Size, aixs As Aixs) As ModelView
            Dim t(Length - 1) As Point3D
            Dim v As Point3D
            Dim sx As Boolean = aixs.HasFlag(Aixs.X)
            Dim sy As Boolean = aixs.HasFlag(Aixs.Y)
            Dim sz As Boolean = aixs.HasFlag(Aixs.Z)
            Dim avgZ(6) As Double
            Dim order(6) As Integer

            ' Transform all the points and store them on the "t" array.
            For i As Integer = 0 To Length - 1
                v = _vertices(i)

                If sx Then v = v.RotateX(angle)
                If sy Then v = v.RotateY(angle)
                If sz Then v = v.RotateZ(angle)

                t(i) = v.Project(clientSize.Width, clientSize.Height, 256, 4)
            Next

            ' Compute the average Z value of each face.
            For i = 0 To 5
                avgZ(i) = (t(_faces(i, 0)).Z + t(_faces(i, 1)).Z + t(_faces(i, 2)).Z + t(_faces(i, 3)).Z) / 4.0
                order(i) = i
            Next

            ' Next we sort the faces in descending order based on the Z value.
            ' The objective is to draw distant faces first. This is called
            ' the PAINTERS ALGORITHM. So, the visible faces will hide the invisible ones.
            ' The sorting algorithm used is the SELECTION SORT.

            Dim iMax As Integer
            Dim tmp As Double

            For i = 0 To 4
                iMax = i
                For j = i + 1 To 5
                    If avgZ(j) > avgZ(iMax) Then
                        iMax = j
                    End If
                Next
                If iMax <> i Then
                    tmp = avgZ(i)
                    avgZ(i) = avgZ(iMax)
                    avgZ(iMax) = tmp

                    tmp = order(i)
                    order(i) = order(iMax)
                    order(iMax) = tmp
                End If
            Next
        End Function
    End Class

    Public Class ModelView : Inherits ModelData

        ''' <summary>
        ''' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
        ''' (表面的绘制的顺序)
        ''' </summary>
        Dim order(6) As Integer

        Public Sub UpdateGraphics(gdi As Graphics)

        End Sub
    End Class

    Public Class ModelData

        ''' <summary>
        ''' 顶点
        ''' </summary>
        Protected _vertices(8) As Point3D
        ''' <summary>
        ''' 表面
        ''' </summary>
        Protected _faces(6, 4) As Integer
    End Class

    Public Enum Aixs As Byte
        X = 2
        Y = 4
        Z = 8
        All = X + Y + Z
    End Enum

    Public Class Cube : Inherits Model

        Public Overrides Sub Draw(gdi As Graphics)

        End Sub
    End Class
End Namespace