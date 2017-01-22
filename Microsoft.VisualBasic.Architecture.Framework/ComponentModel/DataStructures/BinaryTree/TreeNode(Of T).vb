#Region "Microsoft.VisualBasic::df32a6989ed66f0fe6e94b247d65afb8, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\BinaryTree\TreeNode(Of T).vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' Define tree nodes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TreeNode(Of T) : Implements INamedValue
        Implements Value(Of T).IValueOf

        Public Property Name As String Implements INamedValue.Key
        Public Property Value As T Implements Value(Of T).IValueOf.value
        Public Property Left As TreeNode(Of T)
        Public Property Right As TreeNode(Of T)

        ''' <summary>
        ''' Constructor  to create a single node 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="obj"></param>
        Public Sub New(name As String, obj As T)
            With Me
                .Name = name
                .Value = obj
            End With
        End Sub

        Sub New()
        End Sub

        <ScriptIgnore> Public ReadOnly Property IsLeaf As Boolean
            Get
                Return Left Is Nothing AndAlso
                    Right Is Nothing
            End Get
        End Property

        <ScriptIgnore> Public ReadOnly Property AllChilds As List(Of TreeNode(Of T))
            Get
                Dim list As New List(Of TreeNode(Of T))

                For Each x In Me.GetEnumerator
                    Call list.Add(x)
                    Call list.AddRange(x.AllChilds)
                Next

                Return list
            End Get
        End Property

        ''' <summary>
        ''' 递归的得到子节点的数目
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public ReadOnly Property Count As Integer
            Get
                Dim n As Integer

                If Not Left Is Nothing Then
                    n += 1
                    n += Left.Count
                End If

                If Not Right Is Nothing Then
                    n += 1
                    n += Right.Count
                End If

                Return n
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Value Is Nothing Then
                Return Name
            Else
                Return Name & " ==> " & Value.ToString
            End If
        End Function

        ''' <summary>
        ''' 最多只有两个元素
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerable(Of TreeNode(Of T))
            If Not Left Is Nothing Then
                Yield Left
            End If
            If Not Right Is Nothing Then
                Yield Right
            End If
        End Function

        Public Shared Operator +(parent As TreeNode(Of T), child As TreeNode(Of T)) As TreeNode(Of T)
            If parent.Left Is Nothing Then
                parent.Left = child
                Return parent
            End If
            If parent.Right Is Nothing Then
                parent.Right = child
                Return parent
            End If

            Throw New Exception("TreeNode is full, can not append any more!")
        End Operator

        Public Shared Operator -(parent As TreeNode(Of T), child As TreeNode(Of T)) As TreeNode(Of T)
            If Not parent.Left Is Nothing Then
                If parent.Left.Equals(child) Then
                    parent.Left = Nothing
                    Return parent
                End If
            End If
            If Not parent.Right Is Nothing Then
                If parent.Right.Equals(child) Then
                    parent.Right = Nothing
                    Return parent
                End If
            End If
            Return parent
        End Operator
    End Class
End Namespace
