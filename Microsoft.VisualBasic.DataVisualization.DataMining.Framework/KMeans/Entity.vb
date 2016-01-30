Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Namespace KMeans

    Public Class EntityLDM : Implements sIdEnumerable

        Public Property Name As String Implements sIdEnumerable.Identifier
        <Meta(GetType(Double))>
        Public Property Properties As Dictionary(Of String, Double)
        Public Property Cluster As String

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
    End Class

    Public Class Entity : Inherits EntityBase(Of Double)
        Implements sIdEnumerable

        Public Property uid As String Implements sIdEnumerable.Identifier

        Public Overrides Function ToString() As String
            Return $"{uid}  ({Length} Properties)"
        End Function

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

        Public Function ToLDM() As EntityLDM
            Return New EntityLDM With {
                .Name = uid,
                .Properties = Properties.ToArray(Function(x, i) New With {.i = i, .x = x}).ToDictionary(Function(x) CStr(x.i), Function(x) x.x)
            }
        End Function

        Public Function ToLDM(maps As String()) As EntityLDM
            Return New EntityLDM With {
                .Name = uid,
                .Properties = Properties.ToArray(Function(x, i) New With {.i = i, .x = x}).ToDictionary(Function(x) maps(x.i), Function(x) x.x)
            }
        End Function
    End Class
End Namespace