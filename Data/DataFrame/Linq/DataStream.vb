#Region "Microsoft.VisualBasic::59b7af83a8065c130e811084029c686e, Data\DataFrame\Linq\DataStream.vb"

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

    '   Total Lines: 391
    '    Code Lines: 236 (60.36%)
    ' Comment Lines: 92 (23.53%)
    '    - Xml Docs: 82.61%
    ' 
    '   Blank Lines: 63 (16.11%)
    '     File Size: 15.57 KB


    '     Delegate Function
    ' 
    ' 
    '     Class SchemaReader
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module DataLinqStream
    ' 
    '         Function: AsLinq, CastObject, ForEach, OpenHandle
    ' 
    '     Class DataStream
    ' 
    '         Properties: FileName, SchemaOridinal
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: AsLinq, BufferProvider, GetOrdinal, OpenHandle
    ' 
    '         Sub: (+2 Overloads) Dispose, ForEach, ForEachBlock
    '         Structure __taskHelper
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: RunTask
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Text

Namespace IO.Linq

    ''' <summary>
    ''' 获取列在当前的数据集之中的编号
    ''' </summary>
    ''' <param name="Column"></param>
    ''' <returns></returns>
    Public Delegate Function GetOrdinal(column As String) As Integer

    Public Class SchemaReader : Inherits HeaderSchema
        Implements ISchema

        Sub New(fileName$, Optional encoding As Encoding = Nothing, Optional tsv As Boolean = False)
            Call Me.New(RowObject.TryParse(fileName.ReadFirstLine(encoding), tsv))
        End Sub

        Sub New(firstLineHeaders As RowObject)
            Call MyBase.New(firstLineHeaders)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Headers.JoinBy(", ")}]"
        End Function
    End Class

    Public Module DataLinqStream

        ''' <summary>
        ''' 所返回来的tuple对象之中的table字段，是跳过了第一行标题行的Linq迭代器
        ''' </summary>
        ''' <param name="fileName$"></param>
        ''' <param name="encoding"></param>
        ''' <param name="maps">
        ''' Change filed name mapping by:
        ''' 
        ''' ``Csv.Field -> <see cref="PropertyInfo.Name"/>``
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function OpenHandle(fileName$,
                                   Optional encoding As Encoding = Nothing,
                                   Optional tsv As Boolean = False,
                                   Optional maps As Dictionary(Of String, String) = Nothing,
                                   Optional tqdm_wrap As Boolean = True) As (schema As SchemaReader, table As IEnumerable(Of RowObject))

            Dim schema As New SchemaReader(fileName, encoding, tsv)
            Dim source As IEnumerable(Of RowObject) = fileName _
                .IterateAllLines(tqdm_wrap:=tqdm_wrap) _
                .Skip(1) _
                .Select(Function(line)
                            Return New RowObject(line, tsv)
                        End Function)

            If Not maps.IsNullOrEmpty Then
                Call schema.ChangeMapping(maps)
            End If

            Return (schema, source)
        End Function

        <Extension>
        Public Function CastObject(Of T As Class)(schema As SchemaReader, Optional silent As Boolean = False) As Func(Of RowObject, T)
            Dim provider As SchemaProvider = SchemaProvider _
                .CreateObject(Of T)(strict:=False) _
                .CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(provider)
            Dim type As Type = GetType(T)

            Call rowBuilder.IndexOf(schema)
            Call rowBuilder.SolveReadOnlyMetaConflicts(silent)

            Return Function(row As RowObject) As T
                       Dim obj As Object = Activator.CreateInstance(type)
                       Dim data As Object = rowBuilder.FillData(row, obj, "")

                       Return DirectCast(data, T)
                   End Function
        End Function

        ''' <summary>
        ''' Using linq stream method for load a very large csv/tsv file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="handle"></param>
        ''' <param name="parallel"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsLinq(Of T As Class)(handle As (schema As SchemaReader, table As IEnumerable(Of RowObject)), Optional parallel As Boolean = False) As IEnumerable(Of T)
            Dim castObject As Func(Of RowObject, T) = handle.schema.CastObject(Of T)
            Dim rows As IEnumerable(Of RowObject) = handle.table.Populate(parallel,)

            Return rows.Select(castObject)
        End Function

        <Extension>
        Public Iterator Function ForEach(Of T As Class)(handle As (schema As SchemaReader, table As IEnumerable(Of RowObject)),
                                                        apply As Action(Of T),
                                                        Optional parallel As Boolean = False) As IEnumerable(Of Exception)
            For Each obj As T In handle.AsLinq(Of T)(parallel)
                Call apply(obj)
            Next
        End Function
    End Module

    ''' <summary>
    ''' Buffered large text dataset Table reader
    ''' </summary>
    Public Class DataStream
        Implements ISchema
        Implements IDisposable

        ''' <summary>
        ''' The title row, which is the mapping source of the class property name.
        ''' </summary>
        ReadOnly _title As RowObject
        ReadOnly _file As StreamReader
        ReadOnly _schema As Dictionary(Of String, Integer)
        ReadOnly _skip As Integer = -1
        ReadOnly _tsv As Boolean = False

        ''' <summary>
        ''' The columns and their index order
        ''' </summary>
        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal
            Get
                Return _schema
            End Get
        End Property

        Public ReadOnly Property FileName As String

        Sub New()
            _schema = New Dictionary(Of String, Integer)
            _title = New RowObject
        End Sub

        Sub New(file$,
                Optional encoding As Encoding = Nothing,
                Optional bufSize% = 64 * 1024 * 1024,
                Optional trim As Boolean = False,
                Optional skip As Integer = -1,
                Optional tsv As Boolean = False)

            Dim first As String

            Me._FileName = file
            Me._file = file.OpenReader(encoding)
            Me._skip = skip
            Me._tsv = tsv

            If skip > 0 Then
                For i As Integer = 1 To skip
                    Call _file.ReadLine()
                Next
            End If

            first = _file.ReadLine

            If trim Then
                first = Strings.Trim(first)
            End If

            _title = RowObject.TryParse(first, tsv)
            _schema = _title _
                .Select(Function(colName, idx)
                            Return New With {
                                .colName = colName,
                                .ordinal = idx
                            }
                        End Function) _
                .ToDictionary(Function(x) x.colName.ToLower,
                              Function(x)
                                  Return x.ordinal
                              End Function)

            Call $"{file.ToFileURL} handle opened...".debug
        End Sub

        Public Function GetOrdinal(name As String) As Integer Implements ISchema.GetOrdinal
            name = name.ToLower

            If _schema.ContainsKey(name) Then
                Return _schema(name)
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' Providers the data buffer for the <see cref="RowObject"/>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数主要是为了处理第一行数据
        ''' 因为在构造函数部分已经读取了第一行来解析schema，所以在这里需要对第一个数据块做一些额外的处理
        ''' </remarks>
        Private Iterator Function BufferProvider() As IEnumerable(Of String)
            Call _file.BaseStream.Seek(Scan0, SeekOrigin.Begin)

            If _skip > 0 Then
                For i As Integer = 0 To _skip
                    Call _file.ReadLine()
                Next
            Else
                Call _file.ReadLine()
            End If

            Do While Not _file.EndOfStream
                Yield _file.ReadLine
            Loop
        End Function

        ''' <summary>
        ''' For each item in the source data fram, invoke a specific task
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="callback"></param>
        Public Sub ForEach(Of T As {New, Class})(callback As Action(Of T), Optional silent As Boolean = False)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)

            Call rowBuilder.IndexOf(Me)
            Call rowBuilder.SolveReadOnlyMetaConflicts(silent)

            For Each line As String In BufferProvider()
                Dim row As RowObject = RowObject.TryParse(line, _tsv)
                Dim obj As T = New T

                obj = rowBuilder.FillData(row, obj, "")

                Call callback(obj)
            Next
        End Sub

        ''' <summary>
        ''' Processing large dataset in block partitions.(以分块任务的形式来处理一个非常大的数据集)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="invoke">task of this block buffer</param>
        ''' <param name="blockSize">Lines of the data source.(行数)</param>
        ''' <remarks>
        ''' 2016.06.19  代码已经经过测试，没有数据遗漏的bug，请放心使用
        ''' </remarks>
        Public Sub ForEachBlock(Of T As {New, Class})(invoke As Action(Of T()), Optional blockSize As Integer = 10240 * 5, Optional silent As Boolean = False)
            ' 生成schema映射模型
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)

            Call rowBuilder.IndexOf(Me)
            Call rowBuilder.SolveReadOnlyMetaConflicts(silent)

            Dim chunks As IEnumerable(Of String()) = TaskPartitions.SplitIterator(BufferProvider(), blockSize)

            For Each block As String() In chunks
                Dim LQuery As RowObject() = LinqAPI.Exec(Of RowObject) _
                                                                       _
                    () <= From line As String
                          In block.AsParallel
                          Select RowObject.TryParse(line, _tsv)

                Dim values As T() = LinqAPI.Exec(Of T) <=
                                                         _
                    From row As RowObject
                    In LQuery.AsParallel
                    Let obj As Object = New T
                    Let data = rowBuilder.FillData(row, obj, "")
                    Select DirectCast(data, T)

                Call Time(AddressOf New __taskHelper(Of T)(values, invoke).RunTask)
                Call cat(".")
            Next
        End Sub

        ''' <summary>
        ''' 为了减少Lambda表达式所带来的性能损失而构建的一个任务运行帮助对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Private Structure __taskHelper(Of T)

            ''' <summary>
            ''' 赋值任务和数据源
            ''' </summary>
            ''' <param name="source"></param>
            ''' <param name="invoke"></param>
            Sub New(source As T(), invoke As Action(Of T()))
                Me.__source = source
                Me.__task = invoke
            End Sub

            Dim __task As Action(Of T())
            Dim __source As T()

            ''' <summary>
            ''' 运行当前的这个任务
            ''' </summary>
            Public Sub RunTask()
                Call __task(__source)
            End Sub
        End Structure

        ''' <summary>
        ''' Csv to LINQ
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Iterator Function AsLinq(Of T As {New, Class})(Optional parallel As Boolean = False, Optional silent As Boolean = False) As IEnumerable(Of T)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)
            Dim source As IEnumerable(Of String) = If(
                parallel,
                DirectCast(BufferProvider.AsParallel, IEnumerable(Of String)),
                DirectCast(BufferProvider(), IEnumerable(Of String))
            )

            Call rowBuilder.IndexOf(Me)
            Call rowBuilder.SolveReadOnlyMetaConflicts(silent)

            Dim LQuery As IEnumerable(Of T) =
                From line As String
                In source
                Let row As RowObject = RowObject.TryParse(line, _tsv)
                Let obj As T = New T
                Let data As Object = rowBuilder.FillData(row, obj, "")
                Select DirectCast(data, T)

            For Each obj As T In LQuery
                Yield obj
            Next

            Call Reset()
        End Function

        ''' <summary>
        ''' Open the data frame reader for the specific csv document.
        ''' </summary>
        ''' <param name="file">*.csv data file.</param>
        ''' <param name="encoding">The text encoding. default is using <see cref="Encodings.Default"/></param>
        ''' <returns></returns>
        Public Shared Function OpenHandle(file As String,
                                          Optional encoding As Encoding = Nothing,
                                          Optional bufSize As Integer = 64 * 1024 * 1024,
                                          Optional trim As Boolean = False) As DataStream

            Return New DataStream(file, encoding, bufSize,
                                  trim:=trim)
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Overloads Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call FlushMemory()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
