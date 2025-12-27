#Region "Microsoft.VisualBasic::218048263a97266602c2faa5695ca138, gr\network-visualization\Network.IO.Extensions\NetworkFileIO.vb"

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

    '   Total Lines: 110
    '    Code Lines: 71 (64.55%)
    ' Comment Lines: 25 (22.73%)
    '    - Xml Docs: 92.00%
    ' 
    '   Blank Lines: 14 (12.73%)
    '     File Size: 5.00 KB


    ' Module NetworkFileIO
    ' 
    '     Function: IsEmptyTables, (+2 Overloads) Load, loadMetaJson, (+2 Overloads) Save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports localDir = Microsoft.VisualBasic.FileIO.Directory

Public Module NetworkFileIO

    ''' <summary>
    ''' Export the network graph object into data files
    ''' </summary>
    ''' <param name="output">The data directory for the data export, if the value of this directory is null then the data
    ''' will be exported at the current work directory.
    ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
    ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Public Function Save(Of V As Node, E As NetworkEdge)(network As Network(Of V, E),
                                                         output$,
                                                         Optional encoding As Encoding = Nothing,
                                                         Optional silent As Boolean = True) As Boolean

        Dim fs As IFileSystemEnvironment = localDir.FromLocalFileSystem(
            dir:=output Or App.CurrentDirectory.AsDefault
        )

        Return network.Save(output:=fs, encoding, silent)
    End Function

    ''' <summary>
    ''' Export the network graph object into data files
    ''' </summary>
    ''' <param name="output">The data directory for the data export, if the value of this directory is null then the data
    ''' will be exported at the current work directory.
    ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
    ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Public Function Save(Of V As Node, E As NetworkEdge)(g As Network(Of V, E), output As IFileSystemEnvironment,
                                                         Optional encoding As Encoding = Nothing,
                                                         Optional silent As Boolean = True) As Boolean
        Dim args As New Write_csv.Arguments With {
            .strict = False,
            .encoding = encoding Or UTF8,
            .silent = silent
        }

        Call g.nodes.SaveTo(file:=output.OpenFile($"/nodes.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite), args)
        Call g.edges.SaveTo(file:=output.OpenFile($"/network-edges.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite), args)
        Call output.WriteText(g.meta.GetJson(indent:=True), $"/meta.json")

        Return True
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Load(Of V As Node, E As NetworkEdge)(directory As String, Optional silent As Boolean = True) As Network(Of V, E)
        Return New Network(Of V, E) With {
            .edges = $"{directory}/network-edges.csv".LoadCsv(Of E)(mute:=silent),
            .nodes = $"{directory}/nodes.csv".LoadCsv(Of V)(mute:=silent),
            .meta = loadMetaJson(localDir.FromLocalFileSystem(directory))
        }
    End Function

    Private Function loadMetaJson(directory As IFileSystemEnvironment) As MetaData
        Dim metaJson = directory.ReadAllText($"/meta.json")
        Dim meta As MetaData

        If Not metaJson.StringEmpty Then
            meta = Strings.Trim(metaJson).LoadJSON(Of MetaData) Or (New MetaData).AsDefault
        Else
            meta = New MetaData
        End If

        Return meta
    End Function

    ''' <summary>
    ''' Load network graph data from a saved network data direcotry.
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <returns></returns>
    Public Function Load(dir$, Optional cytoscapeFormat As Boolean = False, Optional verbose As Boolean = True) As NetworkTables
        Dim tables = NetworkTables.SearchNetworkTable(directory:=dir)

        If cytoscapeFormat Then
            Return Cytoscape.CytoscapeExportAsTable(tables.edges, tables.nodes)
        Else
            Return New NetworkTables With {
                .edges = tables.edges.LoadCsv(Of NetworkEdge)(mute:=Not verbose),
                .nodes = tables.nodes.LoadCsv(Of Node)(mute:=Not verbose),
                .meta = loadMetaJson(localDir.FromLocalFileSystem(dir))
            }
        End If
    End Function

    Public Function IsEmptyTables(directory As String) As Boolean
        With NetworkTables.SearchNetworkTable(directory)
            Return .edges.IsEmptyTable AndAlso .nodes.IsEmptyTable
        End With
    End Function
End Module
