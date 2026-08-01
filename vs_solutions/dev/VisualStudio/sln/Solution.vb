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

        Public Shared Function Load(sln As String) As Solution
            Return Parser.Parse(path:=sln)
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


End Namespace
