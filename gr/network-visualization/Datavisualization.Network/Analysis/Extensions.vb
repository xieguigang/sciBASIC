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
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis

    <HideModuleName> Public Module Extensions

        ' a-b is tuple
        ' a-b-c is not

        ''' <summary>
        ''' 判断边的两个节点是否只有当前的边连接而再无其他的任何边连接了
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="edge"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isTupleEdge(g As NetworkGraph, edge As Edge) As Boolean
            Dim uset = g.GetEdges(edge.U).ToArray
            Dim vset = g.GetEdges(edge.V).ToArray

            If uset.Length = 1 AndAlso vset.Length = 1 AndAlso uset(Scan0) Is vset(Scan0) AndAlso uset(Scan0) Is edge Then
                Return True
            Else
                Return False
            End If
        End Function
    End Module
End Namespace
