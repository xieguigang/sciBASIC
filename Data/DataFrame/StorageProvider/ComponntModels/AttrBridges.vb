#Region "Microsoft.VisualBasic::90ea1ea505a3376088920b77f22c754e, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\StorageProvider\ComponntModels\AttrBridges.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    Public Class Column : Inherits StorageProvider

        Public Property ColumnDefine As ColumnAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return ColumnDefine.Name
            End Get
        End Property

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.Column
            End Get
        End Property

        Sub New(p As ColumnAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty)
            ColumnDefine = p
        End Sub

        Public Overrides Function ToString([object] As Object) As String
            Return Scripting.ToString([object])
        End Function
    End Class

    Public Class MetaAttribute : Inherits StorageProvider

        Public Property MetaAttribute As Csv.StorageProvider.Reflection.MetaAttribute

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

        Sub New(MetaAttribute As Csv.StorageProvider.Reflection.MetaAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty, MetaAttribute.TypeId)
            Me.MetaAttribute = MetaAttribute
            Me.Dictionary = GetType(Dictionary(Of ,)).MakeGenericType(GetType(String), MetaAttribute.TypeId)
        End Sub

        Public Function CreateDictionary() As IDictionary
            Return DirectCast(Activator.CreateInstance(Dictionary), IDictionary)
        End Function

        Public Overrides Function ToString([object] As Object) As String
            Return ""
        End Function
    End Class

    Public Class CollectionColumn : Inherits StorageProvider

        Public Property CollectionColumn As Csv.StorageProvider.Reflection.CollectionAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return CollectionColumn.Name
            End Get
        End Property

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.CollectionColumn
            End Get
        End Property

        Private Sub New(p As Csv.StorageProvider.Reflection.CollectionAttribute, BindProperty As PropertyInfo, LoadMethod As Func(Of String, Object))
            Call MyBase.New(BindProperty, LoadMethod)
            Me.CollectionColumn = p
        End Sub

        Public Shared Function CreateObject(p As Csv.StorageProvider.Reflection.CollectionAttribute,
                                            BindProperty As PropertyInfo,
                                            ElementType As Type) As CollectionColumn
            Dim LoadData As New __createArray(ElementType, p.Delimiter)
            Return New CollectionColumn(p, BindProperty, AddressOf LoadData.LoadData)
        End Function

        Private Class __createArray

            Dim Cast As Func(Of String, Object)
            Dim Delimiter As String
            Dim Element As Type

            Sub New(ElementType As Type, delimiter As String)
                Me.Delimiter = delimiter
                Me.Element = ElementType

                Cast = Scripting.CasterString(ElementType)
            End Sub

            Public Function LoadData(cellData As String) As Object
                If String.IsNullOrEmpty(cellData) Then
                    Return Nothing
                End If
                Dim Tokens As String() = Strings.Split(cellData, Delimiter)
                Dim array = Tokens.ToArray(Function(s) Cast(s))
                Return Scripting.InputHandler.DirectCast(array, Element)
            End Function

            Public Overrides Function ToString() As String
                Return Element.FullName
            End Function
        End Class

        Public Overrides Function ToString([object] As Object) As String
            If [object] Is Nothing Then
                Return ""
            End If

            Dim array = Scripting.CastArray(Of Object)([object]).ToArray(Function(obj) Scripting.ToString(obj))
            Return String.Join(CollectionColumn.Delimiter, array)
        End Function
    End Class

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
