#Region "Microsoft.VisualBasic::c65c956e6b69aef4f24bfefc78b583b4, Microsoft.VisualBasic.Core\Extensions\Math\StatisticsMathExtensions\DataSample.vb"

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

    '     Class DataSample
    ' 
    '         Properties: Max, Mean, Min, SampleSize
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, Split, ToString
    ' 
    '     Module DataSampleExtensions
    ' 
    '         Function: DoubleSample, IntegerSample
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Math.Statistics

    ''' <summary>
    ''' Numeric value statics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DataSample(Of T As {IComparable, Structure})
        Implements IEnumerable(Of T)

        Protected Friend ranges As IRanges(Of T)
        Protected Friend buffer As List(Of T)
        Protected Friend means As Double

        Public Overridable ReadOnly Property Min As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ranges.Min
            End Get
        End Property

        Public Overridable ReadOnly Property Max As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ranges.Max
            End Get
        End Property

        ''' <summary>
        ''' The sample average
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Mean As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return means
            End Get
        End Property

        ''' <summary>
        ''' The sample size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SampleSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Count
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(sample As IEnumerable(Of T))
            buffer = New List(Of T)(sample)
        End Sub

        Public Iterator Function Split(partSize As Integer) As IEnumerable(Of T())
            For Each chunk As T() In buffer.SplitIterator(partSize)
                Yield chunk
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return buffer.ToArray.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In buffer
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    Public Module DataSampleExtensions

        <Extension>
        Public Function IntegerSample(data As IEnumerable(Of Integer)) As DataSample(Of Integer)
            Dim buf As Integer() = data.ToArray
            Return New DataSample(Of Integer)(buf) With {
                .means = buf.Average,
                .ranges = New IntRange(buf.Min, buf.Max)
            }
        End Function

        <Extension>
        Public Function DoubleSample(data As IEnumerable(Of Double)) As DataSample(Of Double)
            Dim buf As Double() = data.ToArray
            Return New DataSample(Of Double)(buf) With {
                .means = buf.Average,
                .ranges = New DoubleRange(buf.Min, buf.Max)
            }
        End Function
    End Module
End Namespace
