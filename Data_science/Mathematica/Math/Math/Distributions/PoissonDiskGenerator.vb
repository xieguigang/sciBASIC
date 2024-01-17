Imports Microsoft.VisualBasic.Imaging.Math2D
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

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
        Public Shared minDist As Single = 5.0F
        ''' <summary>
        ''' the time of throw darts. Higher k generate better result but slower.
        ''' </summary>
        Public Shared k As Integer = 30
        ''' <summary>
        ''' the range of generated samples. From 0[inclusive] to sampleRange[inclusive]
        ''' </summary>
        Public Shared sampleRange As Single = 256.0F

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

        ''' <summary>
        ''' Determines if inputs are appropriate.
        ''' </summary>
        ''' <returns><c>true</c> if is inputs valid; otherwise, <c>false</c>.</returns>
        Private Shared Function IsInputsValid() As Boolean
            Return minDist > 0.0F AndAlso k > 0 AndAlso sampleRange > minDist
        End Function

        ''' <summary>
        ''' Generate samples. Based on minDist / k / sampleRange.
        ''' </summary>
        Public Shared Function Generate() As List(Of Vector2D)
            If Not IsInputsValid() Then
                ' TODO: handle error.
                Return Nothing
            Else
                Dim poisson As New PoissonDiskGenerator

                Call poisson.Sampling()
                Call poisson.PullSamples()

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
            Dim dartRadians = 0.0F
            Dim dartDist = 0.0F
            Dim gridX = 0, gridY = 0

            ' for each point in active list. 
            ' Note: in cf paper, the point is randomly chosen by its index.
            Dim proc = 0

            While proc <= activePointCount
                ' throw darts to get samples.
                Dim dart = 0

                While dart < k
                    ' randomly chose a dart in the ring area.
                    dartRadians = rand.NextDouble(0, Pi2)
                    dartDist = rand.NextDouble(minDist, 2.0F * minDist) ' range from minDist to 2*minDist ( r to 2r in cf paper )
                    dartX = activePointListX(proc) + dartDist * std.Cos(dartRadians)
                    dartY = activePointListY(proc) + dartDist * std.Sin(dartRadians)
                    gridX = _PositionToGridIndex(dartX)
                    gridY = _PositionToGridIndex(dartY)

                    ' find out if there is samples near this dart.
                    Dim isdebug = True

                    If isdebug And (_WrapRepeatFloat(dartX) - dartX <> 0.0F Or _WrapRepeatFloat(dartY) - dartY <> 0.0F) Then
                        Continue While
                    End If

                    Dim hasSamples = False
                    Dim x = -2

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

                    If hasSamples Then
                        ' there is a sample inside the minimum distance circle, abandon.
                        Continue While
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
    End Class
End Namespace