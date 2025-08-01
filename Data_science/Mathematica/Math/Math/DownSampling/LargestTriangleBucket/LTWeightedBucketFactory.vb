Namespace DownSampling.LargestTriangleBucket

	Public Class LTWeightedBucketFactory
		Implements BucketFactory(Of LTWeightedBucket)

		Public Overridable Function newBucket() As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
			Return New LTWeightedBucket()
		End Function

		Public Overridable Function newBucket(size As Integer) As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
			Return New LTWeightedBucket(size)
		End Function

		Public Overridable Function newBucket(e As [Event]) As LTWeightedBucket Implements BucketFactory(Of LTWeightedBucket).newBucket
			Return New LTWeightedBucket(DirectCast(e, WeightedEvent))
		End Function

	End Class

End Namespace