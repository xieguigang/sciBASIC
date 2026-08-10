#Region "Microsoft.VisualBasic::b12df96939e4775c64b78198c2c23142, vs_solutions\dev\VisualStudio\sln\SolutionWorkspace.vb"

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

    '   Total Lines: 54
    '    Code Lines: 43 (79.63%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (20.37%)
    '     File Size: 1.95 KB


    '     Class SolutionWorkspace
    ' 
    '         Properties: Name, Sln
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetCompileFiles, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM

Namespace sln

    Public Class SolutionWorkspace : Implements IProjectWorkspace

        Public ReadOnly Property Name As String Implements IProjectWorkspace.Name
            Get
                Return Sln.FilePath.BaseName
            End Get
        End Property

        Public ReadOnly Property Sln As Solution

        Dim _VbProjs As VBProject()

        Default Public ReadOnly Property GetSymbol(fullname As String) As LanguageSymbolType
            Get
                For Each proj As VBProject In _VbProjs
                    Dim type As LanguageSymbolType = proj.GetType(fullname)

                    If type IsNot Nothing Then
                        Return type
                    End If
                Next

                Return Nothing
            End Get
        End Property

        Sub New(sln As Solution)
            _Sln = sln
            _VbProjs = (From p As Project
                        In sln.Projects
                        Where p.FullPath.ExtensionSuffix("vbproj")
                        Where p.FullPath.FileExists
                        Select VBProject.Load(p.FullPath)).ToArray
        End Sub

        Public Overrides Function ToString() As String
            Return Sln.FilePath.FileName
        End Function

        Public Iterator Function GetCompileFiles() As IEnumerable(Of String) Implements IProjectWorkspace.GetCompileFiles
            For Each proj As VBProject In _VbProjs
                For Each file As String In DirectCast(proj, IProjectWorkspace).GetCompileFiles
                    Yield file
                Next
            Next
        End Function
    End Class
End Namespace
