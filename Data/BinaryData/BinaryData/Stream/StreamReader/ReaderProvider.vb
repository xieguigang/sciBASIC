#Region "Microsoft.VisualBasic::6d2aa55532f836428f5e322c0e24bb0e, Data\BinaryData\BinaryData\Stream\StreamReader\ReaderProvider.vb"

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

    '   Total Lines: 215
    '    Code Lines: 132 (61.40%)
    ' Comment Lines: 55 (25.58%)
    '    - Xml Docs: 67.27%
    ' 
    '   Blank Lines: 28 (13.02%)
    '     File Size: 7.68 KB


    ' Class ReaderProvider
    ' 
    '     Properties: Length, ReadScalar, URI
    ' 
    '     Constructor: (+4 Overloads) Sub New
    ' 
    '     Function: LoadObject, Open
    ' 
    '     Sub: Cleanup, (+2 Overloads) Dispose, Read
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Text

Public Class ReaderProvider : Implements IDisposable

    ''' <summary>
    ''' The target file path
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property URI As String

    ''' <summary>
    ''' Read a single scalar value
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property ReadScalar As Dictionary(Of TypeCode, Func(Of BinaryDataReader, Object))

    ReadOnly m_bufferedReader As BinaryDataReader
    ReadOnly m_encoding As Encoding

    Public ReadOnly Property Length As Long
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return FileIO.FileSystem.GetFileInfo(URI).Length
        End Get
    End Property

    Shared Sub New()
        Dim bindings = GetType(BinaryDataReader).GetMethods _
            .Select(Function(m)
                        Dim bind As BindAttribute = m.GetCustomAttribute(Of BindAttribute)
                        Return (bind, m)
                    End Function) _
            .Where(Function(fun) Not fun.bind Is Nothing) _
            .ToArray

        ReadScalar = New Dictionary(Of TypeCode, Func(Of BinaryDataReader, Object))

        For Each map In bindings
            Dim func As Func(Of BinaryDataReader, Object)
            Dim calls As MethodInfo = map.m
            Dim par As Object = map.bind.Par

            If map.bind.Par Is Nothing Then
                func = Function(buf) calls.Invoke(buf, {})
            Else
                func = Function(buf) calls.Invoke(buf, {par})
            End If

            ReadScalar(map.bind.Type) = func
        Next
    End Sub

    Sub New(buf As Stream, Optional encoding As Encodings = Encodings.ASCII)
        m_encoding = encoding.CodePage
        m_bufferedReader = New BinaryDataReader(buf, m_encoding)
    End Sub

    Sub New(read As BinaryDataReader)
        m_encoding = read.Encoding
        m_bufferedReader = read
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <param name="buffered"></param>
    ''' <remarks>
    ''' Create a lazy data
    ''' </remarks>
    Sub New(path$,
            Optional encoding As Encodings = Encodings.ASCII,
            Optional buffered& = 1024 * 1024 * 10)

        URI = path$
        m_encoding = encoding.CodePage

        If FileIO.FileSystem.GetFileInfo(path).Length <= buffered Then
            Dim byts As Byte() = FileIO.FileSystem.ReadAllBytes(path)   ' 文件数据将会被缓存
            m_bufferedReader = New BinaryDataReader(New MemoryStream(byts), m_encoding)
        End If
    End Sub

    ''' <summary>
    ''' 请使用<see cref="Cleanup"/>方法来释放资源
    ''' </summary>
    ''' <returns></returns>
    Public Function Open() As BinaryDataReader
        If m_bufferedReader Is Nothing Then
            Dim file As New FileStream(
                URI,
                mode:=FileMode.Open,
                access:=FileAccess.Read,
                share:=FileShare.Read)
            Return New BinaryDataReader(file, m_encoding)
        Else
            Return m_bufferedReader
        End If
    End Function

    ''' <summary>
    ''' 使用这个清理方法来释放<see cref="Open"/>打开的指针
    ''' </summary>
    ''' <param name="reader"></param>
    Public Sub Cleanup(reader As BinaryDataReader)
        If m_bufferedReader Is Nothing OrElse (Not m_bufferedReader Is reader) Then
            Call reader.Close()
            Call reader.Dispose()
        Else
            ' 这个是内存的缓存，不能够释放
        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="run">
    ''' 请不要在这里面执行<see cref="BinaryDataReader.Close()"/>或者<see cref="BinaryDataReader.Dispose()"/>
    ''' </param>
    Public Sub Read(run As Action(Of BinaryDataReader), Optional offset As Long = Scan0)
        If m_bufferedReader Is Nothing Then
            Using file As New FileStream(
                URI,
                mode:=FileMode.Open,
                access:=FileAccess.Read,
                share:=FileShare.Read), reader As New BinaryDataReader(file, m_encoding)

                Call run(reader)
            End Using
        Else
            SyncLock m_bufferedReader
                Call m_bufferedReader.Seek(offset, SeekOrigin.Begin)
                Call run(m_bufferedReader)
                Call m_bufferedReader.Seek(Scan0, SeekOrigin.Begin)
            End SyncLock
        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="offset"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' only read the rawdata with <see cref="FieldAttribute"/> layout information
    ''' </remarks>
    Public Function LoadObject(Of T As {New, Class})(Optional offset As Long = Scan0) As T
        Dim obj As New T
        Dim props As (layout As FieldAttribute, tar As PropertyInfo)() = DataFramework _
            .Schema(GetType(T), PropertyAccess.Writeable, nonIndex:=True) _
            .Values.Select(Function(i)
                               Dim bind As FieldAttribute = i.GetCustomAttribute(Of FieldAttribute)
                               Return (bind, i)
                           End Function) _
            .Where(Function(i) Not i.bind Is Nothing) _
            .OrderBy(Function(i) i.bind.Index) _
            .ToArray
        Dim task As Action(Of BinaryDataReader) =
            Sub(read As BinaryDataReader)
                Dim val As Object

                For Each bind In props
                    val = bind.layout.Read(read, bind.tar)
                    bind.tar.SetValue(obj, val)
                Next
            End Sub

        Call Read(run:=task, offset)

        Return obj
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If Not m_bufferedReader Is Nothing Then
                    Call m_bufferedReader.Dispose()
                End If
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
