Imports Microsoft.VisualBasic.Net.Mailto

Public Module emailParser

    Sub Main()
        Dim email = EmlReader.ParseEMail("C:\Users\Administrator\Downloads\aaaa.eml")

        Pause()
    End Sub
End Module
