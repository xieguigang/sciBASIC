#Region "Microsoft.VisualBasic::cd26317685d5d653fae86884f993792f, Data\BinaryData\HDF5\test\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 36
    '    Code Lines: 25 (69.44%)
    ' Comment Lines: 1 (2.78%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (27.78%)
    '     File Size: 1.37 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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
        Console.WriteLine("全部诊断完成。")
    End Sub

End Module
