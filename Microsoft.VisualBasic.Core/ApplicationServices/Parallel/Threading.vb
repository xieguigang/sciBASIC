#Region "Microsoft.VisualBasic::f9683810290dc0fe5b22bfab369b6ce2, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threading.vb"

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

    '     Module InvokesHelper
    ' 
    '         Function: Invoke
    '         Structure invokeHelper
    ' 
    '             Function: RunTask
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
                Function(task) AddressOf New invokeHelper With {
                    .task = task
                }.RunTask
            Dim invokes As Func(Of Integer)() =
                LinqAPI.Exec(Of Func(Of Integer)) <= From action As Action
                                                     In tasks
                                                     Select getTask(action)
            Return BatchTasks.BatchTask(invokes, numOfThreads).Length
        End Function

        Private Structure invokeHelper

            Dim task As Action

            Public Function RunTask() As Integer
                Call task()
                Return 0
            End Function
        End Structure
    End Module
End Namespace
