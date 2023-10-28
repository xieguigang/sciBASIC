

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans

Public Class KNNGraph

    ReadOnly tree As KdTree(Of EntityClusterModel)

    Sub New(rawdata As IEnumerable(Of EntityClusterModel))
        tree = New KdTree(Of EntityClusterModel)(rawdata, metric:=New ObjAccess)
    End Sub



    Private Class ObjAccess : Inherits KdNodeAccessor(Of EntityClusterModel)

        Dim dims As String()

        Sub New()
            analysis_template = AddressOf loadTemplate
        End Sub

        Private Sub loadTemplate(a As EntityClusterModel)
            dims = a.Properties.Keys.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As EntityClusterModel, dimName As String, value As Double)
            x(dimName) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dims
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As EntityClusterModel, b As EntityClusterModel) As Double
            Throw New NotImplementedException()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As EntityClusterModel, dimName As String) As Double
            Return x(dimName)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As EntityClusterModel, b As EntityClusterModel) As Boolean
            Return a.ID = b.ID
        End Function

        Public Overrides Function activate() As EntityClusterModel
            Return New EntityClusterModel
        End Function
    End Class

End Class