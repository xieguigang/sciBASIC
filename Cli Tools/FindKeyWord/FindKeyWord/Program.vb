Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, AddressOf __runWindows)
    End Function

    Private Function __runWindows() As Integer
        Call Application.EnableVisualStyles()
        Call Application.SetCompatibleTextRenderingDefault(False)
        Call New FormFoundTools().ShowDialog()

        Return 0
    End Function
End Module
