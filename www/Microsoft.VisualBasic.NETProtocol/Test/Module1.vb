Module Module1

    Sub Main()
        For Each s In Microsoft.VisualBasic.Net.HTTP.WebExtensions.DownloadAllLinks("http://120.76.195.65/index.html", "x:\")
            Call s.Warning
        Next
    End Sub
End Module
