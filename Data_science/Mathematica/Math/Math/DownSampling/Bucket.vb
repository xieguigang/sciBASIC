Namespace DownSampling



	''' <summary>
	''' A bucket holds a subset of events and select significant events from it
	''' </summary>
	Public Interface Bucket

		Sub selectInto(result As IList(Of [Event]))

		Sub add(e As [Event])

	End Interface

End Namespace