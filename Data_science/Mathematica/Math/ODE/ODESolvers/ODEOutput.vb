Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class ODEOutput : Implements INamedValue

    <XmlAttribute>
    Public Property ID As String Implements INamedValue.Key
    Public Property X As Sequence
    Public Property Y As NumericVector

    <XmlText>
    Public Property Description As String

    Public ReadOnly Property y0 As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Y.vector.First
        End Get
    End Property

    Public ReadOnly Property xrange As DoubleRange
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New DoubleRange(X.range)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Y.vector.GetJson
    End Function

    Public Iterator Function GetPointsData() As IEnumerable(Of PointF)
        Yield New PointF(X.range.Min, y0)

        For Each xi In X.ToArray.Skip(1).SeqIterator
            Yield New PointF(xi.value, Y(xi))
        Next
    End Function
End Class