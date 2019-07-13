#Region "Microsoft.VisualBasic::023e62024aa46f51d84770b70a446c3f, Microsoft.VisualBasic.Core\Text\Xml\Models\Sequence.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Sequence
    ' 
    '         Properties: n, range, steps
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GenericEnumerator, GetEnumerator, ToArray, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq

Namespace Text.Xml.Models

    ''' <summary>
    ''' A numeric sequence model
    ''' </summary>
    <XmlType("sequence")>
    Public Class Sequence : Implements Enumeration(Of Double)

        ''' <summary>
        ''' [min, max]
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("range")>
        Public Property range As DoubleRange

        ''' <summary>
        ''' 将目标区间对象<see cref="range"/>平均划分为n个区间
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property n As Integer

        Public ReadOnly Property steps As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return range.Length / n
            End Get
        End Property

        Sub New(a#, b#, n%)
            Call Me.New(New DoubleRange(a, b), n)
        End Sub

        Sub New(range As DoubleRange, Optional n% = 100)
            Me.range = range
            Me.n = n
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As Double()
            Return range.Enumerate(n)
        End Function

        Public Overrides Function ToString() As String
            Return range.ToString
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of Double) Implements Enumeration(Of Double).GenericEnumerator
            For Each value As Double In range.Enumerate(n)
                Yield value
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Double).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace
