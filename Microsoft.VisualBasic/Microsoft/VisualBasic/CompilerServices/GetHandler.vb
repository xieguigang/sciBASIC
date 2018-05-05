Imports System.ComponentModel
Imports System.Reflection

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend NotInheritable Class GetHandler
        Implements IRecordEnum
        ' Methods
        Public Sub New(oFile As VB6File)
            Me.m_oFile = oFile
        End Sub

        Public Function Callback(field_info As FieldInfo, ByRef vValue As Object) As Boolean Implements IRecordEnum.Callback
            Dim flag As Boolean
            Dim fieldType As Type = field_info.FieldType
            If (fieldType Is Nothing) Then
                Dim textArray1 As String() = New String() {field_info.Name, "Empty"}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray1)), 5)
            End If
            If fieldType.IsArray Then
                Dim customAttributes As Object() = field_info.GetCustomAttributes(GetType(VBFixedArrayAttribute), False)
                Dim arr As Array = Nothing
                Dim fixedStringLength As Integer = -1
                Dim objArray2 As Object() = field_info.GetCustomAttributes(GetType(VBFixedStringAttribute), False)
                If ((Not objArray2 Is Nothing) AndAlso (objArray2.Length > 0)) Then
                    Dim attribute As VBFixedStringAttribute = DirectCast(objArray2(0), VBFixedStringAttribute)
                    If (attribute.Length > 0) Then
                        fixedStringLength = attribute.Length
                    End If
                End If
                If ((customAttributes Is Nothing) OrElse (customAttributes.Length = 0)) Then
                    Me.m_oFile.GetDynamicArray(arr, fieldType.GetElementType, fixedStringLength)
                Else
                    Dim attribute1 As VBFixedArrayAttribute = DirectCast(customAttributes(0), VBFixedArrayAttribute)
                    Dim firstBound As Integer = attribute1.FirstBound
                    Dim secondBound As Integer = attribute1.SecondBound
                    arr = DirectCast(vValue, Array)
                    Me.m_oFile.GetFixedArray(0, arr, fieldType.GetElementType, firstBound, secondBound, fixedStringLength)
                End If
                vValue = arr
                Return flag
            End If
            Select Case Type.GetTypeCode(fieldType)
                Case TypeCode.DBNull
                    Dim textArray2 As String() = New String() {field_info.Name, "DBNull"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray2)), 5)
                Case TypeCode.Boolean
                    vValue = Me.m_oFile.GetBoolean(0)
                    Return flag
                Case TypeCode.Char
                    vValue = Me.m_oFile.GetChar(0)
                    Return flag
                Case TypeCode.Byte
                    vValue = Me.m_oFile.GetByte(0)
                    Return flag
                Case TypeCode.Int16
                    vValue = Me.m_oFile.GetShort(0)
                    Return flag
                Case TypeCode.Int32
                    vValue = Me.m_oFile.GetInteger(0)
                    Return flag
                Case TypeCode.Int64
                    vValue = Me.m_oFile.GetLong(0)
                    Return flag
                Case TypeCode.Single
                    vValue = Me.m_oFile.GetSingle(0)
                    Return flag
                Case TypeCode.Double
                    vValue = Me.m_oFile.GetDouble(0)
                    Return flag
                Case TypeCode.Decimal
                    vValue = Me.m_oFile.GetDecimal(0)
                    Return flag
                Case TypeCode.DateTime
                    vValue = Me.m_oFile.GetDate(0)
                    Return flag
                Case TypeCode.String
                    Dim objArray3 As Object() = field_info.GetCustomAttributes(GetType(VBFixedStringAttribute), False)
                    If ((Not objArray3 Is Nothing) AndAlso (objArray3.Length <> 0)) Then
                        Dim length As Integer = DirectCast(objArray3(0), VBFixedStringAttribute).Length
                        If (length = 0) Then
                            length = -1
                        End If
                        vValue = Me.m_oFile.GetFixedLengthString(0, length)
                        Return flag
                    End If
                    vValue = Me.m_oFile.GetLengthPrefixedString(0)
                    Return flag
            End Select
            If (fieldType Is GetType(Object)) Then
                Me.m_oFile.GetObject(vValue, 0, True)
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
            Dim args As String() = New String() {field_info.Name, fieldType.Name}
            Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", args)), 5)
        End Function


        ' Fields
        Private m_oFile As VB6File
    End Class
End Namespace

