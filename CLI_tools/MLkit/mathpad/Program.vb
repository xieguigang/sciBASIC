Imports Microsoft.VisualBasic.Math.Scripting

Module Program

    Sub Main()
        Call runMathpad()
    End Sub

    Private Function runMathpad() As Integer
        Dim ans As String
        Dim cmdl As String = String.Empty

        '(log(max(sinh(((1-2-3+4+5+6+7+8+9)-20)^0.5)+5,rnd(-10, 100)))!%5)^3!

        Do While cmdl <> ".quit"
            Console.Write("# ")

            cmdl = Console.ReadLine
            ans = ScriptEngine.Shell(cmdl)

            If Not String.IsNullOrEmpty(ans) Then
                Console.WriteLine("  = {0}", ans)
            End If
        Loop

        Return 0
    End Function
End Module
