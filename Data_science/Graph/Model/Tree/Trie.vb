#Region "Microsoft.VisualBasic::bace54574ae83201d15228ddd19e57f9, Data_science\Graph\Model\Tree\Trie.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 181
    '    Code Lines: 115
    ' Comment Lines: 34
    '   Blank Lines: 32
    '     File Size: 5.45 KB


    ' Class Trie
    ' 
    '     Properties: Root
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: Add, Contains, Count, Find, FindByPrefix
    '               Populate, PopulateWordsByPrefix
    ' 
    ' Class CharacterNode
    ' 
    '     Properties: Character, data, Ends
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 朴素字典树（Trie）
''' </summary>
Public Class Trie(Of T)

    Public ReadOnly Property Root As New CharacterNode(Of T)("*")

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(root As CharacterNode(Of T))
        Me.Root = root
    End Sub

    Sub New()
    End Sub

    ''' <summary>
    ''' 建立字典树
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    Public Function Add(word As String) As CharacterNode(Of T)
        Dim chars As New Pointer(Of Char)(word.SafeQuery)
        Dim child As CharacterNode(Of T) = Root
        Dim c As Char
        Dim [next] As CharacterNode(Of T) = Nothing

        If chars = 0 Then
            Return Root
        End If

        Do While Not chars.EndRead
            c = ++chars

            If child.Childs.ContainsKey(c) Then
                [next] = child(c)
            Else
                [next] = New CharacterNode(Of T)(c) With {
                    .Parent = child
                }
                child.Childs.Add(c, [next])
            End If

            child = [next]
        Loop

        [next].Ends += 1

        Return [next]
    End Function

    Public Function Find(key As String) As (child As CharacterNode(Of T), success As Boolean)
        Dim chars As New Pointer(Of Char)(key.SafeQuery)
        Dim q = FindByPrefix(chars)

        Return q
    End Function

    Private Function FindByPrefix(chars As Pointer(Of Char)) As (child As CharacterNode(Of T), success As Boolean)
        Dim child As CharacterNode(Of T) = Root
        Dim c As Char
        Dim [next] As CharacterNode(Of T) = Nothing

        Do While Not chars.EndRead
            c = ++chars

            If child.Childs.ContainsKey(c) Then
                [next] = child(c)
            Else
                Return (child, False)
            End If

            child = [next]
        Loop

        Return (child, True)
    End Function

    ''' <summary>
    ''' populate words by a given prefix string.
    ''' </summary>
    ''' <param name="prefix">The prefix string</param>
    ''' <returns></returns>
    Public Iterator Function PopulateWordsByPrefix(prefix As String) As IEnumerable(Of String)
        Dim chars As New Pointer(Of Char)(prefix.SafeQuery)

        With FindByPrefix(chars)
            If .success Then
                ' 将所有的子节点所构成的单词返回
                For Each last As Char() In Populate(.child, chars.RawBuffer.AsList)
                    Yield New String(last.ToArray)
                Next
            Else
                ' 不存在，则返回空集合
                Return
            End If
        End With
    End Function

    Private Iterator Function Populate(child As CharacterNode(Of T), prefix As List(Of Char)) As IEnumerable(Of IEnumerable(Of Char))
        If child.Ends > 0 Then
            ' 其自身也算
            Yield prefix.JoinIterates(child.Character)
        End If

        For Each [next] As CharacterNode(Of T) In child.Childs.Values
            If [next].Ends > 0 Then
                ' 其自身也算
                Yield prefix.JoinIterates([next].Character)
            End If

            Dim nextPrefix As New List(Of Char)(prefix.JoinIterates([next].Character))

            For Each pop As IEnumerable(Of Char) In Populate([next], nextPrefix)
                Yield pop
            Next
        Next
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

        If chars = 0 Then
            Return 0
        Else
            With FindByPrefix(chars)
                If .success Then
                    Return .child.Ends
                Else
                    Return 0
                End If
            End With
        End If
    End Function
End Class

''' <summary>
''' 在字典树之中，一个字母构成一个节点
''' </summary>
Public Class CharacterNode(Of T) : Inherits AbstractTree(Of CharacterNode(Of T), Char)

    ''' <summary>
    ''' 以这个字符结束的单词的数目
    ''' </summary>
    ''' <returns></returns>
    Public Property Ends As Integer
    Public Property Character As Char
    Public Property data As T

    Sub New(c As Char)
        Call MyBase.New(qualDeli:="")

        Me.Childs = New Dictionary(Of Char, CharacterNode(Of T))
        Me.Character = c
        Me.label = c
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(c As CharacterNode(Of T)) As Char
        Return c.Character
    End Operator
End Class
