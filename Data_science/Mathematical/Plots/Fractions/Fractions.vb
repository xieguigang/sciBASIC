#Region "Microsoft.VisualBasic::3b611dfcba85b04ff3ac3eca5d47f244, ..\sciBASIC#\Data_science\Mathematical\Plots\Fractions\Fractions.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 扇形/金字塔的数据模型
''' </summary>
Public Class Fractions

    ''' <summary>
    ''' 值范围为``[0, 1]``, 对象在整体中所占的百分比
    ''' </summary>
    ''' <returns></returns>
    Public Property Percentage As Double
    ''' <summary>
    ''' 对象的名称标签
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String
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

    Public Enum ValueLabels
        None
        Percentage
        Value
    End Enum

    Public Function GetValueLabel(type As ValueLabels) As String
        Select Case type
            Case ValueLabels.None
                Return Nothing
            Case ValueLabels.Value
                Return Value.FormatNumeric(2)
            Case ValueLabels.Percentage
                Return (Percentage * 100).FormatNumeric(2) & "%"
            Case Else
                Return Value
        End Select
    End Function
End Class
