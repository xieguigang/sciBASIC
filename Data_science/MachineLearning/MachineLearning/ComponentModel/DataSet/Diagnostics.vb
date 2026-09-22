#Region "Microsoft.VisualBasic::5da6765a50eaeecf6014c730654327d0, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\Diagnostics.vb"

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

    '   Total Lines: 94
    '    Code Lines: 55 (58.51%)
    ' Comment Lines: 31 (32.98%)
    '    - Xml Docs: 93.55%
    ' 
    '   Blank Lines: 8 (8.51%)
    '     File Size: 4.14 KB


    '     Module Diagnostics
    ' 
    '         Function: CheckDataSet, ProjectData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions.Summary

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' The diagnostic helpers of the training <see cref="DataSet"/> object.
    ''' </summary>
    Public Module Diagnostics

        ''' <summary>
        ''' Check the data integrity of a training dataset: whether the input 
        ''' vector size and the output vector size of each sample is consistent 
        ''' with the dataset definition.
        ''' </summary>
        ''' <param name="data">The target <see cref="DataSet"/> object that will be checked.</param>
        ''' <returns>
        ''' A sequence of the <see cref="LogEntry"/> objects, each one describes a 
        ''' problem that was found in the <paramref name="data"/>; an empty 
        ''' sequence means the dataset is valid.
        ''' </returns>
        <Extension>
        Public Iterator Function CheckDataSet(data As DataSet) As IEnumerable(Of LogEntry)
            Dim nSamples = data.DataSamples.size
            Dim size As Size = data.Size
            Dim outputSize As Integer = data.OutputSize

            ' check for sample datas
            For Each sample As Sample In data.DataSamples.AsEnumerable
                If sample.vector.Length <> size.Width Then
                    Yield New LogEntry With {
                        .message = $"sample vector size is not equals to {size.Width}!",
                        .[object] = sample.ID,
                        .time = Now,
                        .level = MSG_TYPES.ERR
                    }
                End If
                If sample.target.Length <> outputSize Then
                    Yield New LogEntry With {
                        .message = $"output vector size is not equals to {outputSize}!",
                        .[object] = sample.ID,
                        .time = Now,
                        .level = MSG_TYPES.ERR
                    }
                End If
            Next

            Dim i As i32 = Scan0

            For Each [property] In data.NormalizeMatrix.matrix.AsEnumerable
                If [property].size <> nSamples Then
                    Yield New LogEntry With {
                        .message = "sample size is not equals to the normalized samples",
                        .level = MSG_TYPES.WRN,
                        .[object] = data.NormalizeMatrix.names.ElementAtOrDefault(++i, $"unknown_{CInt(i) - 1}"),
                        .time = Now
                    }
                End If
            Next
        End Function

        ''' <summary>
        ''' Extract one property column from the sample matrix, and then evaluate 
        ''' the distribution of that column.
        ''' </summary>
        ''' <param name="matrix">
        ''' The sample matrix, in which each row is the input vector of one sample.
        ''' </param>
        ''' <param name="index">The column index of the target property.</param>
        ''' <param name="estimateQuantile">
        ''' Whether the quantile value of the distribution should be estimated?
        ''' </param>
        ''' <returns>
        ''' A <see cref="SampleDistribution"/> object which describes the 
        ''' distribution of the target property column.
        ''' </returns>
        <Extension>
        Friend Function ProjectData(matrix As Double()(), index As Integer, estimateQuantile As Boolean) As SampleDistribution
            ' 遍历每一列的数据,将每一列的数据都执行归一化
            Dim [property] As Double() = matrix _
                .Select(Function(sample)
                            Return sample(index)
                        End Function) _
                .ToArray
            Dim dist As New SampleDistribution([property], estimateQuantile)

            Return dist
        End Function
    End Module
End Namespace
