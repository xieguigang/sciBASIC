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
                            Return New Correlation With {
                                .B = Vector.Zero(width)
                            }
                        End Function) _
                .ToArray
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateSnapshot(genome As Genome, error#) As GridMatrix
        Return New GridMatrix With {
            .[error] = [error],
            .direction = genome.chromosome.A.ToArray,
            .matrix = genome.chromosome _
                .C _
                .Select(Function(c, i)
                            Return New NumericVector With {
                                .name = i,
                                .vector = c.B.ToArray
                            }
                        End Function) _
                .ToArray
        }
    End Function
End Module
