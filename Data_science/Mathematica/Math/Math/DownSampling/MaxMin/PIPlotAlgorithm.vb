Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin

    ''' <summary>
    ''' OSISoft PI PlotValues algorithm. (without interpolation on boundary)
    ''' </summary>
    Public Class PIPlotAlgorithm : Inherits BucketBasedAlgorithm(Of PIPlotBucket, ITimeSignal)

        Public Sub New()
            BucketFactory(New PIPlotBucketFactory)
            SetSpliter(New FixedTimeBucketSplitter(Of PIPlotBucket, ITimeSignal))
        End Sub

        Protected Friend Overrides Function prepare(data As IList(Of ITimeSignal)) As IList(Of ITimeSignal)
            Return data
        End Function

        Protected Friend Overrides Sub beforeSelect(buckets As IList(Of PIPlotBucket), threshold As Integer)
        End Sub

        Public Overrides Function ToString() As String
            Return "PIPlot"
        End Function
    End Class
End Namespace