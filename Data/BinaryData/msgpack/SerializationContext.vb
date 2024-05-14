#Region "Microsoft.VisualBasic::e810832cf5d6c82781203734f26deb1f, Data\BinaryData\msgpack\SerializationContext.vb"

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

    '   Total Lines: 70
    '    Code Lines: 53
    ' Comment Lines: 5
    '   Blank Lines: 12
    '     File Size: 2.63 KB


    ' Class SerializationContext
    ' 
    '     Properties: SerializationMethod
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: (+3 Overloads) RegisterSerializer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization

Public Class SerializationContext

    Friend _serializers As Dictionary(Of Type, MsgPackSerializer)
    Friend _serializationMethod As SerializationMethod

    Public Property SerializationMethod As SerializationMethod
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub RegisterSerializer(Of T)(propertyDefinitions As IList(Of MessagePackMemberDefinition))
        _serializers(GetType(T)) = New MsgPackSerializer(GetType(T), propertyDefinitions)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="propertyNames"></param>
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

    Public Sub RegisterSerializer(Of T)(provider As SchemaProvider(Of T))
        For Each define In provider.GetObjectSchema
            Dim defs As New List(Of MessagePackMemberDefinition)()

            For Each propertyName In define.schema
                defs.Add(New MessagePackMemberDefinition() With {
                    .PropertyName = propertyName.Key,
                    .NilImplication = propertyName.Value
                })
            Next

            _serializers(define.obj) = New MsgPackSerializer(define.obj, defs)
        Next
    End Sub
End Class
