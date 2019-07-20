#Region "Microsoft.VisualBasic::bb97af06418e5ed82978b2b8fa10a0bb, Data_science\MachineLearning\MachineLearning\DataSet\NormalizeMatrix.vb"

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

    '     Class NormalizeMatrix
    ' 
    '         Properties: matrix, names
    ' 
    '         Function: CreateFromSamples, NormalizeInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Distributions

Namespace StoreProcedure

    ''' <summary>
    ''' 进行所输入的样本数据的归一化的矩阵
    ''' </summary>
    Public Class NormalizeMatrix : Inherits XmlDataModel

        ''' <summary>
        ''' 每一个属性都具有一个归一化区间
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("matrix")>
        Public Property matrix As SampleDistribution()
        ''' <summary>
        ''' 属性名称列表,这个序列的长度是和<see cref="matrix"/>的长度一致的,并且元素的顺序一一对应的
        ''' </summary>
        ''' <returns></returns>
        Public Property names As String()

        ''' <summary>
        ''' Normalize the <paramref name="sample"/> inputs <see cref="Sample.status"/> to value range ``[0, 1]``
        ''' </summary>
        ''' <param name="sample"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NormalizeInput(sample As Sample, Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double()
            Return sample.status _
                .vector _
                .Select(Function(x, i)
                            Select Case method
                                Case Normalizer.Methods.NormalScaler
                                    Return Normalizer.ScalerNormalize(matrix(i), x)
                                Case Normalizer.Methods.RelativeScaler
                                    Return Normalizer.RelativeNormalize(matrix(i), x)
                                Case Normalizer.Methods.RangeDiscretizer
                                    Return Normalizer.RangeDiscretizer(matrix(i), x)
                                Case Else
                                    Return Normalizer.ScalerNormalize(matrix(i), x)
                            End Select
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 神经网络会要求输入的属性值之间是可以直接进行比较的,
        ''' 所以为了能够直接进行比较,
        ''' 在这里将sample的每一个属性都按列归一化为``[0,1]``之间的结果
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Public Shared Function CreateFromSamples(samples As IEnumerable(Of Sample), names As IEnumerable(Of String)) As NormalizeMatrix
            With samples.ToArray
                Dim len% = .First.status.Length
                Dim index%
                Dim matrix As New List(Of SampleDistribution)
                Dim averages As New List(Of Double)
                Dim [property] As Double()

                For i As Integer = 0 To len - 1
                    ' 遍历每一列的数据,将每一列的数据都执行归一化
                    index = i
                    [property] = .Select(Function(sample) sample.status(index)).ToArray
                    matrix += New SampleDistribution([property])
                    averages.Add([property].Average)
                Next

                Return New NormalizeMatrix With {
                    .matrix = matrix,
                    .names = names.ToArray
                }
            End With
        End Function
    End Class
End Namespace
