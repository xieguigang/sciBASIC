Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace MonteCarlo

    Public MustInherit Class Model : Inherits ODEs

        ''' <summary>
        ''' 系统的初始值列表
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function yinit() As NamedValue(Of PreciseRandom)()
        ''' <summary>
        ''' 系统的状态列表，即方程里面的参数
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function params() As NamedValue(Of PreciseRandom)()
        Public MustOverride Function eigenvector() As Dictionary(Of String, Eigenvector)

        Protected NotOverridable Overrides Function y0() As var()
            Return {}
        End Function
    End Class
End Namespace