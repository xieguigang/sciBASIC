Namespace Microsoft.VisualBasic.Math.Scripting

    ''' <summary>绘图类型</summary>
    Public Enum PlotKind
        Scatter
        Line
        Surface
    End Enum

    ''' <summary>
    ''' 一条绘图指令（纯数据，不引用任何绘图组件）。
    ''' 由数学脚本引擎产出，供可视化程序解释渲染。
    ''' </summary>
    Public Class PlotCommand
        Public Kind As PlotKind = PlotKind.Scatter
        Public X As Double() = {}
        Public Y As Double() = {}
        ''' <summary>三维散点/曲线可选；二维时为 Nothing</summary>
        Public Z As Double() = Nothing
        ''' <summary>曲面：ZGrid(i)(j)，i 沿 Y 轴、j 沿 X 轴</summary>
        Public ZGrid As Double()() = Nothing
        Public Scheme As String = "viridis"
        Public Label As String = ""

        Public ReadOnly Property Is3D As Boolean
            Get
                If Kind = PlotKind.Surface Then Return True
                Return Z IsNot Nothing
            End Get
        End Property
    End Class

    ''' <summary>脚本执行结果</summary>
    Public Class ScriptResult
        Public Variables As New Dictionary(Of String, Object)()
        Public Commands As New List(Of PlotCommand)()
        Public ErrorMessage As String = ""
        Public Success As Boolean = True
        Public Line As Integer = -1
    End Class

End Namespace
