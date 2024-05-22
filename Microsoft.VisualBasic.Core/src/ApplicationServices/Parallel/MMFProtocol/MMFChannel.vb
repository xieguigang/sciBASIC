#Region "Microsoft.VisualBasic::10e0cb594557883c73921ecdd86d4765, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\MMFProtocol\MMFChannel.vb"

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
    ' Comment Lines: 46 (22.66%)
    '    - Xml Docs: 65.22%
    ' 
    '   Blank Lines: 34 (16.75%)
    '     File Size: 7.77 KB


    '     Delegate Sub
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Class MMFSocket
    ' 
    '         Properties: NewMessageCallBack, URI
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateObject, getMessage, Ping, ReadData, ReadString
    '                   SendMessage, ToString, WriteMessage
    ' 
    '         Sub: __dataArrival, (+2 Overloads) Dispose, (+3 Overloads) SendMessage
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Parallel.MMFProtocol.MapStream
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text

Namespace Parallel.MMFProtocol

    ''' <summary>
    ''' 客户端接受到的数据需要经过反序列化解码方能读取
    ''' </summary>
    ''' <param name="data"></param>
    ''' <remarks></remarks>
    Public Delegate Sub DataArrival(data As Byte())
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="message">UTF8 string</param>
    Public Delegate Sub ReadNewMessage(message As String)

    ''' <summary>
    ''' MMFProtocol socket object for the inter-process communication on the localhost, this can be using for the data exchange between two process.
    ''' </summary>
    ''' <remarks></remarks>
    <[Namespace]("MMFSocket", Description:="MMFProtocol socket object for the inter-process communication on the localhost, this can be using for the data exchange between two process.")>
    Public Class MMFSocket : Implements IDisposable

        Dim _MMFReader As MSIOReader, _MMFWriter As MSWriter
        Dim _UpdateFlag As Long
        Dim _callBacks As DataArrival

        Public Property NewMessageCallBack As ReadNewMessage

        Public ReadOnly Property URI As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <param name="chunkSize">默认的区块大小为100KB，这个对于一般的小文本传输已经足够了</param>
        Sub New(uri As String, Optional chunkSize As Long = 100 * 1024)
            _MMFWriter = New MSWriter(uri, chunkSize)
            _MMFReader = New MSIOReader(uri, AddressOf __dataArrival, chunkSize)
            _URI = uri
        End Sub

        Public Const MMF_PROTOCOL As String = "mmf://"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <param name="dataArrivals">
        ''' Public Delegate Sub <see cref="__dataArrival"/>(byteData As <see cref="System.Byte"/>())
        ''' 会优先于事件<see cref="__dataArrival"></see>的发生</param>
        ''' <remarks></remarks>
        Sub New(uri As String, dataArrivals As DataArrival)
            Call Me.New(uri)
            _callBacks = dataArrivals
        End Sub

        Public Sub SendMessage(byteData As Byte())
            Me._UpdateFlag = _MMFReader.Read.udtBadge + 1

            Dim bytData As Byte() = New MMFStream With {
                .byteData = byteData,
                .udtBadge = Me._UpdateFlag
            }.Serialize

            Call _MMFReader.Update(Me._UpdateFlag)
            Call _MMFWriter.WriteStream(bytData)
        End Sub

        ''' <summary>
        ''' 直接从映射文件之中读取数据
        ''' </summary>
        ''' <returns></returns>
        Public Function ReadData() As Byte()
            Return _MMFReader.Read.byteData
        End Function

        Public Sub SendMessage(raw As RawStream)
            Call SendMessage(raw.Serialize)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"><see cref="System.Text.Encoding.UTF8"/></param>
        Public Sub SendMessage(s As String)
            Dim buf As Byte() = System.Text.Encoding.UTF8.GetBytes(s)
            Call Me.SendMessage(buf)
        End Sub

        Public Function ReadString() As String
            Dim buf As Byte() = ReadData()
            Dim s As String = System.Text.Encoding.UTF8.GetString(buf)
            Return s
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Me._MMFWriter.Free
                    Call Me._MMFReader.Free
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Overrides Function ToString() As String
            Return $"{MMF_PROTOCOL}{_URI}"
        End Function

        Public Function Ping() As Boolean
            Call SendMessage(s:=_PING_MESSAGE)
            Call Threading.Thread.Sleep(100)

            If PingResult = True Then
                PingResult = False
                Return True
            Else
                Return False
            End If
        End Function

        Private Const _PING_MESSAGE As String = "PINT_REQUEST::{BAB0B3FA-C3F3-42F2-A577-9AF57EBDB9C7}"
        Private Const _PING_RETURNS As String = "PING_RETURNS::{BAB0B3FA-C3F3-42F2-A577-9AF57EBDB9C7}"

        Dim PingResult As Boolean = False

        Private Sub __dataArrival(data() As Byte)
            Dim s_MSG As String = System.Text.Encoding.Unicode.GetString(data)

            If String.Equals(s_MSG, _PING_MESSAGE) Then
                Call SendMessage(_PING_RETURNS)
            ElseIf String.Equals(s_MSG, _PING_RETURNS) Then
                PingResult = True
            Else
                If Not _callBacks Is Nothing Then
                    Call _callBacks(data)
                End If

                If Not NewMessageCallBack Is Nothing Then
                    Call NewMessageCallBack()(s_MSG)
                End If
            End If
        End Sub

#Region "ShellScript API"

        <ExportAPI("MMFProtocol.New")>
        Public Shared Function CreateObject(hostName As String, Optional handler As Action(Of Generic.IEnumerable(Of Byte)) = Nothing) As MMFProtocol.MMFSocket
            Dim CallBackHandle As DataArrival = New DataArrival(Sub(data As Byte()) handler(data))
            Return New MMFSocket(hostName, CallBackHandle)
        End Function

        <ExportAPI("SendMessage")>
        Public Shared Function SendMessage(socket As MMFSocket, data As Generic.IEnumerable(Of Byte)) As Boolean
            Try
                Call socket.SendMessage(data.ToArray)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        <ExportAPI("getMessage")>
        Public Shared Function getMessage(data As Generic.IEnumerable(Of Byte), Optional encoding As String = "") As String
            Return TextFileEncodingDetector.ToString(data, encoding)
        End Function

        <ExportAPI("Print.Message")>
        Public Shared Function WriteMessage(data As Generic.IEnumerable(Of Byte)) As String
            Dim Message As String = getMessage(data)
            Call VBDebugger.EchoLine(Message)
            Return Message
        End Function
#End Region
    End Class
End Namespace
