Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

Public Class GraphTree : Inherits TreeNodeBase(Of GraphTree)

    Public Overrides ReadOnly Property MySelf As GraphTree
        Get
            Return Me
        End Get
    End Property

    Public ReadOnly Property Vertex As Vertex

    Sub New(v As Vertex)
        Call MyBase.New(v.ID)
        Vertex = v
    End Sub
End Class
