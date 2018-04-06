Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language

Public Module StreamHelper

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">
    ''' Create from <see cref="WebServiceUtils.GetRequestRaw(String, Boolean, String)"/>
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("Stream.Copy", Info:="Download stream data from the http response.")>
    <Extension> Public Function CopyStream(stream As Stream) As MemoryStream
        If stream Is Nothing Then
            Return New MemoryStream
        End If

        Dim buffer As Byte() = New Byte(64 * 1024) {}
        Dim i As New Value(Of Integer)

        With New MemoryStream()
            Do While i = stream.Read(buffer, 0, buffer.Length) > 0
                Call .Write(buffer, 0, i)
            Loop

            Return .ByRef
        End With
    End Function
End Module
