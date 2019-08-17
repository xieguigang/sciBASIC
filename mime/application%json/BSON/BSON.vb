Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace BSON

    Public Module BSONFormat

        ''' <summary>
        ''' 解析BSON
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <returns></returns>
        Public Function Load(buf As Byte()) As JsonObject
            Using decoder As New Decoder(buf)
                Return decoder.decodeDocument()
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteBuffer(obj As JsonObject, buffer As Stream)
            Call New Encoder().encodeDocument(buffer, obj)
        End Sub

        Public Function GetBuffer(obj As JsonObject) As MemoryStream
            Dim ms As New MemoryStream
            WriteBuffer(obj, buffer:=ms)
            Return ms
        End Function
    End Module
End Namespace

