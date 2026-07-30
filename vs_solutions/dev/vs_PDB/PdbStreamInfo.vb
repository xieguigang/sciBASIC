#Region "Microsoft.VisualBasic::594c61ee23b7c5d0c8acb08a4f5c7783, vs_solutions\dev\vs_PDB\PdbStreamInfo.vb"

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

    '   Total Lines: 13
    '    Code Lines: 9 (69.23%)
    ' Comment Lines: 3 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (7.69%)
    '     File Size: 422 B


    ' Class PdbStreamInfo
    ' 
    '     Properties: Age, Guid, Signature, Version
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Header information decoded from the PDB stream (stream #1).
''' </summary>
Public Class PdbStreamInfo
    Public Property Version As Integer
    Public Property Signature As Integer
    Public Property Age As Integer
    Public Property Guid As Guid

    Public Overrides Function ToString() As String
        Return $"v{Version} sig={Signature} age={Age} {Guid:B}"
    End Function
End Class
