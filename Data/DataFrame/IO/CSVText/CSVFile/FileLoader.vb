#Region "Microsoft.VisualBasic::e8a917c7416fd52db577b9ae24c3b5a0, Data\DataFrame\IO\CSVText\CSVFile\FileLoader.vb"

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
    '    Code Lines: 111 (72.08%)
    ' Comment Lines: 23 (14.94%)
    '    - Xml Docs: 86.96%
    ' 
    '   Blank Lines: 20 (12.99%)
    '     File Size: 7.25 KB


    '     Module FileLoader
    ' 
    '         Function: FastLoad, (+2 Overloads) Load, parallelLoad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace IO.CSVFile

    ''' <summary>
    ''' Wrapper module for read csv/tsv file
    ''' </summary>
    Public Module FileLoader

        ''' <summary>
        ''' If you are sure about your csv data document have no character such like " or, in a cell, then you can try using this fast load method to load your csv data.
        ''' if not, please using the <see cref="load"></see> method to avoid of the data damages.
        ''' (假若你确信你的数据文件之中仅含有数字之类的数据，则可以尝试使用本方法进行快速加载，假若文件之中每一个单元格还含有引起歧义的例如双引号或者逗号，则请不要使用本方法进行加载)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为这个函数是直接通过利用逗号做切割，所以效率是非常高的，但是对文本的格式要求非常高
        ''' </remarks>
        Public Function FastLoad(path As String,
                                 Optional parallel As Boolean = True,
                                 Optional encoding As Encoding = Nothing,
                                 Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
                                 Optional tsv As Boolean = False) As File

            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim lines As String() = path.MapNetFile.ReadAllLines(encoding Or DefaultEncoding)
            Dim headers As New RowObject(lines(Scan0))
            Dim cData As New File
            Dim headerIndex As Integer = headers.IndexOf(skipWhile.Name)
            Dim deli As Char = If(tsv, ASCII.TAB, ","c)

            If parallel Then
                Dim cache = (From x As SeqValue(Of String) In lines.SeqIterator Select x)
                Dim Rows = From line As SeqValue(Of String)
                           In cache.AsParallel
                           Let __innerList As RowObject = New RowObject(line.value.Split(deli))
                           Select i = line.i,
                                data = __innerList
                           Order By i Ascending

                If headerIndex >= 0 Then
                    cData._innerTable = (From item In Rows Where Not skipWhile.Value(item.data(headerIndex)) Select item.data).AsList
                Else
                    cData._innerTable = (From item In Rows Select item.data).AsList
                End If
            Else
                Dim Rows = From strLine As String In lines
                           Let internal As RowObject = New RowObject(strLine.Split(deli))
                           Select internal

                If headerIndex >= 0 Then
                    cData._innerTable = Rows _
                        .Where(Function(r)
                                   Return Not skipWhile.Value(r(headerIndex))
                               End Function) _
                        .AsList
                Else
                    cData._innerTable = Rows.AsList
                End If
            End If

            Call $"CSV data ""{path.ToFileURL}"" load done!   {sw.ElapsedMilliseconds}ms.".debug

            Return cData
        End Function

        Public Function Load(s As Stream, trimBlanks As Boolean,
                             Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
                             Optional isTsv As Boolean = False) As List(Of RowObject)

            Using str As New StreamReader(s)
                Return Load(str.IteratesStream.ToArray, trimBlanks, skipWhile, isTsv:=isTsv)
            End Using
        End Function

        ''' <summary>
        ''' 排序操作在这里会不会大幅度的影响性能？
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="trimBlanks">如果这个选项为真，则会移除所有全部都是逗号分隔符``,,,,,,,,,``的空白行</param>
        ''' <returns></returns>
        Public Function Load(buf As String(), trimBlanks As Boolean,
                             Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
                             Optional isTsv As Boolean = False) As List(Of RowObject)

            If buf.IsNullOrEmpty Then
                Call "the given file content lines is empty!".warning
                ' empty file
                Return New List(Of RowObject)
            Else
                Return ParseInternal(buf, isTsv, trimBlanks, skipWhile)
            End If
        End Function

        Private Function ParseInternal(buf As String(), isTsv As Boolean, trimBlanks As Boolean, skipWhile As NamedValue(Of Func(Of String, Boolean))) As List(Of RowObject)
            Dim first As New RowObject(buf(Scan0), tsv:=isTsv)
            Dim headerIndex As Integer = first.IndexOf(skipWhile.Name)
            Dim delimiter As Char = If(isTsv, ASCII.TAB, ","c)

            If headerIndex = -1 AndAlso Not skipWhile.Name.StringEmpty Then
                Call $"Required test for skip on field: [{skipWhile.Name}], but no such field exists in current file data...".warning
            End If

            Dim table = buf.LoadTableData(headerIndex, isTsv, skipWhile).AsList

            If trimBlanks Then
                table = RowSetParser.GetNoneBlankRows(table).AsList
            End If

            Return first + table
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="headerIndex"></param>
        ''' <param name="isTsv"></param>
        ''' <param name="skipWhile"></param>
        ''' <returns>
        ''' table content data with header has already been removed
        ''' </returns>
        <Extension>
        Private Function LoadTableData(buf$(), headerIndex%, isTsv As Boolean, skipWhile As Func(Of String, Boolean)) As IEnumerable(Of RowObject)
            Dim loader As New RowSetParser(buf, workers:=App.CPUCoreNumbers - 1)
            Dim table As IEnumerable(Of RowObject)

            If buf.Length > 10000 Then
                ' needs run in parallel on processing large dataset
                loader.Run()
                table = loader.GetAllRows
            Else
                table = From s As String
                        In buf
                        Select New RowObject(s, tsv:=isTsv)
            End If

            If headerIndex = -1 Then
                ' returns all
                ' skip 1 for skip of the table header
                Return table.Skip(1)
            Else
                Return table.Skip(1) _
                    .Where(Function(r)
                               If skipWhile(r(headerIndex)) Then
                                   ' is a row that not needed...
                                   Return False
                               Else
                                   Return True
                               End If
                           End Function)
            End If
        End Function

        Private Class RowSetParser : Inherits VectorTask

            ReadOnly buf As String()
            ReadOnly rows As RowObject()

            Friend isTsv As Boolean
            Friend test As Func(Of String, Boolean)

            Public Sub New(buf As String(), Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
                MyBase.New(buf.Length, verbose, workers)

                Me.buf = buf
                Me.rows = New RowObject(buf.Length - 1) {}
            End Sub

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function GetAllRows() As IEnumerable(Of RowObject)
                Return rows
            End Function

            Public Shared Function GetNoneBlankRows(rows As IEnumerable(Of RowObject)) As IEnumerable(Of RowObject)
                Return From r As RowObject
                       In rows
                       Where Not r.IsNullOrEmpty
                       Where r.Any(Function(s) s <> "")
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function GetNoneBlankRows() As IEnumerable(Of RowObject)
                Return GetNoneBlankRows(rows)
            End Function

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim list As New List(Of RowObject)

                For i As Integer = start To ends
                    Call list.Add(New RowObject(buf(i), tsv:=isTsv))
                Next

                SyncLock rows
                    Call Array.Copy(list.ToArray, Scan0, rows, start, list.Count)
                End SyncLock
            End Sub
        End Class
    End Module
End Namespace
