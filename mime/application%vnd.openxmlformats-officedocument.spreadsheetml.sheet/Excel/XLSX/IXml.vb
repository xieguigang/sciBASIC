#Region "Microsoft.VisualBasic::a1cf05a19b4d3b8dba1a4275bc5340d7, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IXml.vb"

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

    '   Total Lines: 23
    '    Code Lines: 16 (69.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (30.43%)
    '     File Size: 571 B


    '     Interface IXml
    ' 
    '         Function: filePath, toXml
    ' 
    '     Module XMLExtensions
    ' 
    '         Function: WriteXml
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace XLSX

    Public Interface IXml

        Function filePath() As String
        Function toXml() As String

    End Interface

    <HideModuleName> Module XMLExtensions

        <Extension>
        Public Function WriteXml(xmlObj As IXml, dir$) As Boolean
            Dim path$ = dir & "/" & xmlObj.filePath()
            Dim xml$ = xmlObj.ToXML()

            Return xml.SaveTo(path, TextEncodings.UTF8WithoutBOM)
        End Function
    End Module
End Namespace
