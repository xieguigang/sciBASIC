Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Namespace KMeans

    Public Class EntityLDM
        Public Property Name As String
        <Meta(GetType(Double))>
        Public Property Properties As Dictionary(Of String, Double)

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared Function Load(path As String) As Entity()
            Return Entity.Load(path)
        End Function
    End Class

    Public Class Entity : Inherits EntityBase(Of Double)

        Public Property uid As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path">Csv文件之中除了第一列是名称标识符，其他的都必须是该实体对象的属性</param>
        ''' <returns></returns>
        Public Shared Function Load(path As String) As Entity()
            Dim data As List(Of EntityLDM) = path.LoadCsv(Of EntityLDM)
            Dim source As Entity() = data.ToArray(
                Function(x) New Entity With {
                    .uid = x.Name,
                    .Properties = x.Properties.Values.ToArray})
            Return source
        End Function
    End Class
End Namespace