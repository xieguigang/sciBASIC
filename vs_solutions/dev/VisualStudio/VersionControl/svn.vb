#Region "Microsoft.VisualBasic::fccd934cd733e5a243e04270d99c5090, sciBASIC#\vs_solutions\dev\VisualStudio\VersionControl\svn.vb"

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

    '   Total Lines: 16
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 405 B


    '     Module svn
    ' 
    '         Function: getLogText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine

Namespace VersionControl

    Public Module svn

        Public Function getLogText(file As String) As String
            Dim command$ = $"log {file.CLIPath}"
            Dim svn As New IORedirectFile("svn", command, debug:=False)

            Call svn.Run()

            Return svn.StandardOutput
        End Function
    End Module
End Namespace
