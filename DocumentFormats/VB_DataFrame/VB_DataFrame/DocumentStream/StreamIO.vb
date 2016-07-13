#Region "Microsoft.VisualBasic::dd68b5dafd9441d8dd9d01840d368595, ..\VisualBasic_AppFramework\DocumentFormats\VB_DataFrame\VB_DataFrame\DocumentStream\StreamIO.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq

Namespace DocumentStream

    Public Module StreamIO

        ''' <summary>
        ''' 根据文件的头部的定义，从<paramref name="types"/>之中选取得到最合适的类型的定义
        ''' </summary>
        ''' <param name="df"></param>
        ''' <param name="types"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function [GetType](df As File, ParamArray types As Type()) As Type
            Dim head As String() = df.First.ToArray
            Dim scores As New Dictionary(Of Type, Integer)

            For Each schema In types.Select(AddressOf SchemaProvider.CreateObject)
                Dim allNames As String() = schema.Properties.ToArray(Function(x) x.Name)
                Dim matches = (From p As String
                               In allNames
                               Where Array.IndexOf(head, p) > -1
                               Select 1).Sum
                Call scores.Add(schema.DeclaringType, matches)
            Next

            Dim desc = From x
                       In scores
                       Select type = x.Key, x.Value
                       Order By Value Descending
            Dim target As Type = desc.FirstOrDefault?.type
            Return target
        End Function

        ''' <summary>
        ''' Save this csv document into a specific file location <paramref name="path"/>.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="lazySaved">Optional, this is for the consideration of performance and memory consumption.
        ''' When a data file is very large, then you may encounter a out of memory exception on a 32 bit platform,
        ''' then you should set this parameter to True to avoid this problem. Defualt is False for have a better
        ''' performance.
        ''' (当估计到文件的数据量很大的时候，请使用本参数，以避免内存溢出致使应用程序崩溃，默认为False，不开启缓存)
        ''' </param>
        ''' <remarks>当目标保存路径不存在的时候，会自动创建文件夹</remarks>
        Public Function SaveDataFrame(df As File,
                                      Optional path As String = "",
                                      Optional lazySaved As Boolean = False,
                                      Optional encoding As Encoding = Nothing) As Boolean

            If String.IsNullOrEmpty(path) Then
                Throw New NullReferenceException("path reference to a null location!")
            End If
            If encoding Is Nothing Then
                encoding = Encoding.UTF8
            End If

            If lazySaved Then
                Return __lazySaved(path, df, encoding)
            End If

            Dim stopwatch = System.Diagnostics.Stopwatch.StartNew
            Dim text As String = df.Generate

            Call Console.WriteLine("Generate csv file document using time {0} ms.", stopwatch.ElapsedMilliseconds)

            Return text.SaveTo(path, encoding)
        End Function

        ''' <summary>
        ''' 在保存大文件时为了防止在保存的过程中出现内存溢出所使用的一种保存方法
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="df"></param>
        ''' <param name="encoding"></param>
        ''' <remarks></remarks>
        Private Function __lazySaved(path As String, df As File, encoding As Encoding) As Boolean
            Call Console.WriteLine("Open csv file handle, and writing chunk buffer into file...")
            Call Console.WriteLine("Object counts is ""{0}""", df._innerTable.Count)

            Call "".SaveTo(path)

            Try
                Dim rowBuffer As RowObject() = df.__createTableVector
                Return __lazyInner(path, rowBuffer, encoding)
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
                Return False
            End Try
        End Function

        Private Function __lazyInner(filepath As String, rowBuffer As RowObject(), encoding As Encoding) As Boolean
            Dim stopWatch = System.Diagnostics.Stopwatch.StartNew
            Dim chunks As RowObject()() = rowBuffer.Split(10240)
            Dim handle As IO.FileStream =
                IO.File.Open(filepath,
                             IO.FileMode.OpenOrCreate,
                             IO.FileAccess.ReadWrite)

            Using writer As New StreamWriter(handle, encoding)
                Dim first As RowObject() = chunks(Scan0)

                ' 第一行会因为并行化而出现在下几行的BUG，所以在这里分开对待
                Call writer.WriteLine(first.First.AsLine)
                Call writer.WriteLine(String.Join(vbCrLf, first.Skip(1).ToArray(Function(x) x.AsLine)))

                For Each block As RowObject() In chunks.Skip(1)
                    Dim sBlock As String = (From row As RowObject
                                            In block.AsParallel
                                            Select row.AsLine).JoinBy(vbCrLf)
                    Call writer.WriteLine(sBlock)
                    Call Console.Write(".")
                Next
            End Using

            Call $"Write csv data file cost time {stopWatch.ElapsedMilliseconds} ms.".__DEBUG_ECHO

            Return True
        End Function
    End Module
End Namespace
