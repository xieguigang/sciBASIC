#Region "Microsoft.VisualBasic::a0e7d4c2ff4e5488dc319edd63614288, Data\DataFrame\DataSet\MySQL.vb"

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

    '   Total Lines: 203
    '    Code Lines: 123 (60.59%)
    ' Comment Lines: 55 (27.09%)
    '    - Xml Docs: 96.36%
    ' 
    '   Blank Lines: 25 (12.32%)
    '     File Size: 8.08 KB


    '     Module MySQL
    ' 
    '         Function: DumpToTable, ImportsMySQLDump, INSERT_LineParser, SQLData, SQLEntityData
    '                   SQLFields, SQLValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Option Strict Off
Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex

Namespace DATA

    ''' <summary>
    ''' MySQL data extensions
    ''' </summary>
    Public Module MySQL

        ''' <summary>
        ''' 提供了一个与SQL DUMP功能类似的拓展方法，这个函数会自动的将目标集合写入所指定的文件夹之中的某一个csv文件。
        ''' 这个csv文件的文件名为泛型类型的Class名称
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="handle$">Directory or file name(``*.csv``)</param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DumpToTable(Of T)(data As IEnumerable(Of T),
                                          handle$,
                                          Optional encoding As Encodings = Encodings.ASCII) As Boolean
            Dim path$ = If(
                handle.Split("."c).Last.TextEquals("csv"),
                handle,
                $"{handle}/{GetType(T).Name}.csv")

            Return data.SaveTo(path,, encoding.CodePage)
        End Function

        ''' <summary>
        ''' 将SQL脚本之中的数据转换为Excel数据框模型，这个函数仅对简单的SQL脚本有效，
        ''' **例如仅包含有数值类型的，字符串之中不包含有逗号，双引号之类的csv格式控制符号，**
        ''' 假若字段之中的值是比较复杂的字符串，则这个函数就无能为力了
        ''' </summary>
        ''' <param name="SQL$"></param>
        ''' <param name="primary$"></param>
        ''' <param name="dumpMode"></param>
        ''' <returns></returns>
        <Extension>
        Public Function SQLEntityData(SQL$, primary$, Optional dumpMode As Boolean = True) As EntityObject()
            Dim data As IEnumerable(Of Dictionary(Of String, String)) = SQL.SQLData(dumpMode)
            Dim out As New List(Of EntityObject)

            For Each o As Dictionary(Of String, String) In data
                out += New EntityObject With {
                    .ID = o(primary),
                    .Properties = o
                }

                Call o.Remove(primary)
            Next

            Return out
        End Function

        ''' <summary>
        ''' 加入是以Dump模型运行的话，标题只会被解析一次，其他的行数据的标题都会使用第一行的数据的标题，
        ''' 这个函数只适用于``INSERT INTO``数据插入语句
        ''' </summary>
        ''' <param name="handle">SQL文本或者文件的路径</param>
        ''' <param name="dumpMode"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SQLData(handle$, Optional dumpMode As Boolean = True) As IEnumerable(Of Dictionary(Of String, String))
            Dim firstHeader As Index(Of String) = handle.LineIterators.First.SQLFields
            Dim getHeader = Function(SQL$) firstHeader

            If Not dumpMode Then
                getHeader = Function(SQL$) SQL.SQLFields
            End If

            ' 在这里不需要跳过第一行的数据，因为第一行不是标题行，也是数据的一部分
            For Each rowSQL As String In handle.LineIterators
                Dim fields As Index(Of String) = getHeader(SQL:=rowSQL)
                Dim values As String() = rowSQL.SQLValues
                Dim row As New Dictionary(Of String, String)

                For Each map As SeqValue(Of String) In fields
                    row.Add(map.value, values(map.i))
                Next

                Yield row
            Next
        End Function

        Const blockMark$ = "[\d']\),\([\d']"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sql$">
        ''' The file path of the MySQL dump *.sql file. This sql file should only contains the ``INSERT INTO`` sql statement.
        ''' </param>
        ''' <returns></returns>
        Public Iterator Function ImportsMySQLDump(sql$) As IEnumerable(Of NamedValue(Of RowObject))
            For Each line As String In sql.IterateAllLines
                If InStr(line, "INSERT INTO") = 1 Then
                    For Each row In line.INSERT_LineParser
                        Yield row
                    Next
                End If
            Next
        End Function

        ''' <summary>
        ''' Handle the MySQL dump
        ''' </summary>
        ''' <param name="insert$">
        ''' ```SQL
        ''' INSERT INTO `term_synonym` VALUES (0,1,'mitochondrial inheritance','EXACT',''),(1,3,'reproductive physiological process','EXACT',''),...
        ''' ```
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function INSERT_LineParser(insert$) As IEnumerable(Of NamedValue(Of RowObject))
            Dim skip%
            Dim quotEscaping As Boolean
            Dim name As String = ""

            For Each c As Char In insert
                If c = "`"c Then
                    If quotEscaping Then
                        quotEscaping = False
                    Else
                        quotEscaping = True
                    End If
                ElseIf quotEscaping Then
                    name &= c
                ElseIf c = "("c Then
                    If Not quotEscaping Then
                        Exit For
                    End If
                End If

                skip += 1
            Next

            Dim data$ = Mid(insert, Start:=skip + 1S).Trim(";"c).Trim("("c, ")"c)
            Dim blocks = r.Split(data, blockMark)
            Dim values As IEnumerable(Of String)

            For Each block As String In blocks
                values = Tokenizer _
                    .CharsParser(block,, ASCII.Mark) _
                    .ToArray

                Yield New NamedValue(Of RowObject) With {
                    .Name = name,
                    .Value = New RowObject(values)
                }
            Next
        End Function

        ''' <summary>
        ''' Parse the ``VALUES`` data from the INSERT INTo SQL statement.
        ''' </summary>
        ''' <param name="insertSQL$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function SQLValues(insertSQL$) As String()
            Dim values$ = Regex.Split(insertSQL, "\)\s*VALUES\s*\(", RegexICSng).Last
            Dim t$() = Tokenizer _
                .CharsParser(values, quot:=ASCII.Mark) _
                .Select(Function(s) s.GetStackValue("'", "'")) _
                .ToArray
            Return t
        End Function

        ''' <summary>
        ''' Gets the fields list from INSERT INTO SQL.
        ''' </summary>
        ''' <param name="insertSQL$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function SQLFields(insertSQL$) As Index(Of String)
            Dim fields$ = Regex _
                .Match(insertSQL, "INSERT INTO .+?\)\s*VALUES\s*\(", RegexICSng) _
                .Value _
                .StringSplit("\s*VALUES\s*\(") _
                .First _
                .GetStackValue("(", ")")
            Dim names$() = Tokenizer _
                .CharsParser(fields) _
                .Select(Function(s) s.GetStackValue("`", "`")) _
                .ToArray

            Return New Index(Of String)(names)
        End Function
    End Module
End Namespace
