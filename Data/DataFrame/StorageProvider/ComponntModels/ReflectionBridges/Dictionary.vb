Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace StorageProvider.ComponentModels

    Public Class MetaAttribute : Inherits StorageProvider

        Public Property MetaAttribute As Reflection.MetaAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return BindProperty.Name
            End Get
        End Property

        Public ReadOnly Property Dictionary As Type

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.MetaAttribute
            End Get
        End Property

        Sub New(attr As Reflection.MetaAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty, attr.TypeId)

            Me.MetaAttribute = attr
            Me.Dictionary = GetType(Dictionary(Of ,)) _
                .MakeGenericType(GetType(String), attr.TypeId)
        End Sub

        Public Function CreateDictionary() As IDictionary
            Return DirectCast(Activator.CreateInstance(Dictionary), IDictionary)
        End Function

        Public Overrides Function ToString([object] As Object) As String
            Return ""
        End Function
    End Class
End Namespace