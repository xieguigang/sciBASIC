#Region "Microsoft.VisualBasic::8d3b610070df4bf6de0cd82b8c3cc496, Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\KeyValuePair.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 72
    '    Code Lines: 54 (75.00%)
    ' Comment Lines: 5 (6.94%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 13 (18.06%)
    '     File Size: 3.00 KB


    '     Class KeyValuePair
    ' 
    '         Properties: Name, ProviderId
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateObject, ToString
    '         Class __LoadValue
    ' 
    '             Properties: Key, Value, ValueType
    ' 
    '             Function: GetValue, ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection

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
