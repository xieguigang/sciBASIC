#Region "Microsoft.VisualBasic::a119b28ea327aec2097f32abe61c2190, Data_science\Visualization\Plots\BarPlot\Data\BarPlotDataExtensions.vb"

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

    '   Total Lines: 224
    '    Code Lines: 178
    ' Comment Lines: 19
    '   Blank Lines: 27
    '     File Size: 8.97 KB


    '     Module BarPlotDataExtensions
    ' 
    '         Function: GroupBy, LoadDataSet, Normalize, SerialDatas, Takes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BarPlot.Data

    Public Module BarPlotDataExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SerialDatas(groups As IEnumerable(Of BarDataSample), serial%, Optional name$ = Nothing) As NamedValue(Of Double)()
            Return groups _
                .Select(Function(g)
                            Return New NamedValue(Of Double) With {
                                .Name = g.Tag,
                                .Description = name,
                                .Value = g.data(serial)
                            }
                        End Function) _
                .ToArray
        End Function

        Public Function LoadDataSet(path$,
                                    Optional schema$ = "scibasic.category31()",
                                    Optional groupByColumn! = yes,
                                    Optional tsv! = no) As BarDataGroup

            Dim csv As File = path.LoadTable
            Dim samples As New List(Of BarDataSample)
            Dim serialsName$()
            Dim matrix As IEnumerable(Of String())

            If groupByColumn Then
                matrix = csv.Columns
            Else
                matrix = csv.Select(Function(row) row.ToArray)
            End If

            With matrix
                serialsName = .First _
                    .Skip(1) _
                    .ToArray

                For Each column As String() In .Skip(1)
                    Dim groupName$ = column(Scan0)
                    Dim data#() = column.Skip(1).AsDouble()
                    Dim sample As New BarDataSample With {
                        .Tag = groupName,
                        .data = data
                    }

                    samples += sample
                Next
            End With

            Dim colors As Color() = Designer.GetColors(schema, serialsName.Length)
            Dim out As New BarDataGroup With {
                .Samples = samples,
                .Serials = serialsName _
                    .SeqIterator _
                    .Select(Function(i)
                                Return New NamedValue(Of Color) With {
                                    .Name = i.value,
                                    .Value = colors(i)
                                }
                            End Function) _
                    .ToArray
            }

            Return out
        End Function

        ''' <summary>
        ''' 有时候绘图的数据的系列数量太多了，尝试使用这个函数将数据进行减少到可以接受的程序
        ''' 所有被删除的数据都会被合并到``other``系列分组之中
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="n%"></param>
        ''' <param name="schema$">对结果进行重新着色，如果这个参数为空值，则不会进行重新着色</param>
        ''' <param name="otherColor">
        ''' 如果<paramref name="schema"/>为空字符串的话，则不会进行重新着色，但是会对新合并的``other``使用这个颜色进行着色
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Takes(data As BarDataGroup, n%,
                              Optional schema$ = "scibasic.category31()",
                              Optional otherColor$ = "gray") As BarDataGroup

            ' 求每一个系列在所有的group中的数量总和
            Dim serials$() = data.Serials.Keys(distinct:=False)
            Dim orders = serials _
                .SeqIterator _
                .Select(Function(i)
                            Dim factor# = Aggregate x As BarDataSample
                                          In data.Samples
                                          Into Sum(x.data(i))
                            Return New Factor(Of String) With {
                                .FactorValue = i.value,
                                .Value = factor * 10000
                            }
                        End Function) _
                .OrderByDescending(Function(x) x.Value) _
                .ToArray

            Dim keepsIndex%()
            Dim mergeIndex%()

            With orders _
                .Skip(n) _
                .Select(Function(x) x.FactorValue) _
                .Indexing

                ' 这些都是需要被合并的
                mergeIndex = serials.Indices(Function(x) .IndexOf(x) > -1)
                keepsIndex = serials.Indices(Function(x) .IndexOf(x) = -1)
            End With

            Dim groups As BarDataSample() = data _
                .Samples _
                .Select(Function(g)
                            Return New BarDataSample With {
                                .Tag = g.Tag,
                                .data = g.data.Takes(keepsIndex)
                            }
                        End Function) _
                .ToArray
            Dim merges = data _
                .Samples _
                .Select(Function(g)
                            Return g.data _
                                .Takes(mergeIndex) _
                                .Sum
                        End Function) _
                .ToArray

            For i As Integer = 0 To groups.Length - 1
                Call groups(i).data.Add(merges(i))
            Next

            Dim colors = data _
                .Serials _
                .Takes(keepsIndex) _
                .AsList

            If Not schema.StringEmpty Then
                Dim colorSchema As LoopArray(Of Color) = Designer.GetColors(schema)
                colors = (colors.Keys + "Other") _
                    .Select(Function(x)
                                Return New NamedValue(Of Color) With {
                                    .Name = x,
                                    .Value = colorSchema.Next
                                }
                            End Function) _
                    .AsList
            Else
                ' 只对新合并的other系列进行重新着色
                colors += New NamedValue(Of Color) With {
                    .Name = "Other",
                    .Value = otherColor.TranslateColor
                }
            End If

            Dim out As New BarDataGroup With {
                .Samples = groups,
                .Serials = colors
            }

            Return out
        End Function

        ''' <summary>
        ''' 将每一个分组内的数据都归一化为100%
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension> Public Function Normalize(data As BarDataGroup) As BarDataGroup
            For Each x As BarDataSample In data.Samples
                x.data = x.data.AsVector / x.StackedSum
            Next

            Return data
        End Function

        <Extension>
        Public Function GroupBy(data As BarDataGroup, groups As Dictionary(Of String, String())) As BarDataGroup
            If groups.IsNullOrEmpty Then
                Return data
            Else
                Dim groupSamples = data.Samples.ToDictionary()
                Dim groupData = groups _
                    .Select(Function(gk)
                                Dim subsets = groupSamples.Takes(gk.Value).ToArray
                                Dim serials#() = subsets(Scan0) _
                                    .data _
                                    .Sequence _
                                    .Select(Function(i) subsets.SerialDatas(i).Values.Sum) _
                                    .ToArray

                                Return New BarDataSample With {
                                    .Tag = gk.Key,
                                    .data = serials
                                }
                            End Function) _
                    .ToArray

                Return New BarDataGroup With {
                    .Samples = groupData,
                    .Serials = data.Serials
                }
            End If
        End Function
    End Module
End Namespace
