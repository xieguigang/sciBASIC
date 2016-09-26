Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.diffEq

Namespace MonteCarlo

    ''' <summary>
    ''' 计算步骤
    ''' 
    ''' 1. 继承<see cref="Model"/>对象并实现具体的过程
    ''' 2. 设置好大概的参数的变化区间
    ''' 3. 设置好大概的函数初始值的变化区间
    ''' </summary>
    Public Class Example : Inherits Model

        Dim sin As var
        Dim a As Double
        Dim f As Double

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(sin) = a * Math.Sin(dx) + f
        End Sub

        Public Overrides Function params() As NamedValue(Of PreciseRandom)()
            Return {
                New NamedValue(Of PreciseRandom)(NameOf(a), New PreciseRandom(-1, 1)),
                New NamedValue(Of PreciseRandom)(NameOf(f), New PreciseRandom(-1, 1))
            }
        End Function

        Public Overrides Function yinit() As NamedValue(Of PreciseRandom)()
            Return {
                New NamedValue(Of PreciseRandom)(NameOf(sin), New PreciseRandom(-1, 2))
            }
        End Function

        Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function
    End Class
End Namespace