Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Module devToolTest

    Sub Main()
        Call loadTest()
    End Sub

    Sub loadTest()
        Dim assembly = New ProjectSpace()

        assembly.ImportFromXmlDocFile("E:\GCModeller\GCModeller\bin\Microsoft.VisualBasic.Framework_v47_dotnet_8da45dcd8060cc9a.xml")

        Pause()
    End Sub
End Module
