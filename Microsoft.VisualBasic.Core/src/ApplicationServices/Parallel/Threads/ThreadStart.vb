#Region "Microsoft.VisualBasic::7603bb486dc1e8a20d45570fcc156849, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\ThreadStart.vb"

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

    '   Total Lines: 25
    '    Code Lines: 15
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 741 B


    '     Class ThreadStart
    ' 
    '         Sub: execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports ParallelTask = System.Threading.Tasks.Task

Namespace Parallel.Threads

    Public MustInherit Class ThreadStart

        Public MustOverride Sub run()

        ''' <summary>
        ''' Run parallel task
        ''' </summary>
        ''' <param name="tasks"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub execute(tasks As IEnumerable(Of ThreadStart))
            Dim pool As New List(Of Task)

            For Each task As ThreadStart In tasks
                pool.Add(ParallelTask.Run(AddressOf task.run))
            Next

            Call ParallelTask.WaitAll(pool.ToArray)
        End Sub
    End Class
End Namespace
