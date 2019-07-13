#Region "Microsoft.VisualBasic::f12559c3de0b1405a154ba528d74b244, www\Microsoft.VisualBasic.Webservices.Bing\test\DEBUG_MAIN.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module DEBUG_MAIN
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Webservices
Imports Microsoft.VisualBasic.Webservices.Bing

Module DEBUG_MAIN

    Sub Main()

        Dim trans = Translation.GetTranslation("aminoglycoside antibiotic")


        Pause()

        Dim n = Bing.SearchEngineProvider.Search("D-fructuronate: C6H9O7 kegg")
    End Sub
End Module
