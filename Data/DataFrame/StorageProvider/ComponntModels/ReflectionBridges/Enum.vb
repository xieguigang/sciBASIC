#Region "Microsoft.VisualBasic::87ee13c371bad058543ac84b5d88a51f, Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Enum.vb"

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

    '   Total Lines: 66
    '    Code Lines: 49 (74.24%)
    ' Comment Lines: 4 (6.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (19.70%)
    '     File Size: 2.52 KB


    '     Class [Enum]
    ' 
    '         Properties: Name, ProviderId
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateObject, ToString, TryGetValue
    '         Class GetEnum
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: TryGetValue
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection

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
