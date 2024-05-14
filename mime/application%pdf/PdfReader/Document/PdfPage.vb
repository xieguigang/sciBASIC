#Region "Microsoft.VisualBasic::3911dde88f511ae53cba65005100d7c2, mime\application%pdf\PdfReader\Document\PdfPage.vb"

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

    '   Total Lines: 195
    '    Code Lines: 160
    ' Comment Lines: 0
    '   Blank Lines: 35
    '     File Size: 6.01 KB


    '     Class PdfPage
    ' 
    '         Properties: AA, Annots, ArtBox, B, BleedBox
    '                     BoxColorInfo, Contents, CropBox, Dur, Group
    '                     ID, LastModified, MediaBox, Metadata, PieceInfo
    '                     PresSteps, PZ, Resources, Rotate, SeparationInfo
    '                     StructParents, Tabs, TemplateInstantiated, Thumb, Trans
    '                     TrimBox, UserUnit, VP
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: FindLeafPages, Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfPage
        Inherits PdfPageInherit

        Private _contents As PdfContents

        Public Sub New(dictionary As PdfDictionary)
            MyBase.New(dictionary.Parent, dictionary.ParseDictionary)
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Overrides Sub FindLeafPages(pages As List(Of PdfPage))
            pages.Add(Me)
        End Sub

        Public ReadOnly Property LastModified As PdfDateTime
            Get
                Return OptionalDateTime("LastModified")
            End Get
        End Property

        Public ReadOnly Property Resources As PdfDictionary
            Get
                Return InheritableMandatoryRefValue(Of PdfDictionary)("Resources")
            End Get
        End Property

        Public ReadOnly Property MediaBox As PdfRectangle
            Get
                Return ArrayToRectangle(InheritableMandatoryValue(Of PdfArray)("MediaBox"))
            End Get
        End Property

        Public ReadOnly Property CropBox As PdfRectangle
            Get
                Return ArrayToRectangle(InheritableOptionalValue(Of PdfArray)("CropBox"))
            End Get
        End Property

        Public ReadOnly Property BleedBox As PdfRectangle
            Get
                Return ArrayToRectangle(OptionalValue(Of PdfArray)("BleedBox"))
            End Get
        End Property

        Public ReadOnly Property TrimBox As PdfRectangle
            Get
                Return ArrayToRectangle(OptionalValue(Of PdfArray)("TrimBox"))
            End Get
        End Property

        Public ReadOnly Property ArtBox As PdfRectangle
            Get
                Return ArrayToRectangle(OptionalValue(Of PdfArray)("ArtBox"))
            End Get
        End Property

        Public ReadOnly Property BoxColorInfo As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("BoxColorInfo")
            End Get
        End Property

        Public ReadOnly Property Contents As PdfContents
            Get

                If _contents Is Nothing Then
                    Dim obj = InheritableMandatoryValue(Of PdfObject)("Contents")
                    _contents = New PdfContents(Me, obj)
                End If

                Return _contents
            End Get
        End Property

        Public ReadOnly Property Rotate As PdfInteger
            Get
                Return InheritableOptionalValue(Of PdfInteger)("Rotate")
            End Get
        End Property

        Public ReadOnly Property Group As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("Group")
            End Get
        End Property

        Public ReadOnly Property Thumb As PdfStream
            Get
                Return OptionalValue(Of PdfStream)("Thumb")
            End Get
        End Property

        Public ReadOnly Property B As PdfArray
            Get
                Return OptionalValue(Of PdfArray)("B")
            End Get
        End Property

        Public ReadOnly Property Dur As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("Dur")
            End Get
        End Property

        Public ReadOnly Property Trans As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("Trans")
            End Get
        End Property

        Public ReadOnly Property Annots As PdfArray
            Get
                Return OptionalValue(Of PdfArray)("Annots")
            End Get
        End Property

        Public ReadOnly Property AA As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("AA")
            End Get
        End Property

        Public ReadOnly Property Metadata As PdfStream
            Get
                Return OptionalValue(Of PdfStream)("Metadata")
            End Get
        End Property

        Public ReadOnly Property PieceInfo As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("PieceInfo")
            End Get
        End Property

        Public ReadOnly Property StructParents As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("StructParents")
            End Get
        End Property

        Public ReadOnly Property ID As PdfString
            Get
                Return OptionalValue(Of PdfString)("ID")
            End Get
        End Property

        Public ReadOnly Property PZ As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("PZ")
            End Get
        End Property

        Public ReadOnly Property SeparationInfo As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("SeparationInfo")
            End Get
        End Property

        Public ReadOnly Property Tabs As PdfName
            Get
                Return OptionalValue(Of PdfName)("Tabs")
            End Get
        End Property

        Public ReadOnly Property TemplateInstantiated As PdfName
            Get
                Return OptionalValue(Of PdfName)("TemplateInstantiated")
            End Get
        End Property

        Public ReadOnly Property PresSteps As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("PresSteps")
            End Get
        End Property

        Public ReadOnly Property UserUnit As PdfInteger
            Get
                Return OptionalValue(Of PdfInteger)("UserUnit")
            End Get
        End Property

        Public ReadOnly Property VP As PdfDictionary
            Get
                Return OptionalValue(Of PdfDictionary)("VP")
            End Get
        End Property
    End Class
End Namespace
