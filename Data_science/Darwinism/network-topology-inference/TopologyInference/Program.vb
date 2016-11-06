Imports Microsoft.VisualBasic.Text

Module Program

    Sub Main()
        Dim network As New NetworkDemo
        Dim result = network.Solve(12000, 0, 5)

        Call result.DataFrame("#").Save("./network.csv", Encodings.ASCII)
    End Sub
End Module
