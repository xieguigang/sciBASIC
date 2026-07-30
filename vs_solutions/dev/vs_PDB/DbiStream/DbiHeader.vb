#Region "Microsoft.VisualBasic::8aa327ba99c9255309aad48bc3788777, vs_solutions\dev\vs_PDB\DbiStream\DbiHeader.vb"

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

    '   Total Lines: 20
    '    Code Lines: 17 (85.00%)
    ' Comment Lines: 3 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 697 B


    ' Class DbiHeader
    ' 
    '     Properties: PdbDllVersion
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Header information carried by the DBI stream.
''' </summary>
Public Class DbiHeader
    Public VersionSignature As Integer
    Public VersionHeader As Integer
    Public Age As Integer
    Public GlobalStreamIndex As UShort
    Public PublicStreamIndex As UShort
    Public SymRecordStreamIndex As UShort
    Public ModInfoSize As Integer
    Public SectionContributionSize As Integer
    Public SectionMapSize As Integer
    Public SourceInfoSize As Integer
    Public TypeServerMapSize As Integer
    Public OptionalDbgHdrSize As Integer
    Public ECSubstreamSize As Integer
    Public Machine As UShort
    Public Property PdbDllVersion As UShort
End Class
