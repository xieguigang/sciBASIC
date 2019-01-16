#Region "Microsoft.VisualBasic::11e27225a444861ad4ff070f0c7d433e, mime\application%netcdf\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.netCDF
Imports Microsoft.VisualBasic.Text

Module Module1

    Sub Main()
        Call testReaderDump()
    End Sub

    Sub testReaderDump()
        Dim path$ = "D:\biodeep\biodeepDB\smartnucl_integrative\16s_contents\SCFA\SCFA测试标曲.AIA\100ppb.CDF"
        Dim file As New netCDFReader(path, Encodings.UTF8WithoutBOM)

        Call file.ToString.__DEBUG_ECHO
        Call file.ToString.SaveTo("./dump.txt")

        Dim massvalue = file.getDataVariable("mass_values")

        Call Xml.SaveAsXml(file, "./output_dump.Xml")


        Pause()
    End Sub

End Module
