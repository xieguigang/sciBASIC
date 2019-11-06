Imports System.Text
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.csv
Imports System.Runtime.CompilerServices

Public Module IO

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="outDIR">The data directory for the data export, if the value of this directory is null then the data
    ''' will be exported at the current work directory.
    ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
    ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Public Function Save(Of T_Node As Node, T_Edge As NetworkEdge)(network As Network(Of T_Node, T_Edge), outDIR$, Optional encoding As Encoding = Nothing) As Boolean
        With outDIR Or App.CurrentDirectory.AsDefault
            Call network.nodes.SaveTo($"{ .ByRef}/nodes.csv", False, encoding Or UTF8)
            Call network.edges.SaveTo($"{ .ByRef}/network-edges.csv", False, encoding Or UTF8)
        End With

        Return True
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Load(Of T_Node As Node, T_Edge As NetworkEdge)(directory As String) As Network(Of T_Node, T_Edge)
        Return New Network(Of T_Node, T_Edge) With {
            .edges = $"{directory}/network-edges.csv".LoadCsv(Of T_Edge),
            .nodes = $"{directory}/nodes.csv".LoadCsv(Of T_Node)
        }
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
                .nodes = tables.nodes.LoadCsv(Of Node)
            }
        End If
    End Function

    Public Function IsEmptyTables(directory As String) As Boolean
        With NetworkTables.SearchNetworkTable(directory)
            Return .edges.IsEmptyTable AndAlso .nodes.IsEmptyTable
        End With
    End Function
End Module
