#Region "Microsoft.VisualBasic::ed6e1f36b1e8f9dd202464d32b8b10fc, Data_science\Mathematica\SignalProcessing\SignalProcessing\NDtw\Dtw.vb"

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

    '   Total Lines: 520
    '    Code Lines: 327
    ' Comment Lines: 109
    '   Blank Lines: 84
    '     File Size: 27.34 KB


    '     Class Dtw
    ' 
    '         Properties: Processor, SeriesVariables, XLength, YLength
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetCost, GetCostMatrix, GetDistanceMatrix, GetPath
    ' 
    '         Sub: Calculate, CalculateDistances, CalculateWithoutSlopeConstraint, CalculateWithSlopeLimit, InitializeArrays
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'(The MIT License)

'Copyright (c) 2012 Darjan Oblak

'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the 'Software'), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:

'The above copyright notice and this permission notice shall be included in all
'copies or substantial portions of the Software.

'THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
'SOFTWARE.

Imports System.Drawing
Imports Microsoft.VisualBasic.Math.SignalProcessing.NDtw.Preprocessing
Imports std = System.Math

Namespace NDtw

    ''' <summary>
    ''' Dynamic Time Warping (DTW) algorithm implementation
    ''' 
    ''' In time series analysis, dynamic time warping (DTW) is an algorithm for
    ''' measuring similarity between two temporal sequences, which may vary in 
    ''' speed. For instance, similarities in walking could be detected using DTW, 
    ''' even if one person was walking faster than the other, or if there were 
    ''' accelerations and decelerations during the course of an observation. DTW 
    ''' has been applied to temporal sequences of video, audio, and graphics data 
    ''' — indeed, any data that can be turned into a one-dimensional sequence can 
    ''' be analyzed with DTW. A well-known application has been automatic speech 
    ''' recognition, to cope with different speaking speeds. Other applications 
    ''' include speaker recognition and online signature recognition. It can also
    ''' be used in partial shape matching applications.
    '''
    ''' In general, DTW Is a method that calculates an optimal match between two 
    ''' given sequences (e.g. time series) with certain restriction And rules:
    '''
    ''' + Every index from the first sequence must be matched With one Or more 
    '''   indices from the other sequence, And vice versa
    ''' + The first index from the first sequence must be matched With the first 
    '''   index from the other sequence (but it does Not have To be its only 
    '''   match)
    ''' + The last index from the first sequence must be matched With the last index 
    '''   from the other sequence (but it does Not have To be its only match)
    ''' + The mapping Of the indices from the first sequence To indices from the 
    '''   other sequence must be monotonically increasing, And vice versa, i.e. If 
    '''   j > i are indices from the first sequence, then there must Not be two indices 
    '''   l > k in the other sequence, such that index i Is matched with index l And 
    '''   index j Is matched with index k, And vice versa.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/doblak/ndtw
    ''' </remarks>
    Public Class Dtw

        Private ReadOnly _isXLongerOrEqualThanY As Boolean
        Private ReadOnly _signalsLengthDifference As Integer
        Private ReadOnly _seriesVariables As GeneralSignal()
        Private ReadOnly _distanceMeasure As DistanceMeasure
        Private ReadOnly _boundaryConstraintStart As Boolean
        Private ReadOnly _boundaryConstraintEnd As Boolean
        Private ReadOnly _sakoeChibaConstraint As Boolean
        Private ReadOnly _sakoeChibaMaxShift As Integer
        Private _calculated As Boolean
        Private _distances As Double()()
        Private _pathCost As Double()()
        Private ReadOnly _useSlopeConstraint As Boolean
        Private ReadOnly _slopeMatrixLookbehind As Integer = 1
        Private ReadOnly _slopeStepSizeDiagonal As Integer
        Private ReadOnly _slopeStepSizeAside As Integer

        '[indexX][indexY][step]
        Private _predecessorStepX As Integer()()()
        Private _predecessorStepY As Integer()()()

        Public ReadOnly Property XLength As Integer
        Public ReadOnly Property YLength As Integer

        Public ReadOnly Property SeriesVariables As GeneralSignal()
            Get
                Return _seriesVariables
            End Get
        End Property

        Public Property Processor As IPreprocessor

        ''' <summary>
        ''' Initialize class that performs single variable DTW calculation for given series and settings.
        ''' </summary>
        ''' <param name="x">Series A, array of values.</param>
        ''' <param name="y">Series B, array of values.</param>
        ''' <param name="distanceMeasure">Distance measure used (how distance for value pair 
        ''' (p,q) of signal elements is calculated from multiple variables).</param>
        ''' <param name="boundaryConstraintStart">Apply boundary constraint at (1, 1).</param>
        ''' <param name="boundaryConstraintEnd">Apply boundary constraint at (m, n).</param>
        ''' <param name="slopeStepSizeDiagonal">Diagonal steps in local window for calculation. 
        ''' Results in Ikatura paralelogram shaped dtw-candidate space. Use in combination with 
        ''' slopeStepSizeAside parameter. Leave null for no constraint.</param>
        ''' <param name="slopeStepSizeAside">Side steps in local window for calculation. Results 
        ''' in Ikatura paralelogram shaped dtw-candidate space. Use in combination with
        ''' slopeStepSizeDiagonal parameter. Leave null for no constraint.</param>
        ''' <param name="sakoeChibaMaxShift">Sakoe-Chiba max shift constraint (side steps). 
        ''' Leave null for no constraint.</param>
        Public Sub New(x As Double(), y As Double(),
                       Optional distanceMeasure As DistanceMeasure = DistanceMeasure.Euclidean,
                       Optional boundaryConstraintStart As Boolean = True,
                       Optional boundaryConstraintEnd As Boolean = True,
                       Optional slopeStepSizeDiagonal As Integer? = Nothing,
                       Optional slopeStepSizeAside As Integer? = Nothing,
                       Optional sakoeChibaMaxShift As Integer? = Nothing)

            Me.New(New GeneralSignal() {New GeneralSignal(x, y)},
                   distanceMeasure,
                   boundaryConstraintStart, boundaryConstraintEnd,
                   slopeStepSizeDiagonal, slopeStepSizeAside,
                   sakoeChibaMaxShift)
        End Sub

        ''' <summary>
        ''' Initialize class that performs multivariate DTW calculation for given series and settings.
        ''' </summary>
        ''' <param name="seriesVariables">Array of series value pairs for different variables with additional 
        ''' options for data preprocessing and weights.</param>
        ''' <param name="distanceMeasure">Distance measure used (how distance for value pair (p,q) of signal 
        ''' elements is calculated from multiple variables).</param>
        ''' <param name="boundaryConstraintStart">Apply boundary constraint at (1, 1).</param>
        ''' <param name="boundaryConstraintEnd">Apply boundary constraint at (m, n).</param>
        ''' <param name="slopeStepSizeDiagonal">Diagonal steps in local window for calculation. Results in 
        ''' Ikatura paralelogram shaped dtw-candidate space. Use in combination with slopeStepSizeAside 
        ''' parameter. Leave null for no constraint.</param>
        ''' <param name="slopeStepSizeAside">Side steps in local window for calculation. Results in Ikatura 
        ''' paralelogram shaped dtw-candidate space. Use in combination with slopeStepSizeDiagonal parameter.
        ''' Leave null for no constraint.</param>
        ''' <param name="sakoeChibaMaxShift">
        ''' Sakoe-Chiba max shift constraint (side steps). Leave null for no constraint.
        ''' </param>
        Public Sub New(seriesVariables As GeneralSignal(),
                       Optional distanceMeasure As DistanceMeasure = DistanceMeasure.Euclidean,
                       Optional boundaryConstraintStart As Boolean = True,
                       Optional boundaryConstraintEnd As Boolean = True,
                       Optional slopeStepSizeDiagonal As Integer? = Nothing,
                       Optional slopeStepSizeAside As Integer? = Nothing,
                       Optional sakoeChibaMaxShift As Integer? = Nothing,
                       Optional preprocessor As IPreprocessor = Nothing)

            _seriesVariables = seriesVariables
            _distanceMeasure = distanceMeasure
            _boundaryConstraintStart = boundaryConstraintStart
            _boundaryConstraintEnd = boundaryConstraintEnd

            If seriesVariables Is Nothing OrElse seriesVariables.Length = 0 Then
                Throw New ArgumentException("Series should have values for at least one variable.")
            End If

            For i = 1 To _seriesVariables.Length - 1
                If _seriesVariables(i).Measures.Length <> _seriesVariables(0).Measures.Length Then
                    Throw New ArgumentException("All variables withing series should have the same number of values.")
                End If

                If _seriesVariables(i).Strength.Length <> _seriesVariables(0).Strength.Length Then
                    Throw New ArgumentException("All variables withing series should have the same number of values.")
                End If
            Next

            XLength = _seriesVariables(0).Measures.Length
            YLength = _seriesVariables(0).Strength.Length

            If _XLength = 0 OrElse _YLength = 0 Then
                Throw New ArgumentException("Both series should have at least one value.")
            End If

            If sakoeChibaMaxShift IsNot Nothing AndAlso sakoeChibaMaxShift < 0 Then
                Throw New ArgumentException("Sakoe-Chiba max shift value should be positive or null.")
            End If

            _isXLongerOrEqualThanY = _XLength >= _YLength
            _signalsLengthDifference = std.Abs(_XLength - _YLength)
            _sakoeChibaConstraint = sakoeChibaMaxShift.HasValue
            _sakoeChibaMaxShift = If(sakoeChibaMaxShift.HasValue, sakoeChibaMaxShift.Value, Integer.MaxValue)

            If slopeStepSizeAside IsNot Nothing OrElse slopeStepSizeDiagonal IsNot Nothing Then
                If slopeStepSizeAside Is Nothing OrElse slopeStepSizeDiagonal Is Nothing Then
                    Throw New ArgumentException("Both values or none for slope constraint must be specified.")
                End If

                If slopeStepSizeDiagonal < 1 Then
                    Throw New ArgumentException("Diagonal slope constraint parameter must be greater than 0.")
                End If

                If slopeStepSizeAside < 0 Then
                    Throw New ArgumentException("Diagonal slope constraint parameter must be greater or equal to 0.")
                End If

                _useSlopeConstraint = True
                _slopeStepSizeAside = slopeStepSizeAside.Value
                _slopeStepSizeDiagonal = slopeStepSizeDiagonal.Value

                _slopeMatrixLookbehind = slopeStepSizeDiagonal.Value + slopeStepSizeAside.Value
            End If

            'todo: throw error when solution (path from (1, 1) to (m, n) is not even possible due to slope constraints)
            If preprocessor Is Nothing Then
                Processor = IPreprocessor.None
            Else
                Processor = preprocessor
            End If
        End Sub

        Private Sub InitializeArrays()
            _distances = New Double(_XLength + _slopeMatrixLookbehind - 1)() {}
            For i As Integer = 0 To _XLength + _slopeMatrixLookbehind - 1
                _distances(i) = New Double(_YLength + _slopeMatrixLookbehind - 1) {}
            Next

            _pathCost = New Double(_XLength + _slopeMatrixLookbehind - 1)() {}
            For i = 0 To _XLength + _slopeMatrixLookbehind - 1
                _pathCost(i) = New Double(_YLength + _slopeMatrixLookbehind - 1) {}
            Next

            _predecessorStepX = New Integer(_XLength + _slopeMatrixLookbehind - 1)()() {}
            For i = 0 To _XLength + _slopeMatrixLookbehind - 1
                _predecessorStepX(i) = New Integer(_YLength + _slopeMatrixLookbehind - 1)() {}
            Next

            _predecessorStepY = New Integer(_XLength + _slopeMatrixLookbehind - 1)()() {}
            For i = 0 To _XLength + _slopeMatrixLookbehind - 1
                _predecessorStepY(i) = New Integer(_YLength + _slopeMatrixLookbehind - 1)() {}
            Next
        End Sub

        Private Sub CalculateDistances()
            For additionalIndex = 1 To _slopeMatrixLookbehind
                'initialize [x.len - 1 + additionalIndex][all] elements
                For i = 0 To _YLength + _slopeMatrixLookbehind - 1
                    _pathCost(_XLength - 1 + additionalIndex)(i) = Double.PositiveInfinity
                Next

                'initialize [all][y.len - 1 + additionalIndex] elements
                For i = 0 To _XLength + _slopeMatrixLookbehind - 1
                    _pathCost(i)(_YLength - 1 + additionalIndex) = Double.PositiveInfinity
                Next
            Next

            'calculate distances for 'data' part of the matrix
            For Each seriesVariable As GeneralSignal In _seriesVariables
                Dim xSeriesForVariable = _Processor(seriesVariable.Measures)
                Dim ySeriesForVariable = _Processor(seriesVariable.Strength)
                'weight for current variable distances that is applied BEFORE the value is further transformed by distance measure
                Dim variableWeight = seriesVariable.weight

                For i As Integer = 0 To _XLength - 1
                    Dim currentDistances = _distances(i)
                    Dim xVal = xSeriesForVariable(i)
                    For j As Integer = 0 To _YLength - 1
                        If _distanceMeasure = DistanceMeasure.Manhattan Then
                            currentDistances(j) += std.Abs(xVal - ySeriesForVariable(j)) * variableWeight
                        ElseIf _distanceMeasure = DistanceMeasure.Maximum Then
                            currentDistances(j) = std.Max(currentDistances(j), std.Abs(xVal - ySeriesForVariable(j)) * variableWeight)
                        Else
                            'Math.Pow(xVal - ySeriesForVariable[j], 2) is much slower, so direct multiplication with temporary variable is used
                            Dim dist = (xVal - ySeriesForVariable(j)) * variableWeight
                            currentDistances(j) += dist * dist
                        End If
                    Next
                Next
            Next

            If _distanceMeasure = DistanceMeasure.Euclidean Then
                For i = 0 To _XLength - 1
                    Dim currentDistances = _distances(i)
                    For j = 0 To _YLength - 1
                        currentDistances(j) = std.Sqrt(currentDistances(j))
                    Next
                Next
            End If
        End Sub

        Private Sub CalculateWithoutSlopeConstraint()
            Dim stepMove0 = {0}
            Dim stepMove1 = {1}

            For i As Integer = _XLength - 1 To 0 Step -1
                Dim currentRowDistances = _distances(i)
                Dim currentRowPathCost = _pathCost(i)
                Dim previousRowPathCost = _pathCost(i + 1)

                Dim currentRowPredecessorStepX = _predecessorStepX(i)
                Dim currentRowPredecessorStepY = _predecessorStepY(i)

                For j As Integer = _YLength - 1 To 0 Step -1
                    'Sakoe-Chiba constraint, but make it wider in one dimension when signal lengths are not equal
                    If _sakoeChibaConstraint AndAlso If(_isXLongerOrEqualThanY, j > i AndAlso j - i > _sakoeChibaMaxShift OrElse j < i AndAlso i - j > _sakoeChibaMaxShift + _signalsLengthDifference, j > i AndAlso j - i > _sakoeChibaMaxShift + _signalsLengthDifference OrElse j < i AndAlso i - j > _sakoeChibaMaxShift) Then
                        currentRowPathCost(j) = Double.PositiveInfinity
                        Continue For
                    End If

                    Dim diagonalNeighbourCost = previousRowPathCost(j + 1)
                    Dim xNeighbourCost = previousRowPathCost(j)
                    Dim yNeighbourCost = currentRowPathCost(j + 1)

                    'on the topright edge, when boundary constrained only assign current distance as path distance to the (m, n) element
                    'on the topright edge, when not boundary constrained, assign current distance as path distance to all edge elements
                    If Double.IsInfinity(diagonalNeighbourCost) AndAlso (Not _boundaryConstraintEnd OrElse i - j = _XLength - _YLength) Then
                        currentRowPathCost(j) = currentRowDistances(j)
                    ElseIf diagonalNeighbourCost <= xNeighbourCost AndAlso diagonalNeighbourCost <= yNeighbourCost Then
                        currentRowPathCost(j) = diagonalNeighbourCost + currentRowDistances(j)
                        currentRowPredecessorStepX(j) = stepMove1
                        currentRowPredecessorStepY(j) = stepMove1
                    ElseIf xNeighbourCost <= yNeighbourCost Then
                        currentRowPathCost(j) = xNeighbourCost + currentRowDistances(j)
                        currentRowPredecessorStepX(j) = stepMove1
                        currentRowPredecessorStepY(j) = stepMove0
                    Else
                        currentRowPathCost(j) = yNeighbourCost + currentRowDistances(j)
                        currentRowPredecessorStepX(j) = stepMove0
                        currentRowPredecessorStepY(j) = stepMove1
                    End If
                Next
            Next
        End Sub

        Private Sub CalculateWithSlopeLimit()
            'precreated array that contain arrays of steps which are used when stepaside path is the optimal one
            'stepAsideMoves*[0] is empty element, because contents are 1-based, access to elements is faster that way
            Dim stepAsideMovesHorizontalX = New Integer(_slopeStepSizeAside + 1 - 1)() {}
            Dim stepAsideMovesHorizontalY = New Integer(_slopeStepSizeAside + 1 - 1)() {}
            Dim stepAsideMovesVerticalX = New Integer(_slopeStepSizeAside + 1 - 1)() {}
            Dim stepAsideMovesVerticalY = New Integer(_slopeStepSizeAside + 1 - 1)() {}
            For i As Integer = 1 To _slopeStepSizeAside
                Dim movesXHorizontal = New List(Of Integer)()
                Dim movesYHorizontal = New List(Of Integer)()
                Dim movesXVertical = New List(Of Integer)()
                Dim movesYVertical = New List(Of Integer)()

                'make steps in horizontal/vertical direction
                For stepAside = 1 To i
                    movesXHorizontal.Add(1)
                    movesYHorizontal.Add(0)

                    movesXVertical.Add(0)
                    movesYVertical.Add(1)
                Next

                'make steps in diagonal direction
                For stepForward = 1 To _slopeStepSizeDiagonal
                    movesXHorizontal.Add(1)
                    movesYHorizontal.Add(1)

                    movesXVertical.Add(1)
                    movesYVertical.Add(1)
                Next

                stepAsideMovesHorizontalX(i) = movesXHorizontal.ToArray()
                stepAsideMovesHorizontalY(i) = movesYHorizontal.ToArray()

                stepAsideMovesVerticalX(i) = movesXVertical.ToArray()
                stepAsideMovesVerticalY(i) = movesYVertical.ToArray()
            Next

            Dim stepMove1 = {1}

            For i As Integer = _XLength - 1 To 0 Step -1
                Dim currentRowDistances = _distances(i)

                Dim currentRowPathCost = _pathCost(i)
                Dim previousRowPathCost = _pathCost(i + 1)

                Dim currentRowPredecessorStepX = _predecessorStepX(i)
                Dim currentRowPredecessorStepY = _predecessorStepY(i)

                For j As Integer = _YLength - 1 To 0 Step -1
                    'Sakoe-Chiba constraint, but make it wider in one dimension when signal lengths are not equal
                    If _sakoeChibaConstraint AndAlso If(_isXLongerOrEqualThanY, j > i AndAlso j - i > _sakoeChibaMaxShift OrElse j < i AndAlso i - j > _sakoeChibaMaxShift + _signalsLengthDifference, j > i AndAlso j - i > _sakoeChibaMaxShift + _signalsLengthDifference OrElse j < i AndAlso i - j > _sakoeChibaMaxShift) Then

                        currentRowPathCost(j) = Double.PositiveInfinity
                        Continue For
                    End If

                    'just initialize lowest cost with diagonal neighbour element
                    Dim lowestCost = previousRowPathCost(j + 1)
                    Dim lowestCostStepX = stepMove1
                    Dim lowestCostStepY = stepMove1

                    For alternativePathAside As Integer = 1 To _slopeStepSizeAside
                        Dim costHorizontalStepAside = 0.0
                        Dim costVerticalStepAside = 0.0

                        For stepAside = 1 To alternativePathAside
                            costHorizontalStepAside += _distances(i + stepAside)(j)
                            costVerticalStepAside += _distances(i)(j + stepAside)
                        Next

                        For stepForward = 1 To _slopeStepSizeDiagonal - 1
                            costHorizontalStepAside += _distances(i + alternativePathAside + stepForward)(j + stepForward)
                            costVerticalStepAside += _distances(i + stepForward)(j + alternativePathAside + stepForward)
                        Next

                        'at final step, add comulative cost
                        costHorizontalStepAside += _pathCost(i + alternativePathAside + _slopeStepSizeDiagonal)(j + _slopeStepSizeDiagonal)

                        'at final step, add comulative cost
                        costVerticalStepAside += _pathCost(i + _slopeStepSizeDiagonal)(j + alternativePathAside + _slopeStepSizeDiagonal)

                        'check if currently considered horizontal stepaside is better than the best option found until now
                        If costHorizontalStepAside < lowestCost Then
                            lowestCost = costHorizontalStepAside
                            lowestCostStepX = stepAsideMovesHorizontalX(alternativePathAside)
                            lowestCostStepY = stepAsideMovesHorizontalY(alternativePathAside)
                        End If

                        'check if currently considered vertical stepaside is better than the best option found until now
                        If costVerticalStepAside < lowestCost Then
                            lowestCost = costVerticalStepAside
                            lowestCostStepX = stepAsideMovesVerticalX(alternativePathAside)
                            lowestCostStepY = stepAsideMovesVerticalY(alternativePathAside)
                        End If
                    Next

                    'on the topright edge, when boundary constrained only assign current distance as path distance to the (m, n) element
                    'on the topright edge, when not boundary constrained, assign current distance as path distance to all edge elements
                    If Double.IsInfinity(lowestCost) AndAlso (Not _boundaryConstraintEnd OrElse i - j = _XLength - _YLength) Then
                        lowestCost = 0
                    End If

                    currentRowPathCost(j) = lowestCost + currentRowDistances(j)
                    currentRowPredecessorStepX(j) = lowestCostStepX
                    currentRowPredecessorStepY(j) = lowestCostStepY
                Next
            Next
        End Sub

        Private Sub Calculate()
            If Not _calculated Then
                InitializeArrays()
                CalculateDistances()

                If _useSlopeConstraint Then
                    CalculateWithSlopeLimit()
                Else
                    CalculateWithoutSlopeConstraint()
                End If

                _calculated = True
            End If
        End Sub

        Public Function GetCost() As Double
            Calculate()

            If _boundaryConstraintStart Then
                Return _pathCost(0)(0)
            End If

            Dim min2 As Double = Aggregate y As Double()
                                 In _pathCost
                                 Let ymin = y(0)
                                 Into Min(ymin)

            Return std.Min(_pathCost(0).Min(), min2)
        End Function

        Public Iterator Function GetPath() As IEnumerable(Of Point)
            Dim indexX = 0
            Dim indexY = 0

            Call Calculate()

            If Not _boundaryConstraintStart Then
                ' find the starting element with lowest cost
                Dim min = Double.PositiveInfinity

                For i As Integer = 0 To std.Max(_XLength, _YLength) - 1
                    If i < _XLength AndAlso _pathCost(i)(0) < min Then
                        indexX = i
                        indexY = 0
                        min = _pathCost(i)(0)
                    End If
                    If i < _YLength AndAlso _pathCost(0)(i) < min Then
                        indexX = 0
                        indexY = i
                        min = _pathCost(0)(i)
                    End If
                Next
            End If

            Yield New Point(indexX, indexY)

            While If(_boundaryConstraintEnd, indexX < _XLength - 1 OrElse indexY < _YLength - 1, indexX < _XLength - 1 AndAlso indexY < _YLength - 1)
                Dim stepX = _predecessorStepX(indexX)(indexY)
                Dim stepY = _predecessorStepY(indexX)(indexY)

                For i As Integer = 0 To stepX.Length - 1
                    indexX += stepX(i)
                    indexY += stepY(i)

                    Yield New Point(indexX, indexY)
                Next
            End While
        End Function

        Public Function GetDistanceMatrix() As Double()()
            Calculate()
            Return _distances
        End Function

        Public Function GetCostMatrix() As Double()()
            Calculate()
            Return _pathCost
        End Function
    End Class
End Namespace
