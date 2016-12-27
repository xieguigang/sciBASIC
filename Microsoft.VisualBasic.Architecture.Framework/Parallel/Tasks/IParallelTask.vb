#Region "Microsoft.VisualBasic::f1949dc0dd55a5a4f0a9cc72039bcdbc, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Parallel\Tasks\IParallelTask.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Parallel.Tasks

    Public MustInherit Class IParallelTask

        Public ReadOnly Property TaskComplete As Boolean
            Get
                Return _TaskComplete
            End Get
        End Property

        Public ReadOnly Property TaskRunning As Boolean
            Get
                Return _RunningTask
            End Get
        End Property

        Protected _RunningTask As Boolean
        Protected _TaskComplete As Boolean = False

        ''' <summary>
        ''' 这个函数会检查<see cref="TaskComplete"/>属性来判断任务是否执行完毕
        ''' </summary>
        Public Sub WaitForExit()
            Do While Not TaskComplete
                Call Threading.Thread.Sleep(1)
            Loop

            Call "Job DONE!".__DEBUG_ECHO
        End Sub

        Protected MustOverride Sub __invokeTask()
    End Class
End Namespace
