Imports System.Collections.Generic

Namespace DownSampling



	''' <summary>
	''' Interface for downSampling algorithms
	''' </summary>
	Public Interface DownSamplingAlgorithm

		''' 
		''' <param name="data"> The original data </param>
		''' <param name="threshold"> Number of data points to be returned </param>
		''' <returns> the downsampled data </returns>
		Function process(data As IList(Of [Event]), threshold As Integer) As IList(Of [Event])

	End Interface

End Namespace