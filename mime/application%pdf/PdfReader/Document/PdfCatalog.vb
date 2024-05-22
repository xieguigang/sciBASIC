#Region "Microsoft.VisualBasic::71ef5ad0667f2c0d2dd1be40af3dfefc, mime\application%pdf\PdfReader\Document\PdfCatalog.vb"

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

    '   Total Lines: 225
    '    Code Lines: 178 (79.11%)
    ' Comment Lines: 3 (1.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 44 (19.56%)
    '     File Size: 7.34 KB


    '     Class PdfCatalog
    ' 
    '         Properties: AA, AcroForm, Collection, Dests, Lang
    '                     Legal, MarkInfo, Metadata, Names, NeedsRendering
    '                     OCProperties, OpenAction, Outlines, OutputIntents, PageLabels
    '                     PageLayout, PageMode, Pages, Perms, PieceInfo
    '                     Requirements, RootPage, SpiderInfo, StructTreeRoot, Threads
    '                     URI, Version, ViewerPreferences
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfCatalog
        Inherits PdfDictionary

        Private _pages As List(Of PdfPage)
        Private _rootPage As PdfPages
        Private _pageLabels As PdfNumberTree
        Private _outlineRoot As PdfOutlineLevel
        Private _structTreeRoot As PdfStructTreeRoot

        Public Sub New(parent As PdfObject, dictionary As ParseDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Version As PdfName
            Get
                Return OptionalValue(Of PdfName)("Version")
            End Get
        End Property

        Public ReadOnly Property Pages As List(Of PdfPage)
            Get

                If _rootPage Is Nothing Then
                    ' Accessing the RootPage will cause the pages to be loaded
                    Dim temp = RootPage
                End If

                Return _pages
            End Get
        End Property

        Public ReadOnly Property RootPage As PdfPages
            Get

                If _rootPage Is Nothing Then
                    Dim dictionary = MandatoryValueRef(Of PdfDictionary)("Pages")

                    ' Page tree construct the hierarchy so that inheritance of properties works correctly
                    _rootPage = New PdfPages(dictionary)

                    ' Flatten the hierarchy into a list of pages, this is more useful for the user
                    _pages = New List(Of PdfPage)()
                    _rootPage.FindLeafPages(_pages)
                End If

                Return _rootPage
            End Get
        End Property

        Public ReadOnly Property PageLabels As PdfNumberTree
            Get

                If _pageLabels Is Nothing Then
                    Dim dictionary = OptionalValueRef(Of PdfDictionary)("PageLabels")
                    If dictionary IsNot Nothing Then _pageLabels = New PdfNumberTree(dictionary)
                End If

                Return _pageLabels
            End Get
        End Property

        Public ReadOnly Property Names As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("Names")
            End Get
        End Property

        Public ReadOnly Property Dests As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("Dests")
            End Get
        End Property

        Public ReadOnly Property ViewerPreferences As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("ViewerPreferences")
            End Get
        End Property

        Public ReadOnly Property PageLayout As PdfName
            Get
                Return OptionalValueRef(Of PdfName)("PageLayout")
            End Get
        End Property

        Public ReadOnly Property PageMode As PdfName
            Get
                Return OptionalValueRef(Of PdfName)("PageMode")
            End Get
        End Property

        Public ReadOnly Property Outlines As PdfOutlineLevel
            Get

                If _outlineRoot Is Nothing Then
                    Dim dictionary = OptionalValueRef(Of PdfDictionary)("Outlines")
                    If dictionary IsNot Nothing Then _outlineRoot = New PdfOutlineLevel(Me, dictionary)
                End If

                Return _outlineRoot
            End Get
        End Property

        Public ReadOnly Property Threads As PdfArray
            Get
                Return OptionalValueRef(Of PdfArray)("Threads")
            End Get
        End Property

        Public ReadOnly Property OpenAction As PdfObject
            Get
                Return OptionalValueRef(Of PdfObject)("OpenAction")
            End Get
        End Property

        Public ReadOnly Property AA As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("AA")
            End Get
        End Property

        Public ReadOnly Property URI As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("URI")
            End Get
        End Property

        Public ReadOnly Property AcroForm As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("AcroForm")
            End Get
        End Property

        Public ReadOnly Property Metadata As PdfStream
            Get
                Return OptionalValueRef(Of PdfStream)("Metadata")
            End Get
        End Property

        Public ReadOnly Property StructTreeRoot As PdfStructTreeRoot
            Get

                If _structTreeRoot Is Nothing Then
                    Dim dictionary = OptionalValueRef(Of PdfDictionary)("StructTreeRoot")
                    If dictionary IsNot Nothing Then _structTreeRoot = New PdfStructTreeRoot(Me, dictionary.ParseDictionary)
                End If

                Return _structTreeRoot
            End Get
        End Property

        Public ReadOnly Property MarkInfo As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("MarkInfo")
            End Get
        End Property

        Public ReadOnly Property Lang As PdfString
            Get
                Return OptionalValueRef(Of PdfString)("Lang")
            End Get
        End Property

        Public ReadOnly Property SpiderInfo As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("SpiderInfo")
            End Get
        End Property

        Public ReadOnly Property OutputIntents As PdfArray
            Get
                Return OptionalValueRef(Of PdfArray)("OutputIntents")
            End Get
        End Property

        Public ReadOnly Property PieceInfo As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("PieceInfo")
            End Get
        End Property

        Public ReadOnly Property OCProperties As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("OCProperties")
            End Get
        End Property

        Public ReadOnly Property Perms As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("Perms")
            End Get
        End Property

        Public ReadOnly Property Legal As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("Legal")
            End Get
        End Property

        Public ReadOnly Property Requirements As PdfArray
            Get
                Return OptionalValueRef(Of PdfArray)("Requirements")
            End Get
        End Property

        Public ReadOnly Property Collection As PdfDictionary
            Get
                Return OptionalValueRef(Of PdfDictionary)("Collection")
            End Get
        End Property

        Public ReadOnly Property NeedsRendering As PdfBoolean
            Get
                Return OptionalValueRef(Of PdfBoolean)("NeedsRendering")
            End Get
        End Property
    End Class
End Namespace
