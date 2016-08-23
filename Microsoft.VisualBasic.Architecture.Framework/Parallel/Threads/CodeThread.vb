#Region "Microsoft.VisualBasic::de89b345581fae1e00c3cc4b12cb98e2, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Parallel\Threads\CodeThread.vb"

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

Imports System.Threading
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Threads

    Public MustInherit Class CodeThread : Inherits ClassObject

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
