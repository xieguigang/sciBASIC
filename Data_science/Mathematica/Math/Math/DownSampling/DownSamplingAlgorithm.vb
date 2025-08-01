Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling



    ''' <summary>
    ''' Interface for downSampling algorithms
    ''' </summary>
    Public Interface DownSamplingAlgorithm

        ''' 
        ''' <param name="data"> The original data </param>
        ''' <param name="threshold"> Number of data points to be returned </param>
        ''' <returns> the downsampled data </returns>
        Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal)

    End Interface

End Namespace