Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.Math2D

    Public Class Transform

        ''' <summary>
        ''' angle for rotation
        ''' </summary>
        ''' <returns></returns>
        Public Property theta As Double
        ''' <summary>
        ''' translate x
        ''' </summary>
        ''' <returns></returns>
        Public Property tx As Double
        ''' <summary>
        ''' translate y
        ''' </summary>
        ''' <returns></returns>
        Public Property ty As Double
        ''' <summary>
        ''' scale x
        ''' </summary>
        ''' <returns></returns>
        Public Property scalex As Double
        ''' <summary>
        ''' scale y
        ''' </summary>
        ''' <returns></returns>
        Public Property scaley As Double

        Public Overrides Function ToString() As String
            Return $"rotate_theta:{theta.ToString("F2")}, translate=({tx.ToString("F2")},{ty.ToString("F2")}), scale=({scalex.ToString("F2")},{scaley.ToString("F2")})"
        End Function

        Public Function ApplyTo(polygon As Polygon2D) As Polygon2D
            '应用变换到多边形[5](@ref)
            Dim transformed As New Polygon2D()
            transformed.xpoints = New Double(polygon.length - 1) {}
            transformed.ypoints = New Double(polygon.length - 1) {}

            Dim cosTheta As Double = std.Cos(theta)
            Dim sinTheta As Double = std.Sin(theta)

            For i As Integer = 0 To polygon.length - 1
                Dim x As Double = polygon.xpoints(i)
                Dim y As Double = polygon.ypoints(i)

                ' 缩放
                x *= scalex
                y *= scaley

                ' 旋转
                Dim xRotated As Double = x * cosTheta - y * sinTheta
                Dim yRotated As Double = x * sinTheta + y * cosTheta

                ' 平移
                transformed.xpoints(i) = xRotated + tx
                transformed.ypoints(i) = yRotated + ty
            Next

            Return transformed
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(args As (theta As Double, tx As Double, ty As Double, scalex As Double, scaley As Double)) As Transform
            Return New Transform With {
                .theta = args.theta,
                .tx = args.tx,
                .ty = args.ty,
                .scalex = args.scalex,
                .scaley = args.scaley
            }
        End Operator

    End Class
End Namespace