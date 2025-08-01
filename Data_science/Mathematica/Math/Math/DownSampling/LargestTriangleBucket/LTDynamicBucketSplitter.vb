Imports std = System.Math

Namespace DownSampling.LargestTriangleBucket


    ''' <summary>
    ''' <para>
    ''' A bucket-splitter dynamically resize bucket according to their SSE(Sum of Square Errors).
    ''' </para>
    ''' <para>
    ''' In each iteration, the bucket with the highest SSE is split into two new buckets, two buckets with the lowest SSE
    ''' are merged into a new one.
    ''' </para>
    ''' <para>
    ''' LTD recommended number of iterations is DataSize / threshold * 10 but it depends. For a plot whit one highly
    ''' fluctuating area and several small peaks, big number of iterations causes small peaks to be lost. So I change the
    ''' formula to DataSize / threshold / 10 and limit the number to 500.
    ''' </para>
    ''' </summary>
    Public Class LTDynamicBucketSplitter
        Implements BucketSplitter(Of LTWeightedBucket, WeightedEvent)

        Private fs As New FixedNumBucketSplitter(Of LTWeightedBucket, WeightedEvent)()

        Public Overridable Property IterationRate As Double = 0.1
        Public Overridable Property MaxIteration As Integer = 500

        Public Overridable Function split(factory As BucketFactory(Of LTWeightedBucket), data As IList(Of WeightedEvent), threshold As Integer) As IList(Of LTWeightedBucket) Implements BucketSplitter(Of LTWeightedBucket, WeightedEvent).split
            ' first split equally
            Dim buckets As IList(Of LTWeightedBucket) = fs.split(factory, data, threshold)
            ' resize buckets
            Dim head As LinkedBucketNode = LinkedBucketNode.fromList(buckets)
            For i As Integer = getItCount(data.Count, threshold) To 0 Step -1
                Dim max As LinkedBucketNode = findMaxSSE(head)
                findMinSSEPair(head, max).merge()
                max.split()
            Next i
            Return LinkedBucketNode.toList(head)
        End Function

        Private Function getItCount(total As Integer, threshold As Integer) As Integer
            Dim itCount As Integer = CInt(std.Truncate(total / threshold * IterationRate))
            If itCount > MaxIteration Then
                itCount = MaxIteration
            ElseIf itCount < 1 Then
                itCount = 1
            End If
            Return itCount
        End Function

        Private Shared Function findMinSSEPair(head As LinkedBucketNode, exclude As LinkedBucketNode) As LinkedBucketNode
            Dim minSSE As Double = Double.MaxValue
            Dim low As LinkedBucketNode = Nothing
            Dim [end] As LinkedBucketNode = head.Next.Next.Next
            [end] = [end].Next()

            Do While [end] IsNot Nothing
                Dim beta As LinkedBucketNode = [end].Last
                Dim alpha As LinkedBucketNode = beta.Last
                If beta Is exclude Then
                    Continue Do
                End If
                Dim sum As Double = alpha.Value.sse() + beta.Value.sse()
                If sum < minSSE Then
                    minSSE = sum
                    low = alpha
                End If
                [end] = [end].Next
            Loop
            Return low
        End Function

        Private Shared Function findMaxSSE(head As LinkedBucketNode) As LinkedBucketNode
            Dim maxSSE As Double = Double.Epsilon
            Dim max As LinkedBucketNode = Nothing
            Dim [end] As LinkedBucketNode = head.End
            Dim n2 As LinkedBucketNode = head.Next.Next
            Do While n2 IsNot [end]
                Dim n1 As LinkedBucketNode = n2.Last
                Dim n3 As LinkedBucketNode = n2.Next
                Dim b As LTWeightedBucket = n2.Value
                If b.calcSSE(n1.Value, n3.Value) > maxSSE AndAlso b.size() > 1 Then
                    maxSSE = b.sse()
                    max = n2
                End If
                n2 = n2.Next
            Loop
            Return max
        End Function

    End Class

End Namespace