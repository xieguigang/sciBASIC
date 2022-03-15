#Region "Microsoft.VisualBasic::55ece0719175b253c6e81b8ca6186854, sciBASIC#\Data_science\Graph\Utils.vb"

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

    '   Total Lines: 83
    '    Code Lines: 53
    ' Comment Lines: 20
    '   Blank Lines: 10
    '     File Size: 2.74 KB


    ' Module Utils
    ' 
    '     Function: Build, Summary, SummaryMe
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module Utils

    ''' <summary>
    ''' Tree to string
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <Extension> Public Function Build(Of T, K)(tree As Tree(Of T, K)) As String
        If tree Is Nothing Then
            Return "()"
        End If

        If tree.IsLeaf Then
            Return tree.ID
        Else
            Dim children = tree _
                .EnumerateChilds _
                .Select(Function(tr) tr.Build) _
                .JoinBy(", ")

            Return $"{tree.ID}({children})"
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
    Public Iterator Function Summary(Of T, K)(tree As Tree(Of T, K), Optional schema As PropertyInfo() = Nothing) As IEnumerable(Of NamedValue(Of Dictionary(Of String, String)))
        If schema.IsNullOrEmpty Then
            schema = DataFramework _
                .Schema(Of T)(PropertyAccess.Readable, nonIndex:=True, primitive:=True) _
                .Values _
                .ToArray
        End If

        Yield tree.SummaryMe(schema)

        For Each c As Tree(Of T, K) In tree.EnumerateChilds.SafeQuery
            For Each value In c.Summary(schema)
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
    Private Function SummaryMe(Of T, K)(this As Tree(Of T, K), schema As PropertyInfo()) As NamedValue(Of Dictionary(Of String, String))
        Dim name$ = this.Label
        Dim values = schema.ToDictionary(
            Function(key) key.Name,
            Function(read)
                Return CStrSafe(read.GetValue(this.Data))
            End Function)

        With values
            Call .Add("tree.ID", this.ID)
            Call .Add("tree.Label", this.Label)
        End With

        Return New NamedValue(Of Dictionary(Of String, String)) With {
            .Name = name,
            .Value = values
        }
    End Function
End Module
