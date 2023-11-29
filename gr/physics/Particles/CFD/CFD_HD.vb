Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

''' <summary>
''' *****************************************************************************
'''                                 - CFD HD -                                   *
'''                                                                              *
''' PROGRAMMER:  Jean Flaherty  04/29/17                                         *
''' CLASS:  CS102                                                                *
''' SEMESTER:  Spring, 2017                                                      *
''' INSTRUCTOR:  Dean Zeller                                                     *
'''                                                                              *
''' DESCRIPTION:                                                                 *
''' This program simulates fluid dynamics using Lattice Boltzman Methods.        *
''' The math/physics was adapted from http://physics.weber.edu/schroeder/fluids/ *
''' The program takes a long time to run because the dimentions are set to       *
''' 480x1920. Staring at the screeen is like watching paint dry so this program  *
''' will take screen shots every 250 time steps so that you can view a sort of   *
''' timelapse version after a good deal of time passes. It took me a day and a   *
''' half to have enough screenshots to make the demo video.                      *
'''                                                                              *
''' EXTERNAL FILES:                                                              *
''' - StdDraw.java                                                               *
''' - RetinaIcon.java                                                            *
''' - Simulation.java                                                            *
'''                                                                              *
''' These files must be in the workspace of the program for it to work correctly.*
'''                                                                              *
''' The drawing commands used in this program are part of the StdDraw            *
''' graphics libary. Slight modifications were made in order to make the graphic *
''' display nicely on a retina display macbook. Include RatinaIcon.java when     *
''' using this verion of StdDraw. However the original StdDraw.java should work  *
''' as well. It can be found at http://introcs.cs.princeton.edu/java/stdlib/     *
'''                                                                              *
''' CREDITS:                                                                     *
''' This program is copyright (c) 2017 Jean Flaherty.                            *
''' Adapted from: http://physics.weber.edu/schroeder/fluids/                     *
''' ******************************************************************************
''' </summary>
''' <remarks>
''' https://github.com/kobejean/cs-102-final-project-cfd
''' </remarks>
Public Class CFD_HD : Inherits Simulation

    ' *************************************************************************
    '                           - SIMULATION VARIABLES -                       *
    ' **************************************************************************

    ' Constants
    Friend velocity As Double = 0.07
    Friend viscocity As Double = 0.02

    ' Here are the arrays of densities by velocity, named by velocity directions with north up:
    Friend n0 As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nN As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nS As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nE As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nW As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nNW As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nNE As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nSW As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend nSE As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)

    ' Calculated variables
    Friend density As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend xvel As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend yvel As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)
    Friend speed2 As Double()() = RectangularArray.Matrix(Of Double)(xdim, ydim)

    ''' <summary>
    ''' Boolean array, true at sites that contain barriers:
    ''' </summary>
    Friend barrier As Boolean()() = RectangularArray.Matrix(Of Boolean)(xdim, ydim)


    ' Calculation short-cuts:
    Friend four9ths As Double = 4.0 / 9
    Friend one9th As Double = 1.0 / 9
    Friend one36th As Double = 1.0 / 36

    ''' <summary>
    ''' *************************************************************************
    '''                            - RESET SIMULATION -                          *
    ''' **************************************************************************
    ''' </summary>

    Public Overrides Sub reset()
        ' initial conditions
        For x As Integer = 0 To xdim - 1
            For y As Integer = 0 To ydim - 1
                Dim relx As Integer = xdim / 2 - x
                Dim rely As Integer = ydim / 2 - y
                Dim r = std.Sqrt(relx * relx + rely * rely)

                barrier(x)(y) = r < std.Min(xdim, ydim) * 0.2

                If barrier(x)(y) Then
                    n0(x)(y) = 0
                    nE(x)(y) = 0
                    nW(x)(y) = 0
                    nN(x)(y) = 0
                    nS(x)(y) = 0
                    nNE(x)(y) = 0
                    nNW(x)(y) = 0
                    nSE(x)(y) = 0
                    nSW(x)(y) = 0
                    xvel(x)(y) = 0
                    yvel(x)(y) = 0
                    speed2(x)(y) = 0
                Else
                    Dim v = velocity
                    n0(x)(y) = four9ths * (1 - 1.5 * v * v)
                    nE(x)(y) = one9th * (1 + 3 * v + 3 * v * v)
                    nW(x)(y) = one9th * (1 - 3 * v + 3 * v * v)
                    nN(x)(y) = one9th * (1 - 1.5 * v * v)
                    nS(x)(y) = one9th * (1 - 1.5 * v * v)
                    nNE(x)(y) = one36th * (1 + 3 * v + 3 * v * v)
                    nSE(x)(y) = one36th * (1 + 3 * v + 3 * v * v)
                    nNW(x)(y) = one36th * (1 - 3 * v + 3 * v * v)
                    nSW(x)(y) = one36th * (1 - 3 * v + 3 * v * v)
                    density(x)(y) = 1
                    xvel(x)(y) = v
                    yvel(x)(y) = 0
                    speed2(x)(y) = v * v
                End If
            Next
        Next

        m_collide = New CFD_collide(Me)
    End Sub


    ''' <summary>
    ''' *************************************************************************
    '''                          - ADVANCE SIMULATION -                          *
    ''' **************************************************************************
    ''' </summary>

    Public Overrides Sub advance()
        SyncLock Me
            collide()
            stream()
            bounce()
        End SyncLock
    End Sub

    Dim m_collide As CFD_collide

    ''' <summary>
    ''' *************************************************************************
    '''                               - COLLIDE -                                *
    ''' Collide particles within each cell.  Adapted from Wagner's D2Q9 code.    *
    ''' From: http://physics.weber.edu/schroeder/fluids/                         *
    ''' **************************************************************************
    ''' </summary>
    Friend Overridable Sub collide()
        Call m_collide.Solve()
    End Sub


    ''' <summary>
    ''' *************************************************************************
    '''                               - STREAM -                                 *
    ''' Stream particles into neighboring cells                                  *
    ''' From: http://physics.weber.edu/schroeder/fluids/                         *
    ''' **************************************************************************
    ''' </summary>
    Friend Overridable Sub stream()
        For x As Integer = 0 To xdim - 1 - 1 ' first start in NW corner...
            For Y As Integer = ydim - 1 To 1 Step -1
                nN(x)(Y) = nN(x)(Y - 1) ' move the north-moving particles
                nNW(x)(Y) = nNW(x + 1)(Y - 1) ' and the northwest-moving particles
            Next
        Next
        For x As Integer = xdim - 1 To 1 Step -1 ' now start in NE corner...
            For Y As Integer = ydim - 1 To 1 Step -1
                nE(x)(Y) = nE(x - 1)(Y) ' move the east-moving particles
                nNE(x)(Y) = nNE(x - 1)(Y - 1) ' and the northeast-moving particles
            Next
        Next
        For x As Integer = xdim - 1 To 1 Step -1 ' now start in SE corner...
            For y As Integer = 0 To ydim - 1 - 1
                nS(x)(y) = nS(x)(y + 1) ' move the south-moving particles
                nSE(x)(y) = nSE(x - 1)(y + 1) ' and the southeast-moving particles
            Next
        Next
        For x As Integer = 0 To xdim - 1 - 1 ' now start in the SW corner...
            For y As Integer = 0 To ydim - 1 - 1
                nW(x)(y) = nW(x + 1)(y) ' move the west-moving particles
                nSW(x)(y) = nSW(x + 1)(y + 1) ' and the southwest-moving particles
            Next
        Next
        ' We missed a few at the left and right edges:
        For y As Integer = 0 To ydim - 1 - 1
            nS(0)(y) = nS(0)(y + 1)
        Next
        For y As Integer = ydim - 1 To 1 Step -1
            nN(xdim - 1)(y) = nN(xdim - 1)(y - 1)
        Next
        ' Now handle left boundary as in Pullan's example code:
        ' Stream particles in from the non-existent space to the left, with the
        ' user-determined speed:
        Dim v = velocity
        For y As Integer = 0 To ydim - 1
            If Not barrier(0)(y) Then
                nE(0)(y) = one9th * (1 + 3 * v + 3 * v * v)
                nNE(0)(y) = one36th * (1 + 3 * v + 3 * v * v)
                nSE(0)(y) = one36th * (1 + 3 * v + 3 * v * v)
            End If
        Next
        ' Try the same thing at the right edge and see if it works:
        For y As Integer = 0 To ydim - 1
            If Not barrier(0)(y) Then
                nW(xdim - 1)(y) = one9th * (1 - 3 * v + 3 * v * v)
                nNW(xdim - 1)(y) = one36th * (1 - 3 * v + 3 * v * v)
                nSW(xdim - 1)(y) = one36th * (1 - 3 * v + 3 * v * v)
            End If
        Next
        ' Now handle top and bottom edges:
        For x As Integer = 0 To xdim - 1
            n0(x)(0) = four9ths * (1 - 1.5 * v * v)
            nE(x)(0) = one9th * (1 + 3 * v + 3 * v * v)
            nW(x)(0) = one9th * (1 - 3 * v + 3 * v * v)
            nN(x)(0) = one9th * (1 - 1.5 * v * v)
            nS(x)(0) = one9th * (1 - 1.5 * v * v)
            nNE(x)(0) = one36th * (1 + 3 * v + 3 * v * v)
            nSE(x)(0) = one36th * (1 + 3 * v + 3 * v * v)
            nNW(x)(0) = one36th * (1 - 3 * v + 3 * v * v)
            nSW(x)(0) = one36th * (1 - 3 * v + 3 * v * v)
            n0(x)(ydim - 1) = four9ths * (1 - 1.5 * v * v)
            nE(x)(ydim - 1) = one9th * (1 + 3 * v + 3 * v * v)
            nW(x)(ydim - 1) = one9th * (1 - 3 * v + 3 * v * v)
            nN(x)(ydim - 1) = one9th * (1 - 1.5 * v * v)
            nS(x)(ydim - 1) = one9th * (1 - 1.5 * v * v)
            nNE(x)(ydim - 1) = one36th * (1 + 3 * v + 3 * v * v)
            nSE(x)(ydim - 1) = one36th * (1 + 3 * v + 3 * v * v)
            nNW(x)(ydim - 1) = one36th * (1 - 3 * v + 3 * v * v)
            nSW(x)(ydim - 1) = one36th * (1 - 3 * v + 3 * v * v)
        Next
    End Sub


    ''' <summary>
    ''' *************************************************************************
    '''                               - BOUNCE -                                 *
    ''' Bounce particles off of barriers:                                        *
    ''' (The ifs are needed to prevent array index out of bounds errors.         *
    '''  Could handle edges separately to avoid this.)                           *
    ''' From: http://physics.weber.edu/schroeder/fluids/                         *
    ''' **************************************************************************
    ''' </summary>

    Friend Overridable Sub bounce()
        For x As Integer = 0 To xdim - 1
            For Y As Integer = 0 To ydim - 1
                If barrier(x)(Y) Then
                    If nN(x)(Y) > 0 Then
                        nS(x)(Y - 1) += nN(x)(Y)
                        nN(x)(Y) = 0
                    End If
                    If nS(x)(Y) > 0 Then
                        nN(x)(Y + 1) += nS(x)(Y)
                        nS(x)(Y) = 0
                    End If
                    If nE(x)(Y) > 0 Then
                        nW(x - 1)(Y) += nE(x)(Y)
                        nE(x)(Y) = 0
                    End If
                    If nW(x)(Y) > 0 Then
                        nE(x + 1)(Y) += nW(x)(Y)
                        nW(x)(Y) = 0
                    End If
                    If nNW(x)(Y) > 0 Then
                        nSE(x + 1)(Y - 1) += nNW(x)(Y)
                        nNW(x)(Y) = 0
                    End If
                    If nNE(x)(Y) > 0 Then
                        nSW(x - 1)(Y - 1) += nNE(x)(Y)
                        nNE(x)(Y) = 0
                    End If
                    If nSW(x)(Y) > 0 Then
                        nNE(x + 1)(Y + 1) += nSW(x)(Y)
                        nSW(x)(Y) = 0
                    End If
                    If nSE(x)(Y) > 0 Then
                        nNW(x - 1)(Y + 1) += nSE(x)(Y)
                        nSE(x)(Y) = 0
                    End If
                End If
            Next
        Next
    End Sub

    Public Palette As SolidBrush()
    Dim r = 1

    ''' <summary>
    ''' *************************************************************************
    '''                            - DRAW SIMULATION -                           *
    ''' **************************************************************************
    ''' </summary>

    Protected Overrides Sub draw(g As IGraphics)
        Dim speedRange As DoubleRange = (From seed In speed2 Select New DoubleRange(seed)).Select(Function(a) {a.Min, a.Max}).IteratesALL.Range
        Dim offset As DoubleRange = New Double() {0, Palette.Length - 1}

        For x As Integer = 0 To xdim - 1
            For Y As Integer = 0 To ydim - 1
                ' draw speed value
                ' Dim S = std.Min(CSng(std.Sqrt(speed2(x)(Y))) * 3.0F, 1.0F)
                ' Dim color As Color = New HSBColor(0.5F, 1.0F, S).ToRgb
                Dim S As Integer = speedRange.ScaleMapping(speed2(x)(Y), offset)
                Dim color As Brush = Palette(S)

                g.FillRectangle(color, New Rectangle(x, Y, r, r))
            Next
        Next
    End Sub
End Class
