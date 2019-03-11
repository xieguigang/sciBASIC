#Region "Microsoft.VisualBasic::e1e6a6f04a689ee9f6207a88f9a6ed52, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\CodeThread.vb"

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
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Threads

    Public MustInherit Class CodeThread : Inherits BaseClass

        Protected ReadOnly __thread As Thread

        Sub New()
            __thread = New Thread(AddressOf __run)
        End Sub

        Protected MustOverride Sub __run()

        Public Shared Function GetThread(x As CodeThread) As Thread
            Return x.__thread
        End Function

        Public Shared Sub Run(x As CodeThread)
            Call x.__thread.Start()
        End Sub

        Public Shared Sub Pause(x As CodeThread)
#Disable Warning
            Call x.__thread.Suspend()
#Enable Warning
        End Sub

        Public Shared Sub [Resume](x As CodeThread)
#Disable Warning
            Call x.__thread.Resume()
#Enable Warning
        End Sub
    End Class
End Namespace
