#Region "Microsoft.VisualBasic::053bccda25d4441c716c447dfbe21d15, gr\network-visualization\Network.IO.Extensions\IO\Serialization\NodeFile.vb"

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

    '   Total Lines: 76
    '    Code Lines: 67 (88.16%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (11.84%)
    '     File Size: 2.92 KB


    ' Module NodeFile
    ' 
    '     Function: ReadNode
    ' 
    '     Sub: SaveOneNode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

Module NodeFile

    <Extension>
    Public Sub SaveOneNode(node As Graph.Node, s As BinaryDataWriter)
        Dim data As NodeData = node.data

        Call s.Write(node.ID)
        Call s.Write(node.label)
        Call s.Write(node.pinned)
        Call s.Write(node.visited)

        If node.degree Is Nothing Then
            Call s.Write(0%)
            Call s.Write(0%)
        Else
            Call s.Write(node.degree.In)
            Call s.Write(node.degree.Out)
        End If

        Call s.Write(data.betweennessCentrality)
        Call s.Write(data.force.X)
        Call s.Write(data.force.Y)
        Call s.Write(If(TypeOf data.initialPostion Is FDGVector2, 2, 3))
        Call s.Write(data.initialPostion.ToArray)
        Call s.Write(Brush.SolidColor(data.color).ToHtmlColor)
        Call s.Write(data.label)
        Call s.Write(data.mass)
        Call s.Write(data.neighborhoods)
        Call s.Write(data.neighbours)
        Call s.Write(data.origID)
        Call s.Write(data.size.TryCount)
        Call s.Write(data.size)
        Call s.Write(data.weights.TryCount)
        Call s.Write(data.weights)
        Call s.Write(data.Properties.GetJson)
    End Sub

    Public Iterator Function ReadNode(file As BinaryDataReader, count As Integer) As IEnumerable(Of Graph.Node)
        For i As Integer = 0 To count - 1
            Dim node As New Graph.Node With {
               .ID = file.ReadInt32,
               .label = file.ReadString,
               .pinned = file.ReadBoolean,
               .visited = file.ReadBoolean,
               .degree = New NodeDegree(file.ReadInt32, file.ReadInt32)
            }
            Dim data As New NodeData With {
                .betweennessCentrality = file.ReadDouble,
                .force = New Point(file.ReadInt32, file.ReadInt32),
                .initialPostion = AbstractVector.FromVector(file.ReadDoubles(file.ReadInt32)),
                .color = New SolidBrush(file.ReadString.TranslateColor),
                .label = file.ReadString,
                .mass = file.ReadDouble,
                .neighbours = file.ReadInt32s(file.ReadInt32),
                .origID = file.ReadString,
                .size = file.ReadDoubles(file.ReadInt32),
                .weights = file.ReadDoubles(file.ReadInt32),
                .Properties = file.ReadString.LoadJSON(Of Dictionary(Of String, String))
            }

            node.data = data

            Yield node
        Next
    End Function

End Module

