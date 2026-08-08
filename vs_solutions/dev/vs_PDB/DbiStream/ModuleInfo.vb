#Region "Microsoft.VisualBasic::e5ed55cbc7eb7933aafefc091135af25, vs_solutions\dev\vs_PDB\DbiStream\ModuleInfo.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9 (60.00%)
    ' Comment Lines: 6 (40.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 638 B


    ' Class ModuleInfo
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' One module entry from the module-info substream.
''' </summary>
Public Class ModuleInfo
    Public ModuleName As String
    Public ObjFileName As String
    ''' <summary>Indices into the source-info substream file table.</summary>
    Public FileIndices As Integer()
    ''' <summary>Offset of this module's C13 line info, relative to the DBI debug-data substream.</summary>
    Public C13Offset As Integer
    Public C13Size As Integer
    ''' <summary>Offset/length of this module's symbols within the symbol stream.</summary>
    Public SymbolOffset As Integer
    Public SymbolSize As Integer
End Class
