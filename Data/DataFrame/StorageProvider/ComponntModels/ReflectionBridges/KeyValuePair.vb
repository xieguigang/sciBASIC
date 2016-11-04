Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace StorageProvider.ComponentModels

    Public Class KeyValuePair : Inherits StorageProvider

        Public Overrides ReadOnly Property Name As String

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.KeyValuePair
            End Get
        End Property

        Dim _KeyProperty As PropertyInfo
        Dim _ValueProperty As PropertyInfo

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">可能会通过<see cref="ColumnAttribute"/>来取别名</param>  
        ''' <param name="BindProperty"></param>
        Private Sub New(Name As String, BindProperty As PropertyInfo, LoadMethod As Func(Of String, Object))
            Call MyBase.New(BindProperty, LoadMethod)
            Me.Name = Name
        End Sub

        Public Shared Function CreateObject(Name As String, BindProperty As PropertyInfo) As KeyValuePair
            Dim KeyValue As Type = BindProperty.PropertyType
            Dim proHash = KeyValue.GetProperties.ToDictionary(Function(prop) prop.Name)
            Dim KeyProperty As PropertyInfo = proHash(NameOf(__LoadValue.Key))
            Dim ValueProperty As PropertyInfo = proHash(NameOf(__LoadValue.Value))
            Dim GetValue As New __LoadValue With {
                .Key = KeyProperty.PropertyType,
                .Value = ValueProperty.PropertyType,
                .ValueType = BindProperty.PropertyType
            }

            Return New KeyValuePair(Name, BindProperty, AddressOf GetValue.GetValue) With {
                ._KeyProperty = KeyProperty,
                ._ValueProperty = ValueProperty
            }
        End Function

        Private Class __LoadValue

            Public Property Key As Type
            Public Property Value As Type
            Public Property ValueType As Type

            Public Function GetValue(str As String) As Object
                Dim Tokens As String() = Strings.Split(str, ":=")
                Dim Key As Object = Scripting.CTypeDynamic(Tokens(Scan0), Me.Key)
                Dim Value As Object = Scripting.CTypeDynamic(Tokens(1), Me.Value)
                Return Activator.CreateInstance(ValueType, {Key, Value})
            End Function

            Public Overrides Function ToString() As String
                Return ValueType.FullName
            End Function
        End Class

        Public Overrides Function ToString([object] As Object) As String
            Dim Key As Object = _KeyProperty.GetValue([object], Nothing)
            Dim value As Object = _ValueProperty.GetValue([object], Nothing)
            Dim strKey As String = Scripting.ToString(Key)
            Dim strValue As String = Scripting.ToString(value)
            Return $"{strKey}:={strValue}"
        End Function
    End Class
End Namespace