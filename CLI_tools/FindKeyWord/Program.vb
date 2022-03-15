#Region "Microsoft.VisualBasic::c4163d36176ea809d115877d73ea8267, sciBASIC#\CLI_tools\FindKeyWord\Program.vb"

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

    '   Total Lines: 14
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 412.00 B


    ' Module Program
    ' 
    '     Function: __runWindows, Main
    ' 
    ' /********************************************************************************/

#End Region

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, AddressOf __runWindows)
    End Function

    Private Function __runWindows() As Integer
        Call Application.EnableVisualStyles()
        Call Application.SetCompatibleTextRenderingDefault(False)
        Call New FormFoundTools().ShowDialog()

        Return 0
    End Function
End Module
