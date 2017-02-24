Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Abstract
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Imaging

Namespace KMeans

    Public Module KMeansNetwork

        ''' <summary>
        ''' 相似度矩阵之中大于0的值会被作为边的强度，小于或者等于0表示没有边
        ''' </summary>
        ''' <param name="kmeans"></param>
        ''' <returns></returns>
        <Extension> Public Function ToNetwork(kmeans As IEnumerable(Of EntityLDM),
                                              Optional colors As Dictionary(Of String, Color) = Nothing,
                                              Optional defaultColor$ = "lightgray") As Network

            Dim data As EntityLDM() = kmeans.ToArray
            Dim edges As New List(Of NetworkEdge)
            Dim clusterColors As New Dictionary(Of NamedValue(Of String))
            Dim [default] As New NamedValue(Of String) With {
                .Name = NameOf([default]),
                .Value = defaultColor
            }

            If Not colors Is Nothing Then
                For Each c As KeyValuePair(Of String, Color) In colors
                    clusterColors += New NamedValue(Of String) With {
                        .Name = c.Key,
                        .Value = c.Value.RGB2Hexadecimal
                    }
                Next
            End If

            Dim getColor = Function(cluster$)
                               Return clusterColors.TryGetValue(cluster, [default]).Value
                           End Function
            Dim nodes As Dictionary(Of Node) = data _
                .Select(Function(n) New Node With {
                    .ID = n.Name,
                    .NodeType = n.Cluster,
                    .Properties = New Dictionary(Of String, String) From {
                        {
                            "color", getColor(cluster:= .NodeType)
                        }
                    }
                }).ToDictionary

            For Each vector As EntityLDM In data
                Dim from$ = vector.Name
                Dim color$

                For Each hit In vector.Properties
                    Dim clusters$() = {
                        nodes(from$), nodes(hit.Key)
                    }.Select(Function(n) n.NodeType) _
                    .Distinct _
                    .ToArray

                    If clusters.Length = 1 Then
                        color = getColor(cluster:=clusters(Scan0))
                    Else
                        color = clusters _
                            .Select(getColor) _
                            .Select(AddressOf TranslateColor) _
                            .Average _
                            .RGB2Hexadecimal
                    End If

                    edges += New NetworkEdge With {
                        .Confidence = hit.Value,
                        .FromNode = from,
                        .ToNode = hit.Key,
                        .InteractionType = clusters.JoinBy("|"),
                        .Properties = New Dictionary(Of String, String) From {
                            {"color", color}
                        }
                    }
                Next
            Next

            Return New Network With {
                .Edges = edges.RemoveDuplicated(False, False), ' 相似度是无方向的
                .Nodes = nodes
            }
        End Function
    End Module
End Namespace