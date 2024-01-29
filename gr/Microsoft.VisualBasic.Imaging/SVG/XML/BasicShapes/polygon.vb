
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace SVG.XML

    ''' <summary>
    ''' 不规则的多边形对象
    ''' </summary>
    ''' 
    <XmlRoot("polygon")>
    Public Class polygon : Inherits polyline

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pts As IEnumerable(Of PointF))
            points = pts _
                .Select(Function(pt) $"{pt.X},{pt.Y}") _
                .ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParsePoints() As PointF()
            Return points _
                .Select(AddressOf FloatPointParser) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Offset2D(offset As PointF) As PointF()
            Return ParsePoints _
                .Select(Function(pt)
                            Return New PointF With {
                                .X = pt.X + offset.X,
                                .Y = pt.Y + offset.Y
                            }
                        End Function) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(polygon As polygon, offset As PointF) As polygon
            Return New polygon(polygon.Offset2D(offset)) With {
                .style = polygon.style,
                .id = polygon.id,
                .class = polygon.class
            }
        End Operator
    End Class

End Namespace