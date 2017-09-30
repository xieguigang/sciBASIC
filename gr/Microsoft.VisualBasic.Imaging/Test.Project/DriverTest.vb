Imports Microsoft.VisualBasic.Imaging.Driver.CSS
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Render
Imports VB = Microsoft.VisualBasic.Language.Runtime

Module DriverTest
    Sub Main()

        Dim css As New CSSFile With {.Selectors = {New Selector With {.Selector = "#CSS", .Properties = New Dictionary(Of String, String) From {{"XXX", "123"}}}}}

        With New VB
            Dim dd As Action(Of Single, Single, String) = AddressOf testPlot

            RuntimeInvoker.RunPlot(dd, css, !A = 99, !B = 123, !CSS = "dertfff")
        End With
    End Sub

    Private Sub testPlot(A!, b!, Optional css$ = "1234")
        Console.WriteLine(A)
        Console.WriteLine(b)
        Console.WriteLine(css)
    End Sub
End Module
