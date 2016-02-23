Imports Microsoft.VisualBasic.Net.Protocols

Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class PipeStream : Inherits RawStream

        Public Property hashTable As Dictionary(Of String, PipeBuffer)

        Sub New(raw As Byte())

        End Sub

        Public Overrides Function Serialize() As Byte()
            Throw New NotImplementedException
        End Function

        Public Shared Function GetValue(raw As Byte(), name As String) As PipeBuffer
            Dim i As Long = Scan0

            Do While True
                Dim buffer As Byte() = raw
            Loop

            Return Nothing
        End Function
    End Class
End Namespace