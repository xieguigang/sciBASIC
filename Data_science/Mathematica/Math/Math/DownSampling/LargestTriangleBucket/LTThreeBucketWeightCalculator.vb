Namespace DownSampling.LargestTriangleBucket


	''' <summary>
	''' Weight = Area of triangle (point A: the previous selected event, point B: this event; point C: average event int the next bucket)
	''' </summary>
	Public Class LTThreeBucketWeightCalculator
		Implements LTWeightCalculator

		Public Overridable Sub calcWeight(triangle As Triangle, buckets As IList(Of LTWeightedBucket)) Implements LTWeightCalculator.calcWeight
			For i As Integer = 1 To buckets.Count - 2
				Dim bucket As LTWeightedBucket = buckets(i)
				Dim last As WeightedEvent = buckets(i - 1).select()(0)
				Dim [next] As WeightedEvent = buckets(i + 1).average()
				For j As Integer = 0 To bucket.size() - 1
					Dim curr As WeightedEvent = bucket.get(j)
					triangle.calc(last, curr, [next])
				Next j
			Next i
		End Sub

	End Class

End Namespace