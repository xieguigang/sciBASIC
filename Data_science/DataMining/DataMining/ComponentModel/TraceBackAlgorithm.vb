Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans

Namespace ComponentModel

    Public MustInherit Class TraceBackAlgorithm

        Protected traceback As TraceBackIterator

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function is a safe function, will never returns the null value
        ''' </returns>
        Public Function GetTraceBack() As IEnumerable(Of NamedCollection(Of String))
            If traceback Is Nothing Then
                Return New NamedCollection(Of String)() {}
            Else
                Return traceback.GetTraceback
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function MeasureCurve(ds As EntityClusterModel(), traceback As TraceBackIterator) As IEnumerable(Of PointF)
            Return MeasureCurve(New DataSetConvertor(ds).GetVectors(ds).ToArray, traceback)
        End Function

        Public Shared Iterator Function MeasureCurve(data As ClusterEntity(), traceback As TraceBackIterator) As IEnumerable(Of PointF)
            Dim score As Double

            For i As Integer = 0 To traceback.size - 1
                traceback.SetTraceback(data, itr:=i)
                score = data.Silhouette

                Yield New PointF(i, score)
            Next
        End Function
    End Class
End Namespace