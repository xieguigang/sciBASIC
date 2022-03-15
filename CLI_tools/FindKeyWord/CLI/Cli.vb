#Region "Microsoft.VisualBasic::6455ffdb01b6c717eaa9f2a113674400, sciBASIC#\CLI_tools\FindKeyWord\CLI\Cli.vb"

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

    '   Total Lines: 243
    '    Code Lines: 190
    ' Comment Lines: 10
    '   Blank Lines: 43
    '     File Size: 9.95 KB


    ' Module CLI
    ' 
    '     Function: __foundKeyMatch, __foundRegex, (+2 Overloads) Found, Peeks, Tails
    '               Trim
    ' 
    '     Sub: __emptyAction
    '     Class FoundResult
    ' 
    '         Properties: File, Index
    ' 
    '         Function: GenerateOutput
    ' 
    '     Structure FileIndex
    ' 
    '         Properties: Line, TextLine
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

<PackageNamespace("TextFile.Utilities", Category:=APICategories.CLI_MAN,
                  Url:="",
                  Description:="Utilities tool for the text file, including finding keywords, large size text file viewer.",
                  Publisher:="xie.guigang@gmail.com")>
Module CLI

    <ExportAPI("Peeks", Usage:="Peeks /in <input.txt> [/length 1024 /out <out.txt>]")>
    Public Function Peeks(args As CommandLine) As Integer
        Dim Input As String = args("/in")
        Dim Out As String = args("/out")
        Dim Len As Integer = args.GetValue("/length", 1024)

        If String.IsNullOrEmpty(Out) Then
            Out = Input.TrimFileExt & ".out.txt"
        End If

        Call Microsoft.VisualBasic.Peeks(Input, Len).SaveTo(Out)

        Return App.Exit(0)
    End Function

    <ExportAPI("Find",
               Info:="Find target file by search text content in the files.",
               Usage:="Find /regex /filtering --key <expression> [--dir <dir> --ext <ext_list>]")>
    <ParameterInfo("--ext", True, Description:="No format limitations.")>
    <ParameterInfo("--dir", True,
                   Description:="The directory which the files searches for, if this parameter is not presented, then the current work directory will be used.")>
    <ParameterInfo("--key", False,
                   Description:="The keyword or regular expression that using for the text search.")>
    Public Function Found(argvs As CommandLine) As Integer
        Dim Key As String = argvs("--key")
        Dim DIR As String = argvs.GetValue("--dir", App.CurrentDirectory)
        Dim Ext As String = argvs("--ext")
        Dim Regex As Boolean = argvs.GetBoolean("/regex")
        Dim FilteringExt As Boolean = argvs.GetBoolean("/filtering")

        If String.IsNullOrEmpty(Key) Then
            Call Console.WriteLine("Please input a not null keyword/regex expression.")
            Return -10
        End If

        Dim Result = Found(Keyword:=Key,
                           DIR:=DIR,
                           FilteringExt:=FilteringExt,
                           _extList:=Ext,
                           _usingRegex:=Regex,
                           Process:=Sub(out, percentage) Call out.__DEBUG_ECHO)
        Dim Output As String = String.Join(vbCrLf, (From item In Result Select item.GenerateOutput).ToArray)

        Call Console.WriteLine(Output)

        Return 0
    End Function

    Public Class FoundResult
        Public Property File As String
        Public Property Index As FileIndex()

        Public Function GenerateOutput() As String
            Dim File As String = Me.File.CliPath
            Dim LQuery = (From idx In Index Select $"{File}{vbTab}{idx.Line}{vbTab}{idx.TextLine }")
            Return String.Join(vbCrLf, LQuery)
        End Function
    End Class

    Public Structure FileIndex
        Public Property Line As Integer
        Public Property TextLine As String
    End Structure

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Keyword"></param>
    ''' <param name="DIR"></param>
    ''' <param name="_extList">a.ext;b.ext;c.ext</param>
    ''' <param name="_usingRegex"></param>
    ''' <param name="FilteringExt"></param>
    ''' <returns></returns>
    Public Function Found(Keyword As String,
                          DIR As String,
                          _extList As String,
                          _usingRegex As Boolean,
                          FilteringExt As Boolean,
                          Optional Process As Action(Of String, Integer) = Nothing) As FoundResult()

        If Process Is Nothing Then
            Process = AddressOf __emptyAction
        End If

        Call Process("Scanning for files...", 1)

        Dim Files = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchAllSubDirectories).ToArray
        Dim ExtList As String()
        Dim FileNumbers As Long = Files.Count

        If Not String.IsNullOrEmpty(_extList) Then
            ExtList = (From s In _extList.Trim.Split(";"c) Select s.Trim.Split("."c).Last.ToLower).ToArray
        Else
            ExtList = New String() {}
        End If

        If FilteringExt Then  '出现的文件名后缀都过滤掉
            Files = (From file As String In Files.AsParallel
                     Let ext = file.Split("."c).Last.ToLower
                     Where Array.IndexOf(ExtList, ext) = -1
                     Select file).ToArray
        Else '只搜索指定的后缀名

            If Not ExtList.IsNullOrEmpty Then
                Files = (From file As String In Files.AsParallel
                         Let ext = file.Split("."c).Last.ToLower
                         Where Array.IndexOf(ExtList, ext) > -1
                         Select file).ToArray
            End If

        End If

        Dim FoundResult As FoundResult()

        Call Process("Pre-processing files...", 1)

        Dim GetFiles As String() = (From path As String In Files.AsParallel
                                    Where path.IsTextFile
                                    Select path).ToArray
        Dim percentage As Integer
        Dim sw = Stopwatch.StartNew

        If _usingRegex Then  '使用正则表达式
            FoundResult = (From File As String In GetFiles.AsParallel
                           Let result = __foundRegex(percentage, GetFiles.Length, File, Keyword, Process)
                           Where Not result Is Nothing
                           Select result).ToArray
        Else
            FoundResult = (From File As String In GetFiles.AsParallel
                           Let result = __foundKeyMatch(percentage, GetFiles.Length, File, Keyword, Process)
                           Where Not result Is Nothing
                           Select result).ToArray
        End If

        Call Process($"Search job done: Processing {FileNumbers} files，using time {sw.ElapsedMilliseconds}ms!", 100)

        Return FoundResult
    End Function

    Private Function __foundRegex(ByRef percentage As Integer, Counts As Integer,
                                  File As String,
                                  Keyword As String,
                                  Process As Action(Of String, Integer)) As FoundResult

        Dim ChunkBuffer As String = IO.File.ReadAllText(File)
        Dim Find As String() = (From m As Match
                                In Regex.Matches(ChunkBuffer, Keyword, RegexOptions.Singleline)
                                Select m.Value).ToArray

        Call Threading.Interlocked.Increment(percentage)
        Call Process($"{File.ToFileURL} searched...", 100 * percentage / Counts)

        If Find.Length > 0 Then
            Return New FoundResult With {
                .File = File,
                .Index = (From s As String
                          In Find
                          Select New FileIndex With {
                              .Line = InStr(ChunkBuffer, s),
                              .TextLine = s}).ToArray
            }
        Else
            Return Nothing
        End If
    End Function

    Private Function __foundKeyMatch(ByRef percentage As Integer, Counts As Integer,
                                     File As String,
                                     Keyword As String,
                                     Process As Action(Of String, Integer)) As FoundResult

        Dim ChunkBuffer As String() = IO.File.ReadAllLines(File)
        Dim Find As FileIndex() = (From i As Integer In ChunkBuffer.Sequence
                                   Let Line As String = ChunkBuffer(i)
                                   Where Not String.IsNullOrEmpty(Line) AndAlso InStr(Line, Keyword, CompareMethod.Text) > 0
                                   Select New FileIndex With {.Line = i, .TextLine = Line}).ToArray

        Call Threading.Interlocked.Increment(percentage)
        Call Process($"{File.ToFileURL} searched...", 100 * percentage / Counts)

        If Find.Length > 0 Then
            Return New FoundResult With {
                .File = File,
                .Index = Find
            }
        Else
            Return Nothing
        End If
    End Function

    Private Sub __emptyAction(process As String, percentage As Integer)
        ' DO_NOTHING
    End Sub

    <ExportAPI("/Tails", Usage:="/Tails /in <in.txt> [/len 1024 /out <out.txt>]")>
    Public Function Tails(args As CommandLine) As Integer
        Dim inFile As String = args("/in")
        Dim out As String = args("/out")
        Dim len As Integer = args.GetValue("/len", 1024)
        Dim value As String = Microsoft.VisualBasic.Tails(inFile, len)

        If String.IsNullOrEmpty(out) Then
            Call Console.WriteLine()
            Call Console.WriteLine(value)
        Else
            Call value.SaveTo(out)
        End If

        Return 0
    End Function

    <ExportAPI("/Trim", Usage:="/Trim /in <in.txt> [/out <out.txt>]")>
    Public Function Trim(args As CommandLine) As Integer
        Dim [in] As String = args - "/in"
        Dim out As String = args.GetValue("/out", [in].TrimFileExt & "-TextTrim.txt")

        Using writer As StreamWriter = out.OpenWriter
            Dim array As New List(Of Char)(ASCII.Nonprintings)

            For Each c As Char In [in].ForEachChar
                If array.IndexOf(c) = -1 Then
                    Call writer.Write(c)
                End If
            Next

            Return 0
        End Using
    End Function
End Module
