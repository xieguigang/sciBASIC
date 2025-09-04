#Region "Microsoft.VisualBasic::dbc4ced7285777b322c169ed2014810c, Data_science\Mathematica\SignalProcessing\MachineVision\HoughCircles\Algorithm.vb"

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

    '   Total Lines: 261
    '    Code Lines: 206 (78.93%)
    ' Comment Lines: 9 (3.45%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 46 (17.62%)
    '     File Size: 11.15 KB


    '     Module Algorithm
    ' 
    '         Function: CircleHough, CircleVector, (+2 Overloads) CreateHoughSpace, CreateNegative, (+3 Overloads) CreateVectorVoteSpace
    '                   DetectEdge, GetGradAngle, (+2 Overloads) GetHighestVotes, GetTableRow, SquareVector
    '                   ToGrayMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace HoughCircles

    Public Module Algorithm

        Public Function CircleHough(baseImage As BitmapBuffer, Optional radius As Single? = Nothing) As EllipseShape()
            ' the circle result
            Dim pick As EllipseShape() = Nothing
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)

            If radius Is Nothing Then
                Dim houghMap = CreateHoughSpace(binarMap)
                pick = GetHighestVotes(houghMap)
            Else
                Dim houghMap = CreateHoughSpace(binarMap, radius)
                pick = GetHighestVotes(houghMap, radius)
            End If

            Return pick
        End Function

        Public Function CircleVector(baseImage As BitmapBuffer, Optional radius As Single? = Nothing) As EllipseShape()
            Dim pick As EllipseShape() = Nothing
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)

            If radius Is Nothing Then
                Dim vSpace = CreateVectorVoteSpace(binarMap, grayImg)
                pick = GetHighestVotes(vSpace, 10)
            Else
                Dim vSpace = CreateVectorVoteSpace(binarMap, radius)
                pick = GetHighestVotes(vSpace, radius, 0)
            End If

            Return pick
        End Function

        Public Function SquareVector(baseImage As BitmapBuffer) As EllipseShape()
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)
            Dim vSpace = CreateVectorVoteSpace(binarMap, grayImg, Table.Square)
            Dim pick = GetHighestVotes(vSpace, 4)

            Return pick
        End Function

        Private Function CreateVectorVoteSpace(binarMap As Boolean(,), radius As Integer) As Short(,)
            Dim height = binarMap.GetLength(0)
            Dim width = binarMap.GetLength(1)
            Dim result = New Short(height - 1, width - 1) {}
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    If binarMap(y, x) Then
                        If y + radius < height Then
                            result(y + radius, x) += 1
                        End If
                        If x + radius < width Then
                            result(y, x + radius) += 1
                        End If
                        If y - radius >= 0 Then
                            result(y - radius, x) += 1
                        End If
                        If x - radius >= 0 Then
                            result(y, x - radius) += 1
                        End If
                    End If
                Next
            Next

            Return result
        End Function

        Private Function CreateVectorVoteSpace(BinarEdgeMap As Boolean(,), grayImg As Short(,)) As Short(,,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim radius = If(binarHeight < binarWidth, binarHeight, binarWidth)

            Dim resultCube = New Short(radius - 1, binarHeight - 1, binarWidth - 1) {}
            For r = 1 To radius - 1
                For Y As Integer = 0 To binarHeight - 1
                    For X As Integer = 0 To binarWidth - 1
                        If BinarEdgeMap(Y, X) Then
                            Dim angle = GetGradAngle(grayImg, X, Y)

                            Dim x0 = CInt(r * std.Cos(angle))
                            Dim y0 = CInt(r * std.Sin(angle))

                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(r, Y + y0, X + x0) += 1
                            End If

                            x0 = CInt(r * std.Cos(angle + std.PI))
                            y0 = CInt(r * std.Sin(angle + std.PI))
                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(r, Y + y0, X + x0) += 1
                            End If

                            'if (y + r < binarHeight) { resultCube[r, y + r, x]++; };
                            'if (x + r < binarWidth) { resultCube[r, y, x + r]++; };
                            'if (y - r >= 0) { resultCube[r, y - r, x]++; };
                            'if (x - r >= 0) { resultCube[r, y, x - r]++; };
                        End If
                    Next
                Next
            Next
            Return resultCube
        End Function

        Private Function GetHighestVotes(accCube As Short(,,), Optional trashold As Integer = 254) As EllipseShape()
            Dim radius = accCube.GetLength(0)
            Dim height = accCube.GetLength(1)
            Dim width = accCube.GetLength(2)

            Dim ResultList As New List(Of EllipseShape)()
            For r As Short = 1 To radius - 1
                Dim vx, vy As Single
                Dim value As Double

                For y As Short = 0 To height - 1
                    For x As Short = 0 To width - 1
                        If accCube(r, y, x) > trashold AndAlso value < accCube(r, y, x) Then
                            vx = x
                            vy = y
                            value = accCube(r, y, x)
                        End If
                    Next
                Next

                If value <> 0 Then
                    ResultList.Add(New EllipseShape(vx, vy, r) With {.value = value})
                End If
            Next
            Return ResultList.ToArray()
        End Function

        Private Function GetHighestVotes(accMatrix As Short(,), radius As Short, Optional trashold As Integer = 254) As EllipseShape()
            Dim height = accMatrix.GetLength(0)
            Dim width = accMatrix.GetLength(1)
            Dim results As New List(Of EllipseShape)()

            For y As Short = 0 To height - 1
                For x As Short = 0 To width - 1
                    If accMatrix(y, x) > trashold Then ' && result.value < accMatrix[y, x])
                        results.Add(New EllipseShape(x, y, radius) With {.value = accMatrix(y, x)})
                    End If
                Next
            Next
            Return results.ToArray()
        End Function

        Private Function CreateHoughSpace(binarMap As Boolean(,)) As Short(,,)
            Dim houghSpace As New HoughSpace(binarMap)
            houghSpace.Run()
            Return houghSpace.getHoughSpace
        End Function

        Private Function CreateHoughSpace(BinarEdgeMap As Boolean(,), radius As Integer) As Short(,)
            Dim houghSpace As New HoughSpaceRadius(BinarEdgeMap, radius)
            houghSpace.Run()
            Return houghSpace.getHoughSpace
        End Function

        Private Function CreateNegative(img As BitmapBuffer) As Short(,)
            Dim grayMatrix = ToGrayMatrix(img)

            Dim height = grayMatrix.GetLength(0)
            Dim width = grayMatrix.GetLength(1)
            Dim result = New Short(height - 1, width - 1) {}
            For Y As Integer = 0 To height - 1
                For X As Integer = 0 To width - 1
                    result(Y, X) = CShort(255 - grayMatrix(Y, X))
                Next
            Next

            Return result
        End Function

        Private Function DetectEdge(binarImg As Short(,)) As Boolean(,)
            Dim task As New DetectEdge(binarImg)
            Call task.Run()
            Return task.getEdges
        End Function

        Private Function GetGradAngle(grayImg As Short(,), x0 As Integer, y0 As Integer) As Double
            Dim defX = grayImg(y0, x0 + 1) - grayImg(y0, x0)

            Return std.Atan2(grayImg(y0 + 1, x0) - grayImg(y0, x0), defX)
        End Function

        Private Function ToGrayMatrix(img As BitmapBuffer) As Short(,)
            Dim grayMatrix = New Short(img.Height - 1, img.Width - 1) {}
            For y As Integer = 0 To img.Height - 1
                For x As Integer = 0 To img.Width - 1
                    Dim color = img.GetPixel(x, y)
                    grayMatrix(y, x) = CShort((color.R + color.G + color.B) / 3)
                Next
            Next

            Return grayMatrix
        End Function

        Private Function CreateVectorVoteSpace(BinarEdgeMap As Boolean(,), grayImg As Short(,), table As TableRow()) As Short(,,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim factor = If(binarHeight < binarWidth, binarHeight, binarWidth)

            Dim resultCube = New Short(factor - 1, binarHeight - 1, binarWidth - 1) {}
            For f = 1 To factor - 1
                For Y As Integer = 0 To binarHeight - 1
                    For X As Integer = 0 To binarWidth - 1
                        If BinarEdgeMap(Y, X) Then
                            Dim angle = GetGradAngle(grayImg, X, Y)

                            Dim row = GetTableRow(table, angle)

                            If row Is Nothing Then
                                Continue For
                            End If

                            Dim x0 = CInt(f * row.Length * std.Cos(angle))
                            Dim y0 = CInt(f * row.Length * std.Sin(angle))

                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(f, Y + y0, X + x0) += 1
                            End If

                            x0 = CInt(f * row.Length * std.Cos(angle + std.PI))
                            y0 = CInt(f * row.Length * std.Sin(angle + std.PI))
                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(f, Y + y0, X + x0) += 1
                            End If

                            'if (y + r < binarHeight) { resultCube[r, y + r, x]++; };
                            'if (x + r < binarWidth) { resultCube[r, y, x + r]++; };
                            'if (y - r >= 0) { resultCube[r, y - r, x]++; };
                            'if (x - r >= 0) { resultCube[r, y, x - r]++; };
                        End If
                    Next
                Next
            Next
            Return resultCube
        End Function

        Private Function GetTableRow(rows As TableRow(), angle As Double) As TableRow
            For i = 0 To rows.Count - 1
                If rows(i).Angle = angle Then
                    Return rows(i)
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace
