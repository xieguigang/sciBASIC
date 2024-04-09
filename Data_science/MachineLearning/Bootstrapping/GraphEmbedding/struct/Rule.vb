Namespace GraphEmbedding.struct

    Public Class Rule

        ReadOnly m_relations As List(Of Relation)

        Public Property confidence() As Double

        Public Sub New()
            m_relations = New List(Of Relation)()
        End Sub

        Public Overridable Function relations() As List(Of Relation)
            Return m_relations
        End Function

        Public Overridable Sub add(r As Relation)
            m_relations.Add(r)
        End Sub
    End Class

End Namespace
