#Region "Microsoft.VisualBasic::b20d146fd0c8ab9b886d43673607cf34, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Math2DHelper.vb"

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

    '     Module Math2DHelper
    ' 
    '         Function: Rotate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D

    Public Module Math2DHelper

        ''' <summary>
        ''' 将目标多边型旋转指定的角度
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <param name="angle#">角度的单位在这里单位为度，不是弧度单位</param>
        ''' <returns></returns>
        <Extension>
        Public Function Rotate(polygon As IEnumerable(Of PointF), angle#) As PointF()
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
