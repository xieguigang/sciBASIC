#Region "Microsoft.VisualBasic::340c072220e0d07afc549c18d7b76dea, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\ConsoleDevices\ProgressBar\Program.vb"

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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading

Namespace Terminal

    Public Module Program

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bar"></param>
        ''' <param name="wait">Sleep time of the thread</param>
        ''' <param name="[end]">Ends at this iteration</param>
        <Extension> Public Sub Run(bar As AbstractBar, wait As Integer, [end] As Integer)
            For cont As Integer = 0 To [end] - 1
                Call bar.[Step]()
                Call Thread.Sleep(wait)
            Next
        End Sub
    End Module
End Namespace
