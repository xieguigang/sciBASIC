Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling



    ''' <summary>
    ''' Split data into buckets with equal time span
    ''' </summary>
    Public Class FixedTimeBucketSplitter(Of B As Bucket, E As ITimeSignal)
        Implements BucketSplitter(Of B, E)

        Public Overridable Function split2(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B)
            Dim buckets As IList(Of B) = New List(Of B)(threshold)
            Dim start As Long = data(0).time
            Dim [end] As Long = data(data.Count - 1).time
            Dim span As Long = [end] - start
            Dim pice As Double = span \ threshold
            Dim time As Double = start
            Dim index As Integer = -1
            For i As Integer = 0 To data.Count - 1
                Dim e As ITimeSignal = data(i)
                If e.time >= time Then
                    time += pice
                    index += 1
                    buckets.Add(factory.newBucket())
                End If
                buckets(index).add(e)
            Next i
            Return buckets
        End Function

        Public Overridable Function split(factory As BucketFactory(Of B), data As IList(Of E), threshold As Integer) As IList(Of B) Implements BucketSplitter(Of B, E).split
            Dim buckets As IList(Of B) = New List(Of B)(threshold)
            For i As Integer = 0 To threshold - 1
                buckets.Add(factory.newBucket())
            Next i
            Dim start As Long = data(0).time
            Dim [end] As Long = data(data.Count - 1).time
            Dim span As Long = [end] - start
            For Each e As ITimeSignal In data
                Dim bindex As Integer = CInt((e.time - start) * threshold \ span)
                bindex = If(bindex >= threshold, threshold - 1, bindex)
                buckets(bindex).add(e)
            Next e
            Return buckets
        End Function
    End Class

End Namespace