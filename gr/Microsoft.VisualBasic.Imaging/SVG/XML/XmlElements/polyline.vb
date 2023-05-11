
Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace SVG.XML

    <XmlRoot("polyline")>
    Public Class polyline : Inherits node

        ''' <summary>
        ''' 定点坐标列表
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property points As String()
        <XmlAttribute("marker-end")>
        Public Property markerEnd As String

        Sub New()
        End Sub

        Public Sub New(data As IEnumerable(Of Double), Optional numDecimalPlaces As Integer? = Nothing)
            If numDecimalPlaces Is Nothing Then
                points = data.Select(Function(d) d.ToString).ToArray
            Else
                points = data _
                    .Select(Function(d)
                                Return d.ToString($"0.{New String("#"c, numDecimalPlaces.Value)}", CultureInfo.InvariantCulture)
                            End Function) _
                    .ToArray
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(x As IEnumerable(Of Double), y As IEnumerable(Of Double), Optional numDecimalPlaces As Integer? = Nothing)
            Call Me.New(x.Zip(y, Function(a, b) New Double() {a, b}).SelectMany(Function(d) d), numDecimalPlaces)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return points.JoinBy(" ")
        End Function

        Public Shared Operator +(line As polyline, offset As PointF) As polyline
            ' Throw New NotImplementedException
            Return Nothing
        End Operator
    End Class
End Namespace