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
                            Return New Correlation With {
                                .B = Vector.rand(-1 ^ 10, 1 ^ -10, width)
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
                .ToArray
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
