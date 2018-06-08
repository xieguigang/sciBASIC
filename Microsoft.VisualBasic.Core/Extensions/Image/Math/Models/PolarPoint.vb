Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' 极坐标点
    ''' </summary>
    Public Class PolarPoint

        Public Property Radius As Double
        ''' <summary>
        ''' Unit in degree.(单位为度)
        ''' </summary>
        ''' <returns></returns>
        Public Property Angle As Single

        ''' <summary>
        ''' 与这个极坐标点等价的笛卡尔直角坐标系上面的坐标点
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Point As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Radius, Angle).ToCartesianPoint
            End Get
        End Property

        ''' <summary>
        ''' 显示这个极坐标点
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"({Radius}, {Angle}°)"
        End Function

        Public Shared Widening Operator CType(polar As (radius#, angle!)) As PolarPoint
            Return New PolarPoint With {
                .Angle = polar.angle,
                .Radius = polar.radius
            }
        End Operator
    End Class
End Namespace