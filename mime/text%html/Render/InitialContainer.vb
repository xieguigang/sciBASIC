#Region "Microsoft.VisualBasic::9e3cd53772c32f14472c066f9802ddbf, mime\text%html\Render\InitialContainer.vb"

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

    '   Total Lines: 423
    '    Code Lines: 230 (54.37%)
    ' Comment Lines: 103 (24.35%)
    '    - Xml Docs: 62.14%
    ' 
    '   Blank Lines: 90 (21.28%)
    '     File Size: 15.85 KB


    '     Class InitialContainer
    ' 
    '         Properties: AvoidGeometryAntialias, AvoidTextAntialias, DocumentSource, LinkRegions, MaximumSize
    '                     MediaBlocks, ScrollOffset
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: BlockCorrection_GetInlineGroups, FindParent
    ' 
    '         Sub: BlockCorrection, CascadeStyles, FeedStyleBlock, FeedStyleSheet, MeasureBounds
    '              ParseDocument
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS

Namespace Render

#If NET48 Then

    ''' <summary>
    ''' HTML rendering
    ''' </summary>
    Public Class InitialContainer
        Inherits CssBox

#Region "Ctor"

        Public Sub New()
            _initialContainer = Me
            _MediaBlocks = New Dictionary(Of String, Dictionary(Of String, CssBlock))()
            _LinkRegions = New Dictionary(Of CssBox, RectangleF)()
            MediaBlocks.Add("all", New Dictionary(Of String, CssBlock)())

            Display = CssConstants.Block

            FeedStyleSheet(CssDefaults.DefaultStyleSheet)
        End Sub

        Public Sub New(documentSource As String)
            Me.New()
            _DocumentSource = documentSource
            ParseDocument()
            CascadeStyles(Me)
            BlockCorrection(Me)
        End Sub

#End Region

#Region "Props"

        ''' <summary>
        ''' Gets the link regions of the container
        ''' </summary>
        Friend ReadOnly Property LinkRegions() As Dictionary(Of CssBox, RectangleF)

        ''' <summary>
        ''' Gets the blocks of style defined on this structure, separated by media type.
        ''' General blocks are defined under the "all" Key.
        ''' </summary>
        ''' <remarks>
        ''' Normal use of this dictionary will be something like:
        ''' 
        ''' MediaBlocks["print"]["strong"].Properties
        ''' 
        ''' - Or -
        ''' 
        ''' MediaBlocks["all"]["strong"].Properties
        ''' </remarks>
        Public ReadOnly Property MediaBlocks() As Dictionary(Of String, Dictionary(Of String, CssBlock))

        ''' <summary>
        ''' Gets the document's source
        ''' </summary>
        Public ReadOnly Property DocumentSource() As String

        ''' <summary>
        ''' Gets or sets a value indicating if antialiasing should be avoided 
        ''' for geometry like backgrounds and borders
        ''' </summary>
        Public Property AvoidGeometryAntialias() As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating if antialiasing should be avoided
        ''' for text rendering
        ''' </summary>
        Public Property AvoidTextAntialias() As Boolean

        ''' <summary>
        ''' Gets or sets the maximum size of the container
        ''' </summary>
        Public Property MaximumSize() As SizeF

        ''' <summary>
        ''' Gets or sets the scroll offset of the document
        ''' </summary>
        Public Property ScrollOffset() As PointF
#End Region

#Region "Methods"

        ''' <summary>
        ''' Feeds the source of the stylesheet
        ''' </summary>
        ''' <param name="stylesheet"></param>
        Public Sub FeedStyleSheet(stylesheet As String)
            If String.IsNullOrEmpty(stylesheet) Then
                Return
            End If

            'Convert everything to lower-case
            stylesheet = stylesheet.ToLower()

            '#Region "Remove comments"

            Dim comments As MatchCollection = Parser.Match(Parser.CssComments, stylesheet)
            While comments.Count > 0
                stylesheet = stylesheet.Remove(comments(0).Index, comments(0).Length)
                comments = Parser.Match(Parser.CssComments, stylesheet)
            End While

            '#End Region

            '#Region "Extract @media blocks"

            'MatchCollection atrules = Parser.Match(Parser.CssAtRules, stylesheet);

            Dim atrules As MatchCollection = Parser.Match(Parser.CssAtRules, stylesheet)
            While atrules.Count > 0
                Dim match As Match = atrules(0)

                'Extract whole at-rule
                Dim atrule As String = match.Value

                'Remove rule from sheet
                stylesheet = stylesheet.Remove(match.Index, match.Length)

                'Just processs @media rules
                If Not atrule.StartsWith("@media") Then
                    Continue While
                End If

                'Extract specified media types
                Dim types As MatchCollection = Parser.Match(Parser.CssMediaTypes, atrule)

                If types.Count = 1 Then
                    Dim line As String = types(0).Value

                    If line.StartsWith("@media") AndAlso line.EndsWith("{") Then
                        'Get specified media types in the at-rule
                        Dim media As String() = line.Substring(6, line.Length - 7).Split(" "c)

                        'Scan media types
                        For i As Integer = 0 To media.Length - 1
                            If String.IsNullOrEmpty(media(i).Trim()) Then
                                Continue For
                            End If

                            'Get blocks inside the at-rule
                            Dim insideBlocks As MatchCollection = Parser.Match(Parser.CssBlocks, atrule)

                            'Scan blocks and feed them to the style sheet
                            For Each insideBlock As Match In insideBlocks
                                FeedStyleBlock(media(i).Trim(), insideBlock.Value)
                            Next
                        Next
                    End If
                End If
                atrules = Parser.Match(Parser.CssAtRules, stylesheet)
            End While

            '#End Region

            '#Region "Extract general blocks"
            'This blocks are added under the "all" keyword

            Dim blocks As MatchCollection = Parser.Match(Parser.CssBlocks, stylesheet)

            For Each match As Match In blocks
                FeedStyleBlock("all", match.Value)
            Next

            '#End Region
        End Sub

        ''' <summary>
        ''' Feeds the style with a block about the specific media.
        ''' When no media is specified, "all" will be used
        ''' </summary>
        ''' <param name="media"></param>
        ''' <param name="block"></param>
        Private Sub FeedStyleBlock(media As String, block As String)
            If String.IsNullOrEmpty(media) Then
                media = "all"
            End If

            Dim bracketIndex As Integer = block.IndexOf("{")
            Dim blockSource As String = block.Substring(bracketIndex).Replace("{", String.Empty).Replace("}", String.Empty)

            If bracketIndex < 0 Then
                Return
            End If

            'TODO: Only supporting definitions like:
            ' h1, h2, h3 {...
            'Support needed for definitions like:
            '* {...
            'h1 h2 {...
            'h1 > h2 {...
            'h1:before {...
            'h1:hover {...
            Dim classes As String() = block.Substring(0, bracketIndex).Split(","c)

            For i As Integer = 0 To classes.Length - 1
                Dim className As String = classes(i).Trim()
                If String.IsNullOrEmpty(className) Then
                    Continue For
                End If

                Dim newblock As New CssBlock(blockSource)

                'Create media blocks if necessary
                If Not MediaBlocks.ContainsKey(media) Then
                    MediaBlocks.Add(media, New Dictionary(Of String, CssBlock)())
                End If

                If Not MediaBlocks(media).ContainsKey(className) Then
                    'Create block
                    MediaBlocks(media).Add(className, newblock)
                Else
                    'Merge newblock and oldblock's properties

                    Dim oldblock As CssBlock = MediaBlocks(media)(className)

                    For Each [property] As String In newblock.Properties.Keys
                        If oldblock.Properties.ContainsKey([property]) Then
                            oldblock.Properties([property]) = newblock.Properties([property])
                        Else
                            oldblock.Properties.Add([property], newblock.Properties([property]))
                        End If
                    Next

                    oldblock.UpdatePropertyValues()
                End If
            Next
        End Sub

        ''' <summary>
        ''' Parses the document
        ''' </summary>
        Private Sub ParseDocument()
            Dim root As InitialContainer = Me
            Dim tags As MatchCollection = Parser.Match(Parser.HtmlTag, DocumentSource)
            Dim curBox As CssBox = root
            Dim lastEnd As Integer = -1

            For Each tagmatch As Match In tags
                Dim text As String = If(tagmatch.Index > 0, DocumentSource.Substring(lastEnd + 1, tagmatch.Index - lastEnd - 1), String.Empty)

                If Not String.IsNullOrEmpty(text.Trim()) Then
                    Dim abox As New CssAnonymousBox(curBox)
                    abox.Text = text
                ElseIf text IsNot Nothing AndAlso text.Length > 0 Then
                    Dim sbox As New CssAnonymousSpaceBox(curBox)
                    sbox.Text = text
                End If

                Dim tag As New HtmlTag(tagmatch.Value)

                If tag.IsClosing Then
                    curBox = FindParent(tag.TagName, curBox)
                ElseIf tag.IsSingle Then
                    Dim foo As New CssBox(curBox, tag)
                Else
                    curBox = New CssBox(curBox, tag)
                End If



                lastEnd = tagmatch.Index + tagmatch.Length - 1
            Next

            Dim finaltext As String = DocumentSource.Substring((If(lastEnd > 0, lastEnd + 1, 0)), DocumentSource.Length - lastEnd - 1 + (If(lastEnd = 0, 1, 0)))

            If Not String.IsNullOrEmpty(finaltext) Then
                Dim abox As New CssAnonymousBox(curBox)
                abox.Text = finaltext
            End If
        End Sub

        ''' <summary>
        ''' Recursively searches for the parent with the specified HTML Tag name
        ''' </summary>
        ''' <param name="tagName"></param>
        ''' <param name="b"></param>
        Private Function FindParent(tagName As String, b As CssBox) As CssBox
            If b Is Nothing Then
                Return InitialContainer
            ElseIf b.HtmlTag IsNot Nothing AndAlso b.HtmlTag.TagName.Equals(tagName, StringComparison.CurrentCultureIgnoreCase) Then
                Return If(b.ParentBox Is Nothing, InitialContainer, b.ParentBox)
            Else
                Return FindParent(tagName, b.ParentBox)
            End If
        End Function

        ''' <summary>
        ''' Applies style to all boxes in the tree
        ''' </summary>
        Private Sub CascadeStyles(startBox As CssBox)
            Dim someBlock As Boolean = False

            For Each b As CssBox In startBox.Boxes
                b.InheritStyle()

                If b.HtmlTag IsNot Nothing Then
                    'Check if tag name matches with a defined class
                    If MediaBlocks("all").ContainsKey(b.HtmlTag.TagName) Then
                        MediaBlocks("all")(b.HtmlTag.TagName).AssignTo(b)
                    End If

                    'Check if class="" attribute matches with a defined style
                    If b.HtmlTag.HasAttribute("class") AndAlso MediaBlocks("all").ContainsKey("." & b.HtmlTag.Attributes("class")) Then
                        MediaBlocks("all")("." & b.HtmlTag.Attributes("class")).AssignTo(b)
                    End If

                    b.HtmlTag.TranslateAttributes(b)

                    'Check for the style="" attribute
                    If b.HtmlTag.HasAttribute("style") Then
                        Dim block As New CssBlock(b.HtmlTag.Attributes("style"))
                        block.AssignTo(b)
                    End If

                    'Check for the <style> tag
                    If b.HtmlTag.TagName.Equals("style", StringComparison.CurrentCultureIgnoreCase) AndAlso b.Boxes.Count = 1 Then
                        FeedStyleSheet(b.Boxes(0).Text)
                    End If

                    'Check for the <link rel=stylesheet> tag
                    If b.HtmlTag.TagName.Equals("link", StringComparison.CurrentCultureIgnoreCase) AndAlso b.GetAttribute("rel", String.Empty).Equals("stylesheet", StringComparison.CurrentCultureIgnoreCase) Then
                        FeedStyleSheet(CssValue.GetStyleSheet(b.GetAttribute("href", String.Empty)))
                    End If
                End If

                CascadeStyles(b)
            Next

            If someBlock Then
                For Each box As CssBox In startBox.Boxes
                    box.Display = CssConstants.Block
                Next
            End If

        End Sub

        ''' <summary>
        ''' Makes block boxes be among only block boxes. 
        ''' Inline boxes should live in a pool of Inline boxes only.
        ''' </summary>
        ''' <param name="startBox"></param>
        Private Sub BlockCorrection(startBox As CssBox)
            Dim inlinesonly As Boolean = startBox.ContainsInlinesOnly()

            If Not inlinesonly Then

                Dim inlinegroups As List(Of List(Of CssBox)) = BlockCorrection_GetInlineGroups(startBox)

                For Each group As List(Of CssBox) In inlinegroups
                    If group.Count = 0 Then
                        Continue For
                    End If

                    If group.Count = 1 AndAlso TypeOf group(0) Is CssAnonymousSpaceBox Then
                        Dim sbox As New CssAnonymousSpaceBlockBox(startBox, group(0))

                        group(0).ParentBox = sbox
                    Else
                        Dim newbox As New CssAnonymousBlockBox(startBox, group(0))

                        For Each inline As CssBox In group
                            inline.ParentBox = newbox
                        Next
                    End If
                Next
            End If

            For Each b As CssBox In startBox.Boxes
                BlockCorrection(b)
            Next
        End Sub

        ''' <summary>
        ''' Scans the boxes (non-deeply) of the box, and returns groups of contiguous inline boxes.
        ''' </summary>
        ''' <param name="box"></param>
        ''' <returns></returns>
        Private Function BlockCorrection_GetInlineGroups(box As CssBox) As List(Of List(Of CssBox))
            Dim result As New List(Of List(Of CssBox))()
            Dim current As List(Of CssBox) = Nothing

            'Scan boxes
            For i As Integer = 0 To box.Boxes.Count - 1
                Dim b As CssBox = box.Boxes(i)

                'If inline, add it to the current group
                If b.Display = CssConstants.Inline Then
                    If current Is Nothing Then
                        current = New List(Of CssBox)()
                        result.Add(current)
                    End If
                    current.Add(b)
                Else
                    current = Nothing
                End If
            Next


            'If last list contains nothing, erase it
            If result.Count > 0 AndAlso result(result.Count - 1).Count = 0 Then
                result.RemoveAt(result.Count - 1)
            End If

            Return result
        End Function

        Public Overrides Sub MeasureBounds(g As Graphics)
            LinkRegions.Clear()

            MyBase.MeasureBounds(g)
        End Sub

#End Region
    End Class
#End If
End Namespace
