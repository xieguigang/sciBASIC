Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace KMeans

    ''' <summary>
    ''' 存储在Csv文件里面的数据模型
    ''' </summary>
    Public Class EntityLDM : Inherits DynamicPropertyBase(Of Double)
        Implements sIdEnumerable

        Public Property Name As String Implements sIdEnumerable.Identifier

        <Meta(GetType(Double))>
        Public Overrides Property Properties As Dictionary(Of String, Double)
            Get
                Return MyBase.Properties
            End Get
            Set(value As Dictionary(Of String, Double))
                MyBase.Properties = value
            End Set
        End Property

        Public Property Cluster As String

        Public Sub Add(key As String, n As Double)
            Call Properties.Add(key, n)
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared Function Load(path As String) As Entity()
            Return Entity.Load(path)
        End Function

        Public Function ToModel() As Entity
            Return New Entity With {
                .uid = Name,
                .Properties = Properties.Values.ToArray
            }
        End Function

        Public Shared Function Load(path As String, map As String) As EntityLDM()
            Dim maps As New Dictionary(Of String, String) From {
                {map, NameOf(EntityLDM.Name)}
            }
            Return path.LoadCsv(Of EntityLDM)(maps:=maps).ToArray
        End Function

        Public Shared Iterator Function FromModel(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))) As IEnumerable(Of EntityLDM)
            For Each x As NamedValue(Of Dictionary(Of String, Double)) In data
                Yield New EntityLDM With {
                    .Name = x.Name,
                    .Properties = x.x
                }
            Next
        End Function
    End Class
End Namespace