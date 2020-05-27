Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Module Distance

    <Extension>
    Public Function Euclidean(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet)) As DistanceMatrix
        Return data.MatrixBuilder(AddressOf EuclideanDistance, True)
    End Function
End Module
