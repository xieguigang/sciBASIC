Namespace GraphEmbedding.struct

    Public Structure Triple

        Public ReadOnly Property head() As Integer
        Public ReadOnly Property tail() As Integer
        Public ReadOnly Property relation() As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="head"></param>
        ''' <param name="tail"></param>
        ''' <param name="relation"></param>
        Public Sub New(head As Integer, tail As Integer, relation As Integer)
            Me.head = head
            Me.tail = tail
            Me.relation = relation
        End Sub

        Public Overrides Function ToString() As String
            Return $"[head:{head()} -> tail:{tail()}] relationship:{relation()}"
        End Function
    End Structure

End Namespace
