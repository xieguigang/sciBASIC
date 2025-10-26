Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace Imaging.Math2D

    ''' <summary>
    ''' 2D Affine Transformation parameters.
    ''' This represents a more general transform than similarity, allowing for non-uniform scaling and shearing.
    ''' x' = ax + by + c
    ''' y' = dx + ey + f
    ''' </summary>
    Public Class AffineTransform : Implements GeometryTransform

        ''' <summary>
        ''' The ‘a’ parameter in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property a As Double = 1
        ''' <summary>
        ''' The ‘b’ parameter in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property b As Double = 0
        ''' <summary>
        ''' The ‘c’ parameter (translation x) in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property c As Double = 0
        ''' <summary>
        ''' The ‘d’ parameter in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property d As Double = 0
        ''' <summary>
        ''' The ‘e’ parameter in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property e As Double = 1
        ''' <summary>
        ''' The ‘f’ parameter (translation y) in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property f As Double = 0

        ''' <summary>
        ''' Creates an identity (no-op) transform.
        ''' </summary>
        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"2D Affine Transform {{ x' = {a.ToString("F4")} x + {b.ToString("F4")} y + {c.ToString("F4")}; y' = {d.ToString("F4")} x + {e.ToString("F4")} y + {f.ToString("F4")} }}"
        End Function

        ''' <summary>
        ''' Applies this transform to a point.
        ''' </summary>
        Public Function ApplyToPoint(pt As PointF) As PointF
            Return New PointF(
                a * pt.X + b * pt.Y + c,
                d * pt.X + e * pt.Y + f
            )
        End Function

        Public Function ApplyTo(polygon As Polygon2D) As Polygon2D Implements GeometryTransform.ApplyTo
            Dim t = polygon.AsEnumerable.Select(Function(pt) ApplyToPoint(pt)).ToArray
            Dim x As Double() = t.Select(Function(pt) CDbl(pt.X)).ToArray
            Dim y As Double() = t.Select(Function(pt) CDbl(pt.Y)).ToArray

            Return New Polygon2D(x, y)
        End Function
    End Class
End Namespace