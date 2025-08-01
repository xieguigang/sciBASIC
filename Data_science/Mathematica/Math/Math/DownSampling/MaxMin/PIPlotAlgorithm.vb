Namespace DownSampling.MaxMin


	''' <summary>
	''' OSISoft PI PlotValues algorithm. (without interpolation on boundary)
	''' </summary>
	Public Class PIPlotAlgorithm
		Inherits BucketBasedAlgorithm(Of PIPlotBucket, [Event])

		Public Sub New()
			BucketFactory = New PIPlotBucketFactory()
			Spliter(New FixedTimeBucketSplitter(Of PIPlotBucket, [Event]))
		End Sub

		Protected Friend Overrides Function prepare(data As IList(Of [Event])) As IList(Of [Event])
			Return data
		End Function

		Protected Friend Overrides Sub beforeSelect(buckets As IList(Of PIPlotBucket), threshold As Integer)
		End Sub

		Public Overrides Function ToString() As String
			Return "PIPlot"
		End Function

	End Class

End Namespace