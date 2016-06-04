Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Threads

Namespace Parallel

    Public Module InvokesHelper

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tasks"></param>
        ''' <param name="numOfThreads">同时执行的句柄的数目</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Invoke(tasks As Action(), numOfThreads As Integer) As Integer
            Dim getTask As Func(Of Action, Func(Of Integer)) =
                Function(task) AddressOf New __invokeHelper With {
                    .__task = task
                }.Task
            Dim invokes As Func(Of Integer)() =
                LinqAPI.Exec(Of Func(Of Integer)) <= From action As Action
                                                     In tasks
                                                     Select getTask(action)
            Return BatchTasks.BatchTask(invokes, numOfThreads).Length
        End Function

        Private Structure __invokeHelper

            Dim __task As Action

            Public Function Task() As Integer
                Call __task()
                Return 0
            End Function
        End Structure
    End Module
End Namespace