#Region "Microsoft.VisualBasic::ed83e7b7feb6cc4ee61e7cc071105a28, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\ProgressBar\Program.vb"

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

    '     Module Program
    ' 
    '         Sub: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Terminal.ProgressBar

    Public Module Program

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bar"></param>
        ''' <param name="wait">Sleep time of the thread</param>
        ''' <param name="[end]">Ends at this iteration</param>
        <Extension> Public Sub Run(bar As AbstractBar, wait%, end%)
            For cont As Integer = 0 To [end] - 1
                Call bar.[Step]()
                Call Thread.Sleep(wait)
            Next
        End Sub
    End Module
End Namespace
