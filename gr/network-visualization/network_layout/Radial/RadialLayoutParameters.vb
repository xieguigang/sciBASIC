Imports System.ComponentModel

Namespace Radial

    ''' <summary>
    ''' 径向布局参数，可在 PropertyGrid 中编辑
    ''' </summary>
    Public Class RadialLayoutParameters

        <Category("布局"), DisplayName("环间距"), Description("每层同心圆环之间的间距（像素），<=0 时自动推算")>
        Public Property Radius As Double = Double.NaN

        Public Overrides Function ToString() As String
            Return "Radial"
        End Function
    End Class

End Namespace
