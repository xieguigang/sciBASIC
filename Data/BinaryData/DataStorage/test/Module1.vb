#Region "Microsoft.VisualBasic::6d6da79b639cd3111267fb5fd95b5005, Data\BinaryData\DataStorage\test\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: Main, summary, testReaderDump, testWriter
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.netCDF
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

            Dim dataPackage As Components.CDFData

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

        Call Xml.SaveAsXml(file, "./output_dump-writer.Xml")

        Pause()
    End Sub

    Sub testReaderDump()
        Dim path$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\16s_contents\SCFA\SCFA测试标曲.AIA\5ppm.CDF"
        Dim file As New netCDFReader(path, Encodings.UTF8WithoutBOM)

        Call file.ToString.__DEBUG_ECHO
        Call file.ToString.SaveTo("./dump.txt")

        Dim massvalue = file.getDataVariable("mass_values")

        Call Xml.SaveAsXml(file, "./output_dump.Xml")
    End Sub

End Module
