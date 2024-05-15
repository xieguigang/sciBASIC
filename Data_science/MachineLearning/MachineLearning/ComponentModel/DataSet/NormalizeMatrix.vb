#Region "Microsoft.VisualBasic::7ab0c18249fff28c8bfe9e648a4295ca, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\NormalizeMatrix.vb"

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

    '   Total Lines: 112
    '    Code Lines: 70
    ' Comment Lines: 29
    '   Blank Lines: 13
    '     File Size: 5.30 KB


    '     Class NormalizeMatrix
    ' 
    '         Properties: matrix, names
    ' 
    '         Function: (+2 Overloads) CreateFromSamples, doNormalInternal, DoNormalize, (+2 Overloads) NormalizeInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' A matrix for make the sample input normalized.(进行所输入的样本数据的归一化的矩阵)
    ''' </summary>
    Public Class NormalizeMatrix : Inherits XmlDataModel

        ''' <summary>
        ''' 每一个属性都具有一个归一化区间
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("matrix")>
        Public Property matrix As XmlList(Of SampleDistribution)
        ''' <summary>
        ''' 属性名称列表,这个序列的长度是和<see cref="matrix"/>的长度一致的,并且元素的顺序一一对应的
        ''' </summary>
        ''' <returns></returns>
        Public Property names As String()

        Public Function DoNormalize(name$, value#, Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double
            Dim i As Integer = Array.IndexOf(names, name)
            Dim dist As SampleDistribution = matrix(i)
            Dim result = doNormalInternal(dist, value, method)

            Return result
        End Function

        Public Shared Function doNormalInternal(dist As SampleDistribution, x#, Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double
            Select Case method
                Case Normalizer.Methods.NormalScaler
                    Return Normalizer.ScalerNormalize(dist, x)
                Case Normalizer.Methods.RelativeScaler
                    Return Normalizer.RelativeNormalize(dist, x)
                Case Normalizer.Methods.RangeDiscretizer
                    Return Normalizer.RangeDiscretizer(dist, x)
                Case Else
                    Return Normalizer.ScalerNormalize(dist, x)
            End Select
        End Function

        ''' <summary>
        ''' Normalize the <paramref name="sample"/> inputs <see cref="Sample.label"/> to value range ``[0, 1]``
        ''' </summary>
        ''' <param name="sample"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NormalizeInput(sample As Sample, Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double()
            Return NormalizeInput(sample.vector, method)
        End Function

        ''' <summary>
        ''' Normalize the <paramref name="sample"/> inputs <see cref="Sample.label"/> to value range ``[0, 1]``
        ''' </summary>
        ''' <param name="sample"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NormalizeInput(sample As IEnumerable(Of Double), Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double()
            Return sample _
                .Select(Function(x, i)
                            Return doNormalInternal(matrix(i), x, method)
                        End Function) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateFromSamples(sampleList As SampleList,
                                                 names As IEnumerable(Of String),
                                                 Optional estimateQuantile As Boolean = True) As NormalizeMatrix

            Return CreateFromSamples(sampleList.AsEnumerable, names, estimateQuantile)
        End Function

        ''' <summary>
        ''' 神经网络会要求输入的属性值之间是可以直接进行比较的,
        ''' 所以为了能够直接进行比较,
        ''' 在这里将sample的每一个属性都按列归一化为``[0,1]``之间的结果
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="names">The property names, not sample id names</param>
        ''' <returns></returns>
        Public Shared Function CreateFromSamples(samples As IEnumerable(Of Sample),
                                                 names As IEnumerable(Of String),
                                                 Optional estimateQuantile As Boolean = True) As NormalizeMatrix

            With samples.Select(Function(sample) sample.vector).ToArray
                Dim len% = .First.Length
                Dim matrix As SampleDistribution() = len _
                    .SeqIterator _
                    .AsParallel _
                    .Select(Function(index)
                                Return (i:=index, Data:= .ProjectData(index, estimateQuantile))
                            End Function) _
                    .OrderBy(Function(data) data.i) _
                    .Select(Function(r) r.Data) _
                    .ToArray

                Return New NormalizeMatrix With {
                    .matrix = matrix,
                    .names = names.ToArray
                }
            End With
        End Function
    End Class
End Namespace
