#Region "a63ab568ea21725b0057c8caaae197d3, ..\Microsoft.VisualBasic.Architecture.Framework\ConsoleDevices\InnerQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Threading
Imports Microsoft.VisualBasic.Parallel

Namespace Terminal

    ''' <summary>
    ''' Task action Queue for terminal QUEUE SOLVER 🙉
    ''' </summary>
    Module InnerQueue

        Public ReadOnly Property InnerThread As New ThreadQueue

        Public Sub AddToQueue(task As Action)
            Call InnerThread.AddToQueue(task)
        End Sub

        ''' <summary>
        ''' Wait for all thread queue job done.(Needed if you are using multiThreaded queue)
        ''' </summary>
        Public Sub WaitQueue()
            Call InnerThread.WaitQueue()
        End Sub
    End Module
End Namespace
