#Region "Microsoft.VisualBasic::862586d68145314943782a63e7554a9e, Data\BinaryData\SQLite3\Writer\Sqlite3Column.vb"

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

    '   Total Lines: 104
    '    Code Lines: 58 (55.77%)
    ' Comment Lines: 25 (24.04%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 21 (20.19%)
    '     File Size: 3.41 KB


    '     Class Sqlite3Column
    ' 
    '         Properties: Name, NotNull, PrimaryKey, Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: EscapeName, IsPlainName, IsRowIdAlias, ToSql, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Writer

    ''' <summary>
    ''' SQLite 数据表的列定义。
    ''' </summary>
    Public Class Sqlite3Column

        ''' <summary>
        ''' 列名
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 声明类型, 例如 INTEGER / VARCHAR / FLOAT / BLOB / BOOLEAN / DATETIME
        ''' </summary>
        Public Property Type As String

        ''' <summary>
        ''' 是否非空(用于生成 DDL 与读取侧的 NOT NULL 校验)
        ''' </summary>
        Public Property NotNull As Boolean

        ''' <summary>
        ''' 是否为主键。当声明类型为 INTEGER 且为主键时, 该列是 rowid 的别名。
        ''' </summary>
        Public Property PrimaryKey As Boolean

        Public Sub New()
        End Sub

        Public Sub New(name As String, type As String, Optional notNull As Boolean = False, Optional primaryKey As Boolean = False)
            Me.Name = name
            Me.Type = type
            Me.NotNull = notNull
            Me.PrimaryKey = primaryKey
        End Sub

        ''' <summary>
        ''' 生成该列在 CREATE TABLE 语句之中的定义片段
        ''' </summary>
        Public Function ToSql() As String
            Dim declared As String = If(String.IsNullOrWhiteSpace(Type), "BLOB", Type.Trim())
            Dim sql As String = EscapeName(Name) & " " & declared

            If NotNull Then
                sql &= " NOT NULL"
            End If

            ' 列级主键使用 INTEGER PRIMARY KEY 形式, 从而与 rowid 别名语义保持一致
            If PrimaryKey Then
                sql &= " PRIMARY KEY"
            End If

            Return sql
        End Function

        ''' <summary>
        ''' 判断该列是否为 rowid 的别名(INTEGER PRIMARY KEY)
        ''' </summary>
        Public Function IsRowIdAlias() As Boolean
            Return PrimaryKey AndAlso String.Equals(If(Type, "").Trim(), "integer", StringComparison.OrdinalIgnoreCase)
        End Function

        ''' <summary>
        ''' 对可能包含特殊字符的标识符使用 [] 转义
        ''' </summary>
        Public Shared Function EscapeName(name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return "[]"
            End If

            If IsPlainName(name) Then
                Return name
            End If

            Return "[" & name.Replace("]", "]]") & "]"
        End Function

        Private Shared Function IsPlainName(name As String) As Boolean
            For i As Integer = 0 To name.Length - 1
                Dim c As Char = name(i)
                Dim isLetter As Boolean = (c >= "a"c AndAlso c <= "z"c) OrElse (c >= "A"c AndAlso c <= "Z"c)
                Dim isDigit As Boolean = c >= "0"c AndAlso c <= "9"c

                If i = 0 Then
                    If Not isLetter AndAlso c <> "_"c Then
                        Return False
                    End If
                ElseIf Not isLetter AndAlso Not isDigit AndAlso c <> "_"c Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Overrides Function ToString() As String
            Return ToSql()
        End Function
    End Class

End Namespace

