Imports System.Drawing
Imports System.Xml.Serialization

Namespace Imaging.Math2D

    ''' <summary>
    ''' 2D Affine Transformation parameters.
    ''' This represents a more general transform than similarity, allowing for non-uniform scaling and shearing.
    ''' x' = ax + by + c
    ''' y' = dx + ey + f
    ''' </summary>
    Public Class AffineTransform

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

        ''' <summary>
        ''' Applies this transform to a point.
        ''' </summary>
        Public Function ApplyToPoint(pt As PointF) As PointF
            Return New PointF(
                a * pt.X + b * pt.Y + c,
                d * pt.X + e * pt.Y + f
            )
        End Function
    End Class
End Namespace