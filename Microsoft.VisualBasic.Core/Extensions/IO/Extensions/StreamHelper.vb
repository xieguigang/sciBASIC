#Region "Microsoft.VisualBasic::0e0b7f03fe5774e8e50fcc26f1552a74, Microsoft.VisualBasic.Core\Extensions\IO\Extensions\StreamHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module StreamHelper
    ' 
    '     Function: CopyStream, PopulateBlocks
    ' 
    '     Sub: Write, WriteLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Module StreamHelper

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">
    ''' Create from <see cref="WebServiceUtils.GetRequestRaw(String, Boolean, String)"/>
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("Stream.Copy", Info:="Download stream data from the http response.")>
    <Extension> Public Function CopyStream(stream As Stream, Optional target As Stream = Nothing, Optional bufferSize% = 64 * 1024) As Stream
        If stream Is Nothing Then
            Return If(target, New MemoryStream)
        End If

        Dim buffer As Byte() = New Byte(bufferSize - 1) {}
        Dim i As VBInteger = Scan0

        With target Or DirectCast(New MemoryStream(), Stream).AsDefault
            Do While (i = stream.Read(buffer, 0, buffer.Length)) > 0
                Call .Write(buffer, 0, i)
            Loop

            Return .ByRef
        End With
    End Function

    ''' <summary>
    ''' 这个函数会重置流的指针位置
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="chunkSize%"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PopulateBlocks(buffer As Stream, Optional chunkSize% = 2048) As IEnumerable(Of Byte())
        Dim chunk As Byte() = New Byte(chunkSize - 1) {}
        Dim ends& = buffer.Length
        Dim dl&

        ' 重置读取指针位置
        Call buffer.Seek(Scan0, SeekOrigin.Begin)

        ' 分块读取buffer，然后写入流数据
        Do While buffer.Position < ends
            dl = ends - buffer.Position

            If dl > chunkSize Then
                ' buffer之中还存在充足的数据进行复制
                Call buffer.Read(chunk, Scan0, chunkSize)
            Else
                ' 数据不足了
                chunk = New Byte(dl - 1) {}
                buffer.Read(chunk, Scan0, dl)
            End If

            Yield chunk

            If dl < chunkSize Then
                Exit Do
            End If
        Loop
    End Function

    <Extension>
    Public Sub Write(stream As Stream, value$, Optional encoding As Encoding = Nothing)
        With (encoding Or UTF8).GetBytes(value)
            Call stream.Write(.ByRef, Scan0, .Length)
            Call stream.Flush()
        End With
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub WriteLine(stream As Stream, Optional value$ = "", Optional encoding As Encoding = Nothing, Optional newLine$ = vbCrLf)
        Call stream.Write(value & newLine, encoding)
    End Sub
End Module
