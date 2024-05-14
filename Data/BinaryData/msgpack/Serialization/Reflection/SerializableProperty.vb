#Region "Microsoft.VisualBasic::2664e780b6929a0c81c85c7d4b011d32, Data\BinaryData\msgpack\Serialization\Reflection\SerializableProperty.vb"

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

    '   Total Lines: 52
    '    Code Lines: 39
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 2.01 KB


    '     Class SerializableProperty
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
Imports System.Runtime.CompilerServices

Namespace Serialization.Reflection

    Friend Class SerializableProperty

        Friend Shared ReadOnly EmptyObjArgs As Object() = {}

        Friend propInfo As PropertyInfo
        Friend name As String
        Friend valueType As Type
        Friend sequence As Integer

        ReadOnly _nilImplication As NilImplication

        Friend Sub New(propInfo As PropertyInfo, Optional sequence As Integer = 0, Optional nilImplication As NilImplication? = Nothing)
            Me.propInfo = propInfo
            Me.name = propInfo.Name
            Me._nilImplication = If(nilImplication, Serialization.NilImplication.MemberDefault)
            Me.sequence = sequence
            Me.valueType = propInfo.PropertyType

            Dim underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType)

            If underlyingType IsNot Nothing Then
                valueType = underlyingType

                If nilImplication.HasValue = False Then
                    _nilImplication = Serialization.NilImplication.Null
                End If
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Serialize(o As Object, writer As BinaryWriter, serializationMethod As SerializationMethod)
            Call SerializeValue(propInfo.GetValue(o, EmptyObjArgs), writer, serializationMethod)
        End Sub

        Friend Sub Deserialize(o As Object, reader As BinaryDataReader)
            Dim val = DeserializeValue(valueType, reader, _nilImplication)
            Dim safeValue = If(val Is Nothing, Nothing, Convert.ChangeType(val, valueType))

            Call propInfo.SetValue(o, safeValue, EmptyObjArgs)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("[SerializableProperty: [{0}] as {1}]", name, valueType)
        End Function
    End Class
End Namespace
