#Region "Microsoft.VisualBasic::cb2f4943b96f8ce1cdf61688e8f0c598, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\XlsxDirectoryPart\docProps.vb"

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

    '   Total Lines: 44
    '    Code Lines: 33 (75.00%)
    ' Comment Lines: 3 (6.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (18.18%)
    '     File Size: 1.75 KB


    '     Class docProps
    ' 
    '         Properties: app, core, custom
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _name
    ' 
    '         Sub: _loadContents, addSheetName, save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.docProps
Imports Microsoft.VisualBasic.Text

Namespace XLSX.Model.Directory

    Public Class docProps : Inherits XlsxDirectoryPart

        Public Property core As core
        Public Property app As XML.docProps.app
        Public Property custom As custom

        Sub New(workdir$)
            Call MyBase.New(workdir)
        End Sub

        ''' <summary>
        ''' 有些文件可能是会不存在的，所以在这里就不抛出错误了，直接返回Nothing
        ''' </summary>
        Protected Overrides Sub _loadContents()
            core = (folder & "/core.xml").LoadXml(Of core)(throwEx:=False)
            custom = (folder & "/custom.xml").LoadXml(Of custom)(throwEx:=False)
            app = (folder & "/app.xml").LoadXml(Of XML.docProps.app)(throwEx:=False)
        End Sub

        Public Sub addSheetName(sheetName As String)
            app.TitlesOfParts.vector.lpstrs = app.TitlesOfParts.vector.lpstrs _
                .JoinIterates({sheetName}) _
                .ToArray
            app.TitlesOfParts.vector.size = app.TitlesOfParts.vector.size + 1
        End Sub

        Public Sub save()
            Call core.GetXml.SaveTo($"{folder}/core.xml", encoding:=Encodings.UTF8WithoutBOM.CodePage)
            Call app.GetXml.SaveTo($"{folder}/app.xml", encoding:=Encodings.UTF8WithoutBOM.CodePage)
            Call custom.GetXml.SaveTo($"{folder}/custom.xml", encoding:=Encodings.UTF8WithoutBOM.CodePage)
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(docProps)
        End Function
    End Class
End Namespace
