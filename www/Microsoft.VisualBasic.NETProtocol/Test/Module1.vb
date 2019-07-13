#Region "Microsoft.VisualBasic::3b4902fdb6587139b49d650377bf0391, www\Microsoft.VisualBasic.NETProtocol\Test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Module1

    Sub Main()

        Call Microsoft.VisualBasic.Net.HTTP.WebSaveAs.SaveAs("https://jsplumbtoolkit.com/", "C:\Users\Admin\OneDrive\jsplumbtoolkit.com")

        Pause()

        For Each s In Microsoft.VisualBasic.Net.HTTP.WebCrawling.DownloadAllLinks("http://120.76.195.65/index.html", "x:\")
            Call s.Warning
        Next
    End Sub
End Module
