Imports System
Imports Microsoft.VisualBasic.DataStorage.HDSPack

Module Program

    Sub Main(args As String())
        Using hds As New StreamPack("./test.hds")
            Dim image = "D:\GCModeller\src\runtime\sciBASIC#\etc\ch07_18.png".ReadBinary
            Dim block = hds.OpenBlock("/path/to/the/image/file/ch07-18.png")

            Call block.Write(image, Scan0, image.Length)
            Call block.Flush()
            Call block.Dispose()
        End Using
    End Sub
End Module
