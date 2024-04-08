Namespace GraphEmbedding.struct
    Public Class Relation

        Private rid_Conflict As Integer
        Private dir As Integer
        Public Sub New(rid As Integer, dir As Integer)
            rid_Conflict = rid
            Me.dir = dir
        End Sub
        Public Overridable Function rid() As Integer
            Return rid_Conflict
        End Function
        Public Overridable Function direction() As Integer
            Return dir
        End Function
    End Class

End Namespace
