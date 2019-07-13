#Region "Microsoft.VisualBasic::efef4408cdd76f68c3c5c5aa29b9e482, Data_science\Mathematica\Math\Math\Distributions\Sample.vb"

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

    '     Class SampleDistribution
    ' 
    '         Properties: average, max, min, quantile, size
    '                     stdErr
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetRange, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Distributions

    ''' <summary>
    ''' The data sample xml model
    ''' </summary>
    Public Class SampleDistribution

        <XmlAttribute> Public Property min As Double
        <XmlAttribute> Public Property max As Double
        <XmlAttribute> Public Property average As Double
        <XmlAttribute> Public Property stdErr As Double
        <XmlAttribute> Public Property size As Integer

        ''' <summary>
        ''' 分别为0%, 25%, 50%, 75%, 100%
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property quantile As Double()

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Double))
            Call Me.New(data.SafeQuery.ToArray)
        End Sub

        Sub New(v As Double())
            Dim q As QuantileEstimationGK = v.GKQuantile

            min = v.Min
            max = v.Max
            average = v.Average
            stdErr = v.StdError
            size = v.Length

            quantile = {
                q.Query(0),
                q.Query(0.25),
                q.Query(0.5),
                q.Query(0.75),
                q.Query(1)
            }
        End Sub

        ''' <summary>
        ''' <see cref="DoubleRange"/> = ``[<see cref="min"/>, <see cref="max"/>]``
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRange() As DoubleRange
            Return {min, max}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return GetJson
        End Function
    End Class
End Namespace
