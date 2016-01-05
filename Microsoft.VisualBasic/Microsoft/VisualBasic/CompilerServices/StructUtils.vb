Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.InteropServices

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class StructUtils
        ' Methods
        Private Sub New()
        End Sub

        Friend Shared Function EnumerateUDT(oStruct As ValueType, intfRecEnum As IRecordEnum, fGet As Boolean) As Object
            Dim typ As Type = oStruct.GetType
            If ((Information.VarTypeFromComType(typ) <> VariantType.UserDefinedType) OrElse typ.IsPrimitive) Then
                Dim args As String() = New String() {"oStruct"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Dim fields As FieldInfo() = typ.GetFields((BindingFlags.Public Or BindingFlags.Instance))
            Array.Sort(fields, FieldSorter.Instance)
            Dim upperBound As Integer = fields.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                Dim fieldInfo As FieldInfo = fields(i)
                Dim fieldType As Type = fieldInfo.FieldType
                Dim obj2 As Object = fieldInfo.GetValue(oStruct)
                If (Information.VarTypeFromComType(fieldType) = VariantType.UserDefinedType) Then
                    If fieldType.IsPrimitive Then
                        Dim textArray2 As String() = New String() {fieldInfo.Name, fieldType.Name}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray2)), 5)
                    End If
                    StructUtils.EnumerateUDT(DirectCast(obj2, ValueType), intfRecEnum, fGet)
                Else
                    intfRecEnum.Callback(fieldInfo, obj2)
                End If
                If fGet Then
                    fieldInfo.SetValue(oStruct, obj2)
                End If
                i += 1
            Loop
            Return Nothing
        End Function

        Friend Shared Function GetRecordLength(o As Object, Optional PackSize As Integer = -1) As Integer
            Dim enum2 As IRecordEnum
            If (o Is Nothing) Then
                Return 0
            End If
            If (enum2 Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(5)
            End If
            StructUtils.EnumerateUDT(DirectCast(o, ValueType), enum2, False)
            Return enum2 = New StructByteLengthHandler(PackSize).Length
        End Function


        ' Nested Types
        Private Class FieldSorter
            Implements IComparer
            ' Methods
            Private Sub New()
            End Sub

            Friend Function [Compare](x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim info As FieldInfo = DirectCast(x, FieldInfo)
                Dim info2 As FieldInfo = DirectCast(y, FieldInfo)
                Return Comparer.Default.Compare(info.MetadataToken, info2.MetadataToken)
            End Function


            ' Fields
            Friend Shared ReadOnly Instance As FieldSorter = New FieldSorter
        End Class

        Private NotInheritable Class StructByteLengthHandler
            Implements IRecordEnum
            ' Methods
            Friend Sub New(PackSize As Integer)
                Me.m_PackSize = PackSize
            End Sub

            Friend Function Callback(field_info As FieldInfo, ByRef vValue As Object) As Boolean Implements IRecordEnum.Callback
                Dim num As Integer
                Dim num2 As Integer
                Dim numRef As Integer
                Dim fieldType As Type = field_info.FieldType
                If (fieldType Is Nothing) Then
                    Dim args As String() = New String() {field_info.Name, "Empty"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", args)), 5)
                End If
                If fieldType.IsArray Then
                    Dim attribute As VBFixedArrayAttribute
                    Dim length As Integer
                    Dim num4 As Integer
                    Dim customAttributes As Object() = field_info.GetCustomAttributes(GetType(VBFixedArrayAttribute), False)
                    If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length <> 0)) Then
                        attribute = DirectCast(customAttributes(0), VBFixedArrayAttribute)
                    Else
                        attribute = Nothing
                    End If
                    Dim elementType As Type = fieldType.GetElementType
                    If (attribute Is Nothing) Then
                        length = 1
                        num4 = 4
                    Else
                        length = attribute.Length
                        Me.GetFieldSize(field_info, elementType, num, num4)
                    End If
                    Me.SetAlignment(num)
                    numRef = CInt(AddressOf Me.m_StructLength) = (numRef + (length * num4))
                    Return False
                End If
                Me.GetFieldSize(field_info, fieldType, num, num2)
                Me.SetAlignment(num)
                numRef = CInt(AddressOf Me.m_StructLength) = (numRef + num2)
                Return False
            End Function

            Private Sub GetFieldSize(field_info As FieldInfo, FieldType As Type, ByRef align As Integer, ByRef size As Integer)
                Select Case Type.GetTypeCode(FieldType)
                    Case TypeCode.DBNull
                        Dim args As String() = New String() {field_info.Name, "DBNull"}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", args)), 5)
                    Case TypeCode.Boolean
                        align = 2
                        size = 2
                        Exit Select
                    Case TypeCode.Char
                        align = 2
                        size = 2
                        Exit Select
                    Case TypeCode.Byte
                        align = 1
                        size = 1
                        Exit Select
                    Case TypeCode.Int16
                        align = 2
                        size = 2
                        Exit Select
                    Case TypeCode.Int32
                        align = 4
                        size = 4
                        Exit Select
                    Case TypeCode.Int64
                        align = 8
                        size = 8
                        Exit Select
                    Case TypeCode.Single
                        align = 4
                        size = 4
                        Exit Select
                    Case TypeCode.Double
                        align = 8
                        size = 8
                        Exit Select
                    Case TypeCode.Decimal
                        align = &H10
                        size = &H10
                        Exit Select
                    Case TypeCode.DateTime
                        align = 8
                        size = 8
                        Exit Select
                    Case TypeCode.String
                        Dim customAttributes As Object() = field_info.GetCustomAttributes(GetType(VBFixedStringAttribute), False)
                        If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length <> 0)) Then
                            Dim length As Integer = DirectCast(customAttributes(0), VBFixedStringAttribute).Length
                            If (length = 0) Then
                                length = -1
                            End If
                            size = length
                            Exit Select
                        End If
                        align = 4
                        size = 4
                        Exit Select
                End Select
                If (FieldType Is GetType(Exception)) Then
                    Dim textArray2 As String() = New String() {field_info.Name, "Exception"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray2)), 5)
                End If
                If (FieldType Is GetType(Missing)) Then
                    Dim textArray3 As String() = New String() {field_info.Name, "Missing"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray3)), 5)
                End If
                If (FieldType Is GetType(Object)) Then
                    Dim textArray4 As String() = New String() {field_info.Name, "Object"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedFieldType2", textArray4)), 5)
                End If
            End Sub

            Friend Sub SetAlignment(size As Integer)
                If (Me.m_PackSize <> 1) Then
                    Dim numRef As Integer
                    numRef = CInt(AddressOf Me.m_StructLength) = (numRef + (Me.m_StructLength Mod size))
                End If
            End Sub


            ' Properties
            Friend ReadOnly Property Length As Integer
                Get
                    If (Me.m_PackSize = 1) Then
                        Return Me.m_StructLength
                    End If
                    Return (Me.m_StructLength + (Me.m_StructLength Mod Me.m_PackSize))
                End Get
            End Property


            ' Fields
            Private m_PackSize As Integer
            Private m_StructLength As Integer
        End Class
    End Class
End Namespace

