Namespace DownSampling.MaxMin

	Public Class PIPlotBucketFactory
		Implements BucketFactory(Of PIPlotBucket)

		Public Overridable Function newBucket() As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
			Return New PIPlotBucket()
		End Function

		Public Overridable Function newBucket(size As Integer) As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
			Return New PIPlotBucket(size)
		End Function

		Public Overridable Function newBucket(e As [Event]) As PIPlotBucket Implements BucketFactory(Of PIPlotBucket).newBucket
			Return New PIPlotBucket(e)
		End Function

	End Class

End Namespace