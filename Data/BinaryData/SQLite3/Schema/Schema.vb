#Region "Microsoft.VisualBasic::74ad6c123ba6023aa8d9c334a1f9e0b0, Data\BinaryData\SQLite3\Schema\Schema.vb"

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

    '   Total Lines: 154
    '    Code Lines: 113 (73.38%)
    ' Comment Lines: 12 (7.79%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 29 (18.83%)
    '     File Size: 6.83 KB


    '     Class Schema
    ' 
    '         Properties: columns, PrimaryKeys, RawSql, Schema, tableName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GenericEnumerator, GetOrdinal, ParseColumns, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Core.SQLSchema

    Public Class Schema : Implements Enumeration(Of NamedValue(Of String))

        Public ReadOnly Property columns As NamedValue(Of String)()
            Get
                Return (From col In Schema.SchemaType Select New NamedValue(Of String)(col)).ToArray
            End Get
        End Property

        Public Property tableName As String
        Public ReadOnly Property RawSql As String
        Public ReadOnly Property Schema As HeaderSchema

        ''' <summary>
        ''' 表中声明的主键列名。SQLite 之中 ``INTEGER PRIMARY KEY`` 是 rowid 的别名,
        ''' 该列在记录体之中以 NULL 存储, 实际取值需要使用 rowid 进行回填。
        ''' </summary>
        Public ReadOnly Property PrimaryKeys As String()
            Get
                Return _primaryKeys.ToArray
            End Get
        End Property

        ReadOnly _primaryKeys As New System.Collections.Generic.List(Of String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(sql$, Optional removeNameEscape As Boolean = True)
            Me.RawSql = sql
            Me.Schema = New HeaderSchema(ParseColumns(sql, removeNameEscape))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOrdinal(column As String) As Integer
            Return Schema.GetOrdinal(column)
        End Function

        Private Iterator Function ParseColumns(sql$, removeNameEscape As Boolean) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As Token() = New SQLParser(sql).GetTokens.ToArray
            Dim [nameOf] = Function(text As Token()) As String
                               Dim raw As String = text(Scan0).text

                               ' 注意: 不能使用 GetStackValue 来剥离方括号, 因为它对长度小于 2 的字符串会直接返回空串
                               If removeNameEscape AndAlso raw IsNot Nothing AndAlso raw.Length >= 2 AndAlso
                                  raw.First = "["c AndAlso raw.Last = "]"c Then

                                   Return raw.Substring(1, raw.Length - 2)
                               End If

                               Return raw
                           End Function

            If Not (tokens(Scan0).isKeyword("create") AndAlso tokens(1).isKeyword("table")) Then
                Throw New InvalidProgramException("Only 'CREATE TABLE' expression is allowed!")
            End If

            Me.tableName = tokens(2).text

            tokens = tokens.Skip(4).Take(tokens.Length - 5).ToArray

            Dim type As String
            Dim name As String
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenTypes.comma)

            For Each block As Token() In blocks _
                .Where(Function(b)
                           Return Not b.Length = 1 AndAlso Not b(Scan0).name = TokenTypes.comma
                       End Function)

                Dim rawName As String = [nameOf](block)

                ' 仅当名称确实被双引号包围时才剥离引号(单字符列名会被 GetStackValue 误判为空串)
                If rawName IsNot Nothing AndAlso rawName.Length >= 2 AndAlso
                   rawName.First = """"c AndAlso rawName.Last = """"c Then

                    name = rawName.Substring(1, rawName.Length - 2)
                Else
                    name = rawName
                End If
                type = block.ElementAtOrNull(1)?.text

                ' 跳过表级约束定义(CHECK/CONSTRAINT/UNIQUE/FOREIGN KEY/PRIMARY KEY),
                ' 这些并不是真实的数据列
                If name.ToUpper = "CHECK" OrElse name.ToUpper = "CONSTRAINT" Then
                    Continue For
                End If
                If name.ToUpper = "UNIQUE" AndAlso block.Length > 1 AndAlso block(1).text = "(" Then
                    Continue For
                End If
                If name.ToUpper = "FOREIGN" AndAlso type IsNot Nothing AndAlso type.ToUpper = "KEY" Then
                    Continue For
                End If
                If block(Scan0).text.ToUpper = "PRIMARY" AndAlso block.Length > 1 AndAlso block(1).text.ToUpper = "KEY" Then
                    ' 表级主键约束: 记录主键列(INTEGER PRIMARY KEY 是 rowid 的别名)
                    For k As Integer = 2 To block.Length - 1
                        Dim pkToken As String = block(k).text

                        If pkToken = "(" OrElse pkToken = ")" OrElse pkToken = "," Then
                            Continue For
                        End If

                        _primaryKeys.Add(pkToken)
                    Next

                    Continue For
                End If

                If type Is Nothing Then
                    ' 未声明类型的列在 SQLite 之中具有 BLOB 亲和性
                    type = "blob"
                ElseIf type.ToLower = "[varchar]" Then
                    If tokens.Length > 2 AndAlso tokens(2).text.IsPattern("\(\s*\d+\s*\)") Then
                        type = type.GetStackValue("[", "]") & tokens(2).text
                    Else
                        type = type.GetStackValue("[", "]")
                    End If
                ElseIf type.ToLower = "not" Then
                    ' 列名之后缺少类型声明, 按 BLOB 亲和性处理
                    type = "blob"
                End If

                ' 列级主键约束: col INTEGER PRIMARY KEY
                For k As Integer = 0 To block.Length - 2
                    If block(k).text.ToUpper = "PRIMARY" AndAlso block(k + 1).text.ToUpper = "KEY" Then
                        _primaryKeys.Add(name)
                        Exit For
                    End If
                Next

                Yield New NamedValue(Of String) With {
                    .Name = name,
                    .Value = type
                }
            Next
        End Function

        Public Overrides Function ToString() As String
            Return Schema.Headers.GetJson
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedValue(Of String)) Implements Enumeration(Of NamedValue(Of String)).GenericEnumerator
            For Each col As NamedValue(Of String) In columns
                Yield col
            Next
        End Function
    End Class
End Namespace
