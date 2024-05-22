#Region "Microsoft.VisualBasic::ce9dff6e75345b85ffc5d27e07848ca1, Data_science\Visualization\Plots\BarPlot\Data\Constructor.vb"

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

    '   Total Lines: 80
    '    Code Lines: 64 (80.00%)
    ' Comment Lines: 7 (8.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (11.25%)
    '     File Size: 3.08 KB


    '     Module Constructor
    ' 
    '         Function: FromDistributes, SimpleSerials
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Distributions

Namespace BarPlot.Data

    Public Module Constructor

        ''' <summary>
        ''' 这个应该是生成直方图的数据
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="base!"></param>
        ''' <param name="serialColor$"></param>
        ''' <returns></returns>
        Public Function FromDistributes(data As IEnumerable(Of Double), Optional base! = 10.0F, Optional serialColor$ = "darkblue") As BarDataGroup
            Dim source = data.Distributes(base!)
            Dim bg As Color = serialColor.ToColor(onFailure:=Color.DarkBlue)
            Dim values As New List(Of Double)
            Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) _
 _
                () <= From lv As Integer
                      In source.Keys
                      Let tag As String = lv.ToString
                      Select New NamedValue(Of Color) With {
                          .Name = tag,
                          .Value = bg
                      }

            For Each x As NamedValue(Of Color) In serials
                values += source(CInt(x.Name)).Value
            Next

            Return New BarDataGroup With {
                .Serials = serials,
                .Samples = {
                    New BarDataSample With {
                        .Tag = "Distribution",
                        .data = values
                    }
                }
            }
        End Function

        <Extension>
        Public Function SimpleSerials(data As IEnumerable(Of NamedValue(Of Double)), Optional posColor$ = "red", Optional ngColor$ = "darkblue") As BarDataGroup
            Dim dataVector As NamedValue(Of Double)() = data.ToArray
            Dim colors As NamedValue(Of Color)() = dataVector _
                .Select(Function(d)
                            Dim color As Color

                            If d.Value > 0 Then
                                color = posColor.ToColor(Color.Red)
                            Else
                                color = ngColor.ToColor(Color.DarkBlue)
                            End If

                            Return New NamedValue(Of Color) With {
                                .Name = d.Name,
                                .Value = color
                            }
                        End Function) _
                .ToArray

            Return New BarDataGroup With {
                .Serials = colors,
                .Samples = {
                    New BarDataSample With {
                        .data = dataVector.Values,
                        .Tag = "N/A"
                    }
                }
            }
        End Function
    End Module
End Namespace
