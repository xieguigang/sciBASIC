#Region "Microsoft.VisualBasic::3f4ec9dd60360d84ae37f06989e41558, Data_science\Visualization\Plots\Fractions\FractionData.vb"

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

    '   Total Lines: 51
    '    Code Lines: 27 (52.94%)
    ' Comment Lines: 19 (37.25%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (9.80%)
    '     File Size: 1.75 KB


    '     Class FractionData
    ' 
    '         Properties: Color, Name, Percentage, Value
    ' 
    '         Function: GetValueLabel, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Fractions

    ''' <summary>
    ''' 扇形/金字塔的数据模型
    ''' </summary>
    Public Class FractionData : Implements INamedValue

        ''' <summary>
        ''' 值范围为``[0, 1]``, 对象在整体中所占的百分比
        ''' </summary>
        ''' <returns></returns>
        Public Property Percentage As Double
        ''' <summary>
        ''' 对象的名称标签
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String Implements IKeyedEntity(Of String).Key
        ''' <summary>
        ''' 扇形、金字塔梯形的填充颜色
        ''' </summary>
        ''' <returns></returns>
        Public Property Color As Color
        ''' <summary>
        ''' 与占整体的百分比相对应的实际数量
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function GetValueLabel(type As ValueLabels) As String
            Select Case type
                Case ValueLabels.None
                    Return Nothing
                Case ValueLabels.Value
                    Return Value.ToString("F" & 2)
                Case ValueLabels.Percentage
                    Return (Percentage * 100).ToString("F2") & "%"
                Case Else
                    Return Value
            End Select
        End Function
    End Class
End Namespace
