#Region "Microsoft.VisualBasic::11a2f3536c9295c078dd79b0eaf96187, Data_science\Darwinism\NonlinearGrid\TopologyInference\Loader.vb"

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
    '     Function: CreateSnapshot, EmptyGridSystem
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module Loader

    ''' <summary>
    ''' 因为累加效应在系统很庞大的时候可能会非常的大,所以在这里全部都是用零来进行初始化的
    ''' </summary>
    ''' <param name="width"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function EmptyGridSystem(width As Integer) As GridSystem
        Return New GridSystem With {
            .A = Vector.Ones(width),
            .C = width.SeqIterator _
                .Select(Function(null)
                            ' 全部使用负数初始化,可以让整个指数为负数
                            ' 从而避免一开始就出现无穷大的结果???
                            '
                            ' 但是如果样本之中的X向量中存在一个非常小的数,则会反而被无限放大??
                            ' 为了避免出现 0 ^ -c = Inf的情况出现
                            ' 这个C向量应该全部都是零初始化，这样子系统初始状态为 Sum(X)
                            Return New Correlation With {
                                .B = Vector.Zero(width),
                                .BC = 1
                            }
                        End Function) _
                .ToArray
        }
        '    .P = width.SeqIterator _
        '        .Select(Function(null)
        '                    ' 累乘效应是十分大的,所以在一开始应该
        '                    ' 是全部设置为零,这样子权重系数就全部都是1
        '                    ' 没有对结果产生影响
        '                    Return New PWeight With {
        '                        .W = Vector.Zero(width)
        '                    }
        '                End Function) _
        '        .ToArray
        '}
    End Function

    ''' <summary>
    ''' Create dataset of the factors in the grid system, and then we are able to save the model to file
    ''' </summary>
    ''' <param name="genome"></param>
    ''' <param name="error#"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateSnapshot(genome As Genome, names$(), error#) As GridMatrix
        Return New GridMatrix With {
            .[error] = [error],
            .direction = genome.chromosome.A.ToArray,
            .correlations = genome.chromosome _
                .C _
                .Select(Function(c, i)
                            Return New NumericVector With {
                                .name = names(i),
                                .vector = c.B.ToArray
                            }
                        End Function) _
                .ToArray,
            .[const] = New Constants With {
                .A = genome.chromosome.AC,
                .B = New NumericVector With {
                    .name = "correlations_const",
                    .vector = genome.chromosome _
                        .C _
                        .Select(Function(ci) ci.BC) _
                        .ToArray
                }
            }
        }
        '    .weights = genome.chromosome _
        '        .P _
        '        .Select(Function(p, i)
        '                    Return New NumericVector With {
        '                        .name = i,
        '                        .vector = p.W.ToArray
        '                    }
        '                End Function) _
        '        .ToArray
        '}
    End Function
End Module
