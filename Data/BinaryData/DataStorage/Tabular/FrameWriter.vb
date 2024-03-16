Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Serialization.JSON

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
        Dim metadata As New Schema(df)

        Call wr.Write(DirectCast(magic, Byte()))
        ' offsets for the metadata
        Call wr.Write(0&)

        For Each name As String In metadata.ordinals
            Dim v As FeatureVector = df.features(name)

            If v.isScalar Then
                Call WriteScalar(wr, v.GetScalarValue)
            End If
        Next

        Dim offset As Long = wr.Position

        Call wr.Write(metadata.GetJson)
        Call wr.Flush()
        Call wr.Seek(magic.Count, SeekOrigin.Begin)
        Call wr.Write(offset)
        Call wr.Flush()

        Return True
    End Function

    Private Sub WriteScalar(wr As BinaryDataWriter, obj As Object)

    End Sub

End Module
