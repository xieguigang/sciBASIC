#Region "Microsoft.VisualBasic::1cd618d84602bb5e02f7e78ba29bf818, Microsoft.VisualBasic.Core\test\LinqExpressiontest.vb"

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

    ' Module LinqExpressiontest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module LinqExpressiontest

    Sub Main()

        Dim any = <exec <%= From x In 100.SeqRandom Select x + 100 %>/>

        Pause()
    End Sub
End Module
