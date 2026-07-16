#Region "Microsoft.VisualBasic::e8ede27043d269e10b5ae0ea49f502a0, gr\network-visualization\network_layout\ForceDirected\MassHandler.vb"

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

    '   Total Lines: 36
    '    Code Lines: 22 (61.11%)
    ' Comment Lines: 8 (22.22%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 6 (16.67%)
    '     File Size: 1.12 KB


    '     Class MassHandler
    ' 
    '         Function: DeltaMass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace ForceDirected

    Public Class MassHandler

        ReadOnly maxRatio As Double = 2

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">node to move</param>
        ''' <param name="b"></param>
        ''' <param name="dx#"></param>
        ''' <param name="dy#"></param>
        ''' <returns></returns>
        Public Function DeltaMass(a As Node, b As Node, dx#, dy#) As PointF
            If a.data.mass = 0.0 AndAlso b.data.mass = 0.0 Then
                Return New PointF(dx, dy)
            ElseIf a.data.mass = 0 Then
                Return New PointF(dx, dy)
            ElseIf b.data.mass = 0 Then
                Return New PointF(0, 0)
            Else
                Dim ratio As Double = b.data.mass / a.data.mass

                If ratio > maxRatio Then
                    ratio = maxRatio
                End If

                Return New PointF(ratio * dx, ratio * dy)
            End If
        End Function
    End Class
End Namespace
