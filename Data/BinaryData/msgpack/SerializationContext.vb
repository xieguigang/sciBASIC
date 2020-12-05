#Region "Microsoft.VisualBasic::0f7b720b2967458785b8901c7ef56a16, Data\BinaryData\msgpack\SerializationContext.vb"

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

    ' Class SerializationContext
    ' 
    '     Properties: SerializationMethod
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: (+2 Overloads) RegisterSerializer
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization

Public Class SerializationContext

    Friend _serializers As Dictionary(Of Type, MsgPackSerializer)
    Friend _serializationMethod As SerializationMethod

    Public Property SerializationMethod As SerializationMethod
        Get
            Return _serializationMethod
        End Get
        Set(value As SerializationMethod)
            If _serializationMethod <> value Then
                Select Case value
                    Case SerializationMethod.Array, SerializationMethod.Map
                        _serializationMethod = value
                    Case Else
                        Throw New ArgumentOutOfRangeException("value")
                End Select

                _serializers = New Dictionary(Of Type, MsgPackSerializer)()
            End If
        End Set
    End Property

    Public Sub New()
        _serializers = New Dictionary(Of Type, MsgPackSerializer)()
        _serializationMethod = SerializationMethod.Array
    End Sub

    Public Sub RegisterSerializer(Of T)(propertyDefinitions As IList(Of MessagePackMemberDefinition))
        _serializers(GetType(T)) = New MsgPackSerializer(GetType(T), propertyDefinitions)
    End Sub

    Public Sub RegisterSerializer(Of T)(ParamArray propertyNames As String())
        Dim defs As New List(Of MessagePackMemberDefinition)()

        For Each propertyName In propertyNames
            defs.Add(New MessagePackMemberDefinition() With {
                .PropertyName = propertyName,
                .NilImplication = NilImplication.MemberDefault
            })
        Next

        _serializers(GetType(T)) = New MsgPackSerializer(GetType(T), defs)
    End Sub
End Class
