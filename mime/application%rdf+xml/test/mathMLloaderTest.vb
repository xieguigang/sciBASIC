Imports Microsoft.VisualBasic.MIME.application.rdf_xml

Module mathMLloaderTest

    Sub Main()
        Dim simple = "E:\GCModeller\src\runtime\sciBASIC#\mime\etc\kinetics1.xml".LoadXml(Of Math)

        Pause()
    End Sub
End Module
