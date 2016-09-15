Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Ordinary differential equation(ODE).(常微分方程的模型)
''' </summary>
Public Class ODE

#Region "Output results"

    Public Property x As Double()
    Public Property y As Double()
#End Region

    ''' <summary>
    ''' Public Delegate Function df(x As Double, y As Double) As Double
    ''' (从这里传递自定义的函数指针)
    ''' </summary>
    ''' <returns></returns>
    Public Property df As ODESolver.df
    ''' <summary>
    ''' ``x=a``的时候的y初始值
    ''' </summary>
    ''' <returns></returns>
    Public Property y0 As Double

    Default Public ReadOnly Property GetValue(xi As Double, yi As Double) As Double
        Get
            Return _df(xi, yi)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return x.GetJson & " --> " & y.GetJson
    End Function
End Class