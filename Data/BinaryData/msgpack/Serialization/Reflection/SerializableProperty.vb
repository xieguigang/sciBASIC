#Region "Microsoft.VisualBasic::391b031e76bf3f6a5ee37976ee263cd7, Data\BinaryData\msgpack\Serialization\Reflection\SerializableProperty.vb"

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

    '     Class SerializableProperty
    ' 
    '         Properties: Name, PropInfo, Sequence, ValueType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Deserialize, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection

Namespace Serialization.Reflection

    Friend Class SerializableProperty

        Private _PropInfo As System.Reflection.PropertyInfo, _Name As String, _ValueType As System.Type
        Friend Shared ReadOnly EmptyObjArgs As Object() = {}
        Private ReadOnly _nilImplication As NilImplication

        Friend Sub New(propInfo As PropertyInfo, Optional sequence As Integer = 0, Optional nilImplication As NilImplication? = Nothing)
            Me.PropInfo = propInfo
            Name = propInfo.Name
            _nilImplication = If(nilImplication, Serialization.NilImplication.MemberDefault)
            Me.Sequence = sequence
            ValueType = propInfo.PropertyType
            Dim underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType)

            If underlyingType IsNot Nothing Then
                ValueType = underlyingType

                If nilImplication.HasValue = False Then
                    _nilImplication = Serialization.NilImplication.Null
                End If
            End If
        End Sub

        Friend Property PropInfo As PropertyInfo
            Get
                Return _PropInfo
            End Get
            Private Set(value As PropertyInfo)
                _PropInfo = value
            End Set
        End Property

        Friend Property Name As String
            Get
                Return _Name
            End Get
            Private Set(value As String)
                _Name = value
            End Set
        End Property

        Friend Property ValueType As Type
            Get
                Return _ValueType
            End Get
            Private Set(value As Type)
                _ValueType = value
            End Set
        End Property

        Friend Property Sequence As Integer

        Friend Sub Serialize(o As Object, writer As BinaryWriter, serializationMethod As SerializationMethod)
            SerializeValue(PropInfo.GetValue(o, EmptyObjArgs), writer, serializationMethod)
        End Sub

        Friend Sub Deserialize(o As Object, reader As BinaryReader)
            Dim val = DeserializeValue(ValueType, reader, _nilImplication)
            Dim safeValue = If(val Is Nothing, Nothing, Convert.ChangeType(val, ValueType))
            PropInfo.SetValue(o, safeValue, EmptyObjArgs)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("[SerializableProperty: Name:{0} ValueType:{1}]", Name, ValueType)
        End Function
    End Class
End Namespace
