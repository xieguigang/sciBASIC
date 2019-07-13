#Region "Microsoft.VisualBasic::853113e2b5a45591c2f8cb33fc040466, Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Dictionary.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class MetaAttribute
    ' 
    '         Properties: Dictionary, MetaAttribute, Name, ProviderId
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateDictionary, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
