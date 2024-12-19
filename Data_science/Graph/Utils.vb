#Region "Microsoft.VisualBasic::45f4a1065f4fcd7f9ae6416b5ac47327, Data_science\Graph\Utils.vb"

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

    '   Total Lines: 85
    '    Code Lines: 55 (64.71%)
    ' Comment Lines: 20 (23.53%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (11.76%)
    '     File Size: 2.88 KB


    ' Module Utils
    ' 
    '     Function: Build, Summary, SummaryMe
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module Utils

    <Extension>
    Public Function Translate(Of V)(bin As BinaryTree(Of String, V)) As Tree(Of V, String)
        Dim root As New Tree(Of V, String) With {
            .Data = bin.Value,
            .label = bin.Key,
            .ID = bin.GetHashCode,
            .Parent = Nothing,
            .Childs = New Dictionary(Of String, Tree(Of V, String))
        }

        If Not bin.Left Is Nothing Then
            Call root.Childs.Add("left", bin.Left.Translate(root))
        End If
        If Not bin.Right Is Nothing Then
            Call root.Childs.Add("right", bin.Right.Translate(root))
        End If

        Return root
    End Function

    <Extension>
    Private Function Translate(Of V)(bin As BinaryTree(Of String, V), parent As Tree(Of V, String)) As Tree(Of V, String)
        Dim node As New Tree(Of V, String) With {
            .Data = bin.Value,
            .label = bin.Key,
            .ID = bin.GetHashCode,
            .Parent = parent,
            .Childs = New Dictionary(Of String, Tree(Of V, String))
        }

        If Not bin.Left Is Nothing Then
            Call node.Childs.Add("left", bin.Left.Translate(node))
        End If
        If Not bin.Right Is Nothing Then
            Call node.Childs.Add("right", bin.Right.Translate(node))
        End If

        Return node
    End Function

    ''' <summary>
    ''' Tree to string
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Build(Of T, K)(tree As Tree(Of T, K), Optional labelKey As Boolean = False) As String
        If tree Is Nothing Then
            Return "()"
        End If

        If tree.IsLeaf Then
            Return If(labelKey, EscapeLabel(tree.label), tree.ID)
        Else
            Dim children = tree _
                .EnumerateChilds _
                .Select(Function(tr) tr.Build) _
                .JoinBy(", ")

            Return $"{If(labelKey, EscapeLabel(tree.label), tree.ID)}({children})"
        End If
    End Function

    Private Function EscapeLabel(label As String) As String
        If label.IndexOf(","c) > -1 OrElse label.IndexOf("("c) > -1 OrElse label.IndexOf(")"c) > -1 Then
            Return $"""{label}"""
        ElseIf label.IndexOf(""""c) > -1 Then
            Return $"'{label}'"
        Else
            Return label
        End If
    End Function

    ''' <summary>
    ''' Summary this tree model its nodes as csv table
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="tree"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Summary(Of T, K, V As {New, INamedValue, DynamicPropertyBase(Of String)})(tree As Tree(Of T, K), Optional schema As PropertyInfo() = Nothing) As IEnumerable(Of V)
        If schema.IsNullOrEmpty Then
            schema = DataFramework _
                .Schema(Of T)(PropertyAccess.Readable, nonIndex:=True, primitive:=True) _
                .Values _
                .ToArray
        End If

        Yield tree.SummaryMe(Of V)(schema)

        For Each c As Tree(Of T, K) In tree.EnumerateChilds.SafeQuery
            For Each value In c.Summary(Of V)(schema)
                Yield value
            Next
        Next
    End Function

    ''' <summary>
    ''' 这个函数不会对<see cref="Tree(Of T, K).Childs"/>进行递归
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="this"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Private Function SummaryMe(Of T, K, V As {New, INamedValue, DynamicPropertyBase(Of String)})(this As Tree(Of T, K), schema As PropertyInfo()) As V
        Dim name$ = this.label
        Dim values As Dictionary(Of String, String) = schema _
            .ToDictionary(Function(key) key.Name,
                          Function(read)
                              Return CStrSafe(read.GetValue(this.Data))
                          End Function)

        With values
            Call .Add("tree.ID", this.ID)
            Call .Add("tree.Label", this.label)
        End With

        Return New V With {
            .Key = name,
            .Properties = values
        }
    End Function
End Module
