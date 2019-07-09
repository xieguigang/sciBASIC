Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' A tree with string term as key
''' </summary>
''' 
<DataContract>
Public Class TermTree(Of T) : Inherits Tree(Of T, String)

    Default Public Overloads Property Child(path As String) As T
        Get
            Return Visit(path.Split("/"c)).Data
        End Get
        Set(value As T)
            Visit(path.Split("/"c)).Data = value
        End Set
    End Property

    Sub New()
        Call MyBase.New(qualDeli:="/")
    End Sub

    Public Function Visit(path As String()) As TermTree(Of T)
        If path.Length = 1 Then
            Return MyBase.Child(path(Scan0))
        Else
            Return Visit(path.Skip(1).ToArray)
        End If
    End Function

    Private Function newChild(name As String, value As T) As TermTree(Of T)
        Return New TermTree(Of T) With {
            .Data = value,
            .ID = Me.ID + Childs.Count,
            .Label = name,
            .Parent = Me,
            .Childs = New Dictionary(Of String, Tree(Of T, String))
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path">Path tokens should seperated with delimiter ``/``.</param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Add(path As String, value As T) As TermTree(Of T)
        Return Add(path.Trim("/"c).Split("/"c), value)
    End Function

    Public Function Add(path As String(), value As T) As TermTree(Of T)
        Dim next$ = path(Scan0)

        If path.Length = 1 Then
            Childs.Add([next], newChild([next], value))

            ' return this new tree leaf
            Return Childs([next])
        Else
            If Not Childs.ContainsKey([next]) Then
                Childs.Add([next], newChild([next], Nothing))
            End If

            Return DirectCast(Childs([next]), TermTree(Of T)).Add(path.Skip(1).ToArray, value)
        End If
    End Function
End Class
