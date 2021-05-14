#Region "Microsoft.VisualBasic::61bf1ed67d5a304c4fa76d806eedffba, mime\application%pdf\PdfReader\Document\IPdfObjectVisitor.vb"

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

    '     Interface IPdfObjectVisitor
    ' 
    '         Sub: (+26 Overloads) Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace PdfReader
    Public Interface IPdfObjectVisitor
        Sub Visit(ByVal array As PdfArray)
        Sub Visit(ByVal [boolean] As PdfBoolean)
        Sub Visit(ByVal contents As PdfCatalog)
        Sub Visit(ByVal contents As PdfContents)
        Sub Visit(ByVal dateTime As PdfDateTime)
        Sub Visit(ByVal dateTime As PdfDictionary)
        Sub Visit(ByVal document As PdfDocument)
        Sub Visit(ByVal identifier As PdfIdentifier)
        Sub Visit(ByVal [integer] As PdfInteger)
        Sub Visit(ByVal info As PdfInfo)
        Sub Visit(ByVal indirectObject As PdfIndirectObject)
        Sub Visit(ByVal name As PdfName)
        Sub Visit(ByVal nameTree As PdfNameTree)
        Sub Visit(ByVal nul As PdfNull)
        Sub Visit(ByVal numberTree As PdfNumberTree)
        Sub Visit(ByVal obj As PdfObject)
        Sub Visit(ByVal reference As PdfObjectReference)
        Sub Visit(ByVal outlineItem As PdfOutlineItem)
        Sub Visit(ByVal outlineLevel As PdfOutlineLevel)
        Sub Visit(ByVal page As PdfPage)
        Sub Visit(ByVal pages As PdfPages)
        Sub Visit(ByVal real As PdfReal)
        Sub Visit(ByVal rectangle As PdfRectangle)
        Sub Visit(ByVal stream As PdfStream)
        Sub Visit(ByVal str As PdfString)
        Sub Visit(ByVal version As PdfVersion)
    End Interface
End Namespace

