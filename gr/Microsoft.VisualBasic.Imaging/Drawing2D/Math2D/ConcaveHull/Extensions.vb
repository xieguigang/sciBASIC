#Region "Microsoft.VisualBasic::7aa2ae0d3ddcfe0280267d24fd404ea4, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\Extensions.vb"

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

    '     Module Extensions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D.ConcaveHull

    Public Module Extensions

        <Extension> Public Function ConcaveHull(points As IEnumerable(Of Point), Optional r# = -1) As Point()
            With New BallConcave(points)
                If r# <= 0 Then
                    r# = .RecomandedRadius
                End If
                Return .GetConcave_Ball(r).ToArray
            End With
        End Function
    End Module
End Namespace
