
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace SVG.XML

    ''' <summary>
    ''' 不规则的多边形对象
    ''' </summary>
    Public Class polygon : Inherits node

        ''' <summary>
        ''' 定点坐标列表
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property points As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return cache
            End Get
            Set(value As String())
                cache = value _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
                data = cache _
                    .Select(AddressOf FloatPointParser) _
                    .ToArray
            End Set
        End Property

        Dim data As PointF()
        Dim cache$()

        Sub New()
        End Sub

        Sub New(pts As IEnumerable(Of PointF))
            data = pts.ToArray
            cache = data _
                .Select(Function(pt) $"{pt.X},{pt.Y}") _
                .ToArray
        End Sub

        Public Shared Operator +(polygon As polygon, offset As PointF) As polygon
            Dim points As PointF() = polygon _
                .data _
                .Select(Function(pt)
                            Return New PointF With {
                                .X = pt.X + offset.X,
                                .Y = pt.Y + offset.Y
                            }
                        End Function) _
                .ToArray
            Return New polygon(points) With {
                .style = polygon.style,
                .id = polygon.id,
                .class = polygon.class
            }
        End Operator
    End Class

End Namespace