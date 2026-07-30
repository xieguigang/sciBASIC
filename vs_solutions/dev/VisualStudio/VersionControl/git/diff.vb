#Region "Microsoft.VisualBasic::5a3b2c1d8e9f0a4b7c6d5e2f1a3b4c7d8e9f0a1, vs_solutions\dev\VisualStudio\VersionControl\git\diff.vb"

' Author:
' 
' asuka (amethyst.asuka@gcmodeller.org)
' xie (genetics@smrucc.org)
' xieguigang (xie.guigang@live.com)
' 
' Copyright (c)2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

' Total Lines:260
' Code Lines:200 (76.92%)
' Comment Lines:40 (15.38%)
' - Xml Docs:95.00%
' 
' Blank Lines:20 (7.69%)
' File Size:8.50 KB


' Enum DiffLineType
' 
' Added, Deleted, Context
' 
' 
' Enum FileChangeKind
' 
' Added, Modified, Deleted, Renamed
' 
' 
' Class DiffLine
' 
' Properties: Type, Content
' 
' 
' Class DiffHunk
' 
' Properties: OldStart, OldCount, NewStart, NewCount, Lines
' 
' 
' Class FileChange
' 
' Properties: FilePath, ChangeKind, Hunks
' 
' 
' Class DiffResult
' 
' Properties: Files
' 
' Function: GetAllAddedLines, GetAllDeletedLines
' 
' 
' Module diff
' 
' Function: GetDiff, ParseDiffText
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language

Namespace VersionControl.Git

    ''' <summary>
    ''' Represents the type of a single diff line.
    ''' </summary>
    Public Enum DiffLineType
        ''' <summary>
        ''' A line that was added (prefixed with '+' in diff output).
        ''' </summary>
        Added
        ''' <summary>
        ''' A line that was deleted (prefixed with '-' in diff output).
        ''' </summary>
        Deleted
        ''' <summary>
        ''' An unchanged context line (prefixed with ' ' in diff output).
        ''' </summary>
        Context
    End Enum

    ''' <summary>
    ''' Represents the kind of change applied to a file.
    ''' </summary>
    Public Enum FileChangeKind
        ''' <summary>
        ''' The file was added (new file).
        ''' </summary>
        Added
        ''' <summary>
        ''' The file was modified (existing file changed).
        ''' </summary>
        Modified
        ''' <summary>
        ''' The file was deleted.
        ''' </summary>
        Deleted
        ''' <summary>
        ''' The file was renamed.
        ''' </summary>
        Renamed
    End Enum

    ''' <summary>
    ''' Represents a single line in a git diff output.
    ''' </summary>
    Public Class DiffLine

        ''' <summary>
        ''' The type of change for this line.
        ''' </summary>
        Public Property Type As DiffLineType

        ''' <summary>
        ''' The raw content of the line (without the leading '+', '-', or ' ' prefix).
        ''' </summary>
        Public Property Content As String

        Public Overrides Function ToString() As String
            Dim prefix As String = If(Type = DiffLineType.Added, "+",
            If(Type = DiffLineType.Deleted, "-", " "))
            Return prefix & Content
        End Function
    End Class

    ''' <summary>
    ''' Represents a contiguous section (hunk) of changes in a file diff.
    ''' </summary>
    Public Class DiffHunk

        ''' <summary>
        ''' The starting line number in the old (original) file.
        ''' </summary>
        Public Property OldStart As Integer

        ''' <summary>
        ''' The number of lines this hunk occupies in the old file.
        ''' </summary>
        Public Property OldCount As Integer

        ''' <summary>
        ''' The starting line number in the new (modified) file.
        ''' </summary>
        Public Property NewStart As Integer

        ''' <summary>
        ''' The number of lines this hunk occupies in the new file.
        ''' </summary>
        Public Property NewCount As Integer

        ''' <summary>
        ''' The list of diff lines in this hunk.
        ''' </summary>
        Public Property Lines As List(Of DiffLine)

        Public Overrides Function ToString() As String
            Return $"@@ -{OldStart},{OldCount} +{NewStart},{NewCount} @@ ({Lines.Count} lines)"
        End Function
    End Class

    ''' <summary>
    ''' Represents the diff changes for a single file.
    ''' </summary>
    Public Class FileChange

        ''' <summary>
        ''' The relative file path that was changed.
        ''' </summary>
        Public Property FilePath As String

        ''' <summary>
        ''' The kind of change applied to this file.
        ''' </summary>
        Public Property ChangeKind As FileChangeKind

        ''' <summary>
        ''' The list of hunks (sections of changes) for this file.
        ''' </summary>
        Public Property Hunks As List(Of DiffHunk)

        Public Overrides Function ToString() As String
            Return $"{ChangeKind}: {FilePath} ({Hunks.Count} hunks)"
        End Function
    End Class

    ''' <summary>
    ''' Represents the complete parsed result of a git diff command.
    ''' </summary>
    Public Class DiffResult

        ''' <summary>
        ''' The list of file changes in this diff.
        ''' </summary>
        Public Property Files As List(Of FileChange)

        ''' <summary>
        ''' Get all added lines across all files.
        ''' </summary>
        ''' <returns>An enumerable of tuples (file path, line content).</returns>
        Public Iterator Function GetAllAddedLines() As IEnumerable(Of (file As String, content As String))
            For Each file In Files
                For Each hunk In file.Hunks
                    For Each line In hunk.Lines
                        If line.Type = DiffLineType.Added Then
                            Yield (file.FilePath, line.Content)
                        End If
                    Next
                Next
            Next
        End Function

        ''' <summary>
        ''' Get all deleted lines across all files.
        ''' </summary>
        ''' <returns>An enumerable of tuples (file path, line content).</returns>
        Public Iterator Function GetAllDeletedLines() As IEnumerable(Of (file As String, content As String))
            For Each file In Files
                For Each hunk In file.Hunks
                    For Each line In hunk.Lines
                        If line.Type = DiffLineType.Deleted Then
                            Yield (file.FilePath, line.Content)
                        End If
                    Next
                Next
            Next
        End Function
    End Class

    ''' <summary>
    ''' Module for invoking git diff command and parsing its output into structured objects.
    ''' </summary>
    Public Module diff

        Private ReadOnly HunkPattern As New Regex("^@@ -(\d+),?(\d*) \+(\d+),?(\d*) @@", RegexOptions.Compiled)
        Private ReadOnly DiffHeaderPattern As New Regex("^diff --git a/(.+?) b/(.+?)$", RegexOptions.Compiled)
        Private ReadOnly NewFilePattern As New Regex("^new file mode \d+$", RegexOptions.Compiled)
        Private ReadOnly DeletedFilePattern As New Regex("^deleted file mode \d+$", RegexOptions.Compiled)
        Private ReadOnly RenameFromPattern As New Regex("^rename from (.+)$", RegexOptions.Compiled)
        Private ReadOnly RenameToPattern As New Regex("^rename to (.+)$", RegexOptions.Compiled)

        ''' <summary>
        ''' Run <c>git diff</c> on the specified folder path and parse the output.
        ''' </summary>
        ''' <param name="directory">The repository folder path to run git diff against.</param>
        ''' <param name="cached">If <c>True</c>, runs <c>git diff --cached</c> (staged changes).</param>
        ''' <returns>A <see cref="DiffResult"/> containing all parsed file changes.</returns>
        Public Function GetDiff(directory As String, Optional cached As Boolean = False, Optional git As String = "git") As DiffResult
            Dim args$ = If(cached, "diff --cached", "diff")
            Dim output As String = PipelineProcess.Call(git, args, workdir:=directory)

            If output.StringEmpty Then
                Return New DiffResult With {.Files = New List(Of FileChange)}
            End If

            Return ParseDiffText(output)
        End Function

        ''' <summary>
        ''' Parse raw git diff text into a structured <see cref="DiffResult"/>.
        ''' </summary>
        ''' <param name="diffText">The raw output from <c>git diff</c>.</param>
        ''' <returns>A <see cref="DiffResult"/> containing all parsed file changes.</returns>
        Public Function ParseDiffText(diffText As String) As DiffResult
            Dim files As New List(Of FileChange)
            Dim lines As String() = diffText.LineTokens

            If lines.Length = 0 Then
                Return New DiffResult With {.Files = files}
            End If

            Dim currentFile As FileChange = Nothing
            Dim currentHunk As DiffHunk = Nothing
            Dim inHunk As Boolean = False
            Dim inHeader As Boolean = False
            Dim renameFrom As String = Nothing

            For Each rawLine As String In lines
                ' Skip empty lines in the diff output
                If rawLine.StringEmpty Then
                    Continue For
                End If

                ' Detect file header: diff --git a/path b/path
                Dim headerMatch As Match = DiffHeaderPattern.Match(rawLine)
                If headerMatch.Success Then
                    ' Save previous file if exists
                    If currentFile IsNot Nothing Then
                        files.Add(currentFile)
                    End If

                    currentFile = New FileChange With {
                        .FilePath = headerMatch.Groups(2).Value,
                        .ChangeKind = FileChangeKind.Modified,
                        .Hunks = New List(Of DiffHunk)
                    }
                    currentHunk = Nothing
                    inHunk = False
                    inHeader = True
                    renameFrom = Nothing
                    Continue For
                End If

                If currentFile Is Nothing Then
                    Continue For
                End If

                ' Detect new file
                If NewFilePattern.IsMatch(rawLine) Then
                    currentFile.ChangeKind = FileChangeKind.Added
                    Continue For
                End If

                ' Detect deleted file
                If DeletedFilePattern.IsMatch(rawLine) Then
                    currentFile.ChangeKind = FileChangeKind.Deleted
                    Continue For
                End If

                ' Detect rename metadata
                Dim renameFromMatch As Match = RenameFromPattern.Match(rawLine)
                If renameFromMatch.Success Then
                    renameFrom = renameFromMatch.Groups(1).Value
                    Continue For
                End If

                Dim renameToMatch As Match = RenameToPattern.Match(rawLine)
                If renameToMatch.Success Then
                    currentFile.FilePath = renameToMatch.Groups(1).Value
                    currentFile.ChangeKind = FileChangeKind.Renamed
                    Continue For
                End If

                ' Detect hunk header: @@ -old,count +new,count @@
                Dim hunkMatch As Match = HunkPattern.Match(rawLine)
                If hunkMatch.Success Then
                    Dim oldStart As Integer = Integer.Parse(hunkMatch.Groups(1).Value)
                    Dim oldCountStr As String = hunkMatch.Groups(2).Value
                    Dim newStart As Integer = Integer.Parse(hunkMatch.Groups(3).Value)
                    Dim newCountStr As String = hunkMatch.Groups(4).Value

                    Dim oldCount As Integer = If(oldCountStr.StringEmpty, 1, Integer.Parse(oldCountStr))
                    Dim newCount As Integer = If(newCountStr.StringEmpty, 1, Integer.Parse(newCountStr))

                    currentHunk = New DiffHunk With {
                    .OldStart = oldStart,
                    .OldCount = oldCount,
                    .NewStart = newStart,
                    .NewCount = newCount,
                    .Lines = New List(Of DiffLine)
                    }
                    currentFile.Hunks.Add(currentHunk)
                    inHunk = True
                    inHeader = False
                    Continue For
                End If

                ' Skip other header lines (index, ---, +++) if we're still in header
                If inHeader AndAlso (rawLine.StartsWith("index ") OrElse
                rawLine.StartsWith("--- ") OrElse
                rawLine.StartsWith("+++ ") OrElse
                rawLine.StartsWith("new file ") OrElse
                rawLine.StartsWith("deleted file ") OrElse
                rawLine.StartsWith("rename ")) Then
                    Continue For
                End If

                ' Parse diff content lines
                If inHunk AndAlso currentHunk IsNot Nothing Then
                    If rawLine.Length > 0 Then
                        Dim prefix As Char = rawLine(0)
                        Select Case prefix
                            Case "+"c
                                currentHunk.Lines.Add(New DiffLine With {
                                .Type = DiffLineType.Added,
                                .Content = rawLine.Substring(1)
                                })
                            Case "-"c
                                currentHunk.Lines.Add(New DiffLine With {
                                .Type = DiffLineType.Deleted,
                                .Content = rawLine.Substring(1)
                                })
                            Case " "c
                                currentHunk.Lines.Add(New DiffLine With {
                                .Type = DiffLineType.Context,
                                .Content = rawLine.Substring(1)
                                })
                            Case "\"c
                                ' Skip "\ No newline at end of file" marker
                                Continue For
                        End Select
                    End If
                End If
            Next

            ' Add the last file if present
            If currentFile IsNot Nothing Then
                files.Add(currentFile)
            End If

            Return New DiffResult With {.Files = files}
        End Function
    End Module
End Namespace
