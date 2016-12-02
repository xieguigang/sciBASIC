#Region "Microsoft.VisualBasic::d2c7fa94fdcef75dbfd4958e582f794c, ..\sciBASIC#\CLI_tools\vbproj\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    Const Condition$ = " '$(Configuration)|$(Platform)' == '%s|%s' "

    <ExportAPI("/config.output",
               Usage:="/config.output /in <*.vbproj/DIR> /output <DIR> /c 'config=<Name>;platform=<type>'")>
    Public Function ConfigOutputPath(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim output As String = args("/output")
        Dim c As Dictionary(Of String, String) = args.GetDictionary("/c")
        Dim files$()
        Dim condition$

        Try
            condition$ = Program.Condition <= {
                c("config"), c("platform")
            }.xFormat
        Catch ex As Exception
            ex = New Exception(c.GetJson, ex)
            Throw ex
        End Try

        If [in].FileExists Then
            files = {[in]}
        Else
            files = (ls - l - r - wildcards("*.vbproj") <= [in]).ToArray
        End If

        For Each xml As String In files
            Dim vbproj As Project = xml.LoadXml(Of Project)(,, AddressOf Project.RemoveNamespace)
            Dim config = vbproj.GetProfile(condition$)
            Dim relOut$ = RelativePath(xml.ParentPath, output)
#If DEBUG Then
            xml = xml.TrimSuffix & "_updated.vbproj"
#End If
            config.OutputPath = relOut
            vbproj.Save(xml, Encodings.UTF8)
        Next

        Return 0
    End Function
End Module
