#Region "Microsoft.VisualBasic::aaf4326dc1384c782ababa9db81676bb, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Commands\Z.vb"

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

    '   Total Lines: 22
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 528 B


    '     Class Z
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Scale, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.PathHelper

    Public Class Z : Inherits Command

        Public Sub New(Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
        End Sub

        Public Overrides Sub Scale(factor As Double)

        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)

        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "z"c, "Z"c)}"
        End Function

    End Class
End Namespace
