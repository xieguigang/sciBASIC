#Region "Microsoft.VisualBasic::c986a780527a386919ac0365df438521, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\Utility\ProgressBar\StackedBar.vb"

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

    '     Class StackedBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: StepProgress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Terminal.ProgressBar

    ''' <summary>
    ''' 两个进度条叠加在一起
    ''' </summary>
    Public Class StackedBar

        Dim bottom, top, background As ConsoleColor

        Sub New(bottomColor As ConsoleColor, topColor As ConsoleColor, background As ConsoleColor)
            Me.bottom = bottomColor
            Me.top = topColor
            Me.background = background
        End Sub

        Public Sub StepProgress(bottom%, top%)

        End Sub
    End Class
End Namespace
