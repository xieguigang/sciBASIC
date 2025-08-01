Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' Assign the first event to the first bucket, the last event to the last bucket.<br />
    ''' Split the rest events into the rest (threshold - 2) buckets each containing approximately equal number of events
    ''' </summary>
    Public Class FixedNumBucketSplitter(Of B As Bucket, E As ITimeSignal)
        Implements BucketSplitter(Of B, E)

        Public Overridable Function split(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B) Implements BucketSplitter(Of B, E).split
            Dim bucketNum As Integer = threshold - 2
            Dim netSize As Integer = data.Count - 2
            Dim bucketSize As Integer = (netSize + bucketNum - 1) \ bucketNum

            Dim buckets As New List(Of B)(threshold)
            For i As Integer = 0 To threshold - 1
                buckets.Add(Nothing)
            Next i

            buckets(0) = factory.newBucket(data(0))
            buckets(threshold - 1) = factory.newBucket(data(data.Count - 1))

            For i As Integer = 0 To bucketNum - 1
                buckets(i + 1) = factory.newBucket(bucketSize)
            Next i
            Dim [step] As Double = netSize * 1.0 / bucketNum
            Dim curr As Double = [step]
            Dim bucketIndex As Integer = 1

            For i As Integer = 1 To netSize
                buckets(bucketIndex).add(data(i))
                If i > curr Then
                    bucketIndex += 1
                    curr += [step]
                End If
            Next

            Return buckets
        End Function

    End Class

End Namespace