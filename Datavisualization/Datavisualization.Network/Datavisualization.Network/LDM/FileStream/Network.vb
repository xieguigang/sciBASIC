Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DataVisualization.Network.LDM.Abstract
Imports Microsoft.VisualBasic.ComponentModel

Namespace FileStream

    ''' <summary>
    ''' The network csv data information with specific type of the datamodel
    ''' </summary>
    ''' <typeparam name="T_Node"></typeparam>
    ''' <typeparam name="T_Edge"></typeparam>
    ''' <remarks></remarks>
    Public Class Network(Of T_Node As Node, T_Edge As NetworkNode)

        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IKeyValuePairObject(Of T_Node(), T_Edge())
        Implements ISaveHandle

        Public Property Nodes As T_Node() Implements ComponentModel.Collection.Generic.IKeyValuePairObject(Of T_Node(), T_Edge()).Identifier
        Public Property Edges As T_Edge() Implements ComponentModel.Collection.Generic.IKeyValuePairObject(Of T_Node(), T_Edge()).Value

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicated()
            Dim LQuery = (From edge As T_Edge
                          In Edges.AsParallel
                          Let uu As String() = (From s As String
                                                In {edge.FromNode, edge.ToNode}
                                                Select s
                                                Order By s Ascending).ToArray
                          Select id = String.Join(";", uu),
                              edge = edge
                          Group By id Into Group).ToArray
            Edges = (From gpEdge
                     In LQuery
                     Select gpEdge.Group.ToArray.First.edge).ToArray
        End Sub

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveSelfLoop()
            Dim LQuery = (From item As T_Edge
                          In Edges.AsParallel
                          Where Not item.SelfLoop
                          Select item).ToArray
            Edges = LQuery
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ExportDir">The data directory for the data export, if the value of this directory is null then the data 
        ''' will be exported at the current work directory.
        ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
        ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Save(Optional ExportDir As String = "",
                             Optional encoding As System.Text.Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            If String.IsNullOrEmpty(ExportDir) Then ExportDir = My.Computer.FileSystem.CurrentDirectory

            Call Nodes.SaveTo(String.Format("{0}/Nodes.csv", ExportDir), False, encoding)
            Call Edges.SaveTo(String.Format("{0}/Edges.csv", ExportDir), False, encoding)

            Return True
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.GetEncodings)
        End Function
    End Class
End Namespace