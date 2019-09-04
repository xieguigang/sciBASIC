Imports Microsoft.VisualBasic.Net.Http

Module wgetTest
    Sub Main()
        Dim wget As New wget("http://mona.fiehnlab.ucdavis.edu/rest/downloads/retrieve/9c822c48-67f4-4600-8b81-ef7491008245", "D:\Database\KEGG\organism\KEGG_Organism\ath\test.png")

        Call wget.Run()
    End Sub
End Module
