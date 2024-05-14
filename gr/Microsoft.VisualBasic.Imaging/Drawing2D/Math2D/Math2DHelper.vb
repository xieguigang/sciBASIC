#Region "Microsoft.VisualBasic::576db843f4b21b9cfc316d847dae8812, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Math2DHelper.vb"

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

    '   Total Lines: 40
    '    Code Lines: 32
    ' Comment Lines: 1
    '   Blank Lines: 7
    '     File Size: 1.38 KB


    '     Module Math2DHelper
    ' 
    '         Function: FillPolygon
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Drawing2D.Math2D

    Public Module Math2DHelper

        <Extension>
        Public Iterator Function FillPolygon(polygon As IEnumerable(Of PointF)) As IEnumerable(Of PointF)
            ' run line scans
            For Each line In polygon.GroupBy(Function(d) d.Y)
                If line.Count = 1 Then
                    Yield line.First
                    Continue For
                End If

                Dim orderX = line.OrderBy(Function(d) d.X).Select(Function(d) d.X).ToArray
                Dim background As Boolean = False
                Dim endX As Integer = orderX.Max

                For Each xi In orderX
                    Yield New PointF(xi, line.Key)
                Next

                For xi As Integer = orderX.Min + 1 To endX
                    Dim xiii As Integer = xi

                    If orderX.Any(Function(xii) stdNum.Abs(xiii - xii) <= 0.05) Then
                        background = Not background
                    ElseIf Not background Then
                        Yield New PointF(xi, line.Key)
                    End If
                Next
            Next
        End Function
    End Module
End Namespace
