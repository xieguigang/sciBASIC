Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Landscape.Wavefront

Module objReaderTest

    Sub Main()
        Using file As StreamReader = "".OpenReader
            Dim obj As OBJ = OBJ.ReadFile(file)

            Pause()
        End Using
    End Sub
End Module
