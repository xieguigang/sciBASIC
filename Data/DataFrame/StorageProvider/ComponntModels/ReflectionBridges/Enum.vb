Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace StorageProvider.ComponentModels

    Public Class [Enum] : Inherits StorageProvider

        ''' <summary>
        ''' 可能会通过<see cref="ColumnAttribute"/>来取别名
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Name As String

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.Enum
            End Get
        End Property

        Private Sub New(BindProperty As PropertyInfo, Method As Func(Of String, Object))
            Call MyBase.New(BindProperty, Method)
        End Sub

        Dim _EnumValues As GetEnum

        Public Function TryGetValue(Name As String) As System.Enum
            Return _EnumValues.TryGetValue(Name)
        End Function

        Public Shared Function CreateObject(Name As String, BindProperty As PropertyInfo) As [Enum]
            Dim typeDef As Type = BindProperty.PropertyType
            Dim GetValues = New GetEnum(typeDef)
            Return New [Enum](BindProperty, AddressOf GetValues.TryGetValue) With {
                ._Name = Name,
                ._EnumValues = GetValues
            }
        End Function

        Public Overrides Function ToString([object] As Object) As String
            Return DirectCast([object], System.Enum).ToString
        End Function

        Private Class GetEnum
            ReadOnly _EnumValues As Dictionary(Of String, System.Enum)

            Sub New(typeDef As Type)
                Dim EnumValues = Scripting.CastArray(Of System.Enum)(typeDef.GetEnumValues)
                Dim EnumNames = typeDef.GetEnumNames
                Dim EnumHash = (From i As Integer
                                In EnumNames.Sequence
                                Select enuName = EnumNames(i), enuValue = EnumValues(i)) _
                                    .ToDictionary(Function(obj) obj.enuName, elementSelector:=Function(obj) obj.enuValue)

                Me._EnumValues = EnumHash
            End Sub

            Public Function TryGetValue(Name As String) As System.Enum
                If _EnumValues.ContainsKey(Name) Then
                    Return _EnumValues(Name)
                Else
                    Return _EnumValues.First.Value
                End If
            End Function
        End Class
    End Class
End Namespace