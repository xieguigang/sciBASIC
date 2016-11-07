
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace FileStream

    Public MustInherit Class INetComponent : Inherits DynamicPropertyBase(Of String)

        <Meta(GetType(String))>
        Public Overrides Property Properties As Dictionary(Of String, String)
            Get
                Return MyBase.Properties
            End Get
            Set(value As Dictionary(Of String, String))
                MyBase.Properties = value
            End Set
        End Property

        Public Sub Add(key As String, value As String)
            Call Properties.Add(key, value)
        End Sub
    End Class
End Namespace