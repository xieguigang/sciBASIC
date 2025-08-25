#Region "Microsoft.VisualBasic::90047379c4788082f0d12f5454c90286, Data\BinaryData\DataStorage\test\Module1.vb"

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

'   Total Lines: 75
'    Code Lines: 50 (66.67%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 25 (33.33%)
'     File Size: 2.40 KB


' Module Module1
' 
'     Sub: Main, summary, testReaderDump, testWriter
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.Text

Module Module1

    Sub summary()
        Dim file As New netCDFReader("D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\build_tools\CVD_kb\duke\2019-1-11test\TrainingSet_ANN_trained.encouraged.debugger.CDF", Encodings.UTF8WithoutBOM)

        Call file.ToString.SaveTo("./testANN_debugger_summary.txt")

        Pause()
    End Sub

    Sub Main()
        Call summary()

        Call testReaderDump()
        Call testWriter()
    End Sub

    Sub testWriter()
        Dim path$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\16s_contents\SCFA\SCFA测试标曲.AIA\5ppm.CDF"
        Dim file As New netCDFReader(path, Encodings.UTF8WithoutBOM)

        Using writer As New CDFWriter("./test.cdf")

            Call writer.GlobalAttributes(file.globalAttributes).Dimensions(file.dimensions)

            Dim dataPackage As ICDFDataVector

            For Each var In file.variables
                If var.name = "mass_values" Then
                    Console.WriteLine()
                End If

                dataPackage = file.getDataVariable(var)
                writer.AddVariable(
                    var.name,
                    dataPackage,
                    var.dimensions.Select(Function(i) file.dimensions(i)).ToArray,
                    file.globalAttributes
                )
            Next

        End Using

        file = New netCDFReader("./test.cdf", Encodings.UTF8WithoutBOM)



        Call file.ToString.__DEBUG_ECHO
        Call file.ToString.SaveTo("./dump-writer.txt")

        Dim massvalue = file.getDataVariable("mass_values")
        Dim scans = file.getDataVariable("actual_scan_number")

        Call Data.Xml.SaveAsXml(file, "./output_dump-writer.Xml")

        Pause()
    End Sub

    Sub testReaderDump()
        Dim path$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\16s_contents\SCFA\SCFA测试标曲.AIA\5ppm.CDF"
        Dim file As New netCDFReader(path, Encodings.UTF8WithoutBOM)

        Call file.ToString.__DEBUG_ECHO
        Call file.ToString.SaveTo("./dump.txt")

        Dim massvalue = file.getDataVariable("mass_values")

        Call Data.Xml.SaveAsXml(file, "./output_dump.Xml")
    End Sub

End Module
