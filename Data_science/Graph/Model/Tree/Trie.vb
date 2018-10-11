Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 朴素字典树（Trie）
''' </summary>
Public Class Trie

    Public ReadOnly Property Root As New CharacterNode("*")

    ''' <summary>
    ''' 建立字典树
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    Public Function Add(word As String) As Trie
        Dim chars As New Pointer(Of Char)(word.SafeQuery)
        Dim child As CharacterNode = Root
        Dim c As Char
        Dim [next] As CharacterNode = Nothing

        If chars = 0 Then
            Return Me
        End If

        Do While Not chars.EndRead
            c = ++chars

            If child.Childs.ContainsKey(c) Then
                [next] = child(c)
            Else
                [next] = New CharacterNode(c) With {
                    .Parent = child
                }
                child.Childs.Add(c, [next])
            End If

            child = [next]
        Loop

        [next].Ends += 1

        Return Me
    End Function

    ''' <summary>
    ''' 查找某一个单词或者前缀是否在这颗字典树之中
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Contains(word As String) As Boolean
        Return Count(word) > 0
    End Function

    ''' <summary>
    ''' 获取某一个单词/前缀在字典树之中的计数
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    Public Function Count(word As String) As Integer
        Dim chars As New Pointer(Of Char)(word.SafeQuery)
        Dim child As CharacterNode = Root
        Dim c As Char
        Dim [next] As CharacterNode = Nothing

        If chars = 0 Then
            Return 0
        End If

        Do While Not chars.EndRead
            c = ++chars

            If child.Childs.ContainsKey(c) Then
                [next] = child(c)
            Else
                Return 0
            End If

            child = [next]
        Loop

        Return child.Ends
    End Function
End Class

''' <summary>
''' 在字典树之中，一个字母构成一个节点
''' </summary>
Public Class CharacterNode : Inherits AbstractTree(Of CharacterNode, Char)

    ''' <summary>
    ''' 以这个字符结束的单词的数目
    ''' </summary>
    ''' <returns></returns>
    Public Property Ends As Integer
    Public Property Character As Char

    Sub New(c As Char)
        Call MyBase.New(qualDeli:="")

        Me.Childs = New Dictionary(Of Char, CharacterNode)
        Me.Character = c
        Me.Label = c
    End Sub
End Class