#Region "Microsoft.VisualBasic::0110500a1a13881ab3099e5d683e5f1d, vs_solutions\dev\VisualStudio\sln\Solution.vb"

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

    '   Total Lines: 37
    '    Code Lines: 20 (54.05%)
    ' Comment Lines: 11 (29.73%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.14 KB


    '     Class Solution
    ' 
    '         Properties: FormatVersion, MinimumVisualStudioVersion, Projects, VisualStudioVersion
    ' 
    '     Enum TypeId
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Project
    ' 
    '         Properties: Guid, Name, NodeType, TreePath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

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
    End Class

    ''' <summary>
    ''' A solution level build configuration / platform pair, e.g. ``Debug|AnyCPU``.
    ''' </summary>
    Public Class SolutionConfiguration
        ''' <summary>
        ''' The combined name, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' The configuration part, e.g. ``Debug``.
        ''' </summary>
        Public Property Configuration As String
        ''' <summary>
        ''' The platform part, e.g. ``AnyCPU``.
        ''' </summary>
        Public Property Platform As String

        Public Sub New()
        End Sub

        Public Sub New(name As String)
            Me.Name = name

            If name IsNot Nothing Then
                Dim parts = name.Split({"|"c}, 2)
                Configuration = parts(0)

                If parts.Length > 1 Then
                    Platform = parts(1)
                End If
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Known Visual Studio project type GUIDs.
    ''' </summary>
    Public Enum TypeId
        <Description("2150E333-8FDC-42A3-9474-1A3956D46DE8")> FolderGroup
        <Description("F184B08F-C81C-45F6-A57F-5ABD9991F28F")> VBProject
        <Description("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")> NjsProject
        <Description("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")> CSharpProject
        <Description("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942")> CppProject
        <Description("F135691A-BF7E-435D-8960-F99683D2D49C")> WebApplication
        <Description("E24C65DC-7377-472B-9ABA-BC803B73C61A")> WebSite
        <Description("3AC096D0-A1C2-E12C-1390-A8335801FDAB")> TestProject
        <Description("VC60C7D7-84AE-47E4-8DED-D1B4E0554DBB")> SolutionFolderAlt
        <Description("Unknown")> Unknown
    End Enum

    ''' <summary>
    ''' A project or solution folder inside the solution.
    ''' </summary>
    Public Class Project
        ''' <summary>
        ''' The project type, derived from the project type GUID.
        ''' </summary>
        Public Property NodeType As TypeId
        ''' <summary>
        ''' The project type GUID as it appears in the solution file.
        ''' </summary>
        Public Property TypeGuid As String
        ''' <summary>
        ''' The project GUID.
        ''' </summary>
        Public Property Guid As String
        ''' <summary>
        ''' The node display name.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' Includes virtual solution folder and project file path.
        ''' </summary>
        Public Property TreePath As String
        ''' <summary>
        ''' Relative path of the project file (e.g. a ``.vbproj``) to the solution file.
        ''' Empty for solution folders.
        ''' </summary>
        Public Property RelativePath As String
        ''' <summary>
        ''' Resolved full path of the project file. Empty for solution folders.
        ''' </summary>
        Public Property FullPath As String
        ''' <summary>
        ''' The GUID of the parent solution folder, or empty for top-level nodes.
        ''' </summary>
        Public Property ParentGuid As String

        ''' <summary>
        ''' True when this node is a solution folder (no physical project file).
        ''' </summary>
        Public ReadOnly Property IsFolder As Boolean
            Get
                Return NodeType = TypeId.FolderGroup OrElse NodeType = TypeId.SolutionFolderAlt
            End Get
        End Property
    End Class
End Namespace
