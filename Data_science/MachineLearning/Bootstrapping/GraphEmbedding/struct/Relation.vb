Namespace GraphEmbedding.struct

    Public Structure Relation

        Public ReadOnly Property rid() As Integer
        Public ReadOnly Property direction() As Integer

        Public Sub New(rid As Integer, dir As Integer)
            _rid = rid
            _direction = dir
        End Sub

    End Structure

End Namespace
