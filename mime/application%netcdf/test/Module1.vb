Imports Microsoft.VisualBasic.MIME.application.netCDF

Module Module1

    Sub Main()
        Dim path$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\16s_contents\SCFA\scfa200ppmAIAEXPRT.AIA\200ppm-未处理.CDF"
        Dim file As New netCDFReader(path)

        Call file.ToString.__DEBUG_ECHO

        Pause()
    End Sub

End Module
