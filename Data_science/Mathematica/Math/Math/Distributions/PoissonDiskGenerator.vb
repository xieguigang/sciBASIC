#Region "Microsoft.VisualBasic::ba670695507b49008a88af71f5bcb517, Data_science\Mathematica\Math\Math\Distributions\PoissonDiskGenerator.vb"

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

    '   Total Lines: 310
    '    Code Lines: 183 (59.03%)
    ' Comment Lines: 72 (23.23%)
    '    - Xml Docs: 63.89%
    ' 
    '   Blank Lines: 55 (17.74%)
    '     File Size: 12.11 KB


    '     Class PoissonDiskGenerator
    ' 
    '         Properties: ResultSet, sampleCount
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _PositionToGridIndex, _WrapIndex, _WrapRepeatFloat, Generate, IsInputsValid
    ' 
    '         Sub: Dispose, getDartR, PullSamples, Sampling
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math
Imports Microsoft.VisualBasic.Imaging

Namespace Distributions

    ''' <summary>
    ''' cf paper: Fast Poisson Disk Sampling in Arbitrary Dimensions. Robert Bridson. ACM SIGGRAPH 2007
    ''' 
    ''' How to use:
    ''' 
    ''' 1. set parameters. ( minDist / k / sampleRange )
    ''' 2. call Generate(). It will return the list contains sample points.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/HexStark/PoissonDiskGeneratorForUnity/tree/master
    ''' </remarks>
    Public NotInheritable Class PoissonDiskGenerator

        ' min distance between each two samples.

        ''' <summary>
        ''' the minimumx distance between any of the two samples.
        ''' </summary>
        Public minDist As Single = 5.0F
        ''' <summary>
        ''' the time of throw darts. Higher k generate better result but slower.
        ''' </summary>
        Public k As Integer = 30
        ''' <summary>
        ''' the range of generated samples. From 0[inclusive] to sampleRange[inclusive]
        ''' </summary>
        Public sampleRange As Single = 256.0F

        Public ReadOnly Property sampleCount As Integer
            Get
                Return m_resultSet.Count
            End Get
        End Property

        Public ReadOnly Property ResultSet As List(Of Vector2D)
            Get
                Return m_resultSet
            End Get
        End Property

        ''' <summary>
        ''' result of samples
        ''' </summary>
        Dim m_resultSet As List(Of Vector2D)

        ''' <summary>
        ''' grid for save sample locations.
        ''' </summary>
        Private grid As Boolean(,)
        Private gridvalueX, gridvalueY As Single(,)
        Private m_CeiledSampleRange As Single
        Private gridCellSize As Single = 0.0F
        Private gridLength As Integer = 0

        Dim activePointCount As Integer = 0
        Dim activePointListX As Single()
        Dim activePointListY As Single()

        Dim grayscale As Bitmap

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Determines if inputs are appropriate.
        ''' </summary>
        ''' <returns><c>true</c> if is inputs valid; otherwise, <c>false</c>.</returns>
        Private Shared Function IsInputsValid(minDist As Single, sampleRange As Single, k As Integer) As Boolean
            Return minDist > 0.0F AndAlso k > 0 AndAlso sampleRange > minDist
        End Function

        ''' <summary>
        ''' Generate samples. Based on minDist / k / sampleRange.
        ''' </summary>
        Public Shared Function Generate(Optional minDist As Single = 5.0F,
                                        Optional sampleRange As Single = 256.0F,
                                        Optional k As Integer = 30,
                                        Optional dart As Bitmap = Nothing) As List(Of Vector2D)

            If Not IsInputsValid(minDist, sampleRange, k) Then
                ' TODO: handle error.
                Return Nothing
            Else
                Dim poisson As New PoissonDiskGenerator With {
                    .minDist = minDist,
                    .sampleRange = sampleRange,
                    .k = k
                }

                If Not dart Is Nothing Then
                    ' make the input dart image grayscale
                    ' and resize to size [sampleRange,sampleRange]
                    Dim grayscale As Bitmap

                    dart = dart.Grayscale
#If NET48 Then
                    grayscale = New Bitmap(CInt(sampleRange), CInt(sampleRange))

                    Using g As Graphics = Graphics.FromImage(grayscale)
                        Call g.DrawImage(dart, 0, 0, grayscale.Width, grayscale.Height)
                    End Using
#Else
                    grayscale = dart.Resize(sampleRange, sampleRange)
#End If
                    poisson.grayscale = grayscale
                End If

                Call poisson.Sampling()
                Call poisson.PullSamples()
                Call poisson.Dispose()

                Return poisson.m_resultSet
            End If
        End Function

        Private Sub PullSamples()
            If m_resultSet IsNot Nothing Then
                m_resultSet.Clear()
            Else
                m_resultSet = New List(Of Vector2D)()
            End If

            Dim i As Integer = 0

            While i <= activePointCount
                If activePointListX(i) <= sampleRange AndAlso activePointListY(i) <= sampleRange Then
                    m_resultSet.Add(New Vector2D(activePointListX(i), activePointListY(i)))
                End If

                i += 1
            End While
        End Sub

        Const Pi2 As Double = std.PI + std.PI

        Private Sub getDartR(ByRef dartX As Single, ByRef dartY As Single, ByRef proc As Integer)
            Dim dartRadians = 0.0F
            Dim dartDist = 0.0F

            If grayscale Is Nothing Then
                ' randomly chose a dart in the ring area.
                dartRadians = rand.NextDouble(0, Pi2)
            Else
                ' get radians from pixel grayscale value
                dartX = activePointListX(proc) - 1
                dartY = activePointListY(proc) - 1

                If dartX < 1 Then
                    dartX = 1
                ElseIf dartX >= sampleRange - 1 Then
                    dartX = sampleRange - 1
                End If
                If dartY < 1 Then
                    dartY = 1
                ElseIf dartY >= sampleRange - 1 Then
                    dartY = sampleRange - 1
                End If

                dartRadians = grayscale.GetPixel(dartX, dartY).GrayScale / 255
                dartRadians = (dartRadians) * rand.NextDouble(0, Pi2)
            End If

            ' range from minDist to 2*minDist ( r to 2r in cf paper )
            dartDist = rand.NextDouble(minDist, 2.0F * minDist)
            dartX = activePointListX(proc) + dartDist * std.Cos(dartRadians)
            dartY = activePointListY(proc) + dartDist * std.Sin(dartRadians)
        End Sub

        Private Sub Sampling()
            ' Init.
            gridCellSize = minDist / std.Sqrt(2.0F)

            ' Create grid.
            gridLength = std.Ceiling(sampleRange / gridCellSize)
            m_CeiledSampleRange = gridLength * gridCellSize
            grid = New Boolean(gridLength - 1, gridLength - 1) {}
            gridvalueX = New Single(gridLength - 1, gridLength - 1) {}
            gridvalueY = New Single(gridLength - 1, gridLength - 1) {}

            ' Create processing list.
            activePointListX = New Single(gridLength * gridLength - 1) {}   ' x 
            activePointListY = New Single(gridLength * gridLength - 1) {}   ' y

            ' randomly add first point
            activePointListX(0) = rand.NextDouble(0, m_CeiledSampleRange)
            activePointListY(0) = rand.NextDouble(0.0F, m_CeiledSampleRange)

            grid(_PositionToGridIndex(activePointListX(0)), _PositionToGridIndex(activePointListY(0))) = True
            gridvalueX(_PositionToGridIndex(activePointListX(0)), _PositionToGridIndex(activePointListY(0))) = activePointListX(0)
            gridvalueY(_PositionToGridIndex(activePointListX(0)), _PositionToGridIndex(activePointListY(0))) = activePointListY(0)

            ' throw darts
            Dim dartX = 0.0F, dartY = 0.0F
            Dim gridX = 0, gridY = 0

            ' for each point in active list. 
            ' Note: in cf paper, the point is randomly chosen by its index.
            Dim proc As Integer = 0

            While proc <= activePointCount
                ' throw darts to get samples.
                Dim dart As Integer = 0

                While dart < k
                    Call getDartR(dartX, dartY, proc)

                    gridX = _PositionToGridIndex(dartX)
                    gridY = _PositionToGridIndex(dartY)

                    ' find out if there is samples near this dart.
                    Dim isdebug = True
                    Dim hasSamples = False

                    If isdebug And (_WrapRepeatFloat(dartX) - dartX <> 0.0F Or _WrapRepeatFloat(dartY) - dartY <> 0.0F) Then
                        ' Continue While
                        ' do nothing at here, disable continue while for avoid the
                        ' dead loop
                        hasSamples = True
                    Else
                        Dim x As Integer = -2

                        While x <= 2
                            Dim y = -2

                            While y <= 2
                                If isdebug Then
                                    Dim xx = gridvalueX(_WrapIndex(gridX + x), _WrapIndex(gridY + y))
                                    Dim yy = gridvalueY(_WrapIndex(gridX + x), _WrapIndex(gridY + y))

                                    If grid(_WrapIndex(gridX + x), _WrapIndex(gridY + y)) Then
                                        hasSamples = hasSamples Or (xx - dartX) * (xx - dartX) + (yy - dartY) * (yy - dartY) < minDist * minDist
                                    End If
                                Else
                                    hasSamples = hasSamples Or grid(_WrapIndex(gridX + x), _WrapIndex(gridY + y))
                                End If

                                y += 1
                            End While

                            x += 1
                        End While
                    End If

                    If hasSamples Then
                        ' there is a sample inside the minimum distance circle, abandon.
                        ' Continue While
                        ' do nothing at here, disable continue while for avoid the
                        ' dead loop
                    Else
                        ' no sample around, add this dart sample into processing list.
                        activePointCount += 1
                        grid(gridX, gridY) = True

                        gridvalueX(gridX, gridY) = _WrapRepeatFloat(dartX)
                        gridvalueY(gridX, gridY) = _WrapRepeatFloat(dartY)

                        activePointListX(activePointCount) = _WrapRepeatFloat(dartX)
                        activePointListY(activePointCount) = _WrapRepeatFloat(dartY)
                    End If

                    dart += 1
                End While

                proc += 1
            End While
        End Sub

        ''' <summary>
        ''' Given a float, return the grid index in any dimenssion 
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        Private Function _PositionToGridIndex(f As Single) As Integer
            Return std.Floor(_WrapRepeatFloat(f) / gridCellSize)
        End Function

        ''' <summary>
        ''' wrap float into generate range
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        Private Function _WrapRepeatFloat(f As Single) As Single
            Return f - std.Floor(f / m_CeiledSampleRange) * m_CeiledSampleRange
        End Function

        ''' <summary>
        ''' wrap grid index into grid length
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Private Function _WrapIndex(index As Integer) As Integer
            Return If(index < 0, index Mod gridLength + gridLength, index Mod gridLength)
        End Function

        Public Sub Dispose()
            If Not grayscale Is Nothing Then
                Call grayscale.Dispose()
            End If
        End Sub
    End Class
End Namespace
