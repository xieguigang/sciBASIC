
Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 扇形/金字塔的数据模型
''' </summary>
Public Class PercentageData

    ''' <summary>
    ''' 对象在整体中所占的百分比
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

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class