Imports Microsoft.VisualBasic.Mathematical

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, AddressOf __calc, AddressOf CLI.CalcImplicit)
    End Function

    Private Function __calc() As Integer
        Dim s As String, Cmdl As String = String.Empty

#If DEBUG Then
        Dim sExpression As String = "1-2-3+4+5+6+7+8+9+55%6*3^2"
        Dim e As Microsoft.VisualBasic.Mathematical.Types.SimpleExpression = SimpleParser.TryParse(sExpression)

        Console.WriteLine("> {0} = {1}", sExpression, e.Evaluate)
#End If
        '(log(max(sinh(((1-2-3+4+5+6+7+8+9)-20)^0.5)+5,rnd(-10, 100)))!%5)^3!

        Do While Cmdl <> ".quit"
            Console.Write("> ")
            Cmdl = Console.ReadLine
            s = ScriptEngine.Shell(Cmdl)
            If Not String.IsNullOrEmpty(s) Then
                Console.WriteLine("  = {0}", s)
            End If
        Loop

        Return 0
    End Function
End Module
