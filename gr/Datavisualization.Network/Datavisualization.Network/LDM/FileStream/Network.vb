Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileStream

    Public Class Network : Inherits Network(Of Node, NetworkEdge)

        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of NetworkEdge))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        Sub New(edges As IEnumerable(Of NetworkEdge), nodes As IEnumerable(Of Node))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        ''' <summary>
        ''' 获取指定节点的连接数量
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function Links(node As String) As Integer
            Dim count% = Edges _
                .Where(Function(x) x.Contains(node)) _
                .Count
            Return count
        End Function

        Public Overloads Shared Function Load(DIR As String) As Network
            Dim list As New List(Of String)(ls - l - r - "*.csv" <= DIR)
            Dim edgeFile$ = $"{DIR}/network-edges.csv"
            Dim nodeFile$ = $"{DIR}/nodes.csv"

            If Not edgeFile.FileExists Then
                For Each path$ In list
                    If InStr(path.BaseName, "edge", CompareMethod.Text) > 0 Then
                        edgeFile = path
                        Exit For
                    End If
                Next

                If Not String.IsNullOrEmpty(edgeFile) Then
                    Call list.Remove(edgeFile)
                Else
                    Throw New MemberAccessException("Edge file not found in the directory: " & DIR)
                End If
            End If

            If Not nodeFile.FileExists Then
                For Each path$ In list
                    If InStr(path.BaseName, "node", CompareMethod.Text) > 0 Then
                        nodeFile = path
                        Exit For
                    End If
                Next

                If String.IsNullOrEmpty(nodeFile) Then
                    Throw New MemberAccessException("Node file not found in the directory: " & DIR)
                End If
            End If

            Return New Network With {
                .Edges = edgeFile.LoadCsv(Of NetworkEdge),
                .Nodes = nodeFile.LoadCsv(Of Node)
            }
        End Function
    End Class
End Namespace