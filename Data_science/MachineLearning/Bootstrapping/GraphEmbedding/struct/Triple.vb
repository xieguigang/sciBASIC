Namespace GraphEmbedding.struct
    Public Class Triple
        Private iHeadEntity As Integer
        Private iTailEntity As Integer
        Private iRelation As Integer

        Public Sub New()
        End Sub

        Public Sub New(i As Integer, j As Integer, k As Integer)
            iHeadEntity = i
            iTailEntity = j
            iRelation = k
        End Sub

        Public Overridable Function head() As Integer
            Return iHeadEntity
        End Function

        Public Overridable Function tail() As Integer
            Return iTailEntity
        End Function

        Public Overridable Function relation() As Integer
            Return iRelation
        End Function
    End Class

End Namespace
