#Region "Microsoft.VisualBasic::51e86c3ac5fa0b5c906c5ec7d2076a80, Data\BinaryData\BinaryData\Extensions\ReaderProvider.vb"

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

    ' Class ReaderProvider
    ' 
    '     Properties: Length, URI
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Open
    ' 
    '     Sub: Cleanup, (+2 Overloads) Dispose, Read
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text

Public Class ReaderProvider
    Implements IDisposable

    Public ReadOnly Property URI As String

    ReadOnly __bufferedReader As BinaryDataReader
    ReadOnly __encoding As Encoding

    Public ReadOnly Property Length As Long
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return FileIO.FileSystem.GetFileInfo(URI).Length
        End Get
    End Property

    Sub New(path$, Optional encoding As Encodings = Encodings.ASCII, Optional buffered& = 1024 * 1024 * 10)
        URI = path$
        __encoding = encoding.CodePage

        If FileIO.FileSystem.GetFileInfo(path).Length <= buffered Then
            Dim byts As Byte() = FileIO.FileSystem.ReadAllBytes(path)   ' 文件数据将会被缓存
            __bufferedReader = New BinaryDataReader(New MemoryStream(byts), __encoding)
        End If
    End Sub

    ''' <summary>
    ''' 请使用<see cref="Cleanup"/>方法来释放资源
    ''' </summary>
    ''' <returns></returns>
    Public Function Open() As BinaryDataReader
        If __bufferedReader Is Nothing Then
            Dim file As New FileStream(
                URI,
                mode:=FileMode.Open,
                access:=FileAccess.Read,
                share:=FileShare.Read)
            Return New BinaryDataReader(file, __encoding)
        Else
            Return __bufferedReader
        End If
    End Function

    ''' <summary>
    ''' 使用这个清理方法来释放<see cref="Open"/>打开的指针
    ''' </summary>
    ''' <param name="reader"></param>
    Public Sub Cleanup(reader As BinaryDataReader)
        If __bufferedReader Is Nothing OrElse (Not __bufferedReader Is reader) Then
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
    Public Sub Read(run As Action(Of BinaryDataReader))
        If __bufferedReader Is Nothing Then
            Using file As New FileStream(
                URI,
                mode:=FileMode.Open,
                access:=FileAccess.Read,
                share:=FileShare.Read), reader As New BinaryDataReader(file, __encoding)

                Call run(reader)
            End Using
        Else
            SyncLock __bufferedReader
                Call run(__bufferedReader)
                Call __bufferedReader.Seek(Scan0, SeekOrigin.Begin)
            End SyncLock
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If Not __bufferedReader Is Nothing Then
                    Call __bufferedReader.Dispose()
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
