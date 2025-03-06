#Region "Microsoft.VisualBasic::962c256bb295c4a9a1c3c91e694fe82f, Data_science\MachineLearning\MLDebugger\ANN\FrameExports.vb"

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

    '   Total Lines: 90
    '    Code Lines: 75 (83.33%)
    ' Comment Lines: 4 (4.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (12.22%)
    '     File Size: 3.69 KB


    ' Module FrameExports
    ' 
    '     Function: ExportErrorCurve, ExportValueFrames, GetTimeIndex, NormalizeSample
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Basic = Microsoft.VisualBasic.Language.Runtime
Imports DataSet = Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure.DataSet
Imports Excel = Microsoft.VisualBasic.Data.Framework.IO.DataSet

Public Module FrameExports

    <Extension>
    Public Function NormalizeSample(samples As DataSet, method As Methods) As Excel()
        Dim matrix As Sample() = samples.PopulateNormalizedSamples(method).ToArray
        Dim names = samples.NormalizeMatrix.names.SeqIterator.ToArray
        Dim dataset = matrix _
            .Select(Function(r)
                        Dim vec As Double() = r.vector

                        Return New Excel With {
                            .ID = r.ID,
                            .Properties = names.ToDictionary(
                                Function(name) name.value,
                                Function(name)
                                    Return vec(name)
                                End Function)
                        }
                    End Function) _
            .ToArray

        Return dataset
    End Function

    ''' <summary>
    ''' 导出误差率曲线数据
    ''' </summary>
    ''' <returns></returns>
    Public Function ExportErrorCurve(cdf As netCDFReader) As DataFrameResolver
        Dim errors As Double() = DirectCast(cdf.getDataVariable("fitness"), doubles).Array
        Dim index As Integer() = DirectCast(cdf.getDataVariable("iterations"), integers).Array

        With New Basic
            Return New DataFrameResolver(!iterations = index, !fitness = errors)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetTimeIndex(cdf As netCDFReader) As String()
        Return DirectCast(cdf.getDataVariable("iterations"), integers) _
            .Array _
            .Select(Function(i) $"T{i}") _
            .ToArray
    End Function

    Public Iterator Function ExportValueFrames(cdf As netCDFReader) As IEnumerable(Of Excel)
        Dim nodes As variable() = cdf.variables _
            .Where(Function(var)
                       Dim isANeuron As attribute = var _
                           .attributes _
                           .FirstOrDefault(Function(a)
                                               Return a.name = "type" AndAlso a.value = "neuron"
                                           End Function)

                       Return Not isANeuron Is Nothing
                   End Function) _
            .ToArray
        Dim times = FrameExports.GetTimeIndex(cdf) _
            .SeqIterator _
            .ToArray
        Dim row As Excel
        Dim nodeValue As Double()

        For Each node As variable In nodes
            nodeValue = DirectCast(cdf.getDataVariable(node), doubles).Array
            row = New Excel With {
                .ID = node.name,
                .Properties = times.ToDictionary(
                    Function(tag) tag.value,
                    Function(i)
                        Return nodeValue.ElementAtOrDefault(i, [default]:=Double.NaN)
                    End Function)
            }

            Yield row
        Next
    End Function
End Module
