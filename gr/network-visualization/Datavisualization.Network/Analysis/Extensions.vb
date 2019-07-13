#Region "Microsoft.VisualBasic::339bfc453b1b44bc03dd595cb9887a9e, gr\network-visualization\Datavisualization.Network\Analysis\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: SearchIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports NetGraph = Microsoft.VisualBasic.Data.visualize.Network.FileStream.NetworkTables

Namespace Analysis

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Function SearchIndex(net As NetGraph, from As Boolean) As Dictionary(Of String, Index(Of String))
            Dim getKey = Function(e As NetworkEdge)
                             If from Then
                                 Return e.FromNode
                             Else
                                 Return e.ToNode
                             End If
                         End Function
            Dim getValue = Function(e As NetworkEdge)
                               If from Then
                                   Return e.ToNode
                               Else
                                   Return e.FromNode
                               End If
                           End Function
            Dim index = net.edges _
                .Select(Function(edge)
                            Return (key:=getKey(edge), value:=getValue(edge))
                        End Function) _
                .GroupBy(Function(t) t.key) _
                .ToDictionary(Function(k) k.Key,
                              Function(g)
                                  Return New Index(Of String)(g.Select(Function(o) o.value).Distinct)
                              End Function)
            Return index
        End Function
    End Module
End Namespace
