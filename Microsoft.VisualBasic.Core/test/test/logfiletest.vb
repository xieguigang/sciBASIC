Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module logfiletest


    Sub readerTest()
        Dim logs = LogReader.Parse("C:\Users\Administrator\AppData\Local\BioDeep\pipeline_calls_2024-02.txt").ToArray

        Pause()
    End Sub
End Module
