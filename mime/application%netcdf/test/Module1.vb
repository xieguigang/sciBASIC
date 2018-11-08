Imports Microsoft.VisualBasic.MIME.application.netCDF
Imports Microsoft.VisualBasic.Text

Module Module1

    Sub Main()
        Dim path$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\16s_contents\SCFA\scfa200ppmAIAEXPRT.AIA\200ppm-未处理.CDF"
        Dim file As New netCDFReader(path, Encodings.UTF8WithoutBOM)

        Call file.ToString.__DEBUG_ECHO

        Dim massvalue = file.getDataVariable("mass_values")

        Pause()
    End Sub

End Module
