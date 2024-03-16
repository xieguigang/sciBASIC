Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Math.DataFrame

Public Module FrameWriter

    Public ReadOnly magic As IReadOnlyCollection(Of Byte) = Encoding.ASCII.GetBytes("dataframe")

    ''' <summary>
    ''' write dataframe object as the binary file
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteFrame(df As DataFrame, file As Stream) As Boolean
        Dim wr As New BinaryDataWriter(file) With {.ByteOrder = ByteOrder.BigEndian}

        Call wr.Write(DirectCast(magic, Byte()))

    End Function

End Module
