#Region "Microsoft.VisualBasic::86cbb85af3f0be68ec362545f795996d, mime\application%pdf\PdfReader\Document\IPdfObjectVisitor.vb"

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

    '   Total Lines: 30
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 0
    '     File Size: 1.21 KB


    '     Interface IPdfObjectVisitor
    ' 
    '         Sub: (+26 Overloads) Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Interface IPdfObjectVisitor
        Sub Visit(array As PdfArray)
        Sub Visit([boolean] As PdfBoolean)
        Sub Visit(contents As PdfCatalog)
        Sub Visit(contents As PdfContents)
        Sub Visit(dateTime As PdfDateTime)
        Sub Visit(dateTime As PdfDictionary)
        Sub Visit(document As PdfDocument)
        Sub Visit(identifier As PdfIdentifier)
        Sub Visit([integer] As PdfInteger)
        Sub Visit(info As PdfInfo)
        Sub Visit(indirectObject As PdfIndirectObject)
        Sub Visit(name As PdfName)
        Sub Visit(nameTree As PdfNameTree)
        Sub Visit(nul As PdfNull)
        Sub Visit(numberTree As PdfNumberTree)
        Sub Visit(obj As PdfObject)
        Sub Visit(reference As PdfObjectReference)
        Sub Visit(outlineItem As PdfOutlineItem)
        Sub Visit(outlineLevel As PdfOutlineLevel)
        Sub Visit(page As PdfPage)
        Sub Visit(pages As PdfPages)
        Sub Visit(real As PdfReal)
        Sub Visit(rectangle As PdfRectangle)
        Sub Visit(stream As PdfStream)
        Sub Visit(str As PdfString)
        Sub Visit(version As PdfVersion)
    End Interface
End Namespace
