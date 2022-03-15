#Region "Microsoft.VisualBasic::394cacda8021e2c830422112d1f974ce, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Math2DHelper.vb"

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

    '   Total Lines: 49
    '    Code Lines: 34
    ' Comment Lines: 7
    '   Blank Lines: 8
    '     File Size: 1.71 KB


    '     Module Math2DHelper
    ' 
    '         Function: FillPolygon, Rotate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports stdnum = System.Math

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

                    If orderX.Any(Function(xii) stdnum.Abs(xiii - xii) <= 0.05) Then
                        background = Not background
                    ElseIf Not background Then
                        Yield New PointF(xi, line.Key)
                    End If
                Next
            Next
        End Function
    End Module
End Namespace
