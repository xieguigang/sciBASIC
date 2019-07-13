#Region "Microsoft.VisualBasic::cb989a65864302229727d204bec99e19, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\ProgressBar\AbstractBar.vb"

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

    '     Class AbstractBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PrintMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Terminal.ProgressBar

    Public MustInherit Class AbstractBar

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Prints a simple message 
        ''' </summary>
        ''' <param name="msg">Message to print</param>
        Public Sub PrintMessage(msg As String)
            Call Console.WriteLine(msg)
        End Sub

        Public MustOverride Sub [Step]()
    End Class
End Namespace
