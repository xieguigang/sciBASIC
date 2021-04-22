
Namespace PdfReader
    Public Module RC4
        Public Function Transform(ByVal key As Byte(), ByVal data As Byte()) As Byte()
            Dim s = EncryptInitalize(key)
            Dim ret = New Byte(data.Length - 1) {}
            Dim i = 0
            Dim j = 0

            For k = 0 To data.Length - 1
                i = i + 1 And 255
                j = j + s(i) And 255
                Swap(s, i, j)
                ret(k) = data(k) Xor s(s(i) + s(j) And 255)
            Next

            Return ret
        End Function

        Private Function EncryptInitalize(ByVal key As Byte()) As Byte()
            Dim s = New Byte(255) {}

            For i = 0 To s.Length - 1
                s(i) = CByte(i)
            Next

            Dim j = 0

            For i = 0 To 256 - 1
                j = j + key(i Mod key.Length) + s(i) And 255
                Swap(s, i, j)
            Next

            Return s
        End Function

        Private Sub Swap(ByVal s As Byte(), ByVal i As Integer, ByVal j As Integer)
            Dim c = s(i)
            s(i) = s(j)
            s(j) = c
        End Sub
    End Module
End Namespace
