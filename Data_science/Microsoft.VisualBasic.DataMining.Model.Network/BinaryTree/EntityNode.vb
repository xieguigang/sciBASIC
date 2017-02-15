
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class EntityNode
        Inherits TreeNodeBase(Of EntityNode)

        Public ReadOnly Property Entity As EntityLDM

        Public Sub New(obj As EntityLDM)
            MyBase.New(obj.Name)
            Me.Entity = obj
        End Sub

        Protected Overrides ReadOnly Property MySelf As EntityNode
            Get
                Return Me
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Entity.GetJson
        End Function
    End Class
End Namespace