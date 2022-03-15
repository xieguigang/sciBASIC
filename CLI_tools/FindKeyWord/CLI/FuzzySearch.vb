#Region "Microsoft.VisualBasic::c77fbd61bc40f29251e18374e234196b, sciBASIC#\CLI_tools\FindKeyWord\CLI\FuzzySearch.vb"

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

    '   Total Lines: 72
    '    Code Lines: 57
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.67 KB


    ' Module CLI
    ' 
    '     Function: FoundEMails, FoundURLs, Search
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Text

Partial Module CLI

    <ExportAPI("--search",
               Usage:="--search /in <in.txt/text> /text <content.txt/text> /out <out.txt> [/min <3> /max <20> /cutoff <0.6>]")>
    Public Function Search(args As CommandLine.CommandLine) As Integer
        Dim inStream As String = args("/in")
        Dim outStream As String = args("/out")
        Dim text As String = args("/text")
        Dim min As Double = args.GetValue("/min", 3)
        Dim max As Double = args.GetValue("/max", 20)
        Dim cutoff As Double = args.GetValue("/cutoff", 0.6)

        If inStream.FileExists Then
            inStream = FileIO.FileSystem.ReadAllText(inStream)
        End If

        If text.FileExists Then
            text = FileIO.FileSystem.ReadAllText(text)
        End If

        Dim result = TextIndexing.FuzzyIndex(text, inStream, cutoff:=cutoff, min:=min, max:=max)
        Dim sbr As StringBuilder = New StringBuilder(10240)

        For Each Line In result
            Call sbr.AppendLine($"{Line.Key.Index}{vbTab}{Line.Key.Segment}{vbTab}{Line.Value.Distance}{vbTab}{Line.Value.Score}{vbTab}{Line.Value.Matches}{vbTab}{Line.Value.DistEdits}")
        Next

        Return sbr.SaveTo(outStream).CLICode
    End Function

    <ExportAPI("/Find.Email", Usage:="/Find.Email /in <inFile/inText> [/out <out.txt>]")>
    Public Function FoundEMails(args As CommandLine.CommandLine) As Integer
        Dim inData As String = args("/in")
        Dim out As String = args("/out")

        If inData.FileExists Then
            inData = FileIO.FileSystem.ReadAllText(inData)
        End If

        Dim urls As String() = StringHelpers.GetEMails(inData)
        If Not String.IsNullOrEmpty(out) Then
            Call IO.File.WriteAllLines(out, urls)
        Else
            Call urls.JoinBy(vbCrLf).__DEBUG_ECHO
        End If

        Return 0
    End Function

    <ExportAPI("/Find.URL", Usage:="/Find.URL /in <inFile/inText> [/out <out.txt>]")>
    Public Function FoundURLs(args As CommandLine.CommandLine) As Integer
        Dim inData As String = args("/in")
        Dim out As String = args("/out")

        If inData.FileExists Then
            inData = FileIO.FileSystem.ReadAllText(inData)
        End If

        Dim urls As String() = StringHelpers.GetURLs(inData)
        If Not String.IsNullOrEmpty(out) Then
            Call IO.File.WriteAllLines(out, urls)
        Else
            Call urls.JoinBy(vbCrLf).__DEBUG_ECHO
        End If

        Return 0
    End Function
End Module
