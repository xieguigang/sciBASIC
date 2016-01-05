Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Reflection

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend NotInheritable Class PutHandler
        Implements IRecordEnum
        ' Methods
        Public Sub New(oFile As VB6File)
            Me.m_oFile = oFile
        End Sub

        Public Function Callback(field_info As FieldInfo, ByRef vValue As Object) As Boolean Implements IRecordEnum.Callback
            Dim flag As Boolean
            Dim str As String
            Dim fieldType As Type = field_info.FieldType
            If (fieldType Is Nothing) Then
                Dim args As String() = New String() {field_info.Name, "Empty"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", args)), 5)
            End If
            If fieldType.IsArray Then
                Dim attribute As VBFixedArrayAttribute
                Dim fixedStringLength As Integer = -1
                Dim objArray As Object() = field_info.GetCustomAttributes(GetType(VBFixedArrayAttribute), False)
                If ((Not objArray Is Nothing) AndAlso (objArray.Length <> 0)) Then
                    attribute = DirectCast(objArray(0), VBFixedArrayAttribute)
                Else
                    attribute = Nothing
                End If
                Dim elementType As Type = fieldType.GetElementType
                If (elementType Is GetType(String)) Then
                    objArray = field_info.GetCustomAttributes(GetType(VBFixedStringAttribute), False)
                    If ((objArray Is Nothing) OrElse (objArray.Length = 0)) Then
                        fixedStringLength = -1
                    Else
                        fixedStringLength = DirectCast(objArray(0), VBFixedStringAttribute).Length
                    End If
                End If
                If (attribute Is Nothing) Then
                    Me.m_oFile.PutDynamicArray(0, DirectCast(vValue, Array), False, fixedStringLength)
                    Return flag
                End If
                Me.m_oFile.PutFixedArray(0, DirectCast(vValue, Array), elementType, fixedStringLength, attribute.FirstBound, attribute.SecondBound)
                Return flag
            End If
            Select Case Type.GetTypeCode(fieldType)
                Case TypeCode.DBNull
                    Dim textArray2 As String() = New String() {field_info.Name, "DBNull"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray2)), 5)
                Case TypeCode.Boolean
                    Me.m_oFile.PutBoolean(0, BooleanType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Char
                    Me.m_oFile.PutChar(0, CharType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Byte
                    Me.m_oFile.PutByte(0, ByteType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Int16
                    Me.m_oFile.PutShort(0, ShortType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Int32
                    Me.m_oFile.PutInteger(0, IntegerType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Int64
                    Me.m_oFile.PutLong(0, LongType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Single
                    Me.m_oFile.PutSingle(0, SingleType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Double
                    Me.m_oFile.PutDouble(0, DoubleType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.Decimal
                    Me.m_oFile.PutDecimal(0, DecimalType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.DateTime
                    Me.m_oFile.PutDate(0, DateType.FromObject(vValue), False)
                    Return flag
                Case TypeCode.String
                    If (vValue Is Nothing) Then
                        str = Nothing
                        Exit Select
                    End If
                    str = vValue.ToString
                    Exit Select
                Case Else
                    If (fieldType Is GetType(Object)) Then
                        Me.m_oFile.PutObject(vValue, 0, True)
                        Return flag
                    End If
                    If (fieldType Is GetType(Exception)) Then
                        Dim textArray3 As String() = New String() {field_info.Name, "Exception"}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray3)), 5)
                    End If
                    If (fieldType Is GetType(Missing)) Then
                        Dim textArray4 As String() = New String() {field_info.Name, "Missing"}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray4)), 5)
                    End If
                    Dim textArray5 As String() = New String() {field_info.Name, fieldType.Name}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray5)), 5)
            End Select
            Dim customAttributes As Object() = field_info.GetCustomAttributes(GetType(VBFixedStringAttribute), False)
            If ((customAttributes Is Nothing) OrElse (customAttributes.Length = 0)) Then
                Me.m_oFile.PutStringWithLength(0, str)
                Return flag
            End If
            Dim length As Integer = DirectCast(customAttributes(0), VBFixedStringAttribute).Length
            If (length = 0) Then
                length = -1
            End If
            Me.m_oFile.PutFixedLengthString(0, str, length)
            Return flag
        End Function


        ' Fields
        Public m_oFile As VB6File
    End Class
End Namespace

