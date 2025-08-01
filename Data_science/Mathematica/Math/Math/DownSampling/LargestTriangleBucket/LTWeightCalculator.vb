Imports System.Collections.Generic

Namespace DownSampling.LargestTriangleBucket


	Public Interface LTWeightCalculator

		Sub calcWeight(triangle As Triangle, buckets As IList(Of LTWeightedBucket))

	End Interface

End Namespace