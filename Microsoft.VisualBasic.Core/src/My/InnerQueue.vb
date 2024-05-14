#Region "Microsoft.VisualBasic::497600900f97d7e319fec3b8ee15cd98, Microsoft.VisualBasic.Core\src\My\InnerQueue.vb"

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

    '   Total Lines: 36
    '    Code Lines: 15
    ' Comment Lines: 16
    '   Blank Lines: 5
    '     File Size: 1.11 KB


    '     Module InnerQueue
    ' 
    '         Properties: InnerThread
    ' 
    '         Sub: AddToQueue, WaitQueue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel

Namespace My

    ''' <summary>
    ''' Task action Queue for terminal QUEUE SOLVER 🙉
    ''' </summary>
    Module InnerQueue

        ''' <summary>
        ''' The internal task queue of the sciBASIC.NET framework
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property InnerThread As New ThreadQueue

        ''' <summary>
        ''' 添加终端输出的任务到任务队列之中
        ''' </summary>
        ''' <param name="task"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddToQueue(task As Action)
            Call InnerThread.AddToQueue(task)
        End Sub

        ''' <summary>
        ''' Wait for all thread queue job done.(Needed if you are using multiThreaded queue)
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WaitQueue()
            Call InnerThread.WaitQueue()
        End Sub
    End Module
End Namespace
