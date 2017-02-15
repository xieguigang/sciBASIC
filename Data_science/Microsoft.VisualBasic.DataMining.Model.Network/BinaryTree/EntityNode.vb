
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class EntityNode
        Inherits TreeNodeBase(Of EntityNode)

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Overrides ReadOnly Property MySelf As EntityNode
            Get
                Return Me
            End Get
        End Property
    End Class
End Namespace