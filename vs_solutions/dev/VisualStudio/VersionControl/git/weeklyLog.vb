#Region "Microsoft.VisualBasic::0ad41caf6b00f4c140a103f13e2e935b, vs_solutions\dev\VisualStudio\VersionControl\git\weeklyLog.vb"

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

    '   Total Lines: 162
    '    Code Lines: 72 (44.44%)
    ' Comment Lines: 67 (41.36%)
    '    - Xml Docs: 91.04%
    ' 
    '   Blank Lines: 23 (14.20%)
    '     File Size: 7.48 KB


    '     Class commitEntry
    ' 
    '         Properties: AddedLines, changes, DeletedLines, meta
    ' 
    '     Module weeklyLog
    ' 
    '         Function: GetWeeklyLog, ParseCommitBlock, ParseWeeklyLogText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language

Namespace VersionControl.Git

    ''' <summary>
    ''' Represents a single commit entry parsed from <c>git log --since="..." -p</c> output, 
    ''' aggregating the commit metadata, the list of changed files with their diff content, 
    ''' and the added/deleted line counts for convenient weekly-report generation.
    ''' </summary>
    Public Class commitEntry

        ''' <summary>
        ''' Gets or sets the commit metadata (hash, author, date, message).
        ''' </summary>
        ''' <returns>A <see cref="log"/>   object describing the commit.</returns>
        Public Property meta As log

        ''' <summary>
        ''' Gets or sets the parsed patch (diff) content for this commit, including the 
        ''' list of changed files and their hunks of changes.
        ''' </summary>
        ''' <returns>A <see cref="DiffResult"/>   containing all changed files of this commit.</returns>
        Public Property changes As DiffResult

        ''' <summary>
        ''' Gets or sets the total number of added lines across all changed files in this commit.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/>   count of added lines.</returns>
        Public Property AddedLines As Integer

        ''' <summary>
        ''' Gets or sets the total number of deleted lines across all changed files in this commit.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/>   count of deleted lines.</returns>
        Public Property DeletedLines As Integer
    End Class

    ''' <summary>
    ''' Module for parsing <c>git log --since="..." -p</c> output into structured commit entries,
    ''' combining the existing <see cref="log"/>   metadata parser and the <see cref="diff"/>   patch parser.
    ''' </summary>
    Public Module weeklyLog

        ''' <summary>
        ''' Parse the raw output text from <c>git log --since="..." -p</c> and convert it into a 
        ''' sequence of <see cref="commitEntry"/>   objects, one per commit, each carrying its 
        ''' metadata, changed files, diff content and added/deleted line counts.
        ''' </summary>
        ''' <param name="text">
        ''' The raw text output from the <c>git log ... -p</c> command. Each commit block starts 
        ''' with a line beginning with <c>"commit "</c>, followed by author/date/message lines and, 
        ''' when the <c>-p</c> option is present, the patch sections starting with 
        ''' <c>"diff --git a/..."</c>.
        ''' </param>
        ''' <returns>
        ''' An <see cref="IEnumerable(Of commitEntry)"/>   sequence of parsed commit entries 
        ''' extracted from the input text.
        ''' </returns>
        Public Iterator Function ParseWeeklyLogText(text As String) As IEnumerable(Of commitEntry)
            If text.StringEmpty Then
                Return
            End If

            ' Split the text into per-commit blocks, each block starts at a "commit " line.
            For Each block As String() In text.LineIterators.Split(Function(line) line.StartsWith("commit "), DelimiterLocation.NextFirst)
                Dim entry As commitEntry = ParseCommitBlock(block)

                If entry IsNot Nothing Then
                    Yield entry
                End If
            Next
        End Function

        ''' <summary>
        ''' Parse a single commit block (an array of text lines) into a <see cref="commitEntry"/>.
        ''' The metadata portion (before the first <c>"diff --git"</c> line) is parsed via the 
        ''' existing <see cref="log.ParseGitLogText"/>   logic, while the patch portion is parsed 
        ''' via the existing <see cref="diff.ParseDiffText"/>   logic.
        ''' </summary>
        Private Function ParseCommitBlock(block As String()) As commitEntry
            ' Locate the boundary between the commit metadata and the patch.
            Dim diffIndex As Integer = Array.FindIndex(block, Function(line) line.StartsWith("diff --git "))

            Dim metaLines As String()
            Dim diffLines As String()

            If diffIndex < 0 Then
                ' No patch in this commit (e.g. a merge commit).
                metaLines = block
                diffLines = New String() {}
            Else
                metaLines = block.Take(diffIndex).ToArray
                diffLines = block.Skip(diffIndex).ToArray
            End If

            ' Parse metadata. log.ParseGitLogText yields one log per block; rebuild the block text.
            Dim meta As log = log.ParseGitLogText(metaLines.JoinBy(vbCrLf)).FirstOrDefault

            If meta Is Nothing Then
                Return Nothing
            End If

            ' Parse the patch (diff) portion.
            Dim changes As DiffResult

            If diffLines.Length = 0 Then
                changes = New DiffResult With {.Files = New List(Of FileChange)}
            Else
                changes = diff.ParseDiffText(diffLines.JoinBy(vbCrLf))
            End If

            ' Count added/deleted lines across all changed files.
            Dim added As Integer = 0
            Dim deleted As Integer = 0

            For Each file As FileChange In changes.Files
                For Each hunk As DiffHunk In file.Hunks
                    For Each line As DiffLine In hunk.Lines
                        If line.Type = DiffLineType.Added Then
                            added += 1
                        ElseIf line.Type = DiffLineType.Deleted Then
                            deleted += 1
                        End If
                    Next
                Next
            Next

            Return New commitEntry With {
                .meta = meta,
                .changes = changes,
                .AddedLines = added,
                .DeletedLines = deleted
            }
        End Function

        ''' <summary>
        ''' Run <c>git --no-pager log --since="&lt;since&gt;" -p</c> on the specified repository 
        ''' directory and parse the output into structured commit entries.
        ''' </summary>
        ''' <param name="directory">The repository folder path to run the git log command against.</param>
        ''' <param name="since">
        ''' The <c>--since</c> argument value, e.g. <c>"1 week ago"</c>, <c>"2 days ago"</c> or a 
        ''' concrete date <c>"2026-07-29"</c>. Defaults to <c>"1 week ago"</c>.
        ''' </param>
        ''' <param name="git">The git executable name or path. Defaults to <c>"git"</c>.</param>
        ''' <returns>
        ''' An <see cref="IEnumerable(Of commitEntry)"/>   sequence of parsed commit entries 
        ''' from the last week (or the period specified by <paramref name="since"/>).
        ''' </returns>
        Public Function GetWeeklyLog(directory As String, Optional since As String = "1 week ago", Optional git As String = "git") As IEnumerable(Of commitEntry)
            Dim args$ = $"--no-pager log --since=""{since}"" -p -- {directory.GetDirectoryFullPath}"
            Dim output As String = PipelineProcess.Call(git, args, workdir:=directory)

            If output.StringEmpty Then
                Return New List(Of commitEntry)
            End If

            Return ParseWeeklyLogText(output)
        End Function
    End Module
End Namespace
