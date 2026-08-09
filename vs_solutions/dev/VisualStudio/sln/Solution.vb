#Region "Microsoft.VisualBasic::7acd847c2080eb489d13c896f84c9a95, vs_solutions\dev\VisualStudio\sln\Solution.vb"

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

'   Total Lines: 300
'    Code Lines: 173 (57.67%)
' Comment Lines: 69 (23.00%)
'    - Xml Docs: 91.30%
' 
'   Blank Lines: 58 (19.33%)
'     File Size: 10.95 KB


'     Class Solution
' 
'         Properties: [Global], Configurations, FilePath, FormatVersion, IsXmlFormat
'                     MinimumVisualStudioVersion, Projects, SolutionGuid, VisualStudioVersion
' 
'         Function: GetChildProjects, GetProject, GetProjectFullPath, GetRootProjects, GetTypeGuid
'                   Load
' 
'         Sub: Save, WriteProjectNode
' 
'     Class SolutionConfiguration
' 
'         Properties: Configuration, Name, Platform
' 
'         Constructor: (+2 Overloads) Sub New
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File

Namespace sln

    ''' <summary>
    ''' Microsoft Visual Studio Solution File, works for both classic .sln (text)
    ''' and the new .slnx (XML) solution formats.
    ''' </summary>
    Public Class Solution

        Public Property FormatVersion As String
        Public Property VisualStudioVersion As String
        Public Property MinimumVisualStudioVersion As String

        ''' <summary>
        ''' The projects and solution folders declared in the solution.
        ''' </summary>
        Public Property Projects As New List(Of Project)

        ''' <summary>
        ''' Solution level build configurations / platforms, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Configurations As New List(Of SolutionConfiguration)

        ''' <summary>
        ''' Global section key/value pairs (e.g. ``SolutionGuid``).
        ''' </summary>
        Public Property [Global] As New [Global]

        ''' <summary>
        ''' The file path of the solution that this model was parsed from.
        ''' </summary>
        Public Property FilePath As String

        ''' <summary>
        ''' True when the source file was a ``.slnx`` (XML) solution.
        ''' </summary>
        Public Property IsXmlFormat As Boolean

        ''' <summary>
        ''' The solution GUID, mirrored from <see cref="[Global].SolutionGuid"/>.
        ''' Setting this also updates the underlying global property.
        ''' </summary>
        Public Property SolutionGuid As String
            Get
                Return [Global].SolutionGuid
            End Get
            Set(value As String)
                [Global].SolutionGuid = value

                If Not String.IsNullOrEmpty(value) Then
                    [Global].Properties("SolutionGuid") = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Resolve the full path of a project relative to the solution file.
        ''' </summary>
        Public Function GetProjectFullPath(p As Project) As String
            If String.IsNullOrEmpty(p.RelativePath) Then
                Return String.Empty
            End If

            If IO.Path.IsPathRooted(p.RelativePath) Then
                Return p.RelativePath
            End If

            If String.IsNullOrEmpty(FilePath) Then
                Return p.RelativePath
            End If

            Return IO.Path.GetFullPath(IO.Path.Combine(IO.Path.GetDirectoryName(FilePath), p.RelativePath))
        End Function

        ''' <summary>
        ''' Find a project or solution folder by its GUID (case-insensitive).
        ''' </summary>
        Public Function GetProject(guid As String) As Project
            If String.IsNullOrEmpty(guid) Then
                Return Nothing
            End If

            Dim g As String = guid.ToUpperInvariant()

            For Each p In Projects
                If p.Guid IsNot Nothing AndAlso p.Guid.ToUpperInvariant() = g Then
                    Return p
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Get all top-level nodes (no parent solution folder).
        ''' </summary>
        Public Function GetRootProjects() As List(Of Project)
            Dim roots As New List(Of Project)

            For Each p In Projects
                If String.IsNullOrEmpty(p.ParentGuid) Then
                    roots.Add(p)
                End If
            Next

            Return roots
        End Function

        ''' <summary>
        ''' Get the direct children of the given parent solution folder GUID.
        ''' </summary>
        Public Function GetChildProjects(parentGuid As String) As List(Of Project)
            Dim children As New List(Of Project)
            Dim g As String = If(parentGuid Is Nothing, "", parentGuid.ToUpperInvariant())

            For Each p In Projects
                If p.ParentGuid IsNot Nothing AndAlso p.ParentGuid.ToUpperInvariant() = g Then
                    children.Add(p)
                End If
            Next

            Return children
        End Function

        Public Shared Function Load(sln As String) As Solution
            Return Parser.Parse(path:=sln)
        End Function

        ''' <summary>
        ''' Save this solution model as a ``.slnx`` (XML) file. The output is
        ''' symmetric with <see cref="SlnxParser.ParseSlnx"/> so it can be re-read.
        ''' </summary>
        ''' <param name="path">
        ''' The target ``.slnx`` file path. When omitted, <see cref="FilePath"/> is used.
        ''' </param>
        Public Sub Save(Optional path As String = Nothing)
            Dim target As String = If(path, FilePath)

            If String.IsNullOrEmpty(target) Then
                Throw New ArgumentNullException(NameOf(path), "A target path must be supplied (FilePath is empty).")
            Else
                FilePath = target
                IsXmlFormat = True

                Call Me.SaveSlnx(target)
            End If
        End Sub
    End Class
End Namespace
