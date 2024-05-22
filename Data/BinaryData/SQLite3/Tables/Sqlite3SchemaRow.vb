#Region "Microsoft.VisualBasic::cd513f5e628b87f17a6b8fbca002f2a5, Data\BinaryData\SQLite3\Tables\Sqlite3SchemaRow.vb"

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

    '   Total Lines: 30
    '    Code Lines: 21 (70.00%)
    ' Comment Lines: 3 (10.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 1.08 KB


    '     Class Sqlite3SchemaRow
    ' 
    '         Properties: name, rootPage, Sql, tableName, type
    ' 
    '         Function: ParseSchema, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3SchemaRow

        Public Property type As String
        Public Property name As String
        Public Property tableName As String
        Public Property rootPage As UInteger
        Public Property Sql As String

        Public Overrides Function ToString() As String
            If type.TextEquals("table") Then
                Return Sql
            Else
                Return $"[{type}] {tableName}|{name}"
            End If
        End Function

        Public Function ParseSchema(Optional removeNameEscape As Boolean = False) As Schema
            ' 有一些字段的名称可能是包含有空格或者小数点之类的,
            ' 则这些字段名称会被[]转义
            ' 在下面的构造函数调用过程之中会根据参数值来自动处理这些转义
            Dim schema As New Schema(Sql, removeNameEscape)
            Return schema
        End Function

    End Class
End Namespace
