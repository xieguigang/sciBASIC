#Region "Microsoft.VisualBasic::b343077c87065c0937e0399023a225d0, Data\BinaryData\BinaryData\SQLite3\Schema\Schema.vb"

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

'     Class Schema
' 
'         Properties: columns
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ParseColumns, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ManagedSqlite.Core.SQLSchema

    Public Class Schema

        Public Property columns As NamedValue(Of String)()
        Public Property tableName As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(sql$, removeNameEscape As Boolean)
            Me.columns = ParseColumns(sql, removeNameEscape).ToArray
        End Sub

        Private Iterator Function ParseColumns(sql$, removeNameEscape As Boolean) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As Token() = New SQLParser(sql).GetTokens.ToArray
            Dim field As NamedValue(Of String)
            Dim type As String
            Dim name As String
            Dim [nameOf] = Function(text As String())
                               If removeNameEscape Then
                                   Return text(Scan0).GetStackValue("[", "]")
                               Else
                                   Return text(Scan0)
                               End If
                           End Function

            If Not (tokens(Scan0).isKeyword("create") AndAlso tokens(1).isKeyword("table")) Then
                Throw New InvalidProgramException("Only 'CREATE TABLE' expression is allowed!")
            End If

            Me.tableName = tokens(2).text

            'For Each column As String In columns.Where(Function(s) Not s.StringEmpty)
            '    tokens = column _
            '        .TrimNewLine _
            '        .Trim(ASCII.TAB, " "c) _
            '        .StringSplit("\s+")

            '    If tokens(Scan0) = "CONSTRAINT" AndAlso tokens.Last.IsPattern("\(.+\)") Then
            '        ' 索引约束之类的表结构信息
            '        ' 则跳过这个非字段定义的表结构信息
            '        ' CONSTRAINT [pk_CdbCompound] PRIMARY KEY ([Id])
            '        Continue For
            '    End If

            '    name = [nameOf](tokens).GetStackValue("""", """")
            '    type = tokens.ElementAtOrDefault(1, "text")

            '    If type.ToLower = "[varchar]" Then
            '        If tokens.Length > 2 AndAlso tokens(2).IsPattern("\(\s*\d+\s*\)") Then
            '            type = type.GetStackValue("[", "]") & tokens(2)
            '        Else
            '            type = type.GetStackValue("[", "]")
            '        End If
            '    ElseIf type.ToLower = "not" AndAlso tokens(2).ToLower = "null" Then
            '        type = "blob"
            '    End If

            '    field = New NamedValue(Of String) With {
            '        .Name = name,
            '        .Value = type
            '    }

            '    Yield field
            'Next
        End Function

        Public Overrides Function ToString() As String
            Return columns.Keys.GetJson
        End Function
    End Class
End Namespace
