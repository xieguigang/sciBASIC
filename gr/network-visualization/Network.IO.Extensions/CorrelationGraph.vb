#Region "Microsoft.VisualBasic::ab1146657baf34d029047e0bfe28b464, gr\network-visualization\Network.IO.Extensions\CorrelationGraph.vb"

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

    '   Total Lines: 96
    '    Code Lines: 71
    ' Comment Lines: 12
    '   Blank Lines: 13
    '     File Size: 3.45 KB


    ' Module CorrelationGraph
    ' 
    '     Function: CreateCorrelationGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' A network graph model base on the correlation matrix between variables.
''' </summary>
Public Module CorrelationGraph

    ''' <summary>
    ''' Create a network graph object from a correlation matrix.
    ''' 
    ''' (变量的属性里面必须是包含有相关度的)
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="cut"><see cref="Abs(Double)"/></param>
    ''' <param name="trim">Removes the duplicated edges and self loops?</param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateCorrelationGraph(data As IEnumerable(Of DataSet),
                                           Optional nodeTypes As Dictionary(Of String, String) = Nothing,
                                           Optional interacts As Dictionary(Of String, String) = Nothing,
                                           Optional cut# = 0R,
                                           Optional trim As Boolean = False) As FileStream.NetworkTables

        Dim array As DataSet() = data.ToArray

        If nodeTypes Is Nothing Then
            nodeTypes = New Dictionary(Of String, String)
        End If
        If interacts Is Nothing Then
            interacts = New Dictionary(Of String, String)
        End If

        VBDebugger.Mute = True

        Dim nodes As FileStream.Node() = LinqAPI.Exec(Of FileStream.Node) _
 _
            () <= From v As DataSet
                  In array
                  Let type As String = nodeTypes.TryGetValue(v.ID, [default]:="variable")
                  Select New FileStream.Node With {
                      .ID = v.ID,
                      .NodeType = type,
                      .Properties = v _
                          .Properties _
                          .ToDictionary(Function(k) k.Key,
                                        Function(k)
                                            Return CStr(k.Value)
                                        End Function)
                  }

        Dim edges As New List(Of FileStream.NetworkEdge)
        Dim interact$
        Dim c#

        For Each var As DataSet In array
            For Each k$ In var.Properties.Keys
                c# = var.Properties(k$)

                If Abs(c) < cut Then
                    Continue For
                End If

                interact = interacts.TryGetValue(
                    $"{var.ID} --> {k}",
                    [default]:="correlates")
                edges += New FileStream.NetworkEdge With {
                    .fromNode = var.ID,
                    .toNode = k,
                    .value = c,
                    .interaction = interact,
                    .Properties = New Dictionary(Of String, String) From {
                        {"type", If(c# > 0, "positive", "negative")},
                        {"abs", Abs(c#)}
                    }
                }
            Next
        Next

        VBDebugger.Mute = False

        Dim out As New FileStream.NetworkTables With {
            .edges = edges,
            .nodes = nodes
        }

        If trim Then
            Return out.Trim
        Else
            Return out
        End If
    End Function
End Module
