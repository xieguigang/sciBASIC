#Region "Microsoft.VisualBasic::66ecf16d68854b44518f45aa638a56b4, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Helper\FitnessHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module FitnessHelper
    ' 
    '         Function: AverageError, (+2 Overloads) Calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Darwinism.GAF.Helper

    Public Module FitnessHelper

        ''' <summary>
        ''' Implements Fitness(Of C, T).Calculate
        ''' </summary>
        ''' <param name="chromosome#"></param>
        ''' <param name="target#"></param>
        ''' <returns></returns>
        Public Function Calculate(chromosome#(), target#()) As Double
            Dim delta# = 0
            Dim v#() = chromosome

            For i As Integer = 0 To chromosome.Length - 1
                delta += (v(i) - target(i)) ^ 2
            Next

            Return delta
        End Function

        Public Function Calculate(chromosome%(), target%()) As Double
            Dim delta# = 0
            Dim v%() = chromosome

            For i As Integer = 0 To chromosome.Length - 1
                delta += (v(i) - target(i)) ^ 2
            Next

            Return delta
        End Function

        ''' <summary>
        ''' 使用平均值来计算fitness
        ''' </summary>
        ''' <param name="errors"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为假设使用<see cref="Double.MaxValue"/>来替代Infinity的计算结果
        ''' 则刚开始的时候，可能比较多的样本都是Inf的结果，此时将无法正常的用系统
        ''' 的平均数函数来计算误差，所以会需要使用到这个函数来完成整体误差的计算
        ''' </remarks>
        <Extension>
        Public Function AverageError(errors As IEnumerable(Of Double)) As Double
            Dim rawErrs = errors.ToArray
            Dim errVector As Double() = rawErrs _
                .Select(Function(e)
                            If Not e.IsNaNImaginary AndAlso
                                e <> Double.MaxValue AndAlso
                                e <> Double.MinValue Then
                                Return e
                            Else
                                Return Long.MaxValue
                            End If
                        End Function) _
                .ToArray

            Return errVector.Average
        End Function
    End Module
End Namespace
