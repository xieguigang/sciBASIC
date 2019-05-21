#Region "Microsoft.VisualBasic::c4bf03db9d09c5b6637bc8a7f817902c, Data_science\MachineLearning\Bootstrapping\Darwinism\DESolver.vb"

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

    '     Module DifferentialEvolutionSolver
    ' 
    '         Function: Fitting
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ODEs
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.DifferentialEvolution
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus

Namespace Darwinism

    ''' <summary>
    ''' Differential Evolution estimates solver.
    ''' </summary>
    Public Module DifferentialEvolutionSolver

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="observation"></param>
        ''' <param name="F"></param>
        ''' <param name="CR"></param>
        ''' <param name="threshold#">
        ''' 现实的曲线太复杂了，因为模型是简单方程，只能够计算出简单的曲线，所以肯定不能完全拟合，
        ''' 最终的结果fitness也会较大，默认的0.1的fitness这个要求肯定不能够达到，
        ''' 所以只要达到一定次数的迭代就足够了，这个fitness的阈值参数值可以设置大一些
        ''' </param>
        ''' <param name="maxIterations%"></param>
        ''' <param name="PopulationSize%"></param>
        ''' <param name="iteratePrints"></param>
        ''' <param name="initOverrides"></param>
        ''' <param name="isRefModel"></param>
        ''' <param name="parallel">并行化计算要在种群的规模足够大的情况下才会有性能上的提升</param>
        ''' <param name="ignores">在计算fitness的时候将要被忽略掉的函数变量的名称</param>
        ''' <returns></returns>
        Public Function Fitting(Of T As MonteCarlo.Model)(
                         observation As ODEsOut,
                         Optional F As Double = 1,
                         Optional CR As Double = 0.5,
                         Optional threshold# = 0.1,
                         Optional maxIterations% = 500000,
                         Optional PopulationSize% = 200,
                         Optional ByRef iteratePrints As List(Of outPrint) = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing,
                         Optional estArgsBase As Dictionary(Of String, Double) = Nothing,
                         Optional ignores$() = Nothing,
                         Optional isRefModel As Boolean = False,
                         Optional parallel As Boolean = False,
                         Optional randomGenerator As IRandomSeeds = Nothing) As var()

            Dim model As Type = GetType(T)
            Dim vars As String() = MonteCarlo.Model.GetParameters(model).ToArray

            If estArgsBase.IsNullOrEmpty Then
                estArgsBase = New Dictionary(Of String, Double)
            End If
            If randomGenerator Is Nothing Then
                randomGenerator = Function() New Random
            End If

            Dim [new] As [New](Of ParameterVector) =
                Function(seed)
                    Dim out As New ParameterVector(randomGenerator) With {
                        .vars = vars _
                        .Select(Function(v) New var(v))
                    }

                    If seed Is Nothing Then
                        Return out
                    Else
                        For Each x As var In out.vars
                            If estArgsBase.ContainsKey(x.Name) Then
                                x.value = estArgsBase(x.Name)
                            Else
                                Dim power# = (
                                    If(seed.NextDouble > 0.5, 1, -1) * seed.Next(vars.Length)
                                )
                                x.value = 100 ^ power
                            End If
                        Next
                    End If

                    Return out
                End Function
            Dim fitness As New GAFFitness(model, observation, initOverrides, isRefModel) With {
                .Ignores = If(ignores.IsNullOrEmpty, {}, ignores)
            }
            Dim iterates As New List(Of outPrint)
            Dim best = DifferentialEvolution.Evolution(
                AddressOf fitness.Calculate,
                [new],
                vars.Length,
                F, CR, threshold,
                maxIterations,
                PopulationSize,
                AddressOf iterates.Add,
                parallel,
                randomGenerator)

            iteratePrints = iterates

            Return best.vars
        End Function
    End Module
End Namespace
