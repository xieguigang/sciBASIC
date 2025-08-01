Namespace DownSampling.LargestTriangleBucket


    ''' <summary>
    ''' Tow-way linked list to perform bucket split and merge.
    ''' </summary>
    Public Class LinkedBucketNode

        Private size As Integer

        ''' <summary>
        ''' split this node into 2 new nodes,each contains a new bucket with half events.
        ''' </summary>
        ''' <returns> if bucket contains more than 2 events, return the last node, else return this. </returns>
        Public Overridable Function split() As LinkedBucketNode
            Dim size As Integer = Value.size()
            If size < 2 Then
                Return Me
            End If
            Dim b0 As New LTWeightedBucket(size \ 2)
            Dim b1 As New LTWeightedBucket(size - size \ 2)
            For i As Integer = 0 To size - 1
                Call (If(i < size \ 2, b0, b1)).add(Value.get(i))
            Next i
            Dim n0 As New LinkedBucketNode(b0)
            Dim n1 As New LinkedBucketNode(b1)
            replace(Me, n0)
            insert(n0, n1)
            Return n1
        End Function

        ''' <summary>
        ''' merge this node and the next node into one.
        ''' </summary>
        ''' <returns> the merged node; </returns>
        Public Overridable Function merge() As LinkedBucketNode
            If [Next] Is Nothing Then
                Return Me
            End If
            Dim m As New LTWeightedBucket(Value.size() + [Next].Value.size())
            For Each e As WeightedEvent In Value
                m.add(e)
            Next e
            For Each e As WeightedEvent In [Next].Value
                m.add(e)
            Next e
            Dim n As New LinkedBucketNode(m)
            Dim tail As LinkedBucketNode = [Next].Next
            concat(Last, n, tail)
            Return n
        End Function

        Public Shared Function fromList(arr As IList(Of LTWeightedBucket)) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Count)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Count - 1
                Dim node As New LinkedBucketNode(arr(i))
                head._End = node
                node.Last = last
                last.Next = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toList(head As LinkedBucketNode) As IList(Of LTWeightedBucket)
            Dim arr As New List(Of LTWeightedBucket)(head.size)
            Dim node As LinkedBucketNode = head.Next
            Do While node IsNot Nothing
                arr.Add(node.Value)
                node = node.Next
            Loop
            Return arr
        End Function

        Public Shared Function fromArray(arr() As LTWeightedBucket) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Length)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Length - 1
                Dim node As New LinkedBucketNode(arr(i))
                head._End = node
                node.Last = last
                last.Next = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toArray(head As LinkedBucketNode) As LTWeightedBucket()
            Dim arr(head.size - 1) As LTWeightedBucket
            Dim node As LinkedBucketNode = head.Next
            Dim index As Integer = 0
            Do While node IsNot Nothing
                arr(index) = node.Value
                index += 1
                node = node.Next
            Loop
            Return arr
        End Function

        Public Shared Sub insert(node As LinkedBucketNode, append As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.Next
            node.Next = append
            append.Last = node
            append.Next = [next]
            If [next] IsNot Nothing Then
                [next].Last = append
            End If
        End Sub

        Public Shared Sub replace(node As LinkedBucketNode, rep As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.Next
            Dim last As LinkedBucketNode = node.Last

            node.Last = Nothing
            node.Next = Nothing
            last.Next = rep
            rep.Last = last
            rep.Next = [next]

            If [next] IsNot Nothing Then
                [next].Last = rep
            End If
        End Sub

        Public Shared Sub concat(head As LinkedBucketNode, node As LinkedBucketNode, tail As LinkedBucketNode)
            head.Next = node
            node.Last = head
            node.Next = tail
            If tail IsNot Nothing Then
                tail.Last = node
            End If
        End Sub

        Public Sub New(size As Integer)
            Me.size = size
        End Sub

        Public Sub New(b As LTWeightedBucket)
            Value = b
        End Sub

        Public Overridable ReadOnly Property [End] As LinkedBucketNode
        Public Overridable Property Last As LinkedBucketNode
        Public Overridable Property [Next] As LinkedBucketNode
        Public Overridable Property Value As LTWeightedBucket

    End Class

End Namespace