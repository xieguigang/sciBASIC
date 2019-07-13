#Region "Microsoft.VisualBasic::75d33877a42e5a7dd0874cad46098feb, Data_science\Mathematica\Math\MathApp\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: CalcImplicit
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Mathematical.Scripting.Types

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
