

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>图表数据系列</summary>
Public Class Series
    Public Property Name As String = ""
    Public Property Color As Color? = Nothing
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property MarkerShape As MarkerShape = MarkerShape.Circle
    Public Property LineStyle As DashStyle = DashStyle.Solid
    Public Property Visible As Boolean = True
End Class
