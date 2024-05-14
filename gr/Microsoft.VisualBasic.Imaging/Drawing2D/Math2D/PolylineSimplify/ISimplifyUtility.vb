#Region "Microsoft.VisualBasic::cd6f4d5479b73ce251008a80dd094437, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\PolylineSimplify\ISimplifyUtility.vb"

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

    '   Total Lines: 33
    '    Code Lines: 7
    ' Comment Lines: 22
    '   Blank Lines: 4
    '     File Size: 1.40 KB


    '     Interface ISimplifyUtility
    ' 
    '         Function: Simplify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' High-performance polyline simplification library
'
' This is a port of simplify-js by Vladimir Agafonkin, Copyright (c) 2012
' https://github.com/mourner/simplify-js
' 
' The code is ported from JavaScript to C#.
' The library is created as portable and 
' is targeting multiple Microsoft plattforms.
'
' This library was ported by imshz @ http://www.shz.no
' https://github.com/imshz/simplify-net
'
' This code is provided as is by the author. For complete license please
' read the original license at https://github.com/mourner/simplify-js

Namespace Drawing2D.Math2D.PolylineSimplify

    Public Interface ISimplifyUtility

        ''' <summary>
        ''' Simplifies a list of points to a shorter list of points.
        ''' </summary>
        ''' <param name="points">Points original list of points</param>
        ''' <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        ''' <param name="highestQuality">Enable highest quality for using Douglas-Peucker, 
        ''' set false for Radial-Distance algorithm</param>
        ''' <returns>Simplified list of points</returns>
        Function Simplify(points As Point(),
                          Optional tolerance As Double = 0.3,
                          Optional highestQuality As Boolean = False) As List(Of Point)

    End Interface
End Namespace
