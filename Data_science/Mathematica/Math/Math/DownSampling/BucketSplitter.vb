Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling


    ''' <summary>
    ''' Split up events into buckets
    ''' </summary>
    Public Interface BucketSplitter(Of B As Bucket, E As ITimeSignal)

        Function split(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B)

    End Interface

End Namespace