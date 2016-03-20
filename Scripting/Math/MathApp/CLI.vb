Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Types

Module CLI

    ''' <summary>
    ''' Execute not found
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function CalcImplicit(args As CommandLine.CommandLine) As Integer
        Dim expr As String = args.CLICommandArgvs
        Dim sep As SimpleExpression = ExpressionParser.TryParse(expr)
        Dim n As Double = sep.Evaluate
        Call Console.WriteLine(n)
        Return n
    End Function
End Module
