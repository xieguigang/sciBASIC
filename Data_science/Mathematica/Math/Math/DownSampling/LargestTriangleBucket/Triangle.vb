#Region "Microsoft.VisualBasic::37a6b874ae097dd2acc7af3f7f430127, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\Triangle.vb"

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

    '   Total Lines: 46
    '    Code Lines: 33 (71.74%)
    ' Comment Lines: 5 (10.87%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 8 (17.39%)
    '     File Size: 1.46 KB


    '     Class Triangle
    ' 
    '         Sub: (+2 Overloads) calc, updateWeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace DownSampling.LargestTriangleBucket

    ''' <summary>
    ''' Calculate a triangle's area
    ''' </summary>
    Public Class Triangle

        Private last As WeightedEvent
        Private curr As WeightedEvent
        Private [next] As WeightedEvent

        ' S=(1/2)*|x1*(y2-y3) + x2*(y3-y1) + x3*(y1-y2)|
        ' S=(1/2)*|y1*(x2-x3) + y2*(x3-x1) + y3*(x1-x2)|
        Private Sub updateWeight()
            If last Is Nothing OrElse curr Is Nothing OrElse [next] Is Nothing Then
                Return
            End If
            Dim dx1 As Double = curr.Time - last.Time
            Dim dx2 As Double = last.Time - [next].Time
            Dim dx3 As Double = [next].Time - curr.Time
            Dim y1 As Double = [next].Value
            Dim y2 As Double = curr.Value
            Dim y3 As Double = last.Value
            Dim s As Double = 0.5 * std.Abs(y1 * dx1 + y2 * dx2 + y3 * dx3)
            curr.Weight = s
        End Sub

        Public Overridable Sub calc(e As WeightedEvent)
            last = curr
            curr = [next]
            [next] = e
            updateWeight()
        End Sub

        Public Overridable Sub calc(last As WeightedEvent, curr As WeightedEvent, [next] As WeightedEvent)
            Me.last = last
            Me.curr = curr
            Me.next = [next]
            updateWeight()
        End Sub

    End Class

End Namespace
