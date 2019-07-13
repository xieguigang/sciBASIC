#Region "Microsoft.VisualBasic::9096a916051799d9c0ff6c7e69c083a9, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\MMFProtocol\Pipeline\PipeStream.vb"

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

    '     Class PipeStream
    ' 
    '         Properties: hashTable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetValue, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols

Namespace Parallel.MMFProtocol.Pipeline

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
