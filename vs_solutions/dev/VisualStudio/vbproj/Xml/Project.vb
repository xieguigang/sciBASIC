﻿#Region "Microsoft.VisualBasic::534ec7a7cf7d5a9755f728310e6360ed, vs_solutions\dev\VisualStudio\vbproj\Xml\Project.vb"

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

    '   Total Lines: 136
    '    Code Lines: 97 (71.32%)
    ' Comment Lines: 13 (9.56%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 26 (19.12%)
    '     File Size: 5.22 KB


    '     Class Project
    ' 
    '         Properties: [Imports], DefaultTargets, FilePath, IsDotNetCoreSDK, ItemGroups
    '                     MainGroup, MimeType, PropertyGroups, Sdk, Targets
    '                     ToolsVersion
    ' 
    '         Function: GetProfile, Load, ProcessDotNetCoreSDK, (+3 Overloads) Save, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text

Namespace vbproj.Xml

    ''' <summary>
    ''' Visual Studio project XML file
    ''' </summary>
    <XmlRoot("Project", [Namespace]:=Project.xmlns)>
    Public Class Project : Implements ISaveHandle, IFileReference

        Public Const xmlns$ = "http://schemas.microsoft.com/developer/msbuild/2003"

        <XmlAttribute> Public Property ToolsVersion As String
        <XmlAttribute> Public Property Sdk As String
        <XmlAttribute> Public Property DefaultTargets As String

        <XmlElement("Import")>
        Public Property [Imports] As Import()

        <XmlElement("PropertyGroup")>
        Public Property PropertyGroups As PropertyGroup()
        <XmlElement("ItemGroup")>
        Public Property ItemGroups As ItemGroup()
        <XmlElement("Target")>
        Public Property Targets As Target()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' returns false means current project object is a legacy .net framework project
        ''' </returns>
        Public ReadOnly Property IsDotNetCoreSDK As Boolean
            Get
                Return Sdk = "Microsoft.NET.Sdk" AndAlso ToolsVersion.StringEmpty
            End Get
        End Property

        Private ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {
                    New ContentType With {.Details = "VisualStudio Project", .FileExt = ".vbproj", .MIMEType = "visualstudio/xml-project", .Name = "Project"}
                }
            End Get
        End Property

        Public ReadOnly Property MainGroup As PropertyGroup
            Get
                Return PropertyGroups.Where(Function(p) p.Condition.StringEmpty).DefaultFirst
            End Get
        End Property

        ''' <summary>
        ''' 读取<see cref="AssemblyInfo"/>文件的时候会需要使用到这个属性
        ''' </summary>
        ''' <returns></returns>
        Private Property FilePath As String Implements IFileReference.FilePath

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetProfile(condition As String) As PropertyGroup
            If InStr(condition, "$(Configuration)") = 0 AndAlso InStr(condition, "$(Platform)") = 0 Then
                condition = $"'$(Configuration)|$(Platform)' == '{condition}'"
            Else
                condition = condition.Trim
            End If

            Return LinqAPI.DefaultFirst(Of PropertyGroup) _
                                                          _
                () <= From x As PropertyGroup
                      In PropertyGroups
                      Where Not x.Condition.StringEmpty AndAlso condition.TextEquals(x.Condition.Trim)
                      Select x
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"[{MimeType(0).ToString}]")


            Return sb.ToString
        End Function

        Public Shared Function Load(file As String) As Project
            Dim vbproj As Project = file.LoadXml(Of Project)(throwEx:=False)

            If vbproj Is Nothing Then
                vbproj = file _
                    .ReadAllText _
                    .CreateObjectFromXmlFragment(Of Project)(
                        preprocess:=AddressOf ProcessDotNetCoreSDK,
                        ignoreXmlNs:=False
                    )
            End If

            vbproj.FilePath = file

            Return vbproj
        End Function

        Private Shared Function ProcessDotNetCoreSDK(xml As String) As String
            xml = xml.Replace("Sdk=""Microsoft.NET.Sdk""", "Sdk=""Microsoft.NET.Sdk"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""")
            Return xml
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using file As Stream = path.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False)
                Return Save(file, encoding)
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function

        Public Function Save(s As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using wr As New StreamWriter(s, encoding)
                Call wr.WriteLine(Me.GetXml)
                Call wr.Flush()
            End Using

            Return True
        End Function

    End Class
End Namespace
