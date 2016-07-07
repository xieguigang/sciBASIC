#Region "Microsoft.VisualBasic::729ffa262a8fd30d96c242dcca4b955f, ..\VisualBasic_AppFramework\UXFramework\Molk+\WindowsApplication1\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Module Module1
    Sub Main()
        Dim doc = Microsoft.VisualBasic.DocumentFormat.HTML.HtmlDocument.Load("G:\Microsoft.VisualBasic_Framework\UXFramework\Molk+\WindowsApplication1\bin\Debug\testDialog.htm")
        doc = Microsoft.VisualBasic.DocumentFormat.HTML.HtmlDocument.Load("G:\Microsoft.VisualBasic_Framework\UXFramework\Molk+\WindowsApplication1\bin\Debug\Microsoft.VisualBasic.DocumentFormat.HTML.xml")
    End Sub
End Module

