Namespace DownSampling.MaxMin

	Public Class MMBucketFactory
		Implements BucketFactory(Of MMBucket)

		Public Overridable Function newBucket() As MMBucket Implements BucketFactory(Of MMBucket).newBucket
			Return New MMBucket()
		End Function

		Public Overridable Function newBucket(size As Integer) As MMBucket Implements BucketFactory(Of MMBucket).newBucket
			Return New MMBucket(size)
		End Function

		Public Overridable Function newBucket(e As [Event]) As MMBucket Implements BucketFactory(Of MMBucket).newBucket
			Return New MMBucket(e)
		End Function

	End Class

End Namespace