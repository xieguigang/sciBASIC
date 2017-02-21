
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class EntityNode
        Inherits TreeNodeBase(Of EntityNode)

        Public ReadOnly Property EntityID As String
        Public ReadOnly Property Type As String

        Public Sub New(name As String, type$)
            MyBase.New(__pathName(name))
            Me.Type = type
            Me.EntityID = name
        End Sub

        Shared ReadOnly virtualPath As New Regex("\[\d+\]\d+(\.\d+)*")

        Private Shared Function __pathName(name$) As String
            If virtualPath.Match(name).Value = name Then
                Return name.Split("."c).Last
            Else
                Return name
            End If
        End Function

        Public Overrides ReadOnly Property MySelf As EntityNode
            Get
                Return Me
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return MyClass.FullyQualifiedName
        End Function
    End Class
End Namespace