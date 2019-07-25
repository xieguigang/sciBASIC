#Region "Microsoft.VisualBasic::8e100823622f774acbd1bd5233f9a410, ApplicationServices\Parallel\Threads\CodeThread.vb"

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
            thread = New Thread(AddressOf run)
        End Sub

        Protected MustOverride Sub run()

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
