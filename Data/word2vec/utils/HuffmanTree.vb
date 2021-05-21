Namespace utils

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 哈夫曼树
    ''' </summary>
    Public Class HuffmanTree

        '    private TreeSet<HuffmanNode> tree = new TreeSet<HuffmanNode>();

        Public Shared Sub make(Of T1 As HuffmanNode)(nodes As ICollection(Of T1))
            Dim tree As New SortedSet(Of HuffmanNode)(nodes)

            While tree.Count > 1
                Dim left As HuffmanNode = tree.PollFirst()
                Dim right As HuffmanNode = tree.PollFirst()
                Dim parent = left.merge(right)
                tree.Add(parent)
            End While
        End Sub

        Public Shared Function getPath(leafNode As HuffmanNode) As IList(Of HuffmanNode)
            Dim nodes As New List(Of HuffmanNode)()
            Dim hn = leafNode

            While hn IsNot Nothing
                nodes.Add(hn)
                hn = hn.parent
            End While

            nodes.Reverse()

            Return nodes
        End Function
    End Class
End Namespace
