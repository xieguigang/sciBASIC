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

Namespace ManagedSqlite.Core

    Public Class Schema

        Public Property columns As NamedValue(Of String)()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(columns$(), removeNameEscape As Boolean)
            Me.columns = ParseColumns(columns, removeNameEscape).ToArray
        End Sub

        Private Iterator Function ParseColumns(columns As String(), removeNameEscape As Boolean) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As String()
            Dim field As NamedValue(Of String)
            Dim name As String
            Dim [nameOf] = Function(text As String())
                               If removeNameEscape Then
                                   Return text(Scan0).GetStackValue("[", "]")
                               Else
                                   Return text(Scan0)
                               End If
                           End Function

            For Each column As String In columns
                tokens = column.TrimNewLine.StringSplit("\s+")

                If tokens(Scan0) = "CONSTRAINT" AndAlso tokens.Last.IsPattern("\(.+\)") Then
                    ' 索引约束之类的表结构信息
                    ' 则跳过这个非字段定义的表结构信息
                    ' CONSTRAINT [pk_CdbCompound] PRIMARY KEY ([Id])
                    Continue For
                End If

                name = [nameOf](tokens)
                field = New NamedValue(Of String) With {
                    .Name = name,
                    .Value = tokens(1)
                }

                Yield field
            Next
        End Function

        Public Overrides Function ToString() As String
            Return columns.Keys.GetJson
        End Function
    End Class
End Namespace
