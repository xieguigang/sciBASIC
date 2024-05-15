#Region "Microsoft.VisualBasic::b954c98073ba5c7221d9ec2597c94b3c, gr\network-visualization\Visualizer\Styling\NodeStyles.vb"

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

    '   Total Lines: 67
    '    Code Lines: 47
    ' Comment Lines: 7
    '   Blank Lines: 13
    '     File Size: 2.67 KB


    '     Module NodeStyles
    ' 
    '         Function: (+2 Overloads) DegreeAsSize, NodeDegreeSize, SetNodeFill
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.FillBrushes
Imports Microsoft.VisualBasic.Language.Default

Namespace Styling

    Public Module NodeStyles

        Public Function NodeDegreeSize(nodes As IEnumerable(Of Node),
                                       sizeRange As DoubleRange,
                                       Optional degree$ = NamesOf.REFLECTION_ID_MAPPING_DEGREE) As Func(Of Node, Single)

            Dim maps = nodes.DegreeAsSize(sizeRange, degree)
            Dim defaultSize As [Default](Of Double) = sizeRange.Min

            Return Function(node As Node) As Single
                       Return maps.FirstOrDefault(Function(map) map.Key Is node).Maps Or defaultSize
                   End Function
        End Function

        ''' <summary>
        ''' Map the node degree as size 
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="getDegree"></param>
        ''' <param name="sizeRange"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DegreeAsSize(nodes As IEnumerable(Of Node),
                                     getDegree As Func(Of Node, Double),
                                     sizeRange As DoubleRange) As Map(Of Node, Double)()

            Return nodes.RangeTransform(getDegree, sizeRange)
        End Function

        <Extension>
        Public Function DegreeAsSize(nodes As IEnumerable(Of Node),
                                     sizeRange As DoubleRange,
                                     Optional degree$ = NamesOf.REFLECTION_ID_MAPPING_DEGREE) As Map(Of Node, Double)()

            Dim valDegree = Function(node As Node)
                                Return node.data(degree).ParseDouble
                            End Function

            Return nodes.DegreeAsSize(
                getDegree:=valDegree,
                sizeRange:=sizeRange
            )
        End Function

        <Extension>
        Public Function SetNodeFill(g As NetworkGraph, fill As IGetBrush) As NetworkGraph
            Dim fills = fill.GetBrush(g.vertex).ToArray

            For Each mapStyle In fills
                mapStyle.Key.data.color = mapStyle.Maps
            Next

            Return g
        End Function
    End Module
End Namespace
