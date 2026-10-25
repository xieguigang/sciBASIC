''' <summary>
''' 双向（左右镜像）条形图的数据模型：两个因子 + 每个样本在这两个因子上的取值。
''' </summary>
''' <remarks>
''' 旧实现里这个类型位于 <c>Microsoft.VisualBasic.Data.Plots.Legacy.BarPlot.Data</c>，
''' 删除 Legacy 绘图代码时把它一并上移到绘图引擎的数据模型层
''' （与 <see cref="BarDataSample"/> / <see cref="BarDataGroup"/> 同级）。
''' </remarks>
Public Class BiDirectionData

    ''' <summary>向左（下）延伸的因子名</summary>
    ''' <returns></returns>
    Public Property Factor1 As String

    ''' <summary>向右（上）延伸的因子名</summary>
    ''' <returns></returns>
    Public Property Factor2 As String

    ''' <summary>数据样本；每个样本的 <see cref="BarDataSample.data"/> 长度应为 2（依次对应 Factor1 / Factor2）</summary>
    ''' <returns></returns>
    Public Property samples As BarDataSample()

    ''' <summary>样本数量</summary>
    Public ReadOnly Property size As Integer
        Get
            Return If(samples Is Nothing, 0, samples.Length)
        End Get
    End Property

    ''' <summary>按索引读取样本</summary>
    ''' <param name="i"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property data(i As Integer) As BarDataSample
        Get
            Return samples(i)
        End Get
    End Property
End Class
