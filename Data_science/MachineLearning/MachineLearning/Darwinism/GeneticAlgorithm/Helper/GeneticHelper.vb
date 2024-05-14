#Region "Microsoft.VisualBasic::a5d5ec8c235b22e68b56b7dadc3b8567, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Helper\GeneticHelper.vb"

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

    '   Total Lines: 190
    '    Code Lines: 88
    ' Comment Lines: 77
    '   Blank Lines: 25
    '     File Size: 7.26 KB


    '     Module GeneticHelper
    ' 
    '         Sub: ByteMutate, (+3 Overloads) Crossover, (+3 Overloads) Mutate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Darwinism.GAF.Helper

    ''' <summary>
    ''' 在这个模块之中,涉及到<see cref="SparseVector"/>的所有函数都是应用于处理非常大的系统而构建的
    ''' </summary>
    Public Module GeneticHelper

        ' 2018-4-29
        '
        ' 关于random对象的使用说明：尽量不要创建新的random对象
        ' 从下面的测试可以看得出来，当使用旧的random对象的时候，可以生成一系列的伪随机数
        ' 但是如果进行random对象的创建的话，则几乎不会再生成新的随机数
        '
        ' With New Random
        '    For i As Integer = 0 To 100
        '        Call .NextDouble.__DEBUG_ECHO
        '    Next
        '
        '    Call "==============================================".__INFO_ECHO
        '
        '    For i As Integer = 0 To 100
        '        Call New Random().NextDouble.__DEBUG_ECHO
        '    Next
        ' End With

#Region "Numeric Value Mutation"

        ' 20190709
        ' integer类型不适合突变计算
        ' 所有的染色体都应该是double类型的
        ' 所以在这里将integer类型的突变帮助函数删除了

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        ''' <param name="v#"></param>
        ''' <param name="random"></param>
        ''' <param name="index">
        ''' + 如果这个坐标参数大于等于零,则会直接按照这个坐标值对指定位置的目标进行突变
        ''' + 反之小于零的时候,则是随机选取一个位置的目标进行突变
        ''' </param>
        ''' <remarks>
        ''' 在进行突变的时候应该是按照给定的范围来进行突变的
        ''' </remarks>
        <Extension>
        Public Sub Mutate(ByRef v#(), random As Random, Optional index% = -1000, Optional rate# = 0.1)
            Dim delta# = (v.Max - v.Min) * rate
            Dim mutationValue#

            ' 20190709 如果v向量全部都是零或者相等数值的话
            ' 将无法产生突变
            ' 在这里测试下，添加一个小数来完成突变
            If delta = 0R Then
                delta = 0.0000001
            End If

            mutationValue = (random.NextDouble * delta) * If(random.NextDouble >= 0.5, 1, -1)

            If index < 0 Then
                v(random.Next(v.Length)) += mutationValue
            Else
                v(index) += mutationValue
            End If
        End Sub

        <Extension>
        Public Sub Mutate(ByRef v As SparseVector, random As Random, Optional index% = -1000, Optional rate# = 0.1)
            Dim delta# = (v.Max - v.Min) * rate
            Dim mutationValue#

            ' 20190709 如果v向量全部都是零或者相等数值的话
            ' 将无法产生突变
            ' 在这里测试下，添加一个小数来完成突变
            If delta = 0R Then
                delta = 0.0000001
            End If

            mutationValue = (random.NextDouble * delta) * If(random.NextDouble >= 0.5, 1, -1)

            If index < 0 Then
                v(random.Next(v.Length)) += mutationValue
            Else
                v(index) += mutationValue
            End If
        End Sub

        <Extension>
        Public Sub Mutate(ByRef v As HalfVector, random As Random, Optional index% = -1000, Optional rate# = 0.1)
            Dim delta# = (v.Max - v.Min) * rate
            Dim mutationValue!

            ' 20190709 如果v向量全部都是零或者相等数值的话
            ' 将无法产生突变
            ' 在这里测试下，添加一个小数来完成突变
            If delta = 0R Then
                delta = 0.0000001
            End If

            mutationValue = (random.NextDouble * delta) * If(random.NextDouble >= 0.5, 1, -1)

            If index < 0 Then
                v(random.Next(v.Length)) += mutationValue
            Else
                v(index) += mutationValue
            End If
        End Sub
#End Region

        ''' <summary>
        ''' 这个函数不是数值变化，而是位值的变化，原来的某位数值为1，则突变后为零，原来某位数值为0，则突变之后为1
        ''' </summary>
        ''' <param name="v%"></param>
        ''' <param name="random"></param>
        <Extension>
        Public Sub ByteMutate(ByRef v%(), random As Random)
            Dim index = random.Next(v.Length)

            If v(index) = 0 Then
                v(index) = 1
            Else
                v(index) = 0
            End If
        End Sub

        ''' <summary>
        ''' Returns list of siblings 
        ''' Siblings are actually new chromosomes, 
        ''' created using any of crossover strategy
        ''' 
        ''' (两个向量的长度必须要一致, 输入的两个数组参数会被同时修改值)
        ''' </summary>
        ''' <param name="random"></param>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <remarks>
        ''' the size of <paramref name="v1"/> and <paramref name="v2"/> should be equals to each other!
        ''' </remarks>
        <Extension>
        Public Sub Crossover(Of T)(random As Random, ByRef v1 As T(), ByRef v2 As T())
            ' 在这里减掉1是为了防止两个变量被全部替换掉
            Dim size As Integer = random.Next(v1.Length - 1)
            Dim index As Integer() = v1.Length.SeqRandom
            Dim tmp As T

            ' one point crossover
            For Each i As Integer In index.Take(size)
                tmp = v1(i)
                v1(i) = v2(i)
                v2(i) = tmp
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Crossover(Of T)(ByRef v1 As T(), ByRef v2 As T())
            Call randf.seeds.Crossover(v1, v2)
        End Sub

        ''' <summary>
        ''' Returns list of siblings 
        ''' Siblings are actually new chromosomes, 
        ''' created using any of crossover strategy
        ''' 
        ''' (两个向量的长度必须要一致, 输入的两个数组参数会被同时修改值)
        ''' </summary>
        ''' <param name="random"></param>
        ''' <param name="v1#"></param>
        ''' <param name="v2#"></param>
        <Extension>
        Public Sub Crossover(random As Random, ByRef v1 As HalfVector, ByRef v2 As HalfVector)
            ' 在这里减掉1是为了防止两个变量被全部替换掉
            Dim index As Integer = random.Next(v1.Length - 1)
            Dim tmp As Double
            Dim a1 = v1.Array
            Dim a2 = v2.Array

            ' one point crossover
            For i As Integer = index To v1.Length - 1
                tmp = a1(i)
                a1(i) = a2(i)
                a2(i) = tmp
            Next
        End Sub

    End Module
End Namespace
