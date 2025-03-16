#Region "Microsoft.VisualBasic::e7571583a26e7e8cddd38404d323db3d, gr\Microsoft.VisualBasic.Imaging\PostScript\SpatialLookup.vb"

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

    '   Total Lines: 94
    '    Code Lines: 75 (79.79%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (20.21%)
    '     File Size: 3.50 KB


    '     Class SpatialHashing
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindCommentShapeByPoint, FindShapeByPoint, HashX, HashY
    ' 
    '         Sub: AddPlot, AddShape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace PostScript

    Public Class SpatialHashing

        ReadOnly gridSize As SizeF
        ReadOnly grid As New Dictionary(Of UInteger, Dictionary(Of UInteger, List(Of PSElement)))

        Public Sub New(width As Single, height As Single, Optional numGrids As Integer = 5)
            Me.gridSize = New SizeF(width / numGrids, height / numGrids)
        End Sub

        Public Sub AddPlot(ps As PostScriptBuilder)
            For Each element As PSElement In ps.AsEnumerable
                Call AddShape(element)
            Next
        End Sub

        Public Sub AddShape(shape As PSElement)
            Dim pos = shape.GetXy
            Dim size = shape.GetSize
            Dim startX As UInteger = HashX(pos.X)
            Dim endX As UInteger = HashX(pos.X + size.Width)
            Dim startY As UInteger = HashY(pos.Y)
            Dim endY As UInteger = HashY(pos.Y + size.Height)
            Dim cX = CUInt((startX + endX) / 2)
            Dim cY = CUInt((startY + endY) / 2)

            For Each xy As UInteger() In New UInteger()() {
                    New UInteger() {startX, startY}, New UInteger() {cX, startY}, New UInteger() {endX, startY},
                    New UInteger() {startX, cY}, New UInteger() {cX, cY}, New UInteger() {endX, cY},
                    New UInteger() {startX, endY}, New UInteger() {cX, endY}, New UInteger() {endX, endY}
                }

                If Not grid.ContainsKey(xy(0)) Then
                    Call grid.Add(xy(0), New Dictionary(Of UInteger, List(Of PSElement)))
                End If
                If Not grid(xy(0)).ContainsKey(xy(1)) Then
                    Call grid(xy(0)).Add(xy(1), New List(Of PSElement))
                End If

                Call grid(xy(0))(xy(1)).Add(shape)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function HashX(x As Single) As UInteger
            Return CUInt(std.Floor(x / gridSize.Width))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function HashY(y As Single) As UInteger
            Return CUInt(std.Floor(y / gridSize.Height))
        End Function

        Public Function FindCommentShapeByPoint(x As Single, y As Single) As PSElement
            Dim hX As UInteger = HashX(x)
            Dim hY As UInteger = HashY(y)

            If grid.ContainsKey(hX) Then
                Dim col = grid(key:=hX)

                If col.ContainsKey(hY) Then
                    Return col(key:=hY) _
                        .Where(Function(s)
                                   Return Not s.comment.StringEmpty(, True)
                               End Function) _
                        .LastOrDefault
                End If
            End If

            Return Nothing
        End Function

        Public Function FindShapeByPoint(x As Single, y As Single) As PSElement
            Dim hX As UInteger = HashX(x)
            Dim hY As UInteger = HashY(y)

            If grid.ContainsKey(hX) Then
                Dim col = grid(key:=hX)

                If col.ContainsKey(hY) Then
                    Return col(key:=hY).LastOrDefault
                End If
            End If

            Return Nothing
        End Function
    End Class
End Namespace
