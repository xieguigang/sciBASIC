Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Math.Statistics.Distributions

Public Class FTest

    Dim x, y As Double()

    <Field("freedom1")>
    Public ReadOnly Property XdegreeOfFreedom As Integer
        Get
            Return x.Length - 1
        End Get
    End Property

    <Field("freedom2")>
    Public ReadOnly Property YdegreeOfFreedom As Integer
        Get
            Return y.Length - 1
        End Get
    End Property

    <Field("var.x")>
    Public ReadOnly Property XVariance As Double
        Get
            Return x.SD
        End Get
    End Property

    <Field("var.y")>
    Public ReadOnly Property YVariance As Double
        Get
            Return y.SD
        End Get
    End Property

    ''' <summary>
    ''' ratio of variances 
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Field("F")>
    Public ReadOnly Property F As Double
        Get
            Dim q As Double = XVariance ^ 2
            Dim p As Double = YVariance ^ 2

            Return q / p
        End Get
    End Property

    <Field("single_tailed p-value")>
    Public ReadOnly Property SingleTailedPval As Double
        Get
            Dim f As Double = Me.F
            Dim single_tailed_pval As Double = Distribution.FDistribution(
                fValue:=f,
                freedom1:=XdegreeOfFreedom,
                freedom2:=YdegreeOfFreedom
            )

            Return single_tailed_pval
        End Get
    End Property

    ''' <summary>
    ''' double tailed p-value
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Field("double_tailed p-value")>
    Public ReadOnly Property PValue As Double
        Get
            Return SingleTailedPval * 2
        End Get
    End Property

    Sub New(x As Double(), y As Double())
        Me.x = x
        Me.y = y
    End Sub

    Public Overrides Function ToString() As String
        Dim reject As String = "true ratio of variances is not equal to 1"
        Dim accept As String = "true ratio of variances is equal to 1"

        Return $"

	F test to compare two variances

data:  x and y
F = {F}, num df = {XdegreeOfFreedom}, denom df = {YdegreeOfFreedom}, p-value = {PValue}
alternative hypothesis: {If(PValue < 0.05, reject, accept)}
95 percent confidence interval:
  1.089699 17.662528
sample estimates:
ratio of variances 
          {F}
"
    End Function
End Class
