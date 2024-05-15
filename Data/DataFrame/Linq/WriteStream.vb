#Region "Microsoft.VisualBasic::713eeece2785b5349249793e911f0374, Data\DataFrame\Linq\WriteStream.vb"

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

    '   Total Lines: 265
    '    Code Lines: 156
    ' Comment Lines: 71
    '   Blank Lines: 38
    '     File Size: 9.93 KB


    '     Class WriteStream
    ' 
    '         Properties: BaseStream, IsMetaIndexed
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [Ctype], (+2 Overloads) Flush, populateLine, ToArray, ToString
    '                   TryFlushObject
    ' 
    '         Sub: (+2 Overloads) Dispose, Flush
    '         Class __ctypeTransform
    ' 
    '             Function: ToString
    ' 
    '             Sub: WriteArray, WriteObj
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Option Strict Off

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace IO.Linq

    ''' <summary>
    ''' The stream writer for the data set, you can handling the ultra large dataset 
    ''' serialize into a csv document by using this writer stream object.
    ''' (文件写入流，这个一般是在遇到非常大的文件流的时候才需要使用)
    ''' </summary>
    Public Class WriteStream(Of T As Class)
        Implements IDisposable

        ReadOnly handle$

        ''' <summary>
        ''' File system object handle for write csv row data.
        ''' </summary>
        ReadOnly _fileIO As StreamWriter

        ''' <summary>
        ''' Schema for creates row data from the inputs object.
        ''' </summary>
        ReadOnly rowWriter As RowWriter
        ReadOnly isTsv As Boolean = False

        Public ReadOnly Property BaseStream As StreamWriter
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _fileIO
            End Get
        End Property

        ''' <summary>
        ''' Has the meta field indexed?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsMetaIndexed As Boolean
            Get
                Return rowWriter.isMetaIndexed
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="strict">Schema parsing of the object strictly?</param>
        ''' <param name="metaKeys">预设的标题头部</param>
        ''' <param name="tsv">
        ''' Save the data frame in tsv format? By default is false means saved in csv format.
        ''' </param>
        Sub New(path As String,
                Optional strict As Boolean = False,
                Optional metaBlank$ = "",
                Optional metaKeys$() = Nothing,
                Optional maps As Dictionary(Of String, String) = Nothing,
                Optional layout As Dictionary(Of String, Integer) = Nothing,
                Optional tsv As Boolean = False,
                Optional encoding As Encodings = Encodings.UTF8WithoutBOM)

            Call Me.New(path.OpenWriter(encoding),
                        strict:=strict,
                        metaBlank:=metaBlank,
                        metaKeys:=metaKeys,
                        maps:=maps,
                        layout:=layout,
                        tsv:=tsv
                 )

            handle = FileIO.FileSystem.GetFileInfo(path).FullName
            isTsv = tsv
        End Sub

        Sub New(write As StreamWriter,
                Optional strict As Boolean = False,
                Optional metaBlank$ = "",
                Optional metaKeys$() = Nothing,
                Optional maps As Dictionary(Of String, String) = Nothing,
                Optional layout As Dictionary(Of String, Integer) = Nothing,
                Optional tsv As Boolean = False)

            Dim typeDef As Type = GetType(T)
            Dim Schema As SchemaProvider =
                SchemaProvider _
                .CreateObjectInternal(typeDef, strict) _
                .CopyReadDataFromObject

            _fileIO = write
            rowWriter = New RowWriter(Schema, metaBlank, layout)
            rowWriter.__cachedIndex = metaKeys
            isTsv = tsv

            Dim title As RowObject = rowWriter.GetRowNames(maps)

            If Not metaKeys.IsNullOrEmpty Then
                title = New RowObject(title.Join(metaKeys))
            End If

            Call _fileIO.WriteLine(populateLine(title))
            Call _fileIO.Flush()
        End Sub

        Public Overrides Function ToString() As String
            Return handle.ToFileURL
        End Function

        ''' <summary>
        ''' Serialize the object data source into the csv document.
        ''' (将对象的数据源写入Csv文件之中）
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function Flush(source As IEnumerable(Of T)) As Boolean
            If source Is Nothing Then
                ' 要不然会出现空行，会造成误解的，所以要在这里提前结束
                Return True
            End If

            Dim LQuery$() = LinqAPI.Exec(Of String) _
 _
                () <= From line As T
                      In source.AsParallel
                      Where Not line Is Nothing  ' 忽略掉空值对象，否则会生成空行
                      Let createdRow As RowObject = rowWriter.ToRow(line, Nothing)
                      Select populateLine(createdRow)  ' 对象到数据的投影

            If LQuery.Length = 0 Then
                Return True
            Else
                For Each line As String In LQuery
                    Call _fileIO.WriteLine(line)
                Next
            End If

            Return True
        End Function

        Private Function populateLine(row As RowObject) As String
            If isTsv Then
                Return row.TsvLine
            Else
                Return row.AsLine
            End If
        End Function

        ''' <summary>
        ''' Write a object into the table file.
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns>
        ''' false will be return if the given object is nothing, else true
        ''' </returns>
        ''' <remarks>
        ''' this method just write data line, not invoke of the <see cref="Stream.Flush()"/>
        ''' </remarks>
        Public Function Flush(obj As T) As Boolean
            If obj Is Nothing Then
                Return False
            Else
                _fileIO.WriteLine(populateLine(rowWriter.ToRow(obj, Nothing)))
            End If

            Return True
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TryFlushObject(any As Object) As Boolean
            Return Flush(DirectCast(any, T))
        End Function

        ''' <summary>
        ''' The base stream flush method is called automatically at dispose process
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Flush()
            Call _fileIO.Flush()
        End Sub

        ''' <summary>
        ''' 这个是配合<see cref="DataStream.ForEachBlock"/>方法使用的
        ''' </summary>
        ''' <typeparam name="Tsrc"></typeparam>
        ''' <param name="[ctype]"></param>
        ''' <returns></returns>
        Public Function ToArray(Of Tsrc)([ctype] As Func(Of Tsrc, IEnumerable(Of T))) As Action(Of Tsrc)
            Return AddressOf New __ctypeTransform(Of Tsrc) With {
                .__IO = Me,
                .__ctypeArray = [ctype]
            }.WriteArray
        End Function

        ''' <summary>
        ''' 这个是配合<see cref="DataStream.ForEach"/>方法使用的
        ''' </summary>
        ''' <typeparam name="Tsrc"></typeparam>
        ''' <param name="_ctype"></param>
        ''' <returns></returns>
        Public Function [Ctype](Of Tsrc)(_ctype As Func(Of Tsrc, T)) As Action(Of Tsrc)
            Return AddressOf New __ctypeTransform(Of Tsrc) With {
                .__IO = Me,
                .__ctyper = _ctype
            }.WriteObj
        End Function

        Private Class __ctypeTransform(Of Tsrc)
            Public __IO As WriteStream(Of T)
            Public __ctypeArray As Func(Of Tsrc, IEnumerable(Of T))
            Public __ctyper As Func(Of Tsrc, T)

            Public Sub WriteArray(source As Tsrc)
                Dim array As T() = __ctypeArray(source).ToArray
                Call __IO.Flush(array)
            End Sub

            Public Sub WriteObj(source As Tsrc)
                Dim obj As T = __ctyper(source)
                Call __IO.Flush(obj)
            End Sub

            Public Overrides Function ToString() As String
                Return $"{GetType(Tsrc).FullName} ->> {GetType(T).FullName}  @{__IO.ToString}"
            End Function
        End Class

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call _fileIO.Flush()
                    ' Call _fileIO.Close()
                    ' Call _fileIO.Dispose()    ' TODO: dispose managed state (managed objects).
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
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
