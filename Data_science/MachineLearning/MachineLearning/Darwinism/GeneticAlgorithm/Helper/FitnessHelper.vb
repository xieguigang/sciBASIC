#Region "Microsoft.VisualBasic::cbd4c957db9e1539c078b0e70bc13f5f, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Helper\FitnessHelper.vb"

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

'     Module FitnessHelper
' 
'         Function: (+2 Overloads) Calculate
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
            With errors.Where(Function(e) Not e.IsNaNImaginary AndAlso e < Double.MaxValue).ToArray
                If .Length = 0 Then
                    Return Double.MaxValue
                Else
                    Return .Average
                End If
            End With
        End Function
    End Module
End Namespace
