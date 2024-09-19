#Region "Microsoft.VisualBasic::2ab72ba42317fd979c89ee7da702bfcf, Data_science\Visualization\Plots\csv\SerialData.vb"

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

'   Total Lines: 107
'    Code Lines: 83 (77.57%)
' Comment Lines: 11 (10.28%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 13 (12.15%)
'     File Size: 4.28 KB


'     Class SerialData
' 
'         Properties: errMinus, errPlus, serial, Statics, tag
'                     value, X, Y
' 
'         Function: (+2 Overloads) GetData, Interpolation, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace TabularRender

    Public Class SerialData

        ''' <summary>
        ''' 系列的名称
        ''' </summary>
        ''' <returns></returns>
        Public Property serial As String
        Public Property X As Single
        Public Property Y As Single
        Public Property value As Double
        Public Property tag As String
        Public Property errPlus As Double
        Public Property errMinus As Double
        Public Property Statics As Double()

        Public Shared Function GetData(csv$, Optional colors As Color() = Nothing, Optional lineWidth! = 2) As IEnumerable(Of ChartPlots.SerialData)
            Return GetData(csv.LoadCsv(Of SerialData), colors, lineWidth)
        End Function

        Public Shared Iterator Function GetData(data As IEnumerable(Of SerialData),
                                                Optional colors As Color() = Nothing,
                                                Optional lineWidth! = 2) As IEnumerable(Of ChartPlots.SerialData)
            Dim gs = From x As SerialData
                     In data
                     Select x
                     Group x By x.serial Into Group

            colors = If(
                colors.IsNullOrEmpty,
                Imaging.ChartColors.Shuffles,
                colors)

            For Each g In gs.SeqIterator

                Yield New ChartPlots.SerialData With {
                    .width = lineWidth,
                    .title = g.value.serial,
                    .color = colors(g.i),
                    .pts = LinqAPI.Exec(Of PointData) <=
                        From x As SerialData
                        In g.value.Group
                        Select New PointData With {
                            .errMinus = x.errMinus,
                            .errPlus = x.errPlus,
                            .pt = New PointF(x.X, x.Y),
                            .tag = x.tag,
                            .value = x.value,
                            .Statics = x.Statics
                        }
                    }
            Next
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 请注意这里只对一个系列的数据进行插值处理，即<paramref name="raw"/>里面的所有标签<see cref="SerialData.serial"/>都必须要相同
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="degree!"></param>
        ''' <param name="resolution%"></param>
        ''' <returns></returns>
        Public Shared Function Interpolation(raw As IEnumerable(Of SerialData),
                                             Optional degree! = 2,
                                             Optional resolution% = 10) As SerialData()
            Dim rawData As SerialData() = raw _
                .OrderBy(Function(x) x.X) _
                .ToArray
            Dim pts As List(Of PointF) = B_Spline.Compute(
                rawData.Select(Function(x) New PointF(x.X, x.Y)),
                degree,
                resolution)
            Dim result As New List(Of SerialData)

            For Each block In pts.SlideWindows(resolution, offset:=resolution).SeqIterator
                Dim pt As SerialData = rawData(block)

                For Each d As PointF In block.value.Items
                    result += New SerialData With {
                        .errMinus = pt.errMinus,
                        .errPlus = pt.errPlus,
                        .serial = pt.serial,
                        .Statics = pt.Statics,
                        .tag = pt.tag,
                        .value = pt.value,
                        .X = d.X,
                        .Y = d.Y
                    }
                Next
            Next

            Return result
        End Function
    End Class
End Namespace
