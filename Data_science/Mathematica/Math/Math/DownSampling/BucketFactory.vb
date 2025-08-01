Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    Public Interface BucketFactory(Of B As Bucket)

        Function newBucket() As B
        Function newBucket(size As Integer) As B
        Function newBucket(e As ITimeSignal) As B

    End Interface

End Namespace