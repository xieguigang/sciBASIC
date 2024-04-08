Imports System.Collections.Generic

Namespace GraphEmbedding.struct

    Public Class Rule
        Private lstRelations As List(Of Relation)
        Public conf As Double
        Public Sub New()
            lstRelations = New List(Of Relation)()
        End Sub
        Public Overridable Function confidence() As Double
            Return conf
        End Function
        Public Overridable Function relations() As List(Of Relation)
            Return lstRelations
        End Function
        Public Overridable Sub add(r As Relation)
            lstRelations.Add(r)
        End Sub
    End Class

End Namespace
