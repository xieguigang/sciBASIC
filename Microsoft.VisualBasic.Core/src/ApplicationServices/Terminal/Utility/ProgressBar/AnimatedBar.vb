#Region "Microsoft.VisualBasic::7ecf9a25b3a13bff5a6779a441d97e3c, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\Utility\ProgressBar\AnimatedBar.vb"

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

    '     Class AnimatedBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: [Step]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.ProgressBar

    Public Class AnimatedBar
        Inherits AbstractBar

        Dim animation As List(Of String)
        Dim counter As Integer

        Public Sub New()
            MyBase.New()

            animation = New List(Of String)() From {"/", "-", "\", "|"}
            counter = 0
        End Sub

        ''' <summary>
        ''' prints the character found in the animation according to the current index
        ''' </summary>
        Public Overrides Sub [Step]()
            Console.Write(vbCr)
            Console.Write(animation(counter) & vbBack)

            counter += 1

            If counter = animation.Count Then
                counter = 0
            End If
        End Sub
    End Class
End Namespace
