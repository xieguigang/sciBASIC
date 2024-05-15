﻿#Region "Microsoft.VisualBasic::5d6070a351bcfca1469daef5d71ba290, vs_solutions\dev\VisualStudio\CodeSign\LicenseMgr.vb"

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

    '   Total Lines: 202
    '    Code Lines: 137
    ' Comment Lines: 27
    '   Blank Lines: 38
    '     File Size: 7.69 KB


    '     Module LicenseMgr
    ' 
    '         Properties: Ignores, Template
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddRegion, Insert, Inserts, RemoveRegion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace CodeSign

    ''' <summary>
    ''' Source code license banner
    ''' </summary>
    Public Module LicenseMgr

        ''' <summary>
        ''' The license template example.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Template As LicenseInfo
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New LicenseInfo With {
                    .Authors = {
                        New NamedValue With {
                            .name = "asuka",
                            .text = "amethyst.asuka@gcmodeller.org"
                        }
                    },
                    .Brief = "Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""As Is"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.",
                    .Copyright = "Copyright (c) 2016 amethyst.asuka@gcmodeller.org",
                    .Title = "The MIT License (MIT)"
            }
            End Get
        End Property

        Sub New()
            Call Template.GetXml.SaveTo(App.HOME & "/License.Template.Xml")
        End Sub

        ''' <summary>
        ''' Enumerates all of the VisualStudio IDE auto generated source file.
        ''' 
        ''' ```json
        ''' {".+\.Designer\.vb", "AssemblyInfo\.vb"}
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        Public Property Ignores As String() = {".+\.Designer\.vb", "AssemblyInfo\.vb"}

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="src">*.vb path</param>
        ''' <param name="info">License meta data</param>
        ''' <returns></returns>
        <Extension>
        Public Function Insert(src As String, info As LicenseInfo, rootDir$, Optional ByRef stat As CodeStatics = Nothing) As Boolean
            Dim file As String = PathExtensions.RelativePath(rootDir.GetDirectoryFullPath, src.GetFullPath, appendParent:=False)
            Dim [in] As String = src.ReadAllText
            Dim path As String = src

            src = Trim([in])
            src = RemoveRegion(src)
            src = AddRegion(src, info, file, stat)

            Try
                Return src.SaveTo(path, Encoding.UTF8)
            Catch ex As Exception
                Dim backup As String = src & ".bak"

                ex = New Exception(path, ex)

                Call [in].SaveTo(backup, Encoding.UTF8)
                Call App.LogException(ex)

                Return False
            End Try
        End Function

        Public Function RemoveRegion(src As String) As String
            Dim region As String = Regex.Match(src, LicenseMgr.Region, RegexOptions.Singleline).Value

            If Not String.IsNullOrEmpty(region) Then
                src = src.Replace(region, "")
                src = src.Trim(vbCr, vbLf)
            End If

            Return src
        End Function

        Public Function AddRegion(src As String, info As LicenseInfo, file As String, Optional ByRef stat As CodeStatics = Nothing) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"#Region ""Microsoft.VisualBasic::{SecurityString.GetMd5Hash(src)}, {file}""")
            Call sb.AppendLine()
            Call sb.AppendLine("    ' Author:")
            Call sb.AppendLine("    ' ")

            For Each author As NamedValue In info.Authors.SafeQuery
                Call sb.AppendLine($"    '       {author.name} ({author.text})")
            Next

            Call sb.AppendLine("    ' ")
            Call sb.AppendLine("    ' " & info.Copyright)
            Call sb.AppendLine("    ' ")
            Call sb.AppendLine("    ' ")
            Call sb.AppendLine("    ' " & info.Title)
            Call sb.AppendLine("    ' ")

            For Each line As String In info.Brief.LineTokens
                Call sb.AppendLine("    ' " & line)
            Next

            sb.AppendLine()
            sb.AppendLine()
            sb.AppendLine()
            sb.AppendLine("    ' /********************************************************************************/")
            sb.AppendLine()
            sb.AppendLine("    ' Summaries:")
            sb.AppendLine()

            sb.AppendLine()

            stat = CodeStatics.StatVB(src)

            sb.AppendLine($"    ' Code Statistics:")
            sb.AppendLine()
            sb.AppendLine($"    '   Total Lines: {stat.totalLines}")
            sb.AppendLine($"    '    Code Lines: {stat.lineOfCodes}")
            sb.AppendLine($"    ' Comment Lines: {stat.commentLines}")
            sb.AppendLine($"    '   Blank Lines: {stat.blankLines}")
            sb.AppendLine($"    '     File Size: {StringFormats.Lanudry(stat.size)}")

            sb.AppendLine()
            sb.AppendLine()

            For Each line As String In VBCodeSignature.SummaryModules(vb:=src, stat).LineTokens
                Call sb.AppendLine("    ' " & line)
            Next

            sb.AppendLine("    ' /********************************************************************************/")

            Call sb.AppendLine()
            Call sb.AppendLine($"#End Region")
            Call sb.AppendLine()

            Call sb.AppendLine(src)

            Return sb.ToString
        End Function

        Const Region As String = "^#Region "".+?\.vb"".+?#End Region"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="info">The root directory</param>
        ''' <returns></returns>
        Public Function Inserts(info As LicenseInfo, rootDir$) As String()
            Dim fails As New List(Of String)

            For Each vb As String In ls - l - r - "*.vb" <= rootDir
                Dim skip As Boolean = False

                ' ignores all of the IDE auto generated source file.
                For Each ig As String In LicenseMgr.Ignores
                    If Regex.Match(vb, ig, RegexICSng).Success Then
                        skip = True
                        Exit For
                    End If
                Next

                If skip Then
                    Continue For
                End If

                If Not vb.Insert(info, rootDir) Then
                    fails += vb
                End If
            Next

            Return fails
        End Function
    End Module
End Namespace
