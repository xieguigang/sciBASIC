Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes cosine similarity between two points, d = 1 - ((X*Y) / (||X||*||Y||))
    ''' </summary>
    Public Class CosineSimilarity
        Implements IDistanceCalculator(Of Double()), IDistanceCalculator(Of Dictionary(Of Integer, Integer)), ISparseMatrixSupport
        Private ReadOnly _tryGet As Func(Of Integer, (Boolean, Double))
        Private ReadOnly _tryAdd As Action(Of Integer, Double)

        Public Sub New(Optional useCaching As Boolean = False, Optional usedWithMultipleThreads As Boolean = False)
            If Not useCaching Then
                ' No caching. Do nothing.
                _tryGet = Function(__) (False, 0)
                _tryAdd = Sub(__, ___)
                          End Sub
            Else
                If usedWithMultipleThreads Then
                    Dim cache = New ConcurrentDictionary(Of Integer, Double)()

                    _tryGet = Function(index)
                                  Dim value As Double = Nothing
                                  Dim hasValue = cache.TryGetValue(index, value)
                                  Return (hasValue, value)
                              End Function
                    _tryAdd = Sub(index, value) cache.TryAdd(index, value)
                Else
                    Dim cache = New Dictionary(Of Integer, Double)()

                    _tryGet = Function(index)
                                  Dim value As Double = Nothing
                                  Dim hasValue = cache.TryGetValue(index, value)
                                  Return (hasValue, value)
                              End Function
                    _tryAdd = Sub(index, value)
                                  If Not cache.ContainsKey(index) Then cache.Add(index, value)
                              End Sub
                End If
            End If
        End Sub

        Public Function GetMostCommonDistanceValueForSparseMatrix() As Double Implements ISparseMatrixSupport.GetMostCommonDistanceValueForSparseMatrix
            Return 1
        End Function

        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim magnitudeOne = CalculateAndCacheMagnitude(indexOne, attributesOne)
            Dim magnitudeTwo = CalculateAndCacheMagnitude(indexTwo, attributesTwo)

            If magnitudeOne = 0 AndAlso magnitudeTwo = 0 Then Return 1

            Dim dotProduct As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                dotProduct += attributesOne(i) * attributesTwo(i)
                i += 1
            End While

            Dim lComputeDistance = stdNum.Max(0, 1 - dotProduct / stdNum.Sqrt(magnitudeOne * magnitudeTwo))
            Return lComputeDistance
        End Function

        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Dictionary(Of Integer, Integer), attributesTwo As Dictionary(Of Integer, Integer)) As Double Implements IDistanceCalculator(Of Dictionary(Of Integer, Integer)).ComputeDistance
            Dim magnitudeOne = CalculateAndCacheMagnitude(indexOne, attributesOne)
            Dim magnitudeTwo = CalculateAndCacheMagnitude(indexTwo, attributesTwo)

            If magnitudeOne = 0 AndAlso magnitudeTwo = 0 Then Return 1

            Dim dotProduct As Double = 0
            If attributesOne.Count < attributesTwo.Count Then
                For Each i In attributesOne.Keys
                    If attributesTwo.ContainsKey(i) Then dotProduct += attributesOne(i) * attributesTwo(i)
                Next
            Else
                For Each i In attributesTwo.Keys
                    If attributesOne.ContainsKey(i) Then dotProduct += attributesOne(i) * attributesTwo(i)
                Next
            End If

            Return stdNum.Max(0, 1 - dotProduct / stdNum.Sqrt(magnitudeOne * magnitudeTwo))
        End Function

        Private Function CalculateAndCacheMagnitude(index As Integer, attributes As Dictionary(Of Integer, Integer)) As Double
            Dim hasValueValue As (hasValue As Boolean, value As Double) = Nothing
            hasValueValue = _tryGet(index)
            If hasValueValue.hasValue Then Return hasValueValue.value

            Dim magnitude = attributes.Keys.Sum(Function(i) stdNum.Pow(attributes(i), 2))
            _tryAdd(index, magnitude)
            Return magnitude
        End Function

        Private Function CalculateAndCacheMagnitude(index As Integer, attributes As Double()) As Double
            Dim hasValueValue As (hasValue As Boolean, value As Double) = Nothing
            hasValueValue = _tryGet(index)
            If hasValueValue.hasValue Then Return hasValueValue.value

            Dim magnitude = attributes.Sum(Function(val) stdNum.Pow(val, 2))
            _tryAdd(index, magnitude)
            Return magnitude
        End Function
    End Class
End Namespace
