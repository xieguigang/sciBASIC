Namespace Xdr

    Public Class EnumHelper(Of T As Structure)

        Private Shared ReadOnly _enumMap As Dictionary(Of T, Integer)
        Private Shared ReadOnly _intMap As Dictionary(Of Integer, T)

        Shared Sub New()
            Dim underType As Type = GetType(T).GetEnumUnderlyingType()
            Dim conv As Func(Of T, Integer)

            If underType Is GetType(Byte) Then
                conv = Function(item) CByte(CType(item, ValueType))
            ElseIf underType Is GetType(SByte) Then
                conv = Function(item) CSByte(CType(item, ValueType))
            ElseIf underType Is GetType(Short) Then
                conv = Function(item) CShort(CType(item, ValueType))
            ElseIf underType Is GetType(UShort) Then
                conv = Function(item) CUShort(CType(item, ValueType))
            ElseIf underType Is GetType(Integer) Then
                conv = Function(item) CType(item, ValueType)
            Else
                Throw New NotSupportedException(String.Format("unsupported type {0}", GetType(T).FullName))
            End If

            _intMap = New Dictionary(Of Integer, T)()
            _enumMap = New Dictionary(Of T, Integer)()

            For Each item In [Enum].GetValues(GetType(T)).Cast(Of T)()
                Dim exist As T = Nothing
                Dim key = conv(item)
                If Not _intMap.TryGetValue(key, exist) Then _intMap.Add(key, item)
                If Not _enumMap.TryGetValue(item, key) Then _enumMap.Add(item, conv(item))
            Next
        End Sub

        Public Shared Function IntToEnum(val As Integer) As T
            Dim exist As T = Nothing
            If _intMap.TryGetValue(val, exist) Then Return exist
            Throw New InvalidCastException(String.Format("type `{0}' not contain {1}", GetType(T).FullName, val))
        End Function

        Public Shared Function EnumToInt(item As T) As Integer
            Dim val As Integer
            If _enumMap.TryGetValue(item, val) Then Return val
            Throw New InvalidCastException(String.Format("enum {0} not contain value {1}", GetType(T).FullName, item))
        End Function
    End Class
End Namespace
