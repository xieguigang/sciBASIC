#Region "Microsoft.VisualBasic::0733cee567dd4ebd13f6f1c2a30935eb, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\MarchingSquares.vb"

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

    '   Total Lines: 434
    '    Code Lines: 189 (43.55%)
    ' Comment Lines: 191 (44.01%)
    '    - Xml Docs: 72.25%
    ' 
    '   Blank Lines: 54 (12.44%)
    '     File Size: 18.47 KB


    '     Class MarchingSquares
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: makeContour, mkIso, mkIsos, (+2 Overloads) ovalOfCassini, padData
    ' 
    '         Sub: interpolateCrossing, isoSubpath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports stdNum = System.Math

' MarchingSquares.java
' 
' Mike Markowski     mike.ab3ap@gmail.com
' Apr 22, 2013
' 
' v0.1 Initial release, Apr 22, 2013
' 
' Copyright 2013 Michael Markowski
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
' along with this program.  If not, see </>.

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' Algorithm taken from: http://en.wikipedia.org/wiki/Marching_squares . See
    ''' that web page for a thorough description and helpful illustrations. In short,
    ''' however, Marching Squares takes a 2d array of numbers and generates isolines
    ''' for specified values.
    ''' 
    ''' <para>The data array generally contains measurements of a natural phenomenon, so
    ''' that adjacent numbers have some relation to each other. For example, they
    ''' might be terrain elevations, RF power from a transmitter, snow fall amounts,
    ''' and so on.
    ''' 
    ''' </para>
    ''' <para>Given a threshold, a copy of the data is made where each value is changed
    ''' to 0 or 1 depending if the measurement is below or above it. The algorithm
    ''' described below is applied one time for each isoline wanted. Each isoline is
    ''' converted to Java GeneralPath instances, which are Shapes supporting holes
    ''' and disconnected regions.
    ''' 
    ''' </para>
    ''' <para>NOTE: data is first padded with a new row at top and another at bottom, as
    ''' well as a new first and last column. The new rows and columns are set to one
    ''' less than the smallest data value. This ensures that all isos will be closed
    ''' polygons. All generated GeneralPaths can then be easily filled and drawn.
    ''' 
    ''' </para>
    ''' <para>Taken from the Wikipedia page:
    ''' 
    ''' </para>
    ''' <para>Basic Algorithm
    ''' 
    ''' </para>
    ''' <para>Here are the steps of the algorithm:
    ''' 
    ''' </para>
    ''' <para>Apply a threshold to the 2D field to make a binary image containing:
    ''' 
    ''' </para>
    ''' <para>1 where the data value is above the isovalue
    ''' 0 where the data value is below the isovalue
    ''' 
    ''' </para>
    ''' <para>Every 2x2 block of pixels in the binary image forms a contouring cell, so
    ''' the whole image is represented by a grid of such cells (shown in green in the
    ''' picture below). Note that this contouring grid is one cell smaller in each
    ''' direction than the original 2D field.
    ''' 
    ''' </para>
    ''' <para>For each cell in the contouring grid:
    ''' 
    ''' </para>
    ''' <para>1. Compose the 4 bits at the corners of the cell to build a binary index:
    ''' walk around the cell in a clockwise direction appending the bit to the index,
    ''' using bitwise OR and left-shift, from most significant bit at the top left,
    ''' to least significant bit at the bottom left. The resulting 4-bit index can
    ''' have 16 possible values in the range 0-15
    ''' 
    ''' </para>
    ''' <para>2. Use the cell index to access a pre-built lookup table with 16 entries
    ''' listing the edges needed to represent the cell (shown in the lower right part
    ''' of the picture below).
    ''' 
    ''' </para>
    ''' <para>3. Apply linear interpolation between the original field data values to
    ''' find the exact position of the contour line along the edges of the cell.
    ''' </para>
    ''' </summary>
    Public Class MarchingSquares

        ''' <summary>
        ''' Coordinates within this distance of each other are considered identical.
        ''' This affects whether new segments are or are not created in an iso
        ''' shape GeneralPath, in particular whether or not to generate a call
        ''' to lineTo().
        ''' </summary>
        ReadOnly epsilon As Double = 0.0000000001
        ReadOnly dimeansion As Size

        Sub New(dimSize As Size, epsilon As Double)
            Me.dimeansion = dimSize
            Me.epsilon = epsilon
        End Sub

        ''' <summary>
        ''' Typically, mkIsos() is the only method in this class that programs will
        ''' call.  The caller supplies a 2d array of doubles representing some
        ''' set of measured data.  Additionally, a 1d array of values is passed
        ''' whose contents are thresholds corresponding to desired islines.
        ''' The method returns a 1d array of GeneralPaths representing those
        ''' isolines.  The GeneralPaths may contain disjoint polygons as well as
        ''' holes.
        ''' 
        ''' <para>Sample call:
        ''' <pre>
        ''' MarchingSquares marchingSquares = new MarchingSquares();
        ''' GenersalPath[] isolines = marchingSquares.mkIsos(data_mW, levels);
        ''' </pre>
        ''' and the isolines can then be filled or drawn as desired.
        ''' 
        ''' </para>
        ''' </summary>
        ''' <param name="data"> measured data to use for isoline generation. </param>
        ''' <param name="levels"> thresholds to use as iso levels. </param>
        ''' <returns> return an array of iso GeneralPaths. Each array element
        ''' corresponds to the same threshold in the 'levels' input array. </returns>
        Public Iterator Function mkIsos(data As Double()(), levels As Double()) As IEnumerable(Of GeneralPath)
            ' Pad data to guarantee iso GeneralPaths will be closed shapes.
            Dim dataP = padData(data, levels)
            Dim path As GeneralPath
            Dim contour As IsoCell()()

            For i As Integer = 0 To levels.Length - 1
                ' Create contour for this level using Marching Squares algorithm.
                contour = makeContour(dataP, levels(i))
                ' Convert contour to GeneralPath.
                path = mkIso(contour, dataP, levels(i))
                path.dimension = dimeansion

                Yield path
            Next
        End Function

        ''' <summary>
        ''' Create neighbor info for a single threshold. Neighbor info indicates
        ''' which of the 4 surrounding data values are above or below the threshold.
        ''' </summary>
        ''' <param name="data"> measured data to use for isoline generation. </param>
        ''' <param name="level"> threshold to use as iso levels. </param>
        ''' <returns> return an array of iso GeneralPaths. Each array element
        ''' corresponds to the same threshold in the 'levels' input array. </returns>
        Friend Function makeContour(data As Double()(), level As Double) As IsoCell()()

            ' Pad data to guarantee iso GeneralPaths will be closed shapes.
            Dim numRows = data.Length
            Dim numCols = data(0).Length

            ' Create array indicating iso cell neighbor info.
            Dim contours As IsoCell()() = RectangularArray.Matrix(Of IsoCell)(numRows - 1, numCols - 1)

            For r = 0 To numRows - 1 - 1

                For c = 0 To numCols - 1 - 1
                    ' Determine if neighbors are above or below threshold.
                    Dim ll = If(data(r)(c) > level, 0, 1)
                    Dim lr = If(data(r)(c + 1) > level, 0, 2)
                    Dim ur = If(data(r + 1)(c + 1) > level, 0, 4)
                    Dim ul = If(data(r + 1)(c) > level, 0, 8)
                    Dim nInfo = ll Or lr Or ur Or ul
                    Dim isFlipped = False
                    ' Deal with ambiguous cases.
                    If nInfo = 5 OrElse nInfo = 10 Then
                        ' Calculate average of 4 surrounding values.
                        Dim center = (data(r)(c) + data(r)(c + 1) + data(r + 1)(c + 1) + data(r + 1)(c)) / 4

                        If nInfo = 5 AndAlso center < level Then
                            isFlipped = True
                        ElseIf nInfo = 10 AndAlso center < level Then
                            isFlipped = True
                        End If
                    End If

                    ' Store neighbor info as one number.
                    contours(r)(c) = New IsoCell()
                    contours(r)(c).flipped = isFlipped
                    contours(r)(c).neighborInfo = nInfo
                Next
            Next

            Return contours
        End Function

        ''' <summary>
        ''' Build up a Shape corresponding to the isoline, and erase isoline at same
        ''' time. Erasing isoline is important because it is expected that this
        ''' method will be called repeatedly until no more isolines exist for a given
        ''' threshold.
        ''' </summary>
        ''' <param name="isoData"> info indicating which adjacent neighbors are above or
        ''' below threshold. </param>
        ''' <param name="data"> measured data. </param>
        ''' <param name="threshold"> this isoline's threshold value. </param>
        ''' <returns> GeneralPath, possibly with disjoint areas and holes,
        ''' representing isolines.  Shape is guaranteed closed and can be filled. </returns>
        Private Function mkIso(isoData As IsoCell()(), data As Double()(), threshold As Double) As GeneralPath
            Dim numRows = isoData.Length
            Dim numCols = isoData(0).Length
            Dim r, c As Integer

            For r = 0 To numRows - 1

                For c = 0 To numCols - 1
                    interpolateCrossing(isoData, data, r, c, threshold)
                Next
            Next

            Dim isoPath As New GeneralPath(threshold)

            For r = 0 To numRows - 1

                For c = 0 To numCols - 1

                    If isoData(r)(c).neighborInfo <> 0 AndAlso isoData(r)(c).neighborInfo <> 5 AndAlso isoData(r)(c).neighborInfo <> 10 AndAlso isoData(r)(c).neighborInfo <> 15 Then
                        Me.isoSubpath(isoData, r, c, isoPath)
                    End If
                Next
            Next

            Return isoPath
        End Function

        ''' <summary>
        ''' A given iso level can be made up of multiple disconnected regions and
        ''' each region can have multiple holes. Both regions and holes are captured
        ''' as individual iso subpaths. An existing GeneralPath is passed to this
        ''' method so that a new subpath, which is a collection of one moveTo and
        ''' multiple lineTo calls, can be added to it.
        ''' </summary>
        ''' <param name="isoData"> info indicating which adjacent neighbors are above or
        ''' below threshold. </param>
        ''' <param name="r"> row in isoData to start new sub-path. </param>
        ''' <param name="c"> column is isoData to start new sub-path. </param>
        ''' <param name="iso"> existing GeneralPath to which sub-path will be added. </param>
        Private Sub isoSubpath(isoData As IsoCell()(), r As Integer, c As Integer, iso As GeneralPath)

            ' Found an iso line at [r][c], so start there.
            Dim prevSide = Side.NONE
            Dim start = isoData(r)(c)
            Dim pt As Point2D = start.normalizedPointCCW(start.firstSideCCW(prevSide))
            Dim x As Double = c + pt.X
            Dim y As Double = r + pt.Y
            iso.MoveTo(x, y)
            pt = start.normalizedPointCCW(start.secondSideCCW(prevSide))
            Dim xPrev As Double = c + pt.X
            Dim yPrev As Double = r + pt.Y

            If stdNum.Abs(x - xPrev) > epsilon AndAlso stdNum.Abs(y - yPrev) > epsilon Then
                iso.LineTo(x, y)
            End If

            prevSide = start.nextCellCCW(prevSide)

            Select Case prevSide
                Case Side.BOTTOM
                    r -= 1
                Case Side.LEFT
                    c -= 1
                Case Side.RIGHT
                    c += 1
                Case Side.TOP
                    r += 1
            End Select

            start.clearIso(prevSide) ' Erase this isoline.
            Dim curCell = isoData(r)(c)

            While curCell IsNot start
                pt = curCell.normalizedPointCCW(curCell.secondSideCCW(prevSide))
                x = c + pt.X
                y = r + pt.Y

                If stdNum.Abs(x - xPrev) > epsilon AndAlso stdNum.Abs(y - yPrev) > epsilon Then
                    iso.LineTo(x, y)
                End If

                xPrev = x
                yPrev = y
                prevSide = curCell.nextCellCCW(prevSide)

                Select Case prevSide
                    Case Side.BOTTOM
                        r -= 1
                    Case Side.LEFT
                        c -= 1
                    Case Side.RIGHT
                        c += 1
                    Case Side.TOP
                        r += 1
                End Select

                curCell.clearIso(prevSide) ' Erase this isoline.
                curCell = isoData(r)(c)
            End While

            iso.ClosePath()
        End Sub

        ''' <summary>
        ''' Surround data with values less than any in the data to guarantee Marching
        ''' Squares will find complete polygons and not march off the edge of the
        ''' data area.
        ''' </summary>
        ''' <param name="data"> 2d data array to be padded </param>
        ''' <returns> array which is a copy of input padded with top/bottom rows and
        ''' left/right columns of values 1 less than smallest value in array. </returns>
        Friend Function padData(data As Double()(), levels As Double()) As Double()()
            Dim rows = data.Length
            Dim cols = data(0).Length

            ' superMin is a value guaranteed to never be included in any isoline.
            ' The idea is to surround the data with low values, forcing polygon
            ' sides to be created.
            Dim superMin = levels(0)

            For i = 1 To levels.Length - 1
                superMin = stdNum.Min(superMin, levels(i))
            Next

            superMin -= 1

            Dim padded = RectangularArray.Matrix(Of Double)(rows + 2, cols + 2)

            For i = 0 To cols + 2 - 1
                padded(0)(i) = superMin
                padded(rows + 1)(i) = superMin
            Next

            For i = 0 To rows + 2 - 1
                padded(i)(0) = superMin
                padded(i)(cols + 1) = superMin
            Next

            For i = 0 To rows - 1

                For j = 0 To cols - 1
                    padded(i + 1)(j + 1) = data(i)(j)
                Next
            Next

            Return padded
        End Function

        Public Overridable Function ovalOfCassini(x As Double, y As Double) As Double
            Return ovalOfCassini(x, y, 0.48, 0.5)
        End Function

        ''' <summary>
        ''' If desired, points of the isos can be drawn using the smooth ovals of
        ''' Cassini.
        ''' </summary>
        ''' <param name="x"> </param>
        ''' <param name="y"> </param>
        ''' <param name="a"> </param>
        ''' <param name="b">
        ''' @return </param>
        Public Overridable Function ovalOfCassini(x As Double, y As Double, a As Double, b As Double) As Double
            Return (x * x + y * y + a * a) * (x * x + y * y + a * a) - 4 * a * a * x * x - b * b * b * b
        End Function

        ''' <summary>
        ''' Once Marching Squares has determined the kinds of lines crossing a cell,
        ''' this method uses linear interpolation to make the crossings more
        ''' representative of the data.
        ''' </summary>
        ''' <param name="isoData"> array of values of 0-15 indicating contour type. </param>
        ''' <param name="data"> original data needed for linear interpolation. </param>
        ''' <param name="r"> current row index. </param>
        ''' <param name="c"> current column index. </param>
        ''' <param name="threshold"> threshold for this iso level. </param>
        Private Sub interpolateCrossing(isoData As IsoCell()(), data As Double()(), r As Integer, c As Integer, threshold As Double)
            Dim a, b As Double
            Dim cell = isoData(r)(c)
            Dim ll = data(r)(c)
            Dim lr = data(r)(c + 1)
            Dim ul = data(r + 1)(c)
            Dim ur = data(r + 1)(c + 1)

            ' Left side of iso cell.
            Select Case cell.neighborInfo
                Case 1, 3, 5, 7, 8, 10, 12, 14
                    a = ll
                    b = ul
                    cell.left = (threshold - a) / (b - a) ' frac from LL
                Case Else
            End Select

            ' Bottom side of iso cell.
            Select Case cell.neighborInfo
                Case 1, 2, 5, 6, 9, 10, 13, 14
                    a = ll
                    b = lr
                    cell.bottom = (threshold - a) / (b - a) ' frac from LL
                Case Else
            End Select

            ' Top side of iso cell.
            Select Case cell.neighborInfo
                Case 4, 5, 6, 7, 8, 9, 10, 11
                    a = ul
                    b = ur
                    cell.top = (threshold - a) / (b - a) ' frac from UL
                Case Else
            End Select

            ' Right side of iso cell.
            Select Case cell.neighborInfo
                Case 2, 3, 4, 5, 10, 11, 12, 13
                    a = lr
                    b = ur
                    cell.right = (threshold - a) / (b - a) ' frac from LR
                Case Else
            End Select
        End Sub

    End Class
End Namespace
