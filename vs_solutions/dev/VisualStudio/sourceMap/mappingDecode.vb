Imports System.Runtime.CompilerServices

Namespace SourceMap

    Public Module mappingDecode

        <Extension>
        Public Iterator Function decodeMappings(map As sourceMap) As IEnumerable(Of mappingLine)
            Dim lines As String() = map.mappings.Split(";"c)

            For Each line As String In lines
                If line.StringEmpty Then
                    Yield New mappingLine
                Else
                    For Each col As String In line.Split(","c)
                        Yield decodeVLQ(encode:=col)
                    Next
                End If
            Next
        End Function

        Private Function decodeVLQ(encode As String) As mappingLine
            Dim locations As Integer() = base64VLQ.getIntegers(encode).ToArray

            If locations.Length < 5 Then ReDim Preserve locations(4)

            Return New mappingLine With {
                .targetCol = locations(0),
                .fileIndex = locations(1),
                .sourceLine = locations(2),
                .sourceCol = locations(3),
                .nameIndex = locations(4)
            }
        End Function
    End Module
End Namespace