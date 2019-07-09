''' <summary>
''' A tree with string term as key
''' </summary>
Public Class TermTree(Of T) : Inherits Tree(Of T, String)

    Default Public Overloads Property Child(path As String) As T
        Get
            Return Visit(path.Split("/"c)).Data
        End Get
        Set(value As T)
            Visit(path.Split("/"c)).Data = value
        End Set
    End Property

    Public Function Visit(path As String()) As TermTree(Of T)
        If path.Length = 1 Then
            Return MyBase.Child(path(Scan0))
        Else
            Return Visit(path.Skip(1).ToArray)
        End If
    End Function
End Class
