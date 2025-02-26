#Region "Microsoft.VisualBasic::221c4057ff78fba14b9d927e6b2aeb7c, Data_science\Visualization\Plots-statistics\Heatmap\DataScaler.vb"

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

    '   Total Lines: 128
    '    Code Lines: 100 (78.12%)
    ' Comment Lines: 17 (13.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (8.59%)
    '     File Size: 5.78 KB


    '     Module DataScaler
    ' 
    '         Function: DoDataScale, ScaleByALL, ScaleByCol, ScaleByRow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace Heatmap

    Public Module DataScaler

        ''' <summary>
        ''' 主要是通过这个方法将样本值转换为索引编号，即将值映射到颜色数组中的索引，即可以使用颜色来表示相对应的样本数值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="scaleMethod"></param>
        ''' <param name="levels%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DoDataScale(data As IEnumerable(Of DataSet), scaleMethod As DrawElements, levels#) As Dictionary(Of String, DataSet)
            Select Case scaleMethod
                Case DrawElements.Cols
                    Return data _
                        .ScaleByCol(levels) _
                        .ToDictionary(Function(x) x.ID)
                Case DrawElements.Rows
                    Return data _
                        .ScaleByRow(levels) _
                        .ToDictionary(Function(x) x.ID)
                Case Else
                    Return data _
                        .ScaleByALL(levels) _
                        .ToDictionary(Function(x) x.ID)
            End Select
        End Function

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByRow(data As IEnumerable(Of DataSet), levels#) As IEnumerable(Of DataSet)
            Dim levelRange As DoubleRange = {0R, levels}

            Return data _
                .Select(Function(x)
                            Dim range As DoubleRange = x.Properties.Values.Range
                            Dim values As Dictionary(Of String, Double)

                            If range.Length = 0 Then
                                values = x.Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key) levels)
                            Else
                                values = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return range.ScaleMapping(x(key), levelRange)
                                                  End Function)
                            End If

                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = values
                            }
                        End Function)
        End Function

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByCol(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim ranges As Dictionary(Of String, DoubleRange) = keys _
                .ToDictionary(Function(key) key,
                              Function(key)
                                  Return list _
                                    .Select(Function(x) x(key)) _
                                    .Range
                              End Function)
            Dim levelRange As DoubleRange = {0R, levels}

            Return list _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return ranges(key).ScaleMapping(x(key), levelRange)
                                                  End Function)
                            }
                        End Function)
        End Function

        <Extension>
        Public Function ScaleByALL(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim range As DoubleRange = list _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .Range
            Dim levelRange As DoubleRange = {0R, levels}

            Return data _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return range.ScaleMapping(x(key), levelRange)
                                                  End Function)
                            }
                        End Function)
        End Function
    End Module
End Namespace
