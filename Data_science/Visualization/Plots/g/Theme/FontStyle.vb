#Region "Microsoft.VisualBasic::50c731ec82de28a8df54c1fc518f9ded, Data_science\Visualization\Plots\g\Theme\FontStyle.vb"

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

    '   Total Lines: 14
    '    Code Lines: 7
    ' Comment Lines: 4
    '   Blank Lines: 3
    '     File Size: 356 B


    '     Class FontStyle
    ' 
    '         Properties: color, font, layout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Graphic.Canvas

    Public Class FontStyle

        Public Property font As String
        Public Property color As String = "#000000"
        ''' <summary>
        ''' 这个是按照百分比进行定位的
        ''' </summary>
        ''' <returns></returns>
        Public Property layout As Layout

    End Class
End Namespace
