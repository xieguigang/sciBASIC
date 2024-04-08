Namespace GraphEmbedding.struct
    Public Class Relation

        Dim _rid As Integer
        Dim dir As Integer

        Public Sub New(rid As Integer, dir As Integer)
            _rid = rid
            Me.dir = dir
        End Sub

        Public Overridable Function rid() As Integer
            Return _rid
        End Function

        Public Overridable Function direction() As Integer
            Return dir
        End Function
    End Class

End Namespace
