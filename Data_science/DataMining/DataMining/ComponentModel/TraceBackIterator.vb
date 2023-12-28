Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel

    ''' <summary>
    ''' A helper module for record the clustering traceback information for run algorithm debug
    ''' </summary>
    Public Class TraceBackIterator

        ReadOnly traceback As New Dictionary(Of String, List(Of String))

        Public Sub SetPoints(Of T As INamedValue)(points As IEnumerable(Of T))
            Call traceback.Clear()

            For Each ti As T In points
                Call traceback.Add(ti.Key, New List(Of String))
            Next
        End Sub

        Public Sub AddIteration(Of T As {INamedValue, IClusterPoint})(points As IEnumerable(Of T))
            For Each ti As T In points
                Call traceback(ti.Key).Add(ti.Cluster.ToString)
            Next
        End Sub

        Public Sub AddIteration(Of T As INamedValue, C As {IClusterPoint, IEnumerable(Of T)})(clusters As IEnumerable(Of C))
            For Each cluster As C In clusters
                For Each point As T In cluster
                    Call traceback(point.Key).Add(cluster.Cluster)
                Next
            Next
        End Sub

        ''' <summary>
        ''' get cluster traceback for each points
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetTraceback() As IEnumerable(Of NamedCollection(Of String))
            For Each point In traceback
                Yield New NamedCollection(Of String)(point.Key, point.Value)
            Next
        End Function

        Public Overrides Function ToString() As String
            If traceback.IsNullOrEmpty Then
                Return "no data..."
            Else
                Return $"{traceback.Count} data points"
            End If
        End Function

    End Class
End Namespace