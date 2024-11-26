#Region "Microsoft.VisualBasic::8f0a88335dd8bfb6e5c7f8cce742ea79, Data\DataFrame\Extensions\DataImports.vb"

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

'   Total Lines: 202
'    Code Lines: 106 (52.48%)
' Comment Lines: 76 (37.62%)
'    - Xml Docs: 90.79%
' 
'   Blank Lines: 20 (9.90%)
'     File Size: 8.33 KB


' Module DataImports
' 
'     Function: (+2 Overloads) [Imports], FixLengthImports, (+3 Overloads) ImportsData, ImportsTsv, (+2 Overloads) RowParsing
'               SampleForType
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports csvFile = Microsoft.VisualBasic.Data.csv.IO.File

''' <summary>
''' Module provides the csv data imports operation of the csv document creates from a text file.
''' (模块提供了从文本文档之中导入数据的方法)
''' </summary>
''' <remarks></remarks>
''' 
<Package("IO_Device.Csv.DataImports",
                  Description:="Module provides the csv data imports operation of the csv document creates from a text file.",
                  Publisher:="xie.guigang@gmail.com")>
Public Module DataImports

    ''' <summary>
    ''' A regex expression string that use for split the line text.
    ''' </summary>
    ''' <remarks></remarks>
    Public Const SplitRegxExpression As String = "[" & vbTab & "{0}](?=(?:[^""]|""[^""]*"")*$)"

    ''' <summary>
    ''' Parse the csv table file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function ReadCsv(file As String, Optional encoding As Encodings = Encodings.UTF8) As File
        Return file.LoadCsv(encoding)
    End Function

    ''' <summary>
    ''' Imports the data in a well formatted text file using a specific delimiter, default delimiter is comma character.
    ''' </summary>
    ''' <param name="txtPath">The file path for the data imports text file.(将要进行数据导入的文本文件)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Function [Imports](<Parameter("txt.Path", "The file path for the data imports text file.")> txtPath$,
                              Optional delimiter$ = ",",
                              Optional encoding As Encoding = Nothing) As File
        Dim lines As String() = txtPath.ReadAllLines(encoding)
        Dim csv As New File(ImportsData(lines, delimiter), txtPath)
        Return csv
    End Function

    ''' <summary>
    ''' Imports data source by using specific delimiter.(这个函数是一个安全的函数，当目标文件不存在的时候，返回的是一个空集合)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="delimiter"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function [Imports](Of T As Class)(path$,
                                             Optional delimiter$ = ",",
                                             Optional encoding As Encoding = Nothing,
                                             Optional nameMaps As Dictionary(Of String, String) = Nothing,
                                             Optional mute As Boolean = False) As IEnumerable(Of T)

        Dim source As IO.File = [Imports](path, delimiter, encoding)

        If source.RowNumbers = 0 Then
            Return New T() {}
        Else
            Return source.AsDataSource(Of T)(False, maps:=nameMaps, silent:=mute)
        End If
    End Function

    <ExportAPI("Data.Imports")>
    Public Function ImportsData(lines As IEnumerable(Of String), Optional delimiter$ = ",") As csvFile
        Dim regexp As String = String.Format(SplitRegxExpression, delimiter)
        Dim LQuery = (From line As String In lines Select RowParsing(line, regexp)).ToArray
        Return New csvFile(LQuery)
    End Function

    ''' <summary>
    ''' 使用特定的分隔符进行数据的导入操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="text$"></param>
    ''' <param name="delimiter$"></param>
    ''' <param name="maps"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ImportsData(Of T As Class)(text$, Optional delimiter$ = ",", Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)
        Return ImportsData(text.LineTokens, delimiter).AsDataSource(Of T)(maps:=maps)
    End Function

    ''' <summary>
    ''' 使用特定的分隔符进行数据的导入操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="text"></param>
    ''' <param name="delimiter$"></param>
    ''' <param name="maps"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ImportsData(Of T As Class)(text As IEnumerable(Of String), Optional delimiter$ = ",", Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)
        Return ImportsData(text, delimiter).AsDataSource(Of T)(maps:=maps)
    End Function

    ''' <summary>
    ''' 导入TSV数据为内存表
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="lines"></param>
    ''' <param name="maps"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ImportsTsv(Of T As Class)(lines As IEnumerable(Of String), Optional maps As NameMapping = Nothing) As IEnumerable(Of T)
        Return ImportsData(lines, ASCII.TAB) _
            .AsDataSource(Of T)(maps:=maps)
    End Function

    ''' <summary>
    ''' Row parsing its column tokens
    ''' </summary>
    ''' <param name="Line"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Function RowParsing(Line As String, SplitRegxExpression As String) As RowObject
        If Line.StringEmpty(whitespaceAsEmpty:=False) Then
            Return New RowObject(New String() {})
        Else
            Dim columns$() = Regex.Split(Line, SplitRegxExpression)

            For i As Integer = 0 To columns.Length - 1
                If Not String.IsNullOrEmpty(columns(i)) Then
                    If columns(i).First = """"c AndAlso columns(i).Last = """"c Then
                        columns(i) = Mid(columns(i), 2, Len(columns(i)) - 2)
                    End If
                End If
            Next

            Return New RowObject(columns)
        End If
    End Function

    ''' <summary>
    ''' Imports the data in a well formatted text file using the fix length as the data separate method.
    ''' </summary>
    ''' <param name="txtPath"></param>
    ''' <param name="length">The string length width of the data row.(固定的列字符数的宽度)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Function FixLengthImports(txtPath$,
                                     <Parameter("Length", "The string length width of the data row.")>
                                     Optional length% = 10,
                                     Optional encoding As Encoding = Nothing) As IO.File
        If encoding Is Nothing Then
            encoding = Encoding.Default
        End If

        Dim Lines As String() = txtPath.ReadAllLines(encoding)
        Dim LQuery As RowObject() = LinqAPI.Exec(Of RowObject) <=
                                                                 _
            From line As String
            In Lines
            Select RowParsing(line, length:=length)

        Dim csv As New File(LQuery, txtPath)
        Return csv
    End Function

    <ExportAPI("Row.Parsing")>
    Public Function RowParsing(line$, length%) As RowObject
        Dim n As Integer = CInt(Len(line) / length) + 1
        Dim cols$() = New String(n - 1) {}

        For i As Integer = 0 To n - 1 Step length
            cols(i) = Mid(line, i, length)
        Next

        Return New RowObject With {
            .buffer = cols.AsList
        }
    End Function

    ''' <summary>
    ''' 从字符串集合之中推测可能的列数据的类型
    ''' </summary>
    ''' <param name="column"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 推测规则：
    ''' 会对数据进行采样
    ''' 类型的优先级别为：
    ''' </remarks>
    ''' 
    <ExportAPI("DataType.Match")>
    <Extension>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function SampleForType(column As IEnumerable(Of String)) As Type
        Return TypeCast.DataImports.SampleForType(column.ToArray)
    End Function
End Module
