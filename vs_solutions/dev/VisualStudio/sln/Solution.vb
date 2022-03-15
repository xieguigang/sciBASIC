#Region "Microsoft.VisualBasic::0110500a1a13881ab3099e5d683e5f1d, sciBASIC#\vs_solutions\dev\VisualStudio\sln\Solution.vb"

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
    '    Code Lines: 20
    ' Comment Lines: 11
    '   Blank Lines: 6
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
    ''' Microsoft Visual Studio Solution File
    ''' </summary>
    Public Class Solution

        Public Property FormatVersion As String
        Public Property VisualStudioVersion As String
        Public Property MinimumVisualStudioVersion As String
        Public Property Projects As Project()

    End Class

    Public Enum TypeId
        <Description("2150E333-8FDC-42A3-9474-1A3956D46DE8")> FolderGroup
        <Description("F184B08F-C81C-45F6-A57F-5ABD9991F28F")> VBProject
        <Description("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")> NjsProject
    End Enum

    Public Class Project
        Public Property NodeType As TypeId
        Public Property Guid As String
        ''' <summary>
        ''' The node display name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' Includes virtual solution folder and vbproj file path 
        ''' </summary>
        ''' <returns></returns>
        Public Property TreePath As String
    End Class
End Namespace
