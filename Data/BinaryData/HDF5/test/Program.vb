Imports System.IO

Module Program

    Sub Main(args As String())
        Dim files As New List(Of String) From {
            "C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_molecule_info.h5",
            "C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_feature_slice.h5"
        }

        Dim outDir As String = Path.Combine(Path.GetTempPath(), "hdf5_diag")
        Directory.CreateDirectory(outDir)

        For Each f In files
            Console.WriteLine()
            Console.WriteLine("################################################################")
            Console.WriteLine("诊断文件: " & f)
            Console.WriteLine("################################################################")

            Dim report = test.Hdf5Diagnostics.Diagnose(f, maxSampleElements:=12, largeDatasetThreshold:=1000000L)

            Dim outPath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(f) & "_report.txt")
            report.WriteToFile(outPath)

            ' 控制台只打印摘要与失败项，完整内容见落盘文件
            Console.WriteLine(report.Render())

            Console.WriteLine()
            Console.WriteLine("完整报告已写入: " & outPath)
        Next

        Console.WriteLine()
        Console.WriteLine("全部诊断完成。按任意键退出。")
        Console.ReadKey()
    End Sub

End Module
