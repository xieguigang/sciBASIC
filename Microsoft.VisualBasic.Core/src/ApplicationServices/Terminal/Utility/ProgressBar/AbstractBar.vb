#Region "Microsoft.VisualBasic::67b50757ed4397f983761e1a926bb3bf, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\AbstractBar.vb"

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

    '   Total Lines: 18
    '    Code Lines: 10
    ' Comment Lines: 4
    '   Blank Lines: 4
    '     File Size: 474 B


    '     Class AbstractBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PrintMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.ProgressBar

    Public MustInherit Class AbstractBar

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Prints a simple message 
        ''' </summary>
        ''' <param name="msg">Message to print</param>
        Public Overridable Sub PrintMessage(msg As String)
            Call Console.WriteLine(msg)
        End Sub

        Public MustOverride Sub [Step]()
    End Class
End Namespace
