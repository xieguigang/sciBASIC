#Region "Microsoft.VisualBasic::cabf613c321cb5689240618dc38bdca7, vs_solutions\dev\VisualStudio\VersionControl\git\log.vb"

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

    '   Total Lines: 76
    '    Code Lines: 20 (26.32%)
    ' Comment Lines: 48 (63.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (10.53%)
    '     File Size: 4.14 KB


    '     Class log
    ' 
    '         Properties: [date], author, commit, message
    ' 
    '         Function: ParseGitLogText, ParseSvnLogText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

Namespace VersionControl.Git

    ''' <summary>
    ''' Represents a single log entry from a version control system, 
    ''' containing the commit identifier, author, timestamp, and commit message.
    ''' This model is designed to be compatible with both <c>git log</c> and <c>svn log</c> output formats.
    ''' </summary>
    Public Class log

        ''' <summary>
        ''' Gets or sets the commit identifier (e.g., the SHA-1 hash for Git, or the revision number for SVN).
        ''' </summary>
        ''' <returns>A <see cref="String"/>   representing the unique identifier of this commit.</returns>
        Public Property commit As String

        ''' <summary>
        ''' Gets or sets the name of the author who created this commit.
        ''' </summary>
        ''' <returns>A <see cref="String"/>   containing the author's name or username.</returns>
        Public Property author As String

        ''' <summary>
        ''' Gets or sets the date and time when this commit was created.
        ''' </summary>
        ''' <returns>A <see cref="Date"/>   value representing the commit timestamp.</returns>
        Public Property [date] As Date

        ''' <summary>
        ''' Gets or sets the commit message or log description associated with this commit.
        ''' </summary>
        ''' <returns>A <see cref="String"/>   containing the commit message text.</returns>
        Public Property message As String

        ''' <summary>
        ''' Parses the raw output text from <c>git log [fileName]</c> command and 
        ''' converts it into a sequence of <see cref="log"/>   objects.
        ''' </summary>
        ''' <param name="text">
        ''' The raw text output from the <c>git log</c> command. Each log block is expected to 
        ''' start with a line beginning with <c>"commit "</c>, followed by author, date, and 
        ''' message lines in standard Git log format.
        ''' </param>
        ''' <returns>
        ''' An <see cref="IEnumerable(Of log)"/>   sequence of parsed <see cref="log"/>   entries 
        ''' extracted from the input text.
        ''' </returns>
        Public Shared Iterator Function ParseGitLogText(text As String) As IEnumerable(Of log)
            For Each block As String() In text.LineIterators.Split(Function(line) line.StartsWith("commit "), DelimiterLocation.NextFirst)
                Yield New log With {.commit = block(Scan0).Trim.Split.Last, .author = block(1).GetTagValue(":", trim:=True).Value, .[date] = Date.Parse(block(2).GetTagValue(":", trim:=True).Value), .message = block.Skip(3).Select(AddressOf Strings.Trim).Where(Function(s) Not s.StringEmpty).JoinBy("; ")}
            Next
        End Function

        ''' <summary>
        ''' Parses the raw output text from <c>svn log [fileName]</c> command and 
        ''' converts it into a sequence of <see cref="log"/>   objects.
        ''' </summary>
        ''' <param name="text">
        ''' The raw text output from the <c>svn log</c> command. Log blocks are delimited by 
        ''' lines of dashes (<c>"----------"</c>). Each block starts with a header line 
        ''' containing the revision number, author, and date separated by pipe (<c>"|"</c>) 
        ''' characters, followed by the commit message body.
        ''' </param>
        ''' <returns>
        ''' An <see cref="IEnumerable(Of log)"/>   sequence of parsed <see cref="log"/>   entries 
        ''' extracted from the input text.
        ''' </returns>
        Public Shared Iterator Function ParseSvnLogText(text As String) As IEnumerable(Of log)
            For Each block As String() In text.LineIterators.Split(Function(line) line.IsPattern("[-]+"), DelimiterLocation.NotIncludes)
                Dim tokens As String() = block(Scan0).Split("|"c).Select(AddressOf Strings.Trim).ToArray
                Yield New log With {.commit = tokens(Scan0), .author = tokens(1), .[date] = Date.Parse(tokens(2)), .message = block.Skip(1).Select(AddressOf Strings.Trim).JoinBy(vbCrLf).Trim(" ", ASCII.TAB, ASCII.CR, ASCII.LF)}
            Next
        End Function
    End Class
End Namespace
