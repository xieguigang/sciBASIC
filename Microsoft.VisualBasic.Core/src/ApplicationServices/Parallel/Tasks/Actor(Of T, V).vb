#Region "Microsoft.VisualBasic::afb02982ac0178b827bb2e0d041591f0, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\Actor(Of T, V).vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 85
    '    Code Lines: 36 (42.35%)
    ' Comment Lines: 35 (41.18%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 14 (16.47%)
    '     File Size: 3.43 KB


    '     Class Actor
    ' 
    ' 
    '         Delegate Sub
    ' 
    '             Sub: Perform
    '         Class State
    ' 
    '             Properties: [error], parameter, result
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Run
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading

Namespace Parallel.Tasks

    ''' <summary>
    ''' This Actor class can be used to call a function which has one 
    ''' parameter, object (T) and returns the result, object (V). The 
    ''' result is returned in a delegate.
    ''' </summary>
    ''' <typeparam name="T">Input type</typeparam>
    ''' <typeparam name="V">Output type</typeparam>
    Public Class Actor(Of T, V)

        Public Delegate Sub WhenComplete(sender As Object, e As State)

        Public Overridable Sub Perform(job As Func(Of T, V), parameter As T, done As WhenComplete)
            Dim task = Sub(x)
                           Dim state As State = TryCast(x, State)
                           Call state.Run()
                           ' If the calling application neglected to provide a WhenComplete delegate
                           ' check if null before attempting to invoke.
                           Call done?.Invoke(Me, state)
                       End Sub

            ' ThreadPool.QueueUserWorkItem takes an object which represents the data
            ' to be used by the queued method in WaitCallback.  I'm using an anonymous 
            ' delegate as the method in WaitCallback, and passing the variable state 
            ' as the data to use.  When a thread becomes available the method will execute.
            ThreadPool.QueueUserWorkItem(task, New State(job, parameter))
        End Sub

        ' Waitcallback requires an object, lets create one.
        Public Class State

            ''' <summary>
            ''' This is the parameter which is passed to the function
            ''' defined as job.
            ''' </summary>
            Public Property parameter() As T

            ''' <summary>
            ''' This will be the response and will be sent back to the 
            ''' calling thread using the delegate (a).
            ''' </summary>
            Public Property result() As V

            ''' <summary>
            ''' Actual method to run.
            ''' </summary>
            Private job As Func(Of T, V)

            ''' <summary>
            ''' Capture any errors and send back to the calling thread.
            ''' </summary>
            Public Property [error]() As Exception

            Public Sub New(j As Func(Of T, V), param As T)
                job = j
                parameter = param
            End Sub

            Const NullException$ = "A value passed to execute is null. Check the response to determine the value."

            ''' <summary>
            '''  Set as an internal types void so only the Actor class can  
            '''  invoke this method.
            ''' </summary>
            Friend Sub Run()
                Try
                    ' I think I should check if the method or parameter is null, and react 
                    ' accordingly.  I can check both values at once and throw a null 
                    ' reference exception.
                    If job Is Nothing Or parameter Is Nothing Then
                        Throw New NullReferenceException(State.NullException)
                    End If

                    result = job(parameter)
                Catch e As Exception
                    [error] = e
                    result = Nothing
                End Try
            End Sub
        End Class
    End Class
End Namespace
