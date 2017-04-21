Namespace Drawing3D.Models.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Circle : Inherits Path3D

        <Obsolete>
        Public Sub New(origin As Point3D, radius As Double)
            Call Me.New(origin, radius, 20)
        End Sub

        ''' <summary>
        ''' 构建三维空间之中的一个圆弧路径
        ''' </summary>
        ''' <param name="origin">相对坐标系原点</param>
        ''' <param name="radius"></param>
        ''' <param name="vertices">构成这个圆形的顶点的数量</param>
        Public Sub New(origin As Point3D, radius As Double, vertices As Double)
            Call MyBase.New()

            Dim deltaAngle# = 2 * Math.PI / vertices

            For i As Integer = 0 To vertices - 1
                Dim p As New Point3D(
                    (radius * Math.Cos(i * deltaAngle)) + origin.X,
                    (radius * Math.Sin(i * deltaAngle)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class

    ''' <summary>
    ''' 圆弧
    ''' </summary>
    Public Class Arc : Inherits Path3D

        Public Sub New(origin As Point3D, radius#, startAngle#, sweepAngle#, vertices#)
            Call MyBase.New

            Dim deltaAngle# = 2 * Math.PI * (sweepAngle / 360) / vertices
            Dim angle# = 2 * Math.PI * (startAngle / 360)

            For i As Integer = 0 To vertices - 1
                Dim p As New Point3D(
                    (radius * Math.Cos(angle)) + origin.X,
                    (radius * Math.Sin(angle)) + origin.Y,
                    origin.Z)

                angle += deltaAngle

                Call Push(p)
            Next

            ' push圆心
            Call Push(origin)
        End Sub

        ''' <summary>
        ''' Create a <see cref="Path3D"/> model that similar with <see cref="Paths.Circle"/> model.
        ''' </summary>
        ''' <param name="origin"></param>
        ''' <param name="radius#"></param>
        ''' <param name="vertices#"></param>
        ''' <returns></returns>
        Public Shared Function Circle(origin As Point3D, radius#, vertices#) As Arc
            Return New Arc(origin, radius, 0, 360, vertices)
        End Function
    End Class
End Namespace