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