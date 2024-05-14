#Region "Microsoft.VisualBasic::0a5d23348b6afb7bf478c9c5746454a8, mime\text%yaml\1.2\YamlParser.vb"

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

    '   Total Lines: 5373
    '    Code Lines: 4570
    ' Comment Lines: 60
    '   Blank Lines: 743
    '     File Size: 189.44 KB


    '     Class YamlParser
    ' 
    '         Function: ParseAliasNode, ParseAnchor, ParseAnchorName, ParseBlank, ParseBlockCollection
    '                   ParseBlockCollectionEntry, ParseBlockCollectionEntryOptionalIndent, ParseBlockContent, ParseBlockExplicitKey, ParseBlockExplicitValue
    '                   ParseBlockKey, ParseBlockMapping, ParseBlockMappingEntry, ParseBlockScalar, ParseBlockScalarModifier
    '                   ParseBlockSequence, ParseBlockSequenceEntry, ParseBlockSimpleKey, ParseBlockSimpleValue, ParseChompedLineBreak
    '                   ParseChompingIndicator, ParseDataItem, ParseDigit, ParseDirective, ParseDirectiveName
    '                   ParseDirectiveParameter, ParseDoubleQuotedMultiLine, ParseDoubleQuotedMultiLineBreak, ParseDoubleQuotedMultiLineFist, ParseDoubleQuotedMultiLineInner
    '                   ParseDoubleQuotedMultiLineLast, ParseDoubleQuotedSingleLine, ParseDoubleQuotedText, ParseEmptyBlock, ParseEmptyFlow
    '                   ParseEmptyLineBlock, ParseEscapedSingleQuote, ParseEscapeSequence, ParseExplicitDocument, ParseExplicitKey
    '                   ParseExplicitValue, ParseFlowContentInBlock, ParseFlowContentInFlow, ParseFlowKey, ParseFlowMapping
    '                   ParseFlowMappingEntry, ParseFlowNodeInFlow, ParseFlowScalarInBlock, ParseFlowScalarInFlow, ParseFlowSequence
    '                   ParseFlowSequenceEntry, ParseFlowSingPair, ParseFoldedLine, ParseFoldedLines, ParseFoldedText
    '                   ParseGlobalTagPrefix, ParseHexDigit, ParseImplicitDocument, ParseIndentedBlock, ParseIndentedBlockNode
    '                   ParseIndentedContent, ParseIndentIndicator, ParseInteger, ParseLetter, ParseLineFolding
    '                   ParseLiteralContent, ParseLiteralFirst, ParseLiteralInner, ParseLiteralText, ParseLocalTagPrefix
    '                   ParseMapping, ParseNamedTagHandle, ParseNodeProperty, ParseNonBreakChar, ParseNonSpaceChar
    '                   ParseNonSpaceSep, ParseNonSpecificTag, ParseNormalizedLineBreak, ParsePlainText, ParsePlainTextChar
    '                   ParsePlainTextCharInFlow, ParsePlainTextFirstChar, ParsePlainTextFirstCharInFlow, ParsePlainTextInFlow, ParsePlainTextInFlowMoreLine
    '                   ParsePlainTextInFlowSingleLine, ParsePlainTextMoreLine, ParsePlainTextMultiLine, ParsePlainTextSingleLine, ParsePrimaryTagHandle
    '                   ParseReservedDirective, ParseReservedLineBreak, ParseScalar, ParseSecondaryTagHandle, ParseSeparatedBlock
    '                   ParseSeparatedBlockNode, ParseSeparatedContent, ParseSequence, ParseShorthandTag, ParseSimpleKey
    '                   ParseSingleQuotedMultiLine, ParseSingleQuotedMultiLineFist, ParseSingleQuotedMultiLineInner, ParseSingleQuotedMultiLineLast, ParseSingleQuotedSingleLine
    '                   ParseSingleQuotedText, ParseSpace, ParseSpacedLine, ParseSpacedLines, ParseSpacedPlainTextChar
    '                   ParseSpacedPlainTextCharInFlow, ParseTag, ParseTagChar, ParseTagDirective, ParseTagHandle
    '                   ParseTagPrefix, ParseUriChar, ParseVerbatimTag, ParseWordChar, ParseYamlDirective
    '                   (+2 Overloads) ParseYamlStream, ParseYamlVersion
    ' 
    '         Sub: ParseComment, ParseComments, ParseDocumentMarker, ParseEmptyLinePlain, ParseEmptyLineQuoted
    '              ParseEndOfDocument, ParseEscapedLineBreak, ParseIgnoredBlank, ParseIgnoredSpace, ParseIndent
    '              ParseInlineComment, ParseInlineComments, ParseLineBreak, ParseSeparationLines, ParseSeparationLinesInFlow
    '              ParseSeparationSpace, ParseSeparationSpaceAsIndent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax

Namespace Grammar

    ''' <summary>
    ''' Liu Junfeng. An almost feature complete YAML parser.
    ''' (http://www.codeproject.com/Articles/28720/YAML-Parser-in-C?msg=4534624#xx4534624xx)
    ''' 
    ''' YAML is a human-friendly, cross language, Unicode based data serialization language 
    ''' designed around the common native data types of agile programming languages. 
    ''' It is broadly useful for programming needs ranging from configuration files to 
    ''' Internet messaging to object persistence to data auditing.
    ''' </summary>
    ''' <![CDATA[
    ''' Dim input As New TextInput(richTextBoxSource.Text)
    '''
    ''' Dim success As Boolean
    ''' Dim parser As New YamlParser()
    ''' Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)
    ''' 
    ''' If success Then
    '''     treeViewData.Nodes.Clear()
    '''     For Each doc As YamlDocument In yamlStream.Documents
    '''         treeViewData.Nodes.Add(YamlEmittor.CreateNode(doc.Root))
    '''     Next
    '''     treeViewData.ExpandAll()
    '''     tabControl1.SelectedTab = tabPageDataTree
    ''' Else
    '''     MessageBox.Show(parser.GetEorrorMessages())
    ''' End If
    ''' ]]>
    Public Class YamlParser

        Public Function ParseYamlStream(input As ParserInput(Of Char), ByRef success As Boolean) As YamlStream
            Me.SetInput(input)
            Dim yamlStream As YamlStream = ParseYamlStream(success)
            If Me.Position < input.Length Then
                success = False
                [Error]("Failed to parse remained input.")
            End If
            Return yamlStream
        End Function

        Private Function ParseYamlStream(ByRef success As Boolean) As YamlStream
            Dim errorCount As Integer = Errors.Count
            Dim yamlStream As New YamlStream()
            Dim start_position As Integer = Position

            While True
                ParseComment(success)
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            While True
                Dim seq_start_position1 As Integer = Position
                Dim yamlDocument As YamlDocument = ParseImplicitDocument(success)
                If success Then
                    yamlStream.Documents.Add(yamlDocument)
                End If
                success = True

                While True
                    yamlDocument = ParseExplicitDocument(success)
                    If success Then
                        yamlStream.Documents.Add(yamlDocument)
                    Else
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While

            success = Not Input.HasInput(Position)
            If Not success Then
                [Error]("Failed to parse end of YamlStream.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return yamlStream
        End Function

        Private Function ParseImplicitDocument(ByRef success As Boolean) As YamlDocument
            Dim errorCount As Integer = Errors.Count
            Dim yamlDocument As New YamlDocument()
            Dim start_position As Integer = Position

            currentDocument = yamlDocument
            currentIndent = -1
            yamlDocument.Root = ParseIndentedBlockNode(success)
            If Not success Then
                [Error]("Failed to parse Root of ImplicitDocument.")
                Position = start_position
                Return yamlDocument
            End If

            ParseEndOfDocument(success)
            success = True

            Return yamlDocument
        End Function

        Private Function ParseExplicitDocument(ByRef success As Boolean) As YamlDocument
            Dim errorCount As Integer = Errors.Count
            Dim yamlDocument As New YamlDocument()
            Dim start_position As Integer = Position

            currentDocument = yamlDocument
            currentIndent = -1
            While True
                Dim directive As Directive = ParseDirective(success)
                If success Then
                    yamlDocument.Directives.Add(directive)
                Else
                    Exit While
                End If
            End While
            success = True

            MatchTerminalString("---", success)
            If Not success Then
                [Error]("Failed to parse '---' of ExplicitDocument.")
                Position = start_position
                Return yamlDocument
            End If

            yamlDocument.Root = ParseSeparatedBlockNode(success)
            If Not success Then
                [Error]("Failed to parse Root of ExplicitDocument.")
                Position = start_position
                Return yamlDocument
            End If

            ParseEndOfDocument(success)
            success = True

            Return yamlDocument
        End Function

        Private Sub ParseEndOfDocument(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            MatchTerminalString("...", success)
            If Not success Then
                [Error]("Failed to parse '...' of EndOfDocument.")
                Position = start_position
                Return
            End If

            ParseInlineComments(success)
            If Not success Then
                [Error]("Failed to parse InlineComments of EndOfDocument.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
        End Sub

        Private Function ParseDirective(ByRef success As Boolean) As Directive
            Dim errorCount As Integer = Errors.Count
            Dim directive As Directive = Nothing

            directive = ParseYamlDirective(success)
            If success Then
                ClearError(errorCount)
                Return directive
            End If

            directive = ParseTagDirective(success)
            If success Then
                ClearError(errorCount)
                Return directive
            End If

            directive = ParseReservedDirective(success)
            If success Then
                ClearError(errorCount)
                Return directive
            End If

            Return directive
        End Function

        Private Function ParseReservedDirective(ByRef success As Boolean) As ReservedDirective
            Dim errorCount As Integer = Errors.Count
            Dim reservedDirective As New ReservedDirective()
            Dim start_position As Integer = Position

            MatchTerminal("%"c, success)
            If Not success Then
                [Error]("Failed to parse '%' of ReservedDirective.")
                Position = start_position
                Return reservedDirective
            End If

            reservedDirective.Name = ParseDirectiveName(success)
            If Not success Then
                [Error]("Failed to parse Name of ReservedDirective.")
                Position = start_position
                Return reservedDirective
            End If

            While True
                While True
                    Dim seq_start_position1 As Integer = Position
                    ParseSeparationSpace(success)
                    If Not success Then
                        [Error]("Failed to parse SeparationSpace of ReservedDirective.")
                        Exit While
                    End If

                    Dim str As String = ParseDirectiveParameter(success)
                    If success Then
                        reservedDirective.Parameters.Add(str)
                    Else
                        [Error]("Failed to parse DirectiveParameter of ReservedDirective.")
                        Position = seq_start_position1
                    End If
                    Exit While
                End While
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            ParseInlineComments(success)
            If Not success Then
                [Error]("Failed to parse InlineComments of ReservedDirective.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return reservedDirective
        End Function

        Private Function ParseDirectiveName(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse (NonSpaceChar)+ of DirectiveName.")
            End If
            Return text.ToString()
        End Function

        Private Function ParseDirectiveParameter(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse (NonSpaceChar)+ of DirectiveParameter.")
            End If
            Return text.ToString()
        End Function

        Private Function ParseYamlDirective(ByRef success As Boolean) As YamlDirective
            Dim errorCount As Integer = Errors.Count
            Dim yamlDirective As New YamlDirective()
            Dim start_position As Integer = Position

            MatchTerminalString("YAML", success)
            If Not success Then
                [Error]("Failed to parse 'YAML' of YamlDirective.")
                Position = start_position
                Return yamlDirective
            End If

            ParseSeparationSpace(success)
            If Not success Then
                [Error]("Failed to parse SeparationSpace of YamlDirective.")
                Position = start_position
                Return yamlDirective
            End If

            yamlDirective.Version = ParseYamlVersion(success)
            If Not success Then
                [Error]("Failed to parse Version of YamlDirective.")
                Position = start_position
                Return yamlDirective
            End If

            ParseInlineComments(success)
            If Not success Then
                [Error]("Failed to parse InlineComments of YamlDirective.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return yamlDirective
        End Function

        Private Function ParseYamlVersion(ByRef success As Boolean) As YamlVersion
            Dim errorCount As Integer = Errors.Count
            Dim yamlVersion As New YamlVersion()
            Dim start_position As Integer = Position

            yamlVersion.Major = ParseInteger(success)
            If Not success Then
                [Error]("Failed to parse Major of YamlVersion.")
                Position = start_position
                Return yamlVersion
            End If

            MatchTerminal("."c, success)
            If Not success Then
                [Error]("Failed to parse '.' of YamlVersion.")
                Position = start_position
                Return yamlVersion
            End If

            yamlVersion.Minor = ParseInteger(success)
            If Not success Then
                [Error]("Failed to parse Minor of YamlVersion.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return yamlVersion
        End Function

        Private Function ParseTagDirective(ByRef success As Boolean) As TagDirective
            Dim errorCount As Integer = Errors.Count
            Dim tagDirective As New TagDirective()
            Dim start_position As Integer = Position

            MatchTerminalString("TAG", success)
            If Not success Then
                [Error]("Failed to parse 'TAG' of TagDirective.")
                Position = start_position
                Return tagDirective
            End If

            ParseSeparationSpace(success)
            If Not success Then
                [Error]("Failed to parse SeparationSpace of TagDirective.")
                Position = start_position
                Return tagDirective
            End If

            tagDirective.Handle = ParseTagHandle(success)
            If Not success Then
                [Error]("Failed to parse Handle of TagDirective.")
                Position = start_position
                Return tagDirective
            End If

            ParseSeparationSpace(success)
            If Not success Then
                [Error]("Failed to parse SeparationSpace of TagDirective.")
                Position = start_position
                Return tagDirective
            End If

            tagDirective.Prefix = ParseTagPrefix(success)
            If Not success Then
                [Error]("Failed to parse Prefix of TagDirective.")
                Position = start_position
                Return tagDirective
            End If

            ParseInlineComments(success)
            If Not success Then
                [Error]("Failed to parse InlineComments of TagDirective.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return tagDirective
        End Function

        Private Function ParseTagHandle(ByRef success As Boolean) As TagHandle
            Dim errorCount As Integer = Errors.Count
            Dim tagHandle As TagHandle = Nothing

            tagHandle = ParseNamedTagHandle(success)
            If success Then
                ClearError(errorCount)
                Return tagHandle
            End If

            tagHandle = ParseSecondaryTagHandle(success)
            If success Then
                ClearError(errorCount)
                Return tagHandle
            End If

            tagHandle = ParsePrimaryTagHandle(success)
            If success Then
                ClearError(errorCount)
                Return tagHandle
            End If

            Return tagHandle
        End Function

        Private Function ParsePrimaryTagHandle(ByRef success As Boolean) As PrimaryTagHandle
            Dim errorCount As Integer = Errors.Count
            Dim primaryTagHandle As New PrimaryTagHandle()

            MatchTerminal("!"c, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse '!' of PrimaryTagHandle.")
            End If
            Return primaryTagHandle
        End Function

        Private Function ParseSecondaryTagHandle(ByRef success As Boolean) As SecondaryTagHandle
            Dim errorCount As Integer = Errors.Count
            Dim secondaryTagHandle As New SecondaryTagHandle()

            MatchTerminalString("!!", success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse '!!' of SecondaryTagHandle.")
            End If
            Return secondaryTagHandle
        End Function

        Private Function ParseNamedTagHandle(ByRef success As Boolean) As NamedTagHandle
            Dim errorCount As Integer = Errors.Count
            Dim namedTagHandle As New NamedTagHandle()
            Dim start_position As Integer = Position

            MatchTerminal("!"c, success)
            If Not success Then
                [Error]("Failed to parse '!' of NamedTagHandle.")
                Position = start_position
                Return namedTagHandle
            End If

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseWordChar(success)
                If success Then
                    namedTagHandle.Name.Add(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse Name of NamedTagHandle.")
                Position = start_position
                Return namedTagHandle
            End If

            MatchTerminal("!"c, success)
            If Not success Then
                [Error]("Failed to parse '!' of NamedTagHandle.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return namedTagHandle
        End Function

        Private Function ParseTagPrefix(ByRef success As Boolean) As TagPrefix
            Dim errorCount As Integer = Errors.Count
            Dim tagPrefix As TagPrefix = Nothing

            tagPrefix = ParseLocalTagPrefix(success)
            If success Then
                ClearError(errorCount)
                Return tagPrefix
            End If

            tagPrefix = ParseGlobalTagPrefix(success)
            If success Then
                ClearError(errorCount)
                Return tagPrefix
            End If

            Return tagPrefix
        End Function

        Private Function ParseLocalTagPrefix(ByRef success As Boolean) As LocalTagPrefix
            Dim errorCount As Integer = Errors.Count
            Dim localTagPrefix As New LocalTagPrefix()
            Dim start_position As Integer = Position

            MatchTerminal("!"c, success)
            If Not success Then
                [Error]("Failed to parse '!' of LocalTagPrefix.")
                Position = start_position
                Return localTagPrefix
            End If

            While True
                Dim ch As Char = ParseUriChar(success)
                If success Then
                    localTagPrefix.Prefix.Add(ch)
                Else
                    Exit While
                End If
            End While
            success = True

            Return localTagPrefix
        End Function

        Private Function ParseGlobalTagPrefix(ByRef success As Boolean) As GlobalTagPrefix
            Dim errorCount As Integer = Errors.Count
            Dim globalTagPrefix As New GlobalTagPrefix()

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseUriChar(success)
                If success Then
                    globalTagPrefix.Prefix.Add(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse Prefix of GlobalTagPrefix.")
            End If
            Return globalTagPrefix
        End Function

        Private Function ParseDataItem(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As New DataItem()
            Dim start_position As Integer = Position

            While True
                Dim seq_start_position1 As Integer = Position
                dataItem.[Property] = ParseNodeProperty(success)
                If Not success Then
                    [Error]("Failed to parse Property of DataItem.")
                    Exit While
                End If

                ParseSeparationLines(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLines of DataItem.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            success = True

            ErrorStatck.Push(errorCount)
            errorCount = Errors.Count
            While True
                dataItem = ParseScalar(success)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                dataItem = ParseSequence(success)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                dataItem = ParseMapping(success)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                Exit While
            End While
            errorCount = ErrorStatck.Pop()
            If Not success Then
                [Error]("Failed to parse (Scalar / Sequence / Mapping) of DataItem.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseScalar(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As Scalar = Nothing

            scalar = ParseFlowScalarInBlock(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar = ParseFlowScalarInFlow(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar = ParseBlockScalar(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParseSequence(ByRef success As Boolean) As Sequence
            Dim errorCount As Integer = Errors.Count
            Dim sequence As Sequence = Nothing

            sequence = ParseFlowSequence(success)
            If success Then
                ClearError(errorCount)
                Return sequence
            End If

            sequence = ParseBlockSequence(success)
            If success Then
                ClearError(errorCount)
                Return sequence
            End If

            Return sequence
        End Function

        Private Function ParseMapping(ByRef success As Boolean) As Mapping
            Dim errorCount As Integer = Errors.Count
            Dim mapping As Mapping = Nothing

            mapping = ParseFlowMapping(success)
            If success Then
                ClearError(errorCount)
                Return mapping
            End If

            mapping = ParseBlockMapping(success)
            If success Then
                ClearError(errorCount)
                Return mapping
            End If

            Return mapping
        End Function

        Private Function ParseIndentedBlockNode(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            IncreaseIndent()
            Dim dataItem As DataItem = ParseIndentedBlock(success)
            DecreaseIndent()
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse IndentedBlock of IndentedBlockNode.")
            End If
            Return dataItem
        End Function

        Private Function ParseSeparatedBlockNode(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            IncreaseIndent()
            Dim dataItem As DataItem = ParseSeparatedBlock(success)
            DecreaseIndent()
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse SeparatedBlock of SeparatedBlockNode.")
            End If
            Return dataItem
        End Function

        Private Function ParseIndentedBlock(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseIndentedContent(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of IndentedBlock.")
                    Exit While
                End If

                dataItem = ParseAliasNode(success)
                If Not success Then
                    [Error]("Failed to parse AliasNode of IndentedBlock.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of IndentedBlock.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of IndentedBlock.")
                    Exit While
                End If

                Dim [property] As NodeProperty = ParseNodeProperty(success)
                If Not success Then
                    [Error]("Failed to parse property of IndentedBlock.")
                    Position = seq_start_position2
                    Exit While
                End If

                dataItem = ParseSeparatedContent(success)
                If success Then
                    SetDataItemProperty(dataItem, [property])
                End If
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseSeparatedBlock(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseSeparatedContent(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                ParseSeparationLines(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLines of SeparatedBlock.")
                    Exit While
                End If

                dataItem = ParseAliasNode(success)
                If Not success Then
                    [Error]("Failed to parse AliasNode of SeparatedBlock.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of SeparatedBlock.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                ParseSeparationSpace(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpace of SeparatedBlock.")
                    Exit While
                End If

                Dim [property] As NodeProperty = ParseNodeProperty(success)
                If Not success Then
                    [Error]("Failed to parse property of SeparatedBlock.")
                    Position = seq_start_position2
                    Exit While
                End If

                dataItem = ParseSeparatedContent(success)
                If success Then
                    SetDataItemProperty(dataItem, [property])
                End If
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseEmptyBlock(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseIndentedContent(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of IndentedContent.")
                    Exit While
                End If

                dataItem = ParseBlockContent(success)
                If Not success Then
                    [Error]("Failed to parse BlockContent of IndentedContent.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of IndentedContent.")
                    Exit While
                End If

                dataItem = ParseFlowContentInBlock(success)
                If Not success Then
                    [Error]("Failed to parse FlowContentInBlock of IndentedContent.")
                    Position = seq_start_position2
                    Exit While
                End If

                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of IndentedContent.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseSeparatedContent(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of SeparatedContent.")
                    Exit While
                End If

                dataItem = ParseIndentedContent(success)
                If Not success Then
                    [Error]("Failed to parse IndentedContent of SeparatedContent.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                ParseSeparationSpace(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpace of SeparatedContent.")
                    Exit While
                End If

                dataItem = ParseBlockScalar(success)
                If Not success Then
                    [Error]("Failed to parse BlockScalar of SeparatedContent.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                ParseSeparationSpace(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpace of SeparatedContent.")
                    Exit While
                End If

                dataItem = ParseFlowContentInBlock(success)
                If Not success Then
                    [Error]("Failed to parse FlowContentInBlock of SeparatedContent.")
                    Position = seq_start_position3
                    Exit While
                End If

                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of SeparatedContent.")
                    Position = seq_start_position3
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseBlockCollectionEntry(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            IncreaseIndent()
            While True
                Dim seq_start_position1 As Integer = Position
                ParseSeparationSpaceAsIndent(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpaceAsIndent of BlockCollectionEntry.")
                    Exit While
                End If

                dataItem = ParseBlockCollection(success)
                If Not success Then
                    [Error]("Failed to parse BlockCollection of BlockCollectionEntry.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            DecreaseIndent()
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseSeparatedBlockNode(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseBlockCollectionEntryOptionalIndent(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            RememberIndent()
            While True
                Dim seq_start_position1 As Integer = Position
                ParseSeparationSpaceAsIndent(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpaceAsIndent of BlockCollectionEntryOptionalIndent.")
                    Exit While
                End If

                dataItem = ParseBlockCollection(success)
                If Not success Then
                    [Error]("Failed to parse BlockCollection of BlockCollectionEntryOptionalIndent.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            RestoreIndent()
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            RememberIndent()
            dataItem = ParseSeparatedBlock(success)
            RestoreIndent()
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseFlowNodeInFlow(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseAliasNode(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseFlowContentInFlow(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                Dim [property] As NodeProperty = ParseNodeProperty(success)
                If success Then
                    dataItem = New Scalar()
                Else
                    [Error]("Failed to parse property of FlowNodeInFlow.")
                    Exit While
                End If

                While True
                    Dim seq_start_position2 As Integer = Position
                    ParseSeparationLinesInFlow(success)
                    If Not success Then
                        [Error]("Failed to parse SeparationLinesInFlow of FlowNodeInFlow.")
                        Exit While
                    End If

                    dataItem = ParseFlowContentInFlow(success)
                    If Not success Then
                        [Error]("Failed to parse FlowContentInFlow of FlowNodeInFlow.")
                        Position = seq_start_position2
                    End If
                    Exit While
                End While
                If success Then
                    SetDataItemProperty(dataItem, [property])
                End If
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseAliasNode(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            MatchTerminal("*"c, success)
            If Not success Then
                [Error]("Failed to parse '*' of AliasNode.")
                Position = start_position
                Return dataItem
            End If

            Dim name As String = ParseAnchorName(success)
            If success Then
                Return GetAnchoredDataItem(name)
            Else
                [Error]("Failed to parse name of AliasNode.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseFlowContentInBlock(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseFlowScalarInBlock(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseFlowSequence(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseFlowMapping(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseFlowContentInFlow(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseFlowScalarInFlow(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseFlowSequence(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseFlowMapping(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseBlockContent(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseBlockScalar(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseBlockSequence(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseBlockMapping(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseBlockCollection(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            dataItem = ParseBlockSequence(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            dataItem = ParseBlockMapping(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseEmptyFlow(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As New DataItem()

            success = True
            If success Then
                Return New Scalar()
            End If
            Return dataItem
        End Function

        Private Function ParseEmptyBlock(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            dataItem = ParseEmptyFlow(success)

            ParseInlineComments(success)
            If Not success Then
                [Error]("Failed to parse InlineComments of EmptyBlock.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseNodeProperty(ByRef success As Boolean) As NodeProperty
            Dim errorCount As Integer = Errors.Count
            Dim nodeProperty As New NodeProperty()

            While True
                Dim seq_start_position1 As Integer = Position
                nodeProperty.Tag = ParseTag(success)
                If Not success Then
                    [Error]("Failed to parse Tag of NodeProperty.")
                    Exit While
                End If

                While True
                    Dim seq_start_position2 As Integer = Position
                    ParseSeparationLines(success)
                    If Not success Then
                        [Error]("Failed to parse SeparationLines of NodeProperty.")
                        Exit While
                    End If

                    nodeProperty.Anchor = ParseAnchor(success)
                    If Not success Then
                        [Error]("Failed to parse Anchor of NodeProperty.")
                        Position = seq_start_position2
                    End If
                    Exit While
                End While
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return nodeProperty
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                nodeProperty.Anchor = ParseAnchor(success)
                If Not success Then
                    [Error]("Failed to parse Anchor of NodeProperty.")
                    Exit While
                End If

                While True
                    Dim seq_start_position4 As Integer = Position
                    ParseSeparationLines(success)
                    If Not success Then
                        [Error]("Failed to parse SeparationLines of NodeProperty.")
                        Exit While
                    End If

                    nodeProperty.Tag = ParseTag(success)
                    If Not success Then
                        [Error]("Failed to parse Tag of NodeProperty.")
                        Position = seq_start_position4
                    End If
                    Exit While
                End While
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return nodeProperty
            End If

            Return nodeProperty
        End Function

        Private Function ParseAnchor(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing
            Dim start_position As Integer = Position

            MatchTerminal("&"c, success)
            If Not success Then
                [Error]("Failed to parse '&' of Anchor.")
                Position = start_position
                Return str
            End If

            str = ParseAnchorName(success)
            If Not success Then
                [Error]("Failed to parse AnchorName of Anchor.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return str
        End Function

        Private Function ParseAnchorName(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse (NonSpaceChar)+ of AnchorName.")
            End If
            Return text.ToString()
        End Function

        Private Function ParseTag(ByRef success As Boolean) As Tag
            Dim errorCount As Integer = Errors.Count
            Dim tag As Tag = Nothing

            tag = ParseVerbatimTag(success)
            If success Then
                ClearError(errorCount)
                Return tag
            End If

            tag = ParseShorthandTag(success)
            If success Then
                ClearError(errorCount)
                Return tag
            End If

            tag = ParseNonSpecificTag(success)
            If success Then
                ClearError(errorCount)
                Return tag
            End If

            Return tag
        End Function

        Private Function ParseNonSpecificTag(ByRef success As Boolean) As NonSpecificTag
            Dim errorCount As Integer = Errors.Count
            Dim nonSpecificTag As New NonSpecificTag()

            MatchTerminal("!"c, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse '!' of NonSpecificTag.")
            End If
            Return nonSpecificTag
        End Function

        Private Function ParseVerbatimTag(ByRef success As Boolean) As VerbatimTag
            Dim errorCount As Integer = Errors.Count
            Dim verbatimTag As New VerbatimTag()
            Dim start_position As Integer = Position

            MatchTerminal("!"c, success)
            If Not success Then
                [Error]("Failed to parse '!' of VerbatimTag.")
                Position = start_position
                Return verbatimTag
            End If

            MatchTerminal("<"c, success)
            If Not success Then
                [Error]("Failed to parse '<' of VerbatimTag.")
                Position = start_position
                Return verbatimTag
            End If

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseUriChar(success)
                If success Then
                    verbatimTag.Chars.Add(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse Chars of VerbatimTag.")
                Position = start_position
                Return verbatimTag
            End If

            MatchTerminal(">"c, success)
            If Not success Then
                [Error]("Failed to parse '>' of VerbatimTag.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return verbatimTag
        End Function

        Private Function ParseShorthandTag(ByRef success As Boolean) As ShorthandTag
            Dim errorCount As Integer = Errors.Count
            Dim shorthandTag As New ShorthandTag()

            While True
                Dim seq_start_position1 As Integer = Position
                ParseNamedTagHandle(success)
                If Not success Then
                    [Error]("Failed to parse NamedTagHandle of ShorthandTag.")
                    Exit While
                End If

                Dim counter As Integer = 0
                While True
                    Dim ch As Char = ParseTagChar(success)
                    If success Then
                        shorthandTag.Chars.Add(ch)
                    Else
                        Exit While
                    End If
                    counter += 1
                End While
                If counter > 0 Then
                    success = True
                End If
                If Not success Then
                    [Error]("Failed to parse Chars of ShorthandTag.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return shorthandTag
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                ParseSecondaryTagHandle(success)
                If Not success Then
                    [Error]("Failed to parse SecondaryTagHandle of ShorthandTag.")
                    Exit While
                End If

                Dim counter As Integer = 0
                While True
                    Dim ch As Char = ParseTagChar(success)
                    If success Then
                        shorthandTag.Chars.Add(ch)
                    Else
                        Exit While
                    End If
                    counter += 1
                End While
                If counter > 0 Then
                    success = True
                End If
                If Not success Then
                    [Error]("Failed to parse Chars of ShorthandTag.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return shorthandTag
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                ParsePrimaryTagHandle(success)
                If Not success Then
                    [Error]("Failed to parse PrimaryTagHandle of ShorthandTag.")
                    Exit While
                End If

                Dim counter As Integer = 0
                While True
                    Dim ch As Char = ParseTagChar(success)
                    If success Then
                        shorthandTag.Chars.Add(ch)
                    Else
                        Exit While
                    End If
                    counter += 1
                End While
                If counter > 0 Then
                    success = True
                End If
                If Not success Then
                    [Error]("Failed to parse Chars of ShorthandTag.")
                    Position = seq_start_position3
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return shorthandTag
            End If

            Return shorthandTag
        End Function

        Private Function ParseFlowScalarInBlock(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As New Scalar()

            scalar.Text = ParsePlainTextMultiLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseSingleQuotedText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseDoubleQuotedText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParseFlowScalarInFlow(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As New Scalar()

            scalar.Text = ParsePlainTextInFlow(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseSingleQuotedText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseDoubleQuotedText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParseBlockScalar(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As New Scalar()

            scalar.Text = ParseLiteralText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseFoldedText(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParsePlainText(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing

            str = ParsePlainTextMultiLine(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            str = ParsePlainTextInFlow(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            Return str
        End Function

        Private Function ParsePlainTextMultiLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParsePlainTextSingleLine(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextSingleLine of PlainTextMultiLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParsePlainTextMoreLine(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParsePlainTextSingleLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim not_start_position1 As Integer = Position
            ParseDocumentMarker(success)
            Position = not_start_position1
            success = Not success
            If Not success Then
                [Error]("Failed to parse !(DocumentMarker) of PlainTextSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            Dim str As String = ParsePlainTextFirstChar(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextFirstChar of PlainTextSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    str = ParsePlainTextChar(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    str = ParseSpacedPlainTextChar(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParsePlainTextMoreLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIgnoredBlank(success)

            Dim str As String = ParseLineFolding(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse LineFolding of PlainTextMoreLine.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of PlainTextMoreLine.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredSpace(success)

            Dim counter As Integer = 0
            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    str = ParsePlainTextChar(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    str = ParseSpacedPlainTextChar(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse ((PlainTextChar / SpacedPlainTextChar))+ of PlainTextMoreLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParsePlainTextInFlow(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParsePlainTextInFlowSingleLine(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextInFlowSingleLine of PlainTextInFlow.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParsePlainTextInFlowMoreLine(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParsePlainTextInFlowSingleLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim not_start_position1 As Integer = Position
            ParseDocumentMarker(success)
            Position = not_start_position1
            success = Not success
            If Not success Then
                [Error]("Failed to parse !(DocumentMarker) of PlainTextInFlowSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            Dim str As String = ParsePlainTextFirstCharInFlow(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextFirstCharInFlow of PlainTextInFlowSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    str = ParsePlainTextCharInFlow(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    str = ParseSpacedPlainTextCharInFlow(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParsePlainTextInFlowMoreLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIgnoredBlank(success)

            Dim str As String = ParseLineFolding(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse LineFolding of PlainTextInFlowMoreLine.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of PlainTextInFlowMoreLine.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredSpace(success)

            Dim counter As Integer = 0
            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    str = ParsePlainTextCharInFlow(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    str = ParseSpacedPlainTextCharInFlow(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(str)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse ((PlainTextCharInFlow / SpacedPlainTextCharInFlow))+ of PlainTextInFlowMoreLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParsePlainTextFirstChar(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim ch As Char = MatchTerminalSet(vbCr & vbLf & vbTab & " -?:,[]{}#&*!|>'""%@`", True, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch)
                Return text.ToString()
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                ch = MatchTerminalSet("-?:", False, success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse ""-?:"" of PlainTextFirstChar.")
                    Exit While
                End If

                ch = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceChar of PlainTextFirstChar.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            Return text.ToString()
        End Function

        Private Function ParsePlainTextChar(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            While True
                Dim seq_start_position1 As Integer = Position
                Dim ch As Char = MatchTerminal(":"c, success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse ':' of PlainTextChar.")
                    Exit While
                End If

                ch = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceChar of PlainTextChar.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                Dim ch As Char = ParseNonSpaceChar(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceChar of PlainTextChar.")
                    Exit While
                End If

                Dim counter As Integer = 0
                While True
                    ch = MatchTerminal("#"c, success)
                    If success Then
                        text.Append(ch)
                    Else
                        Exit While
                    End If
                    counter += 1
                End While
                If counter > 0 Then
                    success = True
                End If
                If Not success Then
                    [Error]("Failed to parse ('#')+ of PlainTextChar.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            text.Length = 0
            Dim ch2 As Char = MatchTerminalSet(vbCr & vbLf & vbTab & " :#", True, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch2)
                Return text.ToString()
            End If

            Return text.ToString()
        End Function

        Private Function ParseSpacedPlainTextChar(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim counter As Integer = 0
            While True
                Dim ch As Char = MatchTerminal(" "c, success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse (' ')+ of SpacedPlainTextChar.")
                Position = start_position
                Return text.ToString()
            End If

            Dim str As String = ParsePlainTextChar(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextChar of SpacedPlainTextChar.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParsePlainTextFirstCharInFlow(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim ch As Char = MatchTerminalSet(vbCr & vbLf & vbTab & " -?:,[]{}#&*!|>'""%@`", True, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch)
                Return text.ToString()
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                ch = MatchTerminalSet("-?:", False, success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse ""-?:"" of PlainTextFirstCharInFlow.")
                    Exit While
                End If

                ch = ParseNonSpaceSep(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceSep of PlainTextFirstCharInFlow.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            Return text.ToString()
        End Function

        Private Function ParsePlainTextCharInFlow(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            While True
                Dim seq_start_position1 As Integer = Position
                Dim ch As Char = MatchTerminalSet(":", False, success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse "":"" of PlainTextCharInFlow.")
                    Exit While
                End If

                ch = ParseNonSpaceSep(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceSep of PlainTextCharInFlow.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                Dim ch As Char = ParseNonSpaceSep(success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse NonSpaceSep of PlainTextCharInFlow.")
                    Exit While
                End If

                ch = MatchTerminal("#"c, success)
                If success Then
                    text.Append(ch)
                Else
                    [Error]("Failed to parse '#' of PlainTextCharInFlow.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return text.ToString()
            End If

            text.Length = 0
            Dim ch2 As Char = MatchTerminalSet(vbCr & vbLf & vbTab & " :#,[]{}", True, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch2)
                Return text.ToString()
            End If

            Return text.ToString()
        End Function

        Private Function ParseSpacedPlainTextCharInFlow(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim counter As Integer = 0
            While True
                Dim ch As Char = MatchTerminal(" "c, success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse (' ')+ of SpacedPlainTextCharInFlow.")
                Position = start_position
                Return text.ToString()
            End If

            Dim str As String = ParsePlainTextCharInFlow(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse PlainTextCharInFlow of SpacedPlainTextCharInFlow.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Sub ParseDocumentMarker(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            While True
                Dim seq_start_position1 As Integer = Position
                success = Position = 0 OrElse TerminalMatch(ControlChars.Lf, Position - 1)
                If Not success Then
                    [Error]("Failed to parse sol of DocumentMarker.")
                    Exit While
                End If

                MatchTerminalString("---", success)
                If Not success Then
                    [Error]("Failed to parse '---' of DocumentMarker.")
                    Position = seq_start_position1
                    Exit While
                End If

                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    ParseSpace(success)
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    ParseLineBreak(success)
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    [Error]("Failed to parse (Space / LineBreak) of DocumentMarker.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                success = Position = 0 OrElse TerminalMatch(ControlChars.Lf, Position - 1)
                If Not success Then
                    [Error]("Failed to parse sol of DocumentMarker.")
                    Exit While
                End If

                MatchTerminalString("...", success)
                If Not success Then
                    [Error]("Failed to parse '...' of DocumentMarker.")
                    Position = seq_start_position2
                    Exit While
                End If

                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    ParseSpace(success)
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    ParseLineBreak(success)
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    [Error]("Failed to parse (Space / LineBreak) of DocumentMarker.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return
            End If

        End Sub

        Private Function ParseDoubleQuotedText(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing

            str = ParseDoubleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            str = ParseDoubleQuotedMultiLine(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            Return str
        End Function

        Private Function ParseDoubleQuotedSingleLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal(""""c, success)
            If Not success Then
                [Error]("Failed to parse '\""' of DoubleQuotedSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet("""\" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapeSequence(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            MatchTerminal(""""c, success)
            If Not success Then
                [Error]("Failed to parse '\""' of DoubleQuotedSingleLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseDoubleQuotedMultiLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParseDoubleQuotedMultiLineFist(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse DoubleQuotedMultiLineFist of DoubleQuotedMultiLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParseDoubleQuotedMultiLineInner(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            str = ParseDoubleQuotedMultiLineLast(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse DoubleQuotedMultiLineLast of DoubleQuotedMultiLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseDoubleQuotedMultiLineFist(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal(""""c, success)
            If Not success Then
                [Error]("Failed to parse '\""' of DoubleQuotedMultiLineFist.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet(" ""\" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapeSequence(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    While True
                        Dim seq_start_position1 As Integer = Position
                        ch = MatchTerminal(" "c, success)
                        If success Then
                            text.Append(ch)
                        Else
                            [Error]("Failed to parse ' ' of DoubleQuotedMultiLineFist.")
                            Exit While
                        End If

                        Dim not_start_position2 As Integer = Position
                        While True
                            ParseIgnoredBlank(success)

                            ParseLineBreak(success)
                            Exit While
                        End While
                        Position = not_start_position2
                        success = Not success
                        If Not success Then
                            [Error]("Failed to parse !((IgnoredBlank LineBreak)) of DoubleQuotedMultiLineFist.")
                            Position = seq_start_position1
                        End If
                        Exit While
                    End While
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            ParseIgnoredBlank(success)

            Dim str As String = ParseDoubleQuotedMultiLineBreak(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse DoubleQuotedMultiLineBreak of DoubleQuotedMultiLineFist.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseDoubleQuotedMultiLineInner(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of DoubleQuotedMultiLineInner.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            Dim counter As Integer = 0
            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet(" ""\" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapeSequence(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    While True
                        Dim seq_start_position1 As Integer = Position
                        ch = MatchTerminal(" "c, success)
                        If success Then
                            text.Append(ch)
                        Else
                            [Error]("Failed to parse ' ' of DoubleQuotedMultiLineInner.")
                            Exit While
                        End If

                        Dim not_start_position2 As Integer = Position
                        While True
                            ParseIgnoredBlank(success)

                            ParseLineBreak(success)
                            Exit While
                        End While
                        Position = not_start_position2
                        success = Not success
                        If Not success Then
                            [Error]("Failed to parse !((IgnoredBlank LineBreak)) of DoubleQuotedMultiLineInner.")
                            Position = seq_start_position1
                        End If
                        Exit While
                    End While
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse ((-"" ""\" & vbCr & vbLf & """ / EscapeSequence / ' ' !((IgnoredBlank LineBreak))))+ of DoubleQuotedMultiLineInner.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            Dim str As String = ParseDoubleQuotedMultiLineBreak(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse DoubleQuotedMultiLineBreak of DoubleQuotedMultiLineInner.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseDoubleQuotedMultiLineLast(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of DoubleQuotedMultiLineLast.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet("""\" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapeSequence(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            MatchTerminal(""""c, success)
            If Not success Then
                [Error]("Failed to parse '\""' of DoubleQuotedMultiLineLast.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseDoubleQuotedMultiLineBreak(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing

            str = ParseLineFolding(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            ParseEscapedLineBreak(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            Return str
        End Function

        Private Function ParseSingleQuotedText(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing

            str = ParseSingleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            str = ParseSingleQuotedMultiLine(success)
            If success Then
                ClearError(errorCount)
                Return str
            End If

            Return str
        End Function

        Private Function ParseSingleQuotedSingleLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal("'"c, success)
            If Not success Then
                [Error]("Failed to parse ''' of SingleQuotedSingleLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet("'" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapedSingleQuote(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            MatchTerminal("'"c, success)
            If Not success Then
                [Error]("Failed to parse ''' of SingleQuotedSingleLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseSingleQuotedMultiLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParseSingleQuotedMultiLineFist(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse SingleQuotedMultiLineFist of SingleQuotedMultiLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParseSingleQuotedMultiLineInner(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            str = ParseSingleQuotedMultiLineLast(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse SingleQuotedMultiLineLast of SingleQuotedMultiLine.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseSingleQuotedMultiLineFist(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal("'"c, success)
            If Not success Then
                [Error]("Failed to parse ''' of SingleQuotedMultiLineFist.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet(" '" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapedSingleQuote(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    While True
                        Dim seq_start_position1 As Integer = Position
                        ch = MatchTerminal(" "c, success)
                        If success Then
                            text.Append(ch)
                        Else
                            [Error]("Failed to parse ' ' of SingleQuotedMultiLineFist.")
                            Exit While
                        End If

                        Dim not_start_position2 As Integer = Position
                        While True
                            ParseIgnoredBlank(success)

                            ParseLineBreak(success)
                            Exit While
                        End While
                        Position = not_start_position2
                        success = Not success
                        If Not success Then
                            [Error]("Failed to parse !((IgnoredBlank LineBreak)) of SingleQuotedMultiLineFist.")
                            Position = seq_start_position1
                        End If
                        Exit While
                    End While
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            ParseIgnoredBlank(success)

            Dim fold As String = ParseLineFolding(success)
            If success Then
                text.Append(fold)
            Else
                [Error]("Failed to parse fold of SingleQuotedMultiLineFist.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseSingleQuotedMultiLineInner(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of SingleQuotedMultiLineInner.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            Dim counter As Integer = 0
            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet(" '" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapedSingleQuote(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    While True
                        Dim seq_start_position1 As Integer = Position
                        ch = MatchTerminal(" "c, success)
                        If success Then
                            text.Append(ch)
                        Else
                            [Error]("Failed to parse ' ' of SingleQuotedMultiLineInner.")
                            Exit While
                        End If

                        Dim not_start_position2 As Integer = Position
                        While True
                            ParseIgnoredBlank(success)

                            ParseLineBreak(success)
                            Exit While
                        End While
                        Position = not_start_position2
                        success = Not success
                        If Not success Then
                            [Error]("Failed to parse !((IgnoredBlank LineBreak)) of SingleQuotedMultiLineInner.")
                            Position = seq_start_position1
                        End If
                        Exit While
                    End While
                    If success Then
                        ClearError(errorCount)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse ((-"" '" & vbCr & vbLf & """ / EscapedSingleQuote / ' ' !((IgnoredBlank LineBreak))))+ of SingleQuotedMultiLineInner.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            Dim fold As String = ParseLineFolding(success)
            If success Then
                text.Append(fold)
            Else
                [Error]("Failed to parse fold of SingleQuotedMultiLineInner.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseSingleQuotedMultiLineLast(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of SingleQuotedMultiLineLast.")
                Position = start_position
                Return text.ToString()
            End If

            ParseIgnoredBlank(success)

            While True
                ErrorStatck.Push(errorCount)
                errorCount = Errors.Count
                While True
                    Dim ch As Char = MatchTerminalSet("'" & vbCr & vbLf, True, success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    ch = ParseEscapedSingleQuote(success)
                    If success Then
                        ClearError(errorCount)
                        text.Append(ch)
                        Exit While
                    End If

                    Exit While
                End While
                errorCount = ErrorStatck.Pop()
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            MatchTerminal("'"c, success)
            If Not success Then
                [Error]("Failed to parse ''' of SingleQuotedMultiLineLast.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseLineFolding(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                str = ParseReservedLineBreak(success)
                If Not success Then
                    [Error]("Failed to parse ReservedLineBreak of LineFolding.")
                    Exit While
                End If

                Dim counter As Integer = 0
                While True
                    While True
                        Dim seq_start_position2 As Integer = Position
                        ParseIgnoredBlank(success)

                        ParseLineBreak(success)
                        If Not success Then
                            [Error]("Failed to parse LineBreak of LineFolding.")
                            Position = seq_start_position2
                        End If
                        Exit While
                    End While
                    If Not success Then
                        Exit While
                    End If
                    counter += 1
                End While
                If counter > 0 Then
                    success = True
                End If
                If Not success Then
                    [Error]("Failed to parse ((IgnoredBlank LineBreak))+ of LineFolding.")
                    Position = seq_start_position1
                    Exit While
                End If

                Dim and_start_position3 As Integer = Position
                ParseIndent(success)
                Position = and_start_position3
                If Not success Then
                    [Error]("Failed to parse &(Indent) of LineFolding.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return str
            End If

            While True
                Dim seq_start_position4 As Integer = Position
                ParseLineBreak(success)
                If Not success Then
                    [Error]("Failed to parse LineBreak of LineFolding.")
                    Exit While
                End If

                Dim and_start_position5 As Integer = Position
                ParseIndent(success)
                Position = and_start_position5
                If success Then
                    Return " "
                Else
                    [Error]("Failed to parse &(Indent) of LineFolding.")
                    Position = seq_start_position4
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return str
            End If

            Return str
        End Function

        Private Function ParseEscapedSingleQuote(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar
            MatchTerminalString("''", success)
            If success Then
                ClearError(errorCount)
                Return "'"c
            Else
                [Error]("Failed to parse '''' of EscapedSingleQuote.")
            End If
            Return ch
        End Function

        Private Sub ParseEscapedLineBreak(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            MatchTerminal("\"c, success)
            If Not success Then
                [Error]("Failed to parse '\\' of EscapedLineBreak.")
                Position = start_position
                Return
            End If

            ParseLineBreak(success)
            If Not success Then
                [Error]("Failed to parse LineBreak of EscapedLineBreak.")
                Position = start_position
                Return
            End If

            While True
                While True
                    Dim seq_start_position1 As Integer = Position
                    ParseIgnoredBlank(success)

                    ParseLineBreak(success)
                    If Not success Then
                        [Error]("Failed to parse LineBreak of EscapedLineBreak.")
                        Position = seq_start_position1
                    End If
                    Exit While
                End While
                If Not success Then
                    Exit While
                End If
            End While
            success = True

        End Sub

        Private Function ParseLiteralText(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal("|"c, success)
            If Not success Then
                [Error]("Failed to parse '|' of LiteralText.")
                Position = start_position
                Return text.ToString()
            End If

            Dim modifier As BlockScalarModifier = ParseBlockScalarModifier(success)
            AddIndent(modifier, success)
            success = True

            ParseInlineComment(success)
            If Not success Then
                [Error]("Failed to parse InlineComment of LiteralText.")
                Position = start_position
                Return text.ToString()
            End If

            Dim str As String = ParseLiteralContent(success)
            DecreaseIndent()
            If success Then
                text.Append(str)
            End If
            success = True

            Return text.ToString()
        End Function

        Private Function ParseFoldedText(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            MatchTerminal(">"c, success)
            If Not success Then
                [Error]("Failed to parse '>' of FoldedText.")
                Position = start_position
                Return text.ToString()
            End If

            Dim modifier As BlockScalarModifier = ParseBlockScalarModifier(success)
            AddIndent(modifier, success)
            success = True

            ParseInlineComment(success)
            If Not success Then
                [Error]("Failed to parse InlineComment of FoldedText.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                While True
                    Dim str_2 As String = ParseEmptyLineBlock(success)
                    If success Then
                        text.Append(str_2)
                    Else
                        Exit While
                    End If
                End While
                success = True

                Dim str As String = ParseFoldedLines(success)
                If success Then
                    text.Append(str)
                Else
                    [Error]("Failed to parse FoldedLines of FoldedText.")
                    Position = seq_start_position1
                    Exit While
                End If

                str = ParseChompedLineBreak(success)
                If success Then
                    text.Append(str)
                Else
                    [Error]("Failed to parse ChompedLineBreak of FoldedText.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseComments(success)
                success = True
                Exit While
            End While
            DecreaseIndent()
            If Not success Then
                [Error]("Failed to parse (((EmptyLineBlock))* FoldedLines ChompedLineBreak (Comments)?) of FoldedText.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseBlockScalarModifier(ByRef success As Boolean) As BlockScalarModifier
            Dim errorCount As Integer = Errors.Count
            Dim blockScalarModifier As New BlockScalarModifier()

            While True
                Dim seq_start_position1 As Integer = Position
                blockScalarModifier.Indent = ParseIndentIndicator(success)
                If Not success Then
                    [Error]("Failed to parse Indent of BlockScalarModifier.")
                    Exit While
                End If

                blockScalarModifier.Chomp = ParseChompingIndicator(success)
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return blockScalarModifier
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                blockScalarModifier.Chomp = ParseChompingIndicator(success)
                If Not success Then
                    [Error]("Failed to parse Chomp of BlockScalarModifier.")
                    Exit While
                End If

                blockScalarModifier.Indent = ParseIndentIndicator(success)
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return blockScalarModifier
            End If

            Return blockScalarModifier
        End Function

        Private Function ParseLiteralContent(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParseLiteralFirst(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse LiteralFirst of LiteralContent.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParseLiteralInner(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            Dim str2 As String = ParseChompedLineBreak(success)
            If success Then
                text.Append(str2)
            Else
                [Error]("Failed to parse str2 of LiteralContent.")
                Position = start_position
                Return text.ToString()
            End If

            ParseComments(success)
            success = True

            Return text.ToString()
        End Function

        Private Function ParseLiteralFirst(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            While True
                Dim str As String = ParseEmptyLineBlock(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of LiteralFirst.")
                Position = start_position
                Return text.ToString()
            End If

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseNonBreakChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse (NonBreakChar)+ of LiteralFirst.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseLiteralInner(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str As String = ParseReservedLineBreak(success)
            If success Then
                text.Append(str)
            Else
                [Error]("Failed to parse ReservedLineBreak of LiteralInner.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                str = ParseEmptyLineBlock(success)
                If success Then
                    text.Append(str)
                Else
                    Exit While
                End If
            End While
            success = True

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of LiteralInner.")
                Position = start_position
                Return text.ToString()
            End If

            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseNonBreakChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If Not success Then
                [Error]("Failed to parse (NonBreakChar)+ of LiteralInner.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return text.ToString()
        End Function

        Private Function ParseFoldedLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of FoldedLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                Dim ch As Char = ParseNonBreakChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParseFoldedLines(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str2 As String = ParseFoldedLine(success)
            If success Then
                text.Append(str2)
            Else
                [Error]("Failed to parse str2 of FoldedLines.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                While True
                    Dim seq_start_position1 As Integer = Position
                    Dim str As String = ParseLineFolding(success)
                    If success Then
                        text.Append(str)
                    Else
                        [Error]("Failed to parse LineFolding of FoldedLines.")
                        Exit While
                    End If

                    str = ParseFoldedLine(success)
                    If success Then
                        text.Append(str)
                    Else
                        [Error]("Failed to parse FoldedLine of FoldedLines.")
                        Position = seq_start_position1
                    End If
                    Exit While
                End While
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParseSpacedLine(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of SpacedLine.")
                Position = start_position
                Return text.ToString()
            End If

            ParseBlank(success)
            If Not success Then
                [Error]("Failed to parse Blank of SpacedLine.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                Dim ch As Char = ParseNonBreakChar(success)
                If success Then
                    text.Append(ch)
                Else
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParseSpacedLines(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()
            Dim start_position As Integer = Position

            Dim str2 As String = ParseSpacedLine(success)
            If success Then
                text.Append(str2)
            Else
                [Error]("Failed to parse str2 of SpacedLines.")
                Position = start_position
                Return text.ToString()
            End If

            While True
                While True
                    Dim seq_start_position1 As Integer = Position
                    ParseLineBreak(success)
                    If Not success Then
                        [Error]("Failed to parse LineBreak of SpacedLines.")
                        Exit While
                    End If

                    Dim str As String = ParseSpacedLine(success)
                    If success Then
                        text.Append(str)
                    Else
                        [Error]("Failed to parse SpacedLine of SpacedLines.")
                        Position = seq_start_position1
                    End If
                    Exit While
                End While
                If Not success Then
                    Exit While
                End If
            End While
            success = True

            Return text.ToString()
        End Function

        Private Function ParseIndentIndicator(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalRange("1"c, "9"c, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse '1'...'9' of IndentIndicator.")
            End If
            Return ch
        End Function

        Private Function ParseChompingIndicator(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = MatchTerminal("-"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = MatchTerminal("+"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Function ParseFlowSequence(ByRef success As Boolean) As Sequence
            Dim errorCount As Integer = Errors.Count
            Dim sequence As New Sequence()
            Dim start_position As Integer = Position

            MatchTerminal("["c, success)
            If Not success Then
                [Error]("Failed to parse '[' of FlowSequence.")
                Position = start_position
                Return sequence
            End If

            ParseSeparationLinesInFlow(success)
            success = True

            While True
                Dim seq_start_position1 As Integer = Position
                Dim dataItem As DataItem = ParseFlowSequenceEntry(success)
                If success Then
                    sequence.Enties.Add(dataItem)
                Else
                    [Error]("Failed to parse FlowSequenceEntry of FlowSequence.")
                    Exit While
                End If

                While True
                    While True
                        Dim seq_start_position2 As Integer = Position
                        MatchTerminal(","c, success)
                        If Not success Then
                            [Error]("Failed to parse ',' of FlowSequence.")
                            Exit While
                        End If

                        ParseSeparationLinesInFlow(success)
                        success = True

                        dataItem = ParseFlowSequenceEntry(success)
                        If success Then
                            sequence.Enties.Add(dataItem)
                        Else
                            [Error]("Failed to parse FlowSequenceEntry of FlowSequence.")
                            Position = seq_start_position2
                        End If
                        Exit While
                    End While
                    If Not success Then
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While
            If Not success Then
                [Error]("Failed to parse Enties of FlowSequence.")
                Position = start_position
                Return sequence
            End If

            MatchTerminal("]"c, success)
            If Not success Then
                [Error]("Failed to parse ']' of FlowSequence.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return sequence
        End Function

        Private Function ParseFlowSequenceEntry(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                dataItem = ParseFlowNodeInFlow(success)
                If Not success Then
                    [Error]("Failed to parse FlowNodeInFlow of FlowSequenceEntry.")
                    Exit While
                End If

                ParseSeparationLinesInFlow(success)
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            ParseFlowSingPair(success)
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseBlockSequence(ByRef success As Boolean) As Sequence
            Dim errorCount As Integer = Errors.Count
            Dim sequence As New Sequence()

            While True
                Dim seq_start_position1 As Integer = Position
                Dim dataItem As DataItem = ParseBlockSequenceEntry(success)
                If success Then
                    sequence.Enties.Add(dataItem)
                Else
                    [Error]("Failed to parse BlockSequenceEntry of BlockSequence.")
                    Exit While
                End If

                While True
                    While True
                        Dim seq_start_position2 As Integer = Position
                        ParseIndent(success)
                        If Not success Then
                            [Error]("Failed to parse Indent of BlockSequence.")
                            Exit While
                        End If

                        dataItem = ParseBlockSequenceEntry(success)
                        If success Then
                            sequence.Enties.Add(dataItem)
                        Else
                            [Error]("Failed to parse BlockSequenceEntry of BlockSequence.")
                            Position = seq_start_position2
                        End If
                        Exit While
                    End While
                    If Not success Then
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse Enties of BlockSequence.")
            End If
            Return sequence
        End Function

        Private Function ParseBlockSequenceEntry(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            MatchTerminal("-"c, success)
            If Not success Then
                [Error]("Failed to parse '-' of BlockSequenceEntry.")
                Position = start_position
                Return dataItem
            End If

            dataItem = ParseBlockCollectionEntry(success)
            If Not success Then
                [Error]("Failed to parse BlockCollectionEntry of BlockSequenceEntry.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseFlowMapping(ByRef success As Boolean) As Mapping
            Dim errorCount As Integer = Errors.Count
            Dim mapping As New Mapping()
            Dim start_position As Integer = Position

            MatchTerminal("{"c, success)
            If Not success Then
                [Error]("Failed to parse '{' of FlowMapping.")
                Position = start_position
                Return mapping
            End If

            ParseSeparationLinesInFlow(success)
            success = True

            While True
                Dim seq_start_position1 As Integer = Position
                Dim mappingEntry As MappingEntry = ParseFlowMappingEntry(success)
                If success Then
                    mapping.Enties.Add(mappingEntry)
                Else
                    [Error]("Failed to parse FlowMappingEntry of FlowMapping.")
                    Exit While
                End If

                While True
                    While True
                        Dim seq_start_position2 As Integer = Position
                        MatchTerminal(","c, success)
                        If Not success Then
                            [Error]("Failed to parse ',' of FlowMapping.")
                            Exit While
                        End If

                        ParseSeparationLinesInFlow(success)
                        success = True

                        mappingEntry = ParseFlowMappingEntry(success)
                        If success Then
                            mapping.Enties.Add(mappingEntry)
                        Else
                            [Error]("Failed to parse FlowMappingEntry of FlowMapping.")
                            Position = seq_start_position2
                        End If
                        Exit While
                    End While
                    If Not success Then
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While
            If Not success Then
                [Error]("Failed to parse Enties of FlowMapping.")
                Position = start_position
                Return mapping
            End If

            MatchTerminal("}"c, success)
            If Not success Then
                [Error]("Failed to parse '}' of FlowMapping.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return mapping
        End Function

        Private Function ParseFlowMappingEntry(ByRef success As Boolean) As MappingEntry
            Dim errorCount As Integer = Errors.Count
            Dim mappingEntry As New MappingEntry()

            While True
                Dim seq_start_position1 As Integer = Position
                mappingEntry.Key = ParseExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseExplicitValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of FlowMappingEntry.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                mappingEntry.Key = ParseExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseEmptyFlow(success)
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                mappingEntry.Key = ParseSimpleKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseExplicitValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of FlowMappingEntry.")
                    Position = seq_start_position3
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position4 As Integer = Position
                mappingEntry.Key = ParseSimpleKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseEmptyFlow(success)
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            Return mappingEntry
        End Function

        Private Function ParseExplicitKey(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                MatchTerminal("?"c, success)
                If Not success Then
                    [Error]("Failed to parse '?' of ExplicitKey.")
                    Exit While
                End If

                ParseSeparationLinesInFlow(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLinesInFlow of ExplicitKey.")
                    Position = seq_start_position1
                    Exit While
                End If

                dataItem = ParseFlowNodeInFlow(success)
                If Not success Then
                    [Error]("Failed to parse FlowNodeInFlow of ExplicitKey.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseSeparationLinesInFlow(success)
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                MatchTerminal("?"c, success)
                If Not success Then
                    [Error]("Failed to parse '?' of ExplicitKey.")
                    Exit While
                End If

                dataItem = ParseEmptyFlow(success)

                ParseSeparationLinesInFlow(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLinesInFlow of ExplicitKey.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseSimpleKey(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            dataItem = ParseFlowKey(success)
            If Not success Then
                [Error]("Failed to parse FlowKey of SimpleKey.")
                Position = start_position
                Return dataItem
            End If

            ParseSeparationLinesInFlow(success)
            success = True

            Return dataItem
        End Function

        Private Function ParseFlowKey(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As New Scalar()

            scalar.Text = ParsePlainTextInFlowSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseDoubleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseSingleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParseBlockKey(ByRef success As Boolean) As Scalar
            Dim errorCount As Integer = Errors.Count
            Dim scalar As New Scalar()

            scalar.Text = ParsePlainTextSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseDoubleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            scalar.Text = ParseSingleQuotedSingleLine(success)
            If success Then
                ClearError(errorCount)
                Return scalar
            End If

            Return scalar
        End Function

        Private Function ParseExplicitValue(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing

            While True
                Dim seq_start_position1 As Integer = Position
                MatchTerminal(":"c, success)
                If Not success Then
                    [Error]("Failed to parse ':' of ExplicitValue.")
                    Exit While
                End If

                ParseSeparationLinesInFlow(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLinesInFlow of ExplicitValue.")
                    Position = seq_start_position1
                    Exit While
                End If

                dataItem = ParseFlowNodeInFlow(success)
                If Not success Then
                    [Error]("Failed to parse FlowNodeInFlow of ExplicitValue.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseSeparationLinesInFlow(success)
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                MatchTerminal(":"c, success)
                If Not success Then
                    [Error]("Failed to parse ':' of ExplicitValue.")
                    Exit While
                End If

                dataItem = ParseEmptyFlow(success)

                ParseSeparationLinesInFlow(success)
                If Not success Then
                    [Error]("Failed to parse SeparationLinesInFlow of ExplicitValue.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return dataItem
            End If

            Return dataItem
        End Function

        Private Function ParseFlowSingPair(ByRef success As Boolean) As MappingEntry
            Dim errorCount As Integer = Errors.Count
            Dim mappingEntry As New MappingEntry()

            While True
                Dim seq_start_position1 As Integer = Position
                mappingEntry.Key = ParseExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowSingPair.")
                    Exit While
                End If

                mappingEntry.Value = ParseExplicitValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of FlowSingPair.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                mappingEntry.Key = ParseExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowSingPair.")
                    Exit While
                End If

                mappingEntry.Value = ParseEmptyFlow(success)
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                mappingEntry.Key = ParseSimpleKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of FlowSingPair.")
                    Exit While
                End If

                mappingEntry.Value = ParseExplicitValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of FlowSingPair.")
                    Position = seq_start_position3
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            Return mappingEntry
        End Function

        Private Function ParseBlockMapping(ByRef success As Boolean) As Mapping
            Dim errorCount As Integer = Errors.Count
            Dim mapping As New Mapping()

            While True
                Dim seq_start_position1 As Integer = Position
                Dim mappingEntry As MappingEntry = ParseBlockMappingEntry(success)
                If success Then
                    mapping.Enties.Add(mappingEntry)
                Else
                    [Error]("Failed to parse BlockMappingEntry of BlockMapping.")
                    Exit While
                End If

                While True
                    While True
                        Dim seq_start_position2 As Integer = Position
                        ParseIndent(success)
                        If Not success Then
                            [Error]("Failed to parse Indent of BlockMapping.")
                            Exit While
                        End If

                        mappingEntry = ParseBlockMappingEntry(success)
                        If success Then
                            mapping.Enties.Add(mappingEntry)
                        Else
                            [Error]("Failed to parse BlockMappingEntry of BlockMapping.")
                            Position = seq_start_position2
                        End If
                        Exit While
                    End While
                    If Not success Then
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse Enties of BlockMapping.")
            End If
            Return mapping
        End Function

        Private Function ParseBlockMappingEntry(ByRef success As Boolean) As MappingEntry
            Dim errorCount As Integer = Errors.Count
            Dim mappingEntry As New MappingEntry()

            While True
                Dim seq_start_position1 As Integer = Position
                mappingEntry.Key = ParseBlockExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of BlockMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseBlockExplicitValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of BlockMappingEntry.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                mappingEntry.Key = ParseBlockExplicitKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of BlockMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseEmptyFlow(success)
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position3 As Integer = Position
                mappingEntry.Key = ParseBlockSimpleKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of BlockMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseBlockSimpleValue(success)
                If Not success Then
                    [Error]("Failed to parse Value of BlockMappingEntry.")
                    Position = seq_start_position3
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            While True
                Dim seq_start_position4 As Integer = Position
                mappingEntry.Key = ParseBlockSimpleKey(success)
                If Not success Then
                    [Error]("Failed to parse Key of BlockMappingEntry.")
                    Exit While
                End If

                mappingEntry.Value = ParseEmptyBlock(success)
                If Not success Then
                    [Error]("Failed to parse Value of BlockMappingEntry.")
                    Position = seq_start_position4
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return mappingEntry
            End If

            Return mappingEntry
        End Function

        Private Function ParseBlockExplicitKey(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            MatchTerminal("?"c, success)
            If Not success Then
                [Error]("Failed to parse '?' of BlockExplicitKey.")
                Position = start_position
                Return dataItem
            End If

            dataItem = ParseBlockCollectionEntry(success)
            If Not success Then
                [Error]("Failed to parse BlockCollectionEntry of BlockExplicitKey.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseBlockExplicitValue(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            ParseIndent(success)
            If Not success Then
                [Error]("Failed to parse Indent of BlockExplicitValue.")
                Position = start_position
                Return dataItem
            End If

            MatchTerminal(":"c, success)
            If Not success Then
                [Error]("Failed to parse ':' of BlockExplicitValue.")
                Position = start_position
                Return dataItem
            End If

            dataItem = ParseBlockCollectionEntry(success)
            If Not success Then
                [Error]("Failed to parse BlockCollectionEntry of BlockExplicitValue.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseBlockSimpleKey(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = Nothing
            Dim start_position As Integer = Position

            dataItem = ParseBlockKey(success)
            If Not success Then
                [Error]("Failed to parse BlockKey of BlockSimpleKey.")
                Position = start_position
                Return dataItem
            End If

            ParseSeparationLines(success)
            success = True

            MatchTerminal(":"c, success)
            If Not success Then
                [Error]("Failed to parse ':' of BlockSimpleKey.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return dataItem
        End Function

        Private Function ParseBlockSimpleValue(ByRef success As Boolean) As DataItem
            Dim errorCount As Integer = Errors.Count
            Dim dataItem As DataItem = ParseBlockCollectionEntry(success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse BlockCollectionEntry of BlockSimpleValue.")
            End If
            Return dataItem
        End Function

        Private Sub ParseComment(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            Dim not_start_position1 As Integer = Position
            success = Not Input.HasInput(Position)
            Position = not_start_position1
            success = Not success
            If Not success Then
                [Error]("Failed to parse !(eof) of Comment.")
                Position = start_position
                Return
            End If

            ParseIgnoredSpace(success)

            While True
                Dim seq_start_position2 As Integer = Position
                MatchTerminal("#"c, success)
                If Not success Then
                    [Error]("Failed to parse '#' of Comment.")
                    Exit While
                End If

                While True
                    ParseNonBreakChar(success)
                    If Not success Then
                        Exit While
                    End If
                End While
                success = True
                Exit While
            End While
            success = True

            ErrorStatck.Push(errorCount)
            errorCount = Errors.Count
            While True
                ParseLineBreak(success)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                success = Not Input.HasInput(Position)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                Exit While
            End While
            errorCount = ErrorStatck.Pop()
            If Not success Then
                [Error]("Failed to parse (LineBreak / eof) of Comment.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
        End Sub

        Private Sub ParseInlineComment(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            While True
                Dim seq_start_position1 As Integer = Position
                ParseSeparationSpace(success)
                If Not success Then
                    [Error]("Failed to parse SeparationSpace of InlineComment.")
                    Exit While
                End If

                While True
                    Dim seq_start_position2 As Integer = Position
                    MatchTerminal("#"c, success)
                    If Not success Then
                        [Error]("Failed to parse '#' of InlineComment.")
                        Exit While
                    End If

                    While True
                        ParseNonBreakChar(success)
                        If Not success Then
                            Exit While
                        End If
                    End While
                    success = True
                    Exit While
                End While
                success = True
                Exit While
            End While
            success = True

            ErrorStatck.Push(errorCount)
            errorCount = Errors.Count
            While True
                ParseLineBreak(success)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                success = Not Input.HasInput(Position)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                Exit While
            End While
            errorCount = ErrorStatck.Pop()
            If Not success Then
                [Error]("Failed to parse (LineBreak / eof) of InlineComment.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
        End Sub

        Private Sub ParseComments(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim counter As Integer = 0
            While True
                ParseComment(success)
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse (Comment)+ of Comments.")
            End If
        End Sub

        Private Sub ParseInlineComments(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            ParseInlineComment(success)
            If Not success Then
                [Error]("Failed to parse InlineComment of InlineComments.")
                Position = start_position
                Return
            End If

            While True
                ParseComment(success)
                If Not success Then
                    Exit While
                End If
            End While
            success = True

        End Sub

        Private Function ParseInteger(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim chars As New List(Of Char)()
            Dim counter As Integer = 0
            While True
                Dim ch As Char = ParseDigit(success)
                If success Then
                    chars.Add(ch)
                Else
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
                Return New String(chars.ToArray())
            Else
                [Error]("Failed to parse chars of Integer.")
            End If
            Return text.ToString()
        End Function

        Private Function ParseWordChar(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = ParseLetter(success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = ParseDigit(success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = MatchTerminal("-"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Function ParseLetter(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = MatchTerminalRange("a"c, "z"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = MatchTerminalRange("A"c, "Z"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Function ParseDigit(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalRange("0"c, "9"c, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse '0'...'9' of Digit.")
            End If
            Return ch
        End Function

        Private Function ParseHexDigit(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = MatchTerminalRange("0"c, "9"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = MatchTerminalRange("A"c, "F"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            ch = MatchTerminalRange("a"c, "f"c, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Function ParseUriChar(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = ParseWordChar(success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                MatchTerminal("%"c, success)
                If Not success Then
                    [Error]("Failed to parse '%' of UriChar.")
                    Exit While
                End If

                Dim char1 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char1 of UriChar.")
                    Position = seq_start_position1
                    Exit While
                End If

                Dim char2 As Char = ParseHexDigit(success)
                If success Then
                    ch = Convert.ToChar(Integer.Parse(String.Format("{0}{1}", char1, char2), System.Globalization.NumberStyles.HexNumber))
                Else
                    [Error]("Failed to parse char2 of UriChar.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            MatchTerminalSet(";/?:@&=+$,_.!~*'()[]", False, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Function ParseTagChar(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            ch = ParseWordChar(success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                MatchTerminal("%"c, success)
                If Not success Then
                    [Error]("Failed to parse '%' of TagChar.")
                    Exit While
                End If

                Dim char1 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char1 of TagChar.")
                    Position = seq_start_position1
                    Exit While
                End If

                Dim char2 As Char = ParseHexDigit(success)
                If success Then
                    ch = Convert.ToChar(Integer.Parse(String.Format("{0}{1}", char1, char2), System.Globalization.NumberStyles.HexNumber))
                Else
                    [Error]("Failed to parse char2 of TagChar.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            MatchTerminalSet(";/?:@&=+$,_.~*'()[]", False, success)
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function

        Private Sub ParseEmptyLinePlain(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            ParseIgnoredSpace(success)

            ParseNormalizedLineBreak(success)
            If Not success Then
                [Error]("Failed to parse NormalizedLineBreak of EmptyLinePlain.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
        End Sub

        Private Sub ParseEmptyLineQuoted(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim start_position As Integer = Position

            ParseIgnoredBlank(success)

            ParseNormalizedLineBreak(success)
            If Not success Then
                [Error]("Failed to parse NormalizedLineBreak of EmptyLineQuoted.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
        End Sub

        Private Function ParseEmptyLineBlock(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim str As String = Nothing
            Dim start_position As Integer = Position

            ParseIgnoredSpace(success)

            str = ParseReservedLineBreak(success)
            If Not success Then
                [Error]("Failed to parse ReservedLineBreak of EmptyLineBlock.")
                Position = start_position
            End If

            If success Then
                ClearError(errorCount)
            End If
            Return str
        End Function

        Private Function ParseNonSpaceChar(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalSet(" " & vbTab & vbCr & vbLf, True, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse -"" " & vbTab & vbCr & vbLf & """ of NonSpaceChar.")
            End If
            Return ch
        End Function

        Private Function ParseNonSpaceSep(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalSet(vbCr & vbLf & vbTab & " ,[]{}", True, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse -""" & vbCr & vbLf & vbTab & " ,[]{}"" of NonSpaceSep.")
            End If
            Return ch
        End Function

        Private Function ParseNonBreakChar(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalSet(vbCr & vbLf, True, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse -""" & vbCr & vbLf & """ of NonBreakChar.")
            End If
            Return ch
        End Function

        Private Sub ParseIgnoredSpace(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            While True
                MatchTerminal(" "c, success)
                If Not success Then
                    Exit While
                End If
            End While
            success = True
        End Sub

        Private Sub ParseIgnoredBlank(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            While True
                MatchTerminalSet(" " & vbTab, False, success)
                If Not success Then
                    Exit While
                End If
            End While
            success = True
        End Sub

        Private Sub ParseSeparationSpace(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim counter As Integer = 0
            While True
                MatchTerminal(" "c, success)
                If Not success Then
                    Exit While
                End If
                counter += 1
            End While
            If counter > 0 Then
                success = True
            End If
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse (' ')+ of SeparationSpace.")
            End If
        End Sub

        Private Sub ParseSeparationLines(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            While True
                Dim seq_start_position1 As Integer = Position
                ParseInlineComments(success)
                If Not success Then
                    [Error]("Failed to parse InlineComments of SeparationLines.")
                    Exit While
                End If

                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of SeparationLines.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return
            End If

            ParseSeparationSpace(success)
            If success Then
                ClearError(errorCount)
                Return
            End If

        End Sub

        Private Sub ParseSeparationLinesInFlow(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            While True
                Dim seq_start_position1 As Integer = Position
                ParseInlineComments(success)
                If success Then
                    detectIndent = False
                Else
                    [Error]("Failed to parse InlineComments of SeparationLinesInFlow.")
                    Exit While
                End If

                ParseIndent(success)
                If Not success Then
                    [Error]("Failed to parse Indent of SeparationLinesInFlow.")
                    Position = seq_start_position1
                    Exit While
                End If

                ParseIgnoredSpace(success)
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return
            End If

            ParseSeparationSpace(success)
            If success Then
                ClearError(errorCount)
                Return
            End If

        End Sub

        Private Sub ParseSeparationSpaceAsIndent(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            Dim counter As Integer = 0
            Dim c As Char

            While True
                c = MatchTerminal(" "c, success)

                'If Not success AndAlso c = ASCII.LF OrElse c = ASCII.CR Then
                '    MatchTerminal(ASCII.LF, True)
                '    MatchTerminal(ASCII.CR, True)

                '    ' 2018-7-28
                '    ' 因为MatchTerminal返回来的就是当前的位置的字符
                '    ' 所以假若if条件通过的话，上面的对lf或者cr的匹配肯定成功
                '    ' 所以在这里可以直接将success设置为真
                '    ' 在这里先后匹配crlf，是为了兼容windows上面的crlf换行
                '    success = True
                'End If

                If success Then
                    currentIndent += 1
                Else
                    Exit While
                End If
                counter += 1
            End While

            If counter > 0 Then
                success = True
            End If

            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse ((' '))+ of SeparationSpaceAsIndent.")
            End If
        End Sub

        Private Sub ParseIndent(ByRef success As Boolean)
            success = ParseIndent()
            Dim errorCount As Integer = Errors.Count
        End Sub

        Private Function ParseSpace(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminal(" "c, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse ' ' of Space.")
            End If
            Return ch
        End Function

        Private Function ParseBlank(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = MatchTerminalSet(" " & vbTab, False, success)
            If success Then
                ClearError(errorCount)
            Else
                [Error]("Failed to parse "" " & vbTab & """ of Blank.")
            End If
            Return ch
        End Function

        Private Sub ParseLineBreak(ByRef success As Boolean)
            Dim errorCount As Integer = Errors.Count
            MatchTerminalString(vbCr & vbLf, success)
            If success Then
                ClearError(errorCount)
                Return
            End If

            MatchTerminal(ControlChars.Cr, success)
            If success Then
                ClearError(errorCount)
                Return
            End If

            MatchTerminal(ControlChars.Lf, success)
            If success Then
                ClearError(errorCount)
                Return
            End If

        End Sub

        Private Function ParseReservedLineBreak(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            Dim text As New StringBuilder()

            Dim str As String = MatchTerminalString(vbCr & vbLf, success)
            If success Then
                ClearError(errorCount)
                text.Append(str)
                Return text.ToString()
            End If

            Dim ch As Char = MatchTerminal(ControlChars.Cr, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch)
                Return text.ToString()
            End If

            ch = MatchTerminal(ControlChars.Lf, success)
            If success Then
                ClearError(errorCount)
                text.Append(ch)
                Return text.ToString()
            End If

            Return text.ToString()
        End Function

        Private Function ParseChompedLineBreak(ByRef success As Boolean) As String
            Dim errorCount As Integer = Errors.Count
            ErrorStatck.Push(errorCount)
            errorCount = Errors.Count
            Dim text As New StringBuilder()

            While True
                While True
                    Dim seq_start_position1 As Integer = Position
                    Dim str As String = ParseReservedLineBreak(success)
                    If success Then
                        text.Append(str)
                    Else
                        [Error]("Failed to parse ReservedLineBreak of ChompedLineBreak.")
                        Exit While
                    End If

                    While True
                        While True
                            Dim seq_start_position2 As Integer = Position
                            ParseIgnoredSpace(success)

                            str = ParseReservedLineBreak(success)
                            If success Then
                                text.Append(str)
                            Else
                                [Error]("Failed to parse ReservedLineBreak of ChompedLineBreak.")
                                Position = seq_start_position2
                            End If
                            Exit While
                        End While
                        If Not success Then
                            Exit While
                        End If
                    End While
                    success = True
                    Exit While
                End While
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                success = Not Input.HasInput(Position)
                If success Then
                    ClearError(errorCount)
                    Exit While
                End If

                Exit While
            End While
            errorCount = ErrorStatck.Pop()
            Return Chomp(text.ToString())
        End Function

        Private Function ParseNormalizedLineBreak(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar
            ParseLineBreak(success)
            If success Then
                ClearError(errorCount)
                Return ControlChars.Lf
            Else
                [Error]("Failed to parse LineBreak of NormalizedLineBreak.")
            End If
            Return ch
        End Function

        Private Function ParseEscapeSequence(ByRef success As Boolean) As Char
            Dim errorCount As Integer = Errors.Count
            Dim ch As Char = ControlChars.NullChar

            MatchTerminalString("\\", success)
            If success Then
                Return "\"c
            End If

            MatchTerminalString("\'", success)
            If success Then
                Return "'"c
            End If

            MatchTerminalString("\""", success)
            If success Then
                Return """"c
            End If

            MatchTerminalString("\r", success)
            If success Then
                Return ControlChars.Cr
            End If

            MatchTerminalString("\n", success)
            If success Then
                Return ControlChars.Lf
            End If

            MatchTerminalString("\t", success)
            If success Then
                Return ControlChars.Tab
            End If

            MatchTerminalString("\v", success)
            If success Then
                Return ControlChars.VerticalTab
            End If

            MatchTerminalString("\a", success)
            If success Then
                Return ChrW(7)
            End If

            MatchTerminalString("\b", success)
            If success Then
                Return ControlChars.Back
            End If

            MatchTerminalString("\f", success)
            If success Then
                Return ControlChars.FormFeed
            End If

            MatchTerminalString("\0", success)
            If success Then
                Return ControlChars.NullChar
            End If

            MatchTerminalString("\/", success)
            If success Then
                Return "/"c
            End If

            MatchTerminalString("\ ", success)
            If success Then
                Return " "c
            End If

            MatchTerminalString("\" & vbTab, success)
            If success Then
                Return ControlChars.Tab
            End If

            MatchTerminalString("\_", success)
            If success Then
                Return " "c
            End If

            MatchTerminalString("\e", success)
            If success Then
                Return ChrW(27)
            End If

            MatchTerminalString("\N", success)
            If success Then
                Return ChrW(133)
            End If

            ' http://stackoverflow.com/questions/14188786/creating-newlines-in-pdf-with-vb-net
            ' Try To add \u2028 instead:
            '
            ' ```vb.net
            ' Private Function ConcatPlacardNumbers(BusinessPlacardCollection As _
            '                                       BusinessPlacardCollection) As String
            '
            '     Dim PlacardNumbersList As New StringBuilder()
            '
            '     For Each BusinessPlacard As BusinessPlacard In BusinessPlacardCollection
            '
            '         PlacardNumbersList.Append(BusinessPlacard.PlacardNumber)
            '         ' PlacardNumbersList.Append(ChrW(8232)) '\u2028 line in decimal form
            '         PlacardNumbersList.Append(ChrW(8233)) '\u2029 paragr. in decimal form
            '
            '     Next
            '
            '     Return PlacardNumbersList.ToString
            '
            ' End Function
            ' 
            ' For paragraphs use \u2029instead. Fore more details
            ' http://blogs.adobe.com/formfeed/2009/01/paragraph_breaks_in_plain_text.html

            MatchTerminalString("\L", success)
            If success Then
                Return ChrW(8232)
            End If

            MatchTerminalString("\P", success)
            If success Then
                Return ChrW(8233)
            End If

            While True
                Dim seq_start_position1 As Integer = Position
                MatchTerminalString("\x", success)
                If Not success Then
                    [Error]("Failed to parse '\\x' of EscapeSequence.")
                    Exit While
                End If

                Dim char1 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char1 of EscapeSequence.")
                    Position = seq_start_position1
                    Exit While
                End If

                Dim char2 As Char = ParseHexDigit(success)
                If success Then
                    Return Convert.ToChar(Integer.Parse(String.Format("{0}{1}", char1, char2), System.Globalization.NumberStyles.HexNumber))
                Else
                    [Error]("Failed to parse char2 of EscapeSequence.")
                    Position = seq_start_position1
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            While True
                Dim seq_start_position2 As Integer = Position
                MatchTerminalString("\u", success)
                If Not success Then
                    [Error]("Failed to parse '\\u' of EscapeSequence.")
                    Exit While
                End If

                Dim char1 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char1 of EscapeSequence.")
                    Position = seq_start_position2
                    Exit While
                End If

                Dim char2 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char2 of EscapeSequence.")
                    Position = seq_start_position2
                    Exit While
                End If

                Dim char3 As Char = ParseHexDigit(success)
                If Not success Then
                    [Error]("Failed to parse char3 of EscapeSequence.")
                    Position = seq_start_position2
                    Exit While
                End If

                Dim char4 As Char = ParseHexDigit(success)
                If success Then
                    Return Convert.ToChar(Integer.Parse(String.Format("{0}{1}{2}{3}", char1, char2, char3, char4), System.Globalization.NumberStyles.HexNumber))
                Else
                    [Error]("Failed to parse char4 of EscapeSequence.")
                    Position = seq_start_position2
                End If
                Exit While
            End While
            If success Then
                ClearError(errorCount)
                Return ch
            End If

            Return ch
        End Function
    End Class
End Namespace
