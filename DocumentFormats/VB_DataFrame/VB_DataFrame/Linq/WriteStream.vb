#Region "Microsoft.VisualBasic::e02d0fa656d4a4c55d06e32cdb4c6cf2, ..\VB_DataFrame\Linq\WriteStream.vb"

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

Option Strict Off

Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream.Linq

    ''' <summary>
    ''' The stream writer for the data set, you can handling the ultra large dataset 
    ''' serialize into a csv document by using this writer stream object.
    ''' (文件写入流，这个一般是在遇到非常大的文件流的时候才需要使用)
    ''' </summary>
    Public Class WriteStream(Of T As Class)
        Implements System.IDisposable

        ReadOnly handle As String

        ''' <summary>
        ''' File system object handle for write csv row data.
        ''' </summary>
        ReadOnly _fileIO As StreamWriter
        ''' <summary>
        ''' Schema for creates row data from the inputs object.
        ''' </summary>
        ReadOnly RowWriter As RowWriter

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="Explicit">Schema parsing of the object strictly?</param>
        Sub New(path As String, Optional Explicit As Boolean = False, Optional metaBlank As String = "")
            Dim typeDef As Type = GetType(T)
            Dim Schema = SchemaProvider.CreateObject(typeDef, Explicit).CopyReadDataFromObject
            Dim parent As String = path.ParentPath

            Call FileIO.FileSystem.CreateDirectory(parent)

            RowWriter = New RowWriter(Schema, metaBlank)
            handle = FileIO.FileSystem.GetFileInfo(path).FullName

            Dim file As New FileStream(handle,
                                       FileMode.OpenOrCreate,
                                       FileAccess.ReadWrite,
                                       share:=FileShare.Read)

            _fileIO = New IO.StreamWriter(file)

            Dim title As RowObject = RowWriter.GetRowNames
            Dim sTitle As String = title.AsLine
            Call _fileIO.WriteLine(sTitle)
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
            If source.IsNullOrEmpty Then
                Return True  ' 要不然会出现空行，会造成误解的，所以要在这里提前结束
            End If

            Dim LQuery As String() = LinqAPI.Exec(Of String) <=
                From line As T
                In source.AsParallel
                Where Not line Is Nothing  ' 忽略掉空值对象，否则会生成空行
                Let CreatedRow As RowObject =
                    RowWriter.ToRow(line)
                Select CreatedRow.AsLine  ' 对象到数据的投影

            Call _fileIO.WriteLine(String.Join(vbCrLf, LQuery))

            Return True
        End Function

        Public Function Flush(obj As T) As Boolean
            If obj Is Nothing Then
                Return False
            End If

            Dim line As String = RowWriter.ToRow(obj).AsLine
            Call _fileIO.WriteLine(line)

            Return True
        End Function

        Public Sub Flush()
            Call _fileIO.Flush()
        End Sub

        ''' <summary>
        ''' 这个是配合<see cref="DataStream.ForEachBlock(Of T)(Action(Of T()), Integer)"/>方法使用的
        ''' </summary>
        ''' <typeparam name="Tsrc"></typeparam>
        ''' <param name="_ctype"></param>
        ''' <returns></returns>
        Public Function ToArray(Of Tsrc)(_ctype As Func(Of Tsrc, T())) As Action(Of Tsrc)
            Return AddressOf New __ctypeTransform(Of Tsrc) With {
                .__IO = Me,
                .__ctypeArray = _ctype
            }.WriteArray
        End Function

        ''' <summary>
        ''' 这个是配合<see cref="DataStream.ForEach(Of T)(Action(Of T))"/>方法使用的
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
            Public __ctypeArray As Func(Of Tsrc, T())
            Public __ctyper As Func(Of Tsrc, T)

            Public Sub WriteArray(source As Tsrc)
                Dim array As T() = __ctypeArray(source)
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
                    Call _fileIO.Close()
                    Call _fileIO.Dispose()    ' TODO: dispose managed state (managed objects).
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
