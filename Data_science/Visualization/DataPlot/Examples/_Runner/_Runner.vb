Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Driver

' 临时运行入口：注册 Windows GDI+ 光栅驱动后批量生成示例 PNG（验证完成后删除整个 _Runner 目录）
Module Program
    Sub Main()
        ImageDriver.Register()

        Dim outDir = Path.Combine(
            "g:\pixelArtist\src\framework\Data_science\Visualization\DataPlot\Examples\Output")
        ChartExamples.RunAll(outDir)
    End Sub
End Module
