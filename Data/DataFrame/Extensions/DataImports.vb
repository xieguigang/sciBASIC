#Region "Microsoft.VisualBasic::529e23d01fa7ba7be9550f83c4e02c91, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\Extensions\DataImports.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Module provides the csv data imports operation of the csv document creates from a text file.
''' (模块提供了从文本文档之中导入数据的方法)
''' </summary>
''' <remarks></remarks>
''' 
<PackageNamespace("IO_Device.Csv.DataImports",
                  Description:="Module provides the csv data imports operation of the csv document creates from a text file.(模块提供了从文本文档之中导入数据的方法)",
                  Publisher:="xie.guigang@gmail.com")>
Public Module DataImports

    ''' <summary>
    ''' A regex expression string that use for split the line text.
    ''' </summary>
    ''' <remarks></remarks>
    Public Const SplitRegxExpression As String = "[" & vbTab & "{0}](?=(?:[^""]|""[^""]*"")*$)"

    ''' <summary>
    ''' Imports the data in a well formatted text file using a specific delimiter, default delimiter is comma character.
    ''' </summary>
    ''' <param name="txtPath">The file path for the data imports text file.(将要进行数据导入的文本文件)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("--Imports", Info:="Imports the data in a well formatted text file using a specific delimiter, default delimiter is comma character.")>
    Public Function [Imports](<Parameter("txt.Path", "The file path for the data imports text file.")> txtPath As String,
                              Optional delimiter As String = ",",
                              Optional encoding As System.Text.Encoding = Nothing) As DocumentStream.File
        If encoding Is Nothing Then
            encoding = System.Text.Encoding.Default
        End If

        Dim Lines As String() = IO.File.ReadAllLines(txtPath, encoding)
        Dim Csv As DocumentStream.File = New DocumentStream.File(ImportsData(Lines, delimiter), txtPath)
        Return Csv
    End Function

    ''' <summary>
    ''' Imports data source by using specific delimiter
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="delimiter"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension> Public Function [Imports](Of T As Class)(path As String, Optional delimiter As String = ",", Optional encoding As System.Text.Encoding = Nothing) As T()
        Dim source As DocumentStream.File = [Imports](path, delimiter, encoding)
        If source.RowNumbers = 0 Then
            Return New T() {}
        Else
            Return source.AsDataSource(Of T)(False)
        End If
    End Function

    <ExportAPI("Data.Imports")>
    Public Function ImportsData(<Parameter("str.Data")> s_Data As IEnumerable(Of String),
                                Optional delimiter As String = ",") As DocumentStream.File
        Dim Expression As String = String.Format(SplitRegxExpression, delimiter)
        Dim LQuery = (From line As String In s_Data Select RowParsing(line, Expression)).ToArray
        Return New DocumentStream.File(LQuery)
    End Function

    ''' <summary>
    ''' Row parsing its column tokens
    ''' </summary>
    ''' <param name="Line"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Row.Parsing", Info:="Row parsing its column tokens")>
    Public Function RowParsing(Line As String, SplitRegxExpression As String) As DocumentStream.RowObject
        Dim Row = Regex.Split(Line, SplitRegxExpression)
        For i As Integer = 0 To Row.Count - 1
            If Not String.IsNullOrEmpty(Row(i)) Then
                If Row(i).First = """"c AndAlso Row(i).Last = """"c Then
                    Row(i) = Mid(Row(i), 2, Len(Row(i)) - 2)
                End If
            End If
        Next
        Return Row
    End Function

    ''' <summary>
    ''' Imports the data in a well formatted text file using the fix length as the data separate method.
    ''' </summary>
    ''' <param name="txtPath"></param>
    ''' <param name="length">The string length width of the data row.(固定的列字符数的宽度)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Imports.FixLength", Info:="Imports the data in a well formatted text file using the fix length as the data separate method.")>
    Public Function FixLengthImports(txtPath As String,
                                     <Parameter("Length", "The string length width of the data row.")> Optional length As Integer = 10,
                                     Optional encoding As System.Text.Encoding = Nothing) As Microsoft.VisualBasic.Data.csv.DocumentStream.File
        If encoding Is Nothing Then
            encoding = System.Text.Encoding.Default
        End If

        Dim Lines As String() = IO.File.ReadAllLines(txtPath, encoding)
        Dim LQuery As Csv.DocumentStream.RowObject() = (From line As String In Lines Select RowParsing(line, length:=length)).ToArray
        Dim Csv As New DocumentStream.File(LQuery, txtPath)
        Return Csv
    End Function

    <ExportAPI("Row.Parsing")>
    Public Function RowParsing(line As String, length As Integer) As DocumentStream.RowObject
        Dim n As Integer = CInt(Len(line) / length) + 1
        Dim cols As String() = New String(n - 1) {}
        For i As Integer = 0 To n - 1 Step length
            cols(i) = Mid(line, i, length)
        Next
        Return New DocumentStream.RowObject With {
            ._innerColumns = cols.ToList
        }
    End Function

    ''' <summary>
    ''' 从字符串集合之中推测可能的数据类型
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
    Public Function SampleForType(column As IEnumerable(Of String)) As Type
        Dim n As Integer = column.Count
        Dim LQuery As Integer = (From s As String In column Let Dbl As Double = Val(s) Let ss As String = Dbl.ToString Where String.Equals(ss, s) Select 1).ToArray.Count
        If LQuery = n Then Return GetType(Double)

        LQuery = (From s As String In column Let Int As Integer = CInt(Val(s)) Let ss As String = Int.ToString Where String.Equals(ss, s) Select 1).ToArray.Count
        If LQuery = n Then Return GetType(Integer)

        LQuery = (From s As String In column Let Bol As Boolean = Boolean.Parse(s) Let ss As String = Bol.ToString Where String.Equals(ss, s) Select 1).ToArray.Count
        If LQuery = n Then Return GetType(Boolean)

        LQuery = (From s As String In column Let Dat As Date = Date.Parse(s)
                  Where Dat.Year = 0 AndAlso Dat.Month = 0 AndAlso Dat.Day = 0 AndAlso Dat.Hour = 0 AndAlso Dat.Minute = 0 AndAlso Dat.Second = 0
                  Select 1).ToArray.Count
        If LQuery = 0 Then Return GetType(Date)

        Return GetType(String)
    End Function
End Module
