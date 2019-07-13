#Region "Microsoft.VisualBasic::8e100823622f774acbd1bd5233f9a410, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\CodeThread.vb"

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

    '     Class CodeThread
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetThread
    ' 
    '         Sub: [Resume], Pause, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading

Namespace Parallel.Threads

    Public MustInherit Class CodeThread

        Protected ReadOnly thread As Thread

        Sub New()
            thread = New Thread(AddressOf __run)
        End Sub

        Protected MustOverride Sub __run()

        Public Shared Function GetThread(x As CodeThread) As Thread
            Return x.thread
        End Function

        Public Shared Sub Run(x As CodeThread)
            Call x.thread.Start()
        End Sub

        Public Shared Sub Pause(x As CodeThread)
#Disable Warning
            Call x.thread.Suspend()
#Enable Warning
        End Sub

        Public Shared Sub [Resume](x As CodeThread)
#Disable Warning
            Call x.thread.Resume()
#Enable Warning
        End Sub
    End Class
End Namespace
