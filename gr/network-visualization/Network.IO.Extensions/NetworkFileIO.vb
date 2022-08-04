#Region "Microsoft.VisualBasic::996fa113e507e6d78aeedc18ed1a1d9b, sciBASIC#\gr\network-visualization\Network.IO.Extensions\NetworkFileIO.vb"

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

    '   Total Lines: 81
    '    Code Lines: 56
    ' Comment Lines: 15
    '   Blank Lines: 10
    '     File Size: 3.57 KB


    ' Module NetworkFileIO
    ' 
    '     Function: IsEmptyTables, (+2 Overloads) Load, loadMetaJson, Save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module NetworkFileIO

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="output">The data directory for the data export, if the value of this directory is null then the data
    ''' will be exported at the current work directory.
    ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
    ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Public Function Save(Of T_Node As Node, T_Edge As NetworkEdge)(network As Network(Of T_Node, T_Edge),
                                                                   output$,
                                                                   Optional encoding As Encoding = Nothing,
                                                                   Optional silent As Boolean = True) As Boolean
        With output Or App.CurrentDirectory.AsDefault
            Call network.nodes.SaveTo($"{ .ByRef}/nodes.csv", False, encoding Or UTF8, silent:=silent)
            Call network.edges.SaveTo($"{ .ByRef}/network-edges.csv", False, encoding Or UTF8, silent:=silent)
            Call network.meta.GetJson(indent:=True).SaveTo($"{ .ByRef}/meta.json", UTF8)
        End With

        Return True
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Load(Of T_Node As Node, T_Edge As NetworkEdge)(directory As String, Optional silent As Boolean = True) As Network(Of T_Node, T_Edge)
        Return New Network(Of T_Node, T_Edge) With {
            .edges = $"{directory}/network-edges.csv".LoadCsv(Of T_Edge)(mute:=silent),
            .nodes = $"{directory}/nodes.csv".LoadCsv(Of T_Node)(mute:=silent),
            .meta = loadMetaJson(directory)
        }
    End Function

    Private Function loadMetaJson(directory As String) As MetaData
        Dim metaJson = $"{directory}/meta.json"
        Dim meta As MetaData

        If metaJson.FileExists Then
            meta = metaJson.LoadJSON(Of MetaData) Or (New MetaData).AsDefault
        Else
            meta = New MetaData
        End If

        Return meta
    End Function

    ''' <summary>
    ''' Load network graph data from a saved network data direcotry.
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns></returns>
    Public Function Load(DIR$, Optional cytoscapeFormat As Boolean = False) As NetworkTables
        Dim tables = NetworkTables.SearchNetworkTable(directory:=DIR)

        If cytoscapeFormat Then
            Return Cytoscape.CytoscapeExportAsTable(tables.edges, tables.nodes)
        Else
            Return New NetworkTables With {
                .edges = tables.edges.LoadCsv(Of NetworkEdge),
                .nodes = tables.nodes.LoadCsv(Of Node),
                .meta = loadMetaJson(DIR)
            }
        End If
    End Function

    Public Function IsEmptyTables(directory As String) As Boolean
        With NetworkTables.SearchNetworkTable(directory)
            Return .edges.IsEmptyTable AndAlso .nodes.IsEmptyTable
        End With
    End Function
End Module
