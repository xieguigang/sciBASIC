#Region "Microsoft.VisualBasic::241779621929953950df7cd48e2a92eb, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\IParallelTask.vb"

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

    '   Total Lines: 43
    '    Code Lines: 31 (72.09%)
    ' Comment Lines: 3 (6.98%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (20.93%)
    '     File Size: 1.18 KB


    '     Class IParallelTask
    ' 
    '         Properties: TaskComplete, TaskRunning
    ' 
    '         Sub: WaitForExit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading

Namespace Parallel.Tasks

    Public MustInherit Class IParallelTask

        Public ReadOnly Property TaskRunning As Boolean
            Get
                Return _RunningTask
            End Get
        End Property

        Protected _RunningTask As Boolean
        Protected _signal As New ManualResetEvent(initialState:=False)

        Dim isComplete As Boolean = False

        Public Property TaskComplete As Boolean
            Get
                Return isComplete
            End Get
            Protected Set(value As Boolean)
                isComplete = value

                If isComplete Then
                    _signal.Set()
                End If
            End Set
        End Property

        ''' <summary>
        ''' 这个函数会检查<see cref="TaskComplete"/>属性来判断任务是否执行完毕
        ''' </summary>
        Public Sub WaitForExit()
            If Not TaskComplete Then
                Call _signal.Reset()
                Call _signal.WaitOne()
            End If
        End Sub

        Protected MustOverride Sub doInvokeTask()
    End Class
End Namespace
