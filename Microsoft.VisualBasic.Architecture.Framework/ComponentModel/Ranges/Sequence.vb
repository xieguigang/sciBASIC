
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace ComponentModel.Ranges

    <XmlType("numeric-sequence")>
    Public Class Sequence : Implements IRanges(Of Double)

        <XmlElement("range")>
        Public Property Range As DoubleRange
        <XmlAttribute>
        Public Property n As Integer

        Public ReadOnly Property Steps As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Length / n
            End Get
        End Property

        Public ReadOnly Property Min As Double Implements IRange(Of Double).Min
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Min
            End Get
        End Property
        Public ReadOnly Property Max As Double Implements IRange(Of Double).Max
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Max
            End Get
        End Property

        Sub New(a#, b#, n%)
            Me.Range = New DoubleRange(a, b)
            Me.n = n
        End Sub

        Sub New()

        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsInside(x As Double) As Boolean Implements IRanges(Of Double).IsInside
            Return Range.IsInside(x)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsInside(range As IRanges(Of Double)) As Boolean Implements IRanges(Of Double).IsInside
            Return range.IsInside(range)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsOverlapping(range As IRanges(Of Double)) As Boolean Implements IRanges(Of Double).IsOverlapping
            Return range.IsOverlapping(range)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As Double()
            Return Range.Enumerate(n)
        End Function

        Public Overrides Function ToString() As String
            Return Range.ToString
        End Function
    End Class
End Namespace