Imports Microsoft.VisualBasic.Imaging.LayoutModel

' Mike Markowski mike.ab3ap@gmail.com Apr 22, 2013
' 
' v0.1 2013-04-22.  Initial release, Apr 22, 2013
' v0.2 2013-11-12.  Bug fix.  Corrected start of subpath in firstSideCCW().
' v0.3 2021-05-12.  Bug fix.  Corrected case 11 start of subpath in
'                   firstSideCCW() to return TOP, not RIGHT.
'                   Thanks to Giovanni Martino!
' 
' Copyright 2013 Michael Markowski
' 
' This program is free software: you can redistribute it and/or modify it under
' the terms of the GNU General Public License as published by the Free Software
' Foundation, either version 3 of the License, or (at your option) any later
' version.
' 
' This program is distributed in the hope that it will be useful, but WITHOUT
' ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
' FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
' details.
' 
' You should have received a copy of the GNU General Public License along with
' this program. If not, see </>.


Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' IsoCell is used to describe properties of data cells, in particular,
    ''' indicating whether neighbors are above or below a threshold value. Other
    ''' methods also help the caller follow isolines from cell to cell in a
    ''' counter-clockwise manner. That is useful when converting the isolines to
    ''' GeneralPaths.
    ''' </summary>
    Public Class IsoCell

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Friend flipped_Renamed As Boolean
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Friend neighborInfo_Renamed As Integer
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Friend left_Renamed, right_Renamed, top_Renamed, bottom_Renamed As Double

        Public Sub New()
            flipped_Renamed = False
        End Sub

        ''' <summary>
        ''' Determine is this cell is one of tow ambiguous cases in Marching Squares
        ''' and, if so, whether or not it must be "flipped" to the other case.
        ''' </summary>
        ''' <returns> Indication whether this cell is or isn't flipped. </returns>
        Public Overridable Property flipped As Boolean
            Get
                Return flipped_Renamed
            End Get
            Set(ByVal value As Boolean)
                flipped_Renamed = value
            End Set
        End Property


        ''' <summary>
        ''' Retrieve the neighbor info of this cell. Each of its neighbors is given a
        ''' value of 1, 2, 4 or 8 depending on whether or not each is above or below
        ''' a threshold. These values are the foundation of the Marching Squares
        ''' algorithm.
        ''' 
        ''' @return
        ''' </summary>
        Public Overridable Property neighborInfo As Integer
            Get
                Return neighborInfo_Renamed
            End Get
            Set(ByVal value As Integer)
                neighborInfo_Renamed = value
            End Set
        End Property


        ''' <summary>
        ''' After Marching Squares determines what kind of crossings go through each
        ''' cell, this method can be used to save linearly interpolated values that
        ''' more closely follow data values. So rather than using cell crossing
        ''' values of, e.g., (0.5, 0), plotting is better if the data indicated, say,
        ''' (0.83, 0) should be used.
        ''' </summary>
        ''' <param name="cellSide"> which side crossing is wanted. </param>
        ''' <returns> crossing based on data and normalized to [0, 1]. </returns>
        Public Overridable Function normalizedPointCCW(ByVal cellSide As Side) As Point2D
            Select Case cellSide
                Case Side.BOTTOM
                    Return New Point2D(bottom_Renamed, 0)
                Case Side.LEFT
                    Return New Point2D(0, left_Renamed)
                Case Side.RIGHT
                    Return New Point2D(1, right_Renamed)
                Case Side.TOP
                    Return New Point2D(top_Renamed, 1)
                Case Else
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' Depending on this cell's neighbor info, which is an integer in [0, 15],
        ''' this method determines the first side that would used in a counter-
        ''' clockwise traversal of an isoline.
        ''' </summary>
        ''' <param name="prev"> previous side, used only for ambiguous cases of 5 and 10. </param>
        ''' <returns> side to start with in a CCW traversal. </returns>
        Public Overridable Function firstSideCCW(ByVal prev As Side) As Side
            Select Case neighborInfo_Renamed
                Case 1, 3, 7
                    Return Side.LEFT
                Case 2, 6, 14
                    Return Side.BOTTOM
                Case 4, 12, 13
                    Return Side.RIGHT
                Case 8, 9, 11
                    Return Side.TOP
                Case 5

                    Select Case prev
                        Case Side.LEFT
                            Return Side.RIGHT
                        Case Side.RIGHT
                            Return Side.LEFT
                        Case Else
                            Console.WriteLine(Me.[GetType]().FullName & ".firstSideCCW: case 5!")
                            Environment.Exit(1)
                    End Select
                Case 10
                    Select Case prev
                        Case Side.BOTTOM
                            Return Side.TOP
                        Case Side.TOP
                            Return Side.BOTTOM
                        Case Else
                            Console.WriteLine(Me.[GetType]().FullName & ".firstSideCCW: case 10!")
                            Environment.Exit(1)
                    End Select
                Case Else
                    Console.WriteLine(Me.[GetType]().FullName & ".firstSideCCW: default!")
                    Environment.Exit(1)
            End Select

            Return Nothing
        End Function

        ''' <summary>
        ''' Depending on this cell's neighbor info, which is an integer in [0, 15],
        ''' this method determines the second side of a cell that would used in a
        ''' counter-clockwise traversal of an isoline.
        ''' </summary>
        ''' <param name="prev"> previous side, used only for ambiguous cases of 5 and 10. </param>
        ''' <returns> side to finish with in a call during a CCW traversal. </returns>
        Public Overridable Function secondSideCCW(ByVal prev As Side) As Side
            Select Case neighborInfo_Renamed
                Case 8, 12, 14
                    Return Side.LEFT
                Case 1, 9, 13
                    Return Side.BOTTOM
                Case 2, 3, 11
                    Return Side.RIGHT
                Case 4, 6, 7
                    Return Side.TOP
                Case 5

                    Select Case prev
                        Case Side.LEFT ' Normal case 5.
                            Return If(flipped_Renamed, Side.BOTTOM, Side.TOP)
                        Case Side.RIGHT ' Normal case 5.
                            Return If(flipped_Renamed, Side.TOP, Side.BOTTOM)
                        Case Else
                            Console.WriteLine(Me.[GetType]().FullName & ".secondSideCCW: case 5!")
                            Environment.Exit(1)
                    End Select
                Case 10
                    Select Case prev
                        Case Side.BOTTOM ' Normal case 10
                            Return If(flipped_Renamed, Side.RIGHT, Side.LEFT)
                        Case Side.TOP ' Normal case 10
                            Return If(flipped_Renamed, Side.LEFT, Side.RIGHT)
                        Case Else
                            Console.WriteLine(Me.[GetType]().FullName & ".secondSideCCW: case 10!")
                            Environment.Exit(1)
                    End Select
                Case Else
                    Console.WriteLine(Me.[GetType]().FullName & ".secondSideCCW: shouldn't be here!  Neighborinfo = " & neighborInfo_Renamed)
                    Environment.Exit(1)
                    Return Side.NONE
            End Select
        End Function

        ''' <summary>
        ''' Find the next cell to use in a CCW traversal of an isoline.
        ''' </summary>
        ''' <param name="prev"> previous side, used only for ambiguous cases of 5 and 10. </param>
        ''' <returns> next cell to use in a CCW traversal. </returns>
        Public Overridable Function nextCellCCW(ByVal prev As Side) As Side
            Return secondSideCCW(prev)
        End Function

        ''' <summary>
        ''' Clear neighbor info in this cell. When building up shapes, it is possible
        ''' to have disjoint isoshapes and holes in them. An easy way to build up a
        ''' new shape from neighborInfo is to build sub-paths for one isoline at a
        ''' time. As the shape is built up, it is necessary to erase the line
        ''' afterward so that subsequent searches for isolines will not loop
        ''' infinitely.
        ''' </summary>
        ''' <param name="prev"> </param>
        Public Overridable Sub clearIso(ByVal prev As Side)
            Select Case neighborInfo_Renamed
                Case 0, 5, 10, 15
                Case Else
                    neighborInfo_Renamed = 15
            End Select
        End Sub

        ''' <returns> Get interpolated crossing on left edge of cell. </returns>
        Public Overridable Property left As Double
            Get
                Return left_Renamed
            End Get
            Set(ByVal value As Double)
                left_Renamed = value
            End Set
        End Property


        ''' <returns> Get interpolated crossing on right edge of cell. </returns>
        Public Overridable Property right As Double
            Get
                Return right_Renamed
            End Get
            Set(ByVal value As Double)
                right_Renamed = value
            End Set
        End Property


        ''' <returns> Get interpolated crossing on top edge of cell. </returns>
        Public Overridable Property top As Double
            Get
                Return top_Renamed
            End Get
            Set(ByVal value As Double)
                top_Renamed = value
            End Set
        End Property


        ''' <returns> Get interpolated crossing on bottom edge of cell. </returns>
        Public Overridable Property bottom As Double
            Get
                Return bottom_Renamed
            End Get
            Set(ByVal value As Double)
                bottom_Renamed = value
            End Set
        End Property
    End Class
End Namespace
