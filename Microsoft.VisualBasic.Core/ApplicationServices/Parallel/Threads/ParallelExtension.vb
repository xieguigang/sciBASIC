#Region "Microsoft.VisualBasic::add210b8a4f87e7cf23051aaaa02d33d, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\ParallelExtension.vb"

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

    '     Module ParallelExtension
    ' 
    '         Function: AsyncTask, RunTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.MethodsExtension

Namespace Parallel

    ''' <summary>
    ''' Parallel based on the threading
    ''' </summary>
    Public Module ParallelExtension

        ''' <summary>
        ''' Start a new thread and then returns the background thread task handle.
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns></returns>
        <Extension> Public Function RunTask(start As ThreadStart) As Thread
            Dim thread As New Thread(start)
            Call thread.Start()
            Return thread
        End Function

        ' 2018-10-6
        ' 下面的这个函数会导致函数调用的时候重载失败
        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        '<Extension> Public Function RunTask(method As Action) As Thread
        '    Return New ThreadStart(AddressOf method.Method.Invoke).RunTask
        'End Function

        ''' <summary>
        ''' 运行一个后台任务
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns></returns>
        Public Function AsyncTask(start As ThreadStart) As IAsyncResult
            Return start.BeginInvoke(Nothing, Nothing)
        End Function
    End Module
End Namespace
