Imports System.Data
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel

    ''' <summary>
    ''' A helper module for record the clustering traceback information for run algorithm debug
    ''' </summary>
    Public Class TraceBackIterator

        ReadOnly traceback As New Dictionary(Of String, List(Of String))

        Dim len As Integer = 0

        Public ReadOnly Property size As Integer
            Get
                Return len
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' from json data
        ''' </summary>
        Sub New(traceback As IEnumerable(Of NamedCollection(Of String)))
            Me.traceback = traceback _
                .ToDictionary(Function(a) a.name,
                              Function(a)
                                  Return New List(Of String)(a)
                              End Function)

            If Me.traceback.Count > 0 Then
                Me.len = Me.traceback.First.Value.Count
            End If
        End Sub

        Public Sub SetPoints(Of T As INamedValue)(points As IEnumerable(Of T))
            traceback.Clear()
            len = 0

            For Each ti As T In points
                Call traceback.Add(ti.Key, New List(Of String))
            Next
        End Sub

        Public Sub AddIteration(Of T As {INamedValue, IClusterPoint})(points As IEnumerable(Of T))
            len += 1

            For Each ti As T In points
                Call traceback(ti.Key).Add(ti.Cluster.ToString)
            Next
        End Sub

        Public Sub AddIteration(Of T As INamedValue, C As {IClusterPoint, IEnumerable(Of T)})(clusters As IEnumerable(Of C))
            Dim class_id As String

            len += 1

            For Each cluster As C In clusters
                class_id = cluster.Cluster.ToString
                For Each point As T In cluster
                    Call traceback(point.Key).Add(class_id)
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

        Public Sub SetTraceback(Of T As {INamedValue, IClusterPoint})(data As IEnumerable(Of T), itr As Integer)
            If itr > len - 1 Then
                Throw New InvalidConstraintException($"the given iteration number({itr}) is out of range of the save traceback data({size})!")
            End If

            For Each xi As T In data
                xi.Cluster = traceback(xi.Key)(itr)
            Next
        End Sub

        Public Overrides Function ToString() As String
            If traceback.IsNullOrEmpty Then
                Return "no data..."
            Else
                Return $"{traceback.Count} data points"
            End If
        End Function

    End Class
End Namespace