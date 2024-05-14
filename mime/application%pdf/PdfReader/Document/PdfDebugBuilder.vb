#Region "Microsoft.VisualBasic::c61ca96a5f5ab3988e3558ad0900b454, mime\application%pdf\PdfReader\Document\PdfDebugBuilder.vb"

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

    '   Total Lines: 389
    '    Code Lines: 322
    ' Comment Lines: 2
    '   Blank Lines: 65
    '     File Size: 13.91 KB


    '     Class PdfDebugBuilder
    ' 
    '         Properties: Document, Resolve, StreamContent
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Append, AppendObject, CurrentLevelNewLine, PopLevel, PushLevel
    '              PushNextLevel, (+26 Overloads) Visit, (+2 Overloads) VisitNotNull
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace PdfReader
    Public Class PdfDebugBuilder
        Implements IPdfObjectVisitor

        Private _obj As PdfObject
        Private _sb As StringBuilder = New StringBuilder()
        Private _indents As Stack(Of Integer) = New Stack(Of Integer)()
        Private _index As Integer

        Public Sub New()
            Me.New(Nothing)
        End Sub

        Public Sub New(obj As PdfObject)
            _obj = obj
        End Sub

        Public Overrides Function ToString() As String
            If _obj IsNot Nothing Then
                _index = 0
                _indents = New Stack(Of Integer)()
                _indents.Push(_index)
                _sb = New StringBuilder()
                _obj.Visit(Me)
                Return _sb.ToString()
            End If

            Return String.Empty
        End Function

        Public Property Document As PdfDocument
        Public Property Resolve As Boolean
        Public Property StreamContent As Boolean

        Public Sub Visit(array As PdfArray) Implements IPdfObjectVisitor.Visit
            Append("[")
            Dim first = True

            For Each child In array.Objects

                If Not first Then
                    Append(" ")
                Else
                    first = False
                End If

                child.Visit(Me)
            Next

            Append("]")
        End Sub

        Public Sub Visit([boolean] As PdfBoolean) Implements IPdfObjectVisitor.Visit
            Append([boolean])
        End Sub

        Public Sub Visit(catalog As PdfCatalog) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Catalog")
            CurrentLevelNewLine()
            catalog.RootPage.Visit(Me)
            VisitNotNull(catalog.PageLabels, "PageLabels")
            VisitNotNull(catalog.Names, "Names")
            VisitNotNull(catalog.Dests, "Dests")
            VisitNotNull(catalog.ViewerPreferences, "ViewerPreferences")
            VisitNotNull(catalog.PageLayout, "PageLayout")
            VisitNotNull(catalog.PageMode, "PageMode")
            VisitNotNull(catalog.Outlines, "Outlines")
            VisitNotNull(catalog.Threads, "Threads")
            VisitNotNull(catalog.OpenAction, "OpenAction")
            VisitNotNull(catalog.AA, "AA")
            VisitNotNull(catalog.URI, "URI")
            VisitNotNull(catalog.AcroForm, "AcroForm")
            VisitNotNull(catalog.Metadata, "Metadata")
            VisitNotNull(catalog.StructTreeRoot, "StructTreeRoot")
            VisitNotNull(catalog.MarkInfo, "MarkInfo")
            VisitNotNull(catalog.Lang, "Lang")
            VisitNotNull(catalog.SpiderInfo, "SpiderInfo")
            VisitNotNull(catalog.OutputIntents, "OutputIntents")
            VisitNotNull(catalog.PieceInfo, "PieceInfo")
            VisitNotNull(catalog.OCProperties, "OCProperties")
            VisitNotNull(catalog.Perms, "Perms")
            VisitNotNull(catalog.Legal, "Legal")
            VisitNotNull(catalog.Requirements, "Requirements")
            VisitNotNull(catalog.Collection, "Collection")
            VisitNotNull(catalog.NeedsRendering, "NeedsRendering")
            VisitNotNull(catalog.Version, "Version")
            PopLevel()
        End Sub

        Public Sub Visit(contents As PdfContents) Implements IPdfObjectVisitor.Visit
            If contents.Streams.Count > 1 Then
                PushNextLevel()
                Append("Multiple Streams")

                For Each stream In contents.Streams
                    CurrentLevelNewLine()
                    stream.Visit(Me)
                Next

                PopLevel()
            Else
                contents.Streams(0).Visit(Me)
            End If
        End Sub

        Public Sub Visit(dateTime As PdfDateTime) Implements IPdfObjectVisitor.Visit
            Append(dateTime)
        End Sub

        Public Sub Visit(dictionary As PdfDictionary) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("<<")
            Dim index = 0
            Dim newLine = False
            Dim count = dictionary.Count

            For Each entry In dictionary
                VisitNotNull(entry.Value, $"/{entry.Key}", newLine)
                If count > 1 Then newLine = True
                index += 1
            Next

            Append(">>")
            PopLevel()
        End Sub

        Public Sub Visit(document As PdfDocument) Implements IPdfObjectVisitor.Visit
            Me.Document = document
            PushNextLevel()
            Append("Document")
            VisitNotNull(document.Catalog)
            VisitNotNull(document.Info)
            VisitNotNull(document.Version, "Version")
            PopLevel()
            CurrentLevelNewLine()
        End Sub

        Public Sub Visit(identifier As PdfIdentifier) Implements IPdfObjectVisitor.Visit
            Append(identifier)
        End Sub

        Public Sub Visit(indirectObject As PdfIndirectObject) Implements IPdfObjectVisitor.Visit
            PushLevel()
            Append($"Id:{indirectObject.Id} Gen:{indirectObject.Gen} Offset:{indirectObject.Offset}")
            VisitNotNull(indirectObject.Child)
            CurrentLevelNewLine()
            PopLevel()
        End Sub

        Public Sub Visit(info As PdfInfo) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Info")
            VisitNotNull(info.Title, "Title")
            VisitNotNull(info.Author, "Author")
            VisitNotNull(info.Subject, "Subject")
            VisitNotNull(info.Keywords, "Keywords")
            VisitNotNull(info.Creator, "Creator")
            VisitNotNull(info.Producer, "Producer")
            VisitNotNull(info.CreationDate, "CreationDate")
            VisitNotNull(info.ModDate, "ModDate")
            VisitNotNull(info.Trapped, "Trapped")
            PopLevel()
        End Sub

        Public Sub Visit([integer] As PdfInteger) Implements IPdfObjectVisitor.Visit
            Append([integer])
        End Sub

        Public Sub Visit(name As PdfName) Implements IPdfObjectVisitor.Visit
            Append($"/{name}")
        End Sub

        Public Sub Visit(nameTree As PdfNameTree) Implements IPdfObjectVisitor.Visit
            Append($"NameTree {nameTree.LimitMin} -> {nameTree.LimitMax}")
        End Sub

        Public Sub Visit(nul As PdfNull) Implements IPdfObjectVisitor.Visit
            Append(nul)
        End Sub

        Public Sub Visit(numberTree As PdfNumberTree) Implements IPdfObjectVisitor.Visit
            Append($"NumberTree {numberTree.LimitMin} -> {numberTree.LimitMax}")
        End Sub

        Public Sub Visit(obj As PdfObject) Implements IPdfObjectVisitor.Visit
            Append(obj.ToString())
        End Sub

        Public Sub Visit(reference As PdfObjectReference) Implements IPdfObjectVisitor.Visit
            Append($"{reference.Id} {reference.Gen} R")

            If Resolve AndAlso Document IsNot Nothing Then
                Dim obj = Document.ResolveReference(reference)

                If obj IsNot Nothing Then
                    Append(" ")
                    obj.Visit(Me)
                End If
            End If
        End Sub

        Public Sub Visit(outlineItem As PdfOutlineItem) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Item")
            VisitNotNull(outlineItem.Title, "Title")
            VisitNotNull(outlineItem.Dest, "Dest")
            VisitNotNull(outlineItem.A, "A")
            VisitNotNull(outlineItem.SE, "SE")
            VisitNotNull(outlineItem.C, "C")
            VisitNotNull(outlineItem.F, "F")

            For Each item In outlineItem.Items
                CurrentLevelNewLine()
                item.Visit(Me)
            Next

            PopLevel()
        End Sub

        Public Sub Visit(outlineLevel As PdfOutlineLevel) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Level")

            For Each item In outlineLevel.Items
                CurrentLevelNewLine()
                item.Visit(Me)
            Next

            PopLevel()
        End Sub

        Public Sub Visit(page As PdfPage) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Page")
            VisitNotNull(page.LastModified, "LastModified")
            VisitNotNull(page.Resources, "Resources")
            VisitNotNull(page.MediaBox, "MediaBox")
            VisitNotNull(page.CropBox, "CropBox")
            VisitNotNull(page.BleedBox, "BleedBox")
            VisitNotNull(page.TrimBox, "TrimBox")
            VisitNotNull(page.ArtBox, "ArtBox")
            VisitNotNull(page.BoxColorInfo, "BoxColorInfo")
            VisitNotNull(page.Contents, "Contents")
            VisitNotNull(page.Rotate, "Rotate")
            VisitNotNull(page.Group, "Group")
            VisitNotNull(page.Thumb, "Thumb")
            VisitNotNull(page.B, "B")
            VisitNotNull(page.Dur, "Dur")
            VisitNotNull(page.Trans, "Trans")
            VisitNotNull(page.Annots, "Annots")
            VisitNotNull(page.AA, "AA")
            VisitNotNull(page.Metadata, "Metadata")
            VisitNotNull(page.PieceInfo, "PieceInfo")
            VisitNotNull(page.StructParents, "StructParents")
            VisitNotNull(page.ID, "ID")
            VisitNotNull(page.PZ, "PZ")
            VisitNotNull(page.SeparationInfo, "SeparationInfo")
            VisitNotNull(page.Tabs, "Tabs")
            VisitNotNull(page.TemplateInstantiated, "TemplateInstantiated")
            VisitNotNull(page.PresSteps, "PresSteps")
            VisitNotNull(page.UserUnit, "UserUnit")
            VisitNotNull(page.VP, "VP")
            PopLevel()
        End Sub

        Public Sub Visit(pages As PdfPages) Implements IPdfObjectVisitor.Visit
            PushNextLevel()
            Append("Pages")

            For Each child As PdfObject In pages.Children
                CurrentLevelNewLine()
                child.Visit(Me)
            Next

            PopLevel()
        End Sub

        Public Sub Visit(real As PdfReal) Implements IPdfObjectVisitor.Visit
            Append(real)
        End Sub

        Public Sub Visit(rectangle As PdfRectangle) Implements IPdfObjectVisitor.Visit
            Append(rectangle)
        End Sub

        Public Sub Visit(stream As PdfStream) Implements IPdfObjectVisitor.Visit
            PushLevel()
            stream.Dictionary.Visit(Me)

            If StreamContent Then
                Dim content = stream.Value

                If Not String.IsNullOrEmpty(content) Then
                    ' Count how many binary data bytes found in the first 50
                    Dim binary = 0
                    Dim bytes = stream.ValueAsBytes
                    Dim i = 0

                    While i < bytes.Length AndAlso i < 50
                        If bytes(i) > 128 Then binary += 1
                        i += 1
                    End While

                    CurrentLevelNewLine()
                    Append("(START CONTENT)")
                    CurrentLevelNewLine()
                    CurrentLevelNewLine()

                    ' More than 4 binary bytes means we think it must be binary data
                    If binary > 4 Then
                        For Each b In stream.ValueAsBytes
                            Append($"{b} ")
                        Next
                    Else

                        For Each line As String In content.Split(Microsoft.VisualBasic.Strings.ChrW(13))
                            Append(line)
                            CurrentLevelNewLine()
                        Next
                    End If

                    CurrentLevelNewLine()
                    Append("(END CONTENT)")
                End If
            End If

            PopLevel()
        End Sub

        Public Sub Visit(str As PdfString) Implements IPdfObjectVisitor.Visit
            Append($"'{str}'")
        End Sub

        Public Sub Visit(version As PdfVersion) Implements IPdfObjectVisitor.Visit
            Append($"{version}")
        End Sub

        Private Sub VisitNotNull(obj As PdfObject, Optional newLine As Boolean = True)
            If obj IsNot Nothing Then
                If newLine Then CurrentLevelNewLine()
                obj.Visit(Me)
            End If
        End Sub

        Private Sub VisitNotNull(obj As PdfObject, name As String, Optional newLine As Boolean = True)
            If obj IsNot Nothing Then
                If newLine Then CurrentLevelNewLine()
                Append($"{name} ")
                obj.Visit(Me)
            End If
        End Sub

        Private Sub PushLevel()
            _indents.Push(_index)
        End Sub

        Private Sub PushNextLevel()
            _indents.Push(_index + 2)
        End Sub

        Private Sub PopLevel()
            _index = _indents.Pop()
        End Sub

        Private Sub Append(obj As Object)
            Dim str As String = obj.ToString()
            _sb.Append(str)
            _index += str.Length
        End Sub

        Private Sub AppendObject(name As String, obj As Object)
            If obj IsNot Nothing Then
                CurrentLevelNewLine()
                Append($"{name} {obj.ToString()}")
            End If
        End Sub

        Private Sub CurrentLevelNewLine()
            Dim indent As Integer = _indents.Peek()
            _index = indent
            _sb.Append($"
{New String(" "c, _index)}")
        End Sub
    End Class
End Namespace
