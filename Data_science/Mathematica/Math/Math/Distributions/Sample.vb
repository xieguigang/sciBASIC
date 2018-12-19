Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Distributions

    ''' <summary>
    ''' The data sample xml model
    ''' </summary>
    Public Class SampleDistribution

        Public Property min As Double
        Public Property max As Double
        Public Property average As Double
        Public Property stdErr As Double
        Public Property size As Integer

        ''' <summary>
        ''' 分别为0%, 25%, 50%, 75%, 100%
        ''' </summary>
        ''' <returns></returns>
        Public Property quantile As Double()

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Double))
            With data.ToArray
                Dim q As QuantileEstimationGK = .GKQuantile

                min = .Min
                max = .Max
                average = .Average
                stdErr = .StdError
                size = .Length

                quantile = {
                    q.Query(0),
                    q.Query(0.25),
                    q.Query(0.5),
                    q.Query(0.75),
                    q.Query(1)
                }
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRange() As DoubleRange
            Return {min, max}
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace