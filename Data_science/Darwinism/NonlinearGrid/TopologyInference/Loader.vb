#Region "Microsoft.VisualBasic::3e54c45c2ac36008f71a2cfeb40cf029, Data_science\Darwinism\NonlinearGrid\TopologyInference\Loader.vb"

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

    ' Module Loader
    ' 
    '     Function: Correlation, CreateSnapshot, EmptyGridSystem, ToArray
    ' 
    '     Sub: (+3 Overloads) Truncate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology.BigData
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Module Loader

    <Extension>
    Public Function Correlation(samples As IEnumerable(Of Sample)) As Vector
        Dim dataArray = samples.ToArray
        Dim target = dataArray.Select(Function(d) d.target(Scan0)).ToArray
        Dim cor = Iterator Function() As IEnumerable(Of Double)
                      Dim array As Double()
                      Dim pcc As Double
#Disable Warning
                      For i As Integer = 0 To dataArray(Scan0).status.Length - 1
                          array = dataArray.Select(Function(r) r.status(i)).ToArray
                          pcc = Correlations.GetPearson(array, target)

                          Yield pcc
                      Next
#Enable Warning
                  End Function

        Return New Vector(cor())
    End Function

    ''' <summary>
    ''' 因为累加效应在系统很庞大的时候可能会非常的大,所以在这里全部都是用零来进行初始化的
    ''' </summary>
    ''' <param name="width"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function EmptyGridSystem(width As Integer, Optional cor As Vector = Nothing, Optional power As Vector = Nothing, Optional bigData As Boolean = False) As [Variant](Of GridSystem, SparseGridSystem)
        Dim A As Vector = cor Or New Vector(1, width).AsDefault

        If bigData Then
            Dim correlationMatrix = width _
                .SeqIterator _
                .AsParallel _
                .Select(Function(null)
                            ' 全部使用负数初始化,可以让整个指数为负数
                            ' 从而避免一开始就出现无穷大的结果???
                            '
                            ' 但是如果样本之中的X向量中存在一个非常小的数,则会反而被无限放大??
                            ' 为了避免出现 0 ^ -c = Inf的情况出现
                            ' 这个C向量应该全部都是零初始化，这样子系统初始状态为 Sum(X)
                            Dim powerFactor As Vector

                            If power Is Nothing Then
                                powerFactor = Vector.rand(-1, 1, width)
                            Else
                                powerFactor = New Vector(power)
                            End If

                            Dim powerC As New SparseCorrelation With {
                                .B = New HalfVector(powerFactor),
                                .BC = 0.005
                            }

                            Return (null, powerC)
                        End Function) _
                .OrderBy(Function(c) c.Item1) _
                .Select(Function(c)
                            Return c.Item2
                        End Function) _
                .ToArray

            Return New SparseGridSystem With {
                .A = New HalfVector(A),
                .C = correlationMatrix
            }
        Else
            Dim correlationMatrix = width _
                .SeqIterator _
                .Select(Function(null)
                            ' 全部使用负数初始化,可以让整个指数为负数
                            ' 从而避免一开始就出现无穷大的结果???
                            '
                            ' 但是如果样本之中的X向量中存在一个非常小的数,则会反而被无限放大??
                            ' 为了避免出现 0 ^ -c = Inf的情况出现
                            ' 这个C向量应该全部都是零初始化，这样子系统初始状态为 Sum(X)
                            Dim powerFactor As Vector

                            If power Is Nothing Then
                                powerFactor = Vector.rand(-0.0001, 0.0001, width)
                            Else
                                powerFactor = New Vector(power)
                            End If

                            Return New Correlation With {
                                .B = powerFactor,
                                .BC = 0.0001
                            }
                        End Function) _
                .ToArray

            Return New GridSystem With {
                .A = A,
                .C = correlationMatrix
            }
        End If
    End Function

    ''' <summary>
    ''' Create dataset of the factors in the grid system, and then we are able to save the model to file
    ''' </summary>
    ''' <param name="genome"></param>
    ''' <param name="error#"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateSnapshot(Of V, IC As ICorrelation(Of V))(genome As IGrid(Of V, IC), dist As NormalizeMatrix, Optional names$() = Nothing, Optional error# = -1) As GridMatrix
        Return New GridMatrix With {
            .[error] = [error],
            .direction = genome.A.DoCall(AddressOf ToArray),
            .correlations = genome _
                .C _
                .Select(Function(c, i)
                            Dim factorName$ = names.ElementAtOrDefault(i)

                            If factorName.StringEmpty Then
                                factorName = $"factor_{i}"
                            End If

                            Return New NumericVector With {
                                .name = factorName,
                                .vector = c.B.DoCall(AddressOf ToArray)
                            }
                        End Function) _
                .ToArray,
            .[const] = New Constants With {
                .A = genome.AC,
                .B = New NumericVector With {
                    .name = "correlations_const",
                    .vector = genome _
                        .C _
                        .Select(Function(ci) ci.BC) _
                        .ToArray
                }
            },
            .samples = dist
        }
    End Function

    Private Function ToArray(Of V)(vec As V) As Double()
        If TypeOf vec Is Vector Then
            Return DirectCast(CObj(vec), Vector).ToArray
        ElseIf TypeOf vec Is HalfVector Then
            Return DirectCast(CObj(vec), HalfVector).AsVector.ToArray
        Else
            Throw New NotImplementedException(vec.GetType.FullName)
        End If
    End Function

    <Extension>
    Friend Sub Truncate(vec As Vector, limits As Double)
        Dim ref = vec.Array

        For i As Integer = 0 To vec.Length - 1
            If Math.Abs(ref(i)) > limits Then
                ref(i) = Math.Sign(ref(i)) * randf.seeds.NextDouble * (limits)
            End If
        Next
    End Sub

    <Extension>
    Friend Sub Truncate(vec As SparseVector, limits As Double)
        Dim ref = vec.Array

        For i As Integer = 0 To ref.Length - 1
            If Math.Abs(ref(i)) > limits Then
                ref(i) = Math.Sign(ref(i)) * randf.seeds.NextDouble * (limits)
            End If
        Next
    End Sub

    <Extension>
    Friend Sub Truncate(vec As HalfVector, limits As Double)
        Dim ref = vec.Array

        For i As Integer = 0 To ref.Length - 1
            If Math.Abs(CSng(ref(i))) > limits Then
                ref(i) = Math.Sign(ref(i)) * randf.seeds.NextDouble * (limits)
            End If
        Next
    End Sub
End Module
