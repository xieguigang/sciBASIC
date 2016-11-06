Module Module1

    Sub Main()
        For Each s In Microsoft.VisualBasic.Net.HTTP.WebExtensions.DownloadAllLinks("http://cn.bing.com/dict/search?q=recursive&FORM=BDVSP6&mkt=zh-cn", "x:\")
            Call s.__DEBUG_ECHO
        Next
    End Sub
End Module
