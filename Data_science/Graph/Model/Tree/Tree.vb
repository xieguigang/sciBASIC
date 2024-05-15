#Region "Microsoft.VisualBasic::bd4dd3ff341aa52cbe7d7255b8bcaf86, Data_science\Graph\Model\Tree\Tree.vb"

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

    '   Total Lines: 89
    '    Code Lines: 45
    ' Comment Lines: 28
    '   Blank Lines: 16
    '     File Size: 2.52 KB


    ' Class Tree
    ' 
    '     Properties: Data
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' Class Tree
    ' 
    '     Properties: Data
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Add, FindNode, hasNode, PopulateAllNodes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization

''' <summary>
''' Tree node with data..(可以直接被使用的树对象类型)
''' </summary>
''' <typeparam name="T"></typeparam>
''' 
<DataContract>
Public Class Tree(Of T, K) : Inherits AbstractTree(Of Tree(Of T, K), K)

    Public Property Data As T

    Sub New(Optional qualDeli$ = ".")
        MyBase.New(qualDeli)
    End Sub

    Sub New()
        Call MyBase.New(".")
    End Sub
End Class

''' <summary>
''' 使用字符串<see cref="String"/>作为键名的树节点
''' </summary>
''' <typeparam name="T">the value type of the ``Data`` property</typeparam>
''' <remarks>
''' 在这里如果直接继承<see cref="Tree(Of T, K)"/>类型的话，会导致child的类型错误
''' </remarks>
Public Class Tree(Of T) : Inherits AbstractTree(Of Tree(Of T), String)

    Public Property Data As T

    Sub New(Optional qualDeli$ = ".")
        MyBase.New(qualDeli)
    End Sub

    Public Function hasNode(name As String) As Boolean
        Return Childs.ContainsKey(name)
    End Function

    Public Function FindNode(links As String(), Optional i As Integer = Scan0) As Tree(Of T)
        Dim key As String = links(i)

        If Childs.ContainsKey(key) Then
            If i = links.Length - 1 Then
                ' is end of the link
                Return Childs(key)
            Else
                ' continute to next layer
                Return Childs(key).FindNode(links, i + 1)
            End If
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' add target <paramref name="child"/> node into current childs 
    ''' collection and the assign the target <paramref name="child"/> 
    ''' parent to current node.
    ''' </summary>
    ''' <param name="child"></param>
    ''' <returns></returns>
    Public Overridable Function Add(child As Tree(Of T)) As Tree(Of T)
        Childs.Add(child.label, child)
        child.Parent = Me

        Return Me
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>
    ''' the first element is the tree root node
    ''' </returns>
    Public Function PopulateAllNodes() As IEnumerable(Of Tree(Of T))
        Dim list As New List(Of Tree(Of T))

        ' add tree root node
        Call list.Add(Me)

        For Each node In Childs.Values
            Call list.AddRange(node.PopulateAllNodes)
        Next

        Return list
    End Function
End Class
