Imports std = System.Math

Namespace DownSampling.Mixed


	''' <summary>
	''' Merge other algorithms' results into a new result, then deduplicate and sort it.
	''' </summary>
	Public Class MixedAlgorithm
		Implements DownSamplingAlgorithm

		Private map As New Dictionary(Of DownSamplingAlgorithm, Double)()

		Public Overridable Sub add(da As DownSamplingAlgorithm, rate As Double)
			map.Add(da, rate)
		End Sub

		Public Overridable Function process(data As IList(Of [Event]), threshold As Integer) As IList(Of [Event]) Implements DownSamplingAlgorithm.process
			If map.Empty Then
				Return data
			End If
			Dim [set] As New HashSet(Of [Event])()
			For Each da As DownSamplingAlgorithm In map.Keys
				Dim subList As IList(Of [Event]) = da.process(data, CInt(std.Truncate(threshold * map(da))))

				For Each item In subList
					[set].Add(item)
				Next

			Next da
			Dim result As New List(Of [Event])([set].Count)
			CType(result, List(Of [Event])).AddRange([set])
			result.Sort(AddressOf EventOrder.BY_TIME_ASC)
			Return result
		End Function

		Public Overrides Function ToString() As String
			Dim name As String = "MIXED"
			If Not map.Empty Then
				name &= map.ToString()
			End If
			Return name
		End Function

	End Class

End Namespace