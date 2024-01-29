
Imports System.Drawing
Imports System.Xml.Serialization

Namespace Scripting

    Public Module DataGenerator

        Public Delegate Function DataFunction(x As Double) As Double

        Public Iterator Function generateDataTuples([function] As DataFunction, from As Double, [to] As Double, size As Integer) As IEnumerable(Of DataPoint)
            For Each xi As Double In seq(from, [to], by:=([to] - from) / size)
                Yield New DataPoint(xi, [function](xi))
            Next
        End Function

    End Module

    Public Class DataPoint

        <XmlAttribute("x")> Public Property X As Double
        <XmlAttribute("y")> Public Overridable Property Y As Double

        Sub New()
        End Sub

        Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        Public Shared Narrowing Operator CType(point As DataPoint) As PointF
            Return New PointF(point.X, point.Y)
        End Operator
    End Class
End Namespace