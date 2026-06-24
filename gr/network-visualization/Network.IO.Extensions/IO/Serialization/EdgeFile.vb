#Region "Microsoft.VisualBasic::c5dbe07c2be8d9260494d68e7c6c3b12, gr\network-visualization\Network.IO.Extensions\IO\Serialization\EdgeFile.vb"

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

    '   Total Lines: 32
    '    Code Lines: 27 (84.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.16 KB


    ' Module EdgeFile
    ' 
    '     Function: ReadNetwork
    ' 
    '     Sub: SaveOneEdge
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Module EdgeFile

    <Extension>
    Public Sub SaveOneEdge(edge As Edge, s As BinaryDataWriter)
        Dim table As New NetworkEdge(edge)

        Call s.Write(table.fromNode)
        Call s.Write(table.toNode)
        Call s.Write(table.value)
        Call s.Write(If(table.interaction, ""))
        Call s.Write(table.Properties.GetJson)
    End Sub

    Public Iterator Function ReadNetwork(file As BinaryDataReader, count As Integer) As IEnumerable(Of NetworkEdge)
        For i As Integer = 0 To count - 1
            Yield New NetworkEdge With {
                .fromNode = file.ReadString,
                .toNode = file.ReadString,
                .value = file.ReadDouble,
                .interaction = file.ReadString,
                .Properties = file.ReadString.LoadJSON(Of Dictionary(Of String, String))
            }
        Next
    End Function

End Module
