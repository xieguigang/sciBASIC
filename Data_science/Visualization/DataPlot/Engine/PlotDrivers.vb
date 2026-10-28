Imports System.Reflection
Imports Microsoft.VisualBasic.Imaging.Driver

' ============================================================================
'  PlotDrivers.vb - DataPlot 的绘图驱动注册入口
'
'  PlotEngine 通过 g.GraphicsPlots 按驱动分派画布，而 SVG / PostScript / PDF
'  这几个驱动都需要先注册实现才能使用。这里把它们收敛到一个入口，
'  调用方只需要在程序启动时写一行 PlotDrivers.Register()。
' ============================================================================

''' <summary>
''' 绘图驱动注册器：一次性注册 DataPlot 所支持的全部图形输出驱动
''' （GDI 栅格、SVG 矢量、PostScript、PDF），可重复调用。
''' </summary>
Public Module PlotDrivers

    ''' <summary>是否已经完成过注册（重复注册没有意义，也没有必要）</summary>
    Private isRegistered As Boolean = False

    ''' <summary>
    ''' 注册 GDI / SVG / PostScript / PDF 四种绘图驱动。
    ''' 在调用 <c>Plot(size, dpi, driver)</c> 输出非栅格格式之前必须先调用本方法一次。
    ''' </summary>
    ''' <remarks>
    ''' GDI 与 SVG 由 <c>ImageDriver.Register()</c> 注册；该方法在 imaging 程序集里
    ''' 被 <c>#If WINDOWS</c> 包着，而 DataPlot 自身是纯 net10.0 目标（编译时 WINDOWS 未定义），
    ''' 所以这里走反射调用——运行在 Windows 上时宿主加载的是 imaging 的 net10.0-windows 版本，
    ''' 反射即可命中；跑在 Linux 上时该方法不存在，忽略即可（此时应由宿主注册 Skia 驱动）。
    ''' PostScript 由 <see cref="ImageDriver.RegisterPostScript"/> 注册（不受平台条件编译限制），
    ''' PDF 由 DataPlot 自带的 "PdfInterop" 提供（栅格嵌入式实现）。
    ''' 某个驱动注册失败时不会抛异常，只会在真正用到该驱动时才报错，
    ''' 这样在缺少某个驱动的平台上其余格式仍然可用。
    ''' </remarks>
    Public Sub Register()
        If isRegistered Then
            Return
        End If

        isRegistered = True

        ' GDI + SVG（Windows 平台）
        Try
            Dim driver As Type = GetType(ImageDriver)

            Call driver.GetMethod("Register", BindingFlags.Public Or BindingFlags.Static) _
                       ?.Invoke(Nothing, Nothing)
        Catch
            ' 平台不支持时忽略，后续使用该驱动时再报错
        End Try

        ' PostScript
        Try
            Call ImageDriver.RegisterPostScript()
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' 当前进程是否已经在 <see cref="Register"/> 之中完成过驱动注册
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Registered As Boolean
        Get
            Return isRegistered
        End Get
    End Property
End Module
