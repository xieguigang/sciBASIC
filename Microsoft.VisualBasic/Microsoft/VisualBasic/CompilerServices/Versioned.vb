Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Dynamic

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class Versioned
        ' Methods
        Private Sub New()
        End Sub

        Public Shared Function CallByName(Instance As Object, MethodName As String, UseCallType As CallType, ParamArray Arguments As Object()) As Object
            Select Case UseCallType
                Case CallType.Method
                    Return NewLateBinding.LateCall(Instance, Nothing, MethodName, Arguments, Nothing, Nothing, Nothing, False)
                Case CallType.Get
                    Return NewLateBinding.LateGet(Instance, Nothing, MethodName, Arguments, Nothing, Nothing, Nothing)
                Case CallType.Let, CallType.Set
                    Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
                    If (Not instance Is Nothing) Then
                        IDOBinder.IDOSet(instance, MethodName, Nothing, Arguments)
                    Else
                        NewLateBinding.LateSet(instance, Nothing, MethodName, Arguments, Nothing, Nothing, False, False, UseCallType)
                    End If
                    Return Nothing
            End Select
            Dim args As String() = New String() {"CallType"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
        End Function

        Public Shared Function IsNumeric(Expression As Object) As Boolean
            Dim convertible As IConvertible = TryCast(Expression, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return True
                    Case TypeCode.Char, TypeCode.String
                        Dim num As Double
                        Dim str As String = convertible.ToString(Nothing)
                        Try
                            Dim num2 As Long
                            If Utils.IsHexOrOctValue(str, num2) Then
                                Return True
                            End If
                        Catch exception As FormatException
                            Return False
                        End Try
                        Return Conversions.TryParseDouble(str, num)
                    Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                        Return True
                End Select
            End If
            Return False
        End Function

        Public Shared Function SystemTypeName(VbName As String) As String
            Dim s As String = Strings.Trim(VbName).ToUpperInvariant

            ' <PrivateImplementationDetails>
            Select Case ComputeStringHash(s)
                Case &H1B2E0F7A
                    If (s = "OBJECT") Then
                        Return "System.Object"
                    End If
                    Exit Select
                Case &H21EC44B5
                    If (s = "SHORT") Then
                        Return "System.Int16"
                    End If
                    Exit Select
                Case &HD75DF9F
                    If (s = "BYTE") Then
                        Return "System.Byte"
                    End If
                    Exit Select
                Case &H19FA1E69
                    If (s = "SINGLE") Then
                        Return "System.Single"
                    End If
                    Exit Select
                Case &H48AF9A2C
                    If (s = "DECIMAL") Then
                        Return "System.Decimal"
                    End If
                    Exit Select
                Case &H824F138E
                    If (s = "ULONG") Then
                        Return "System.UInt64"
                    End If
                    Exit Select
                Case &H880B7E9F
                    If (s = "BOOLEAN") Then
                        Return "System.Boolean"
                    End If
                    Exit Select
                Case &H8D1D3425
                    If (s = "INTEGER") Then
                        Return "System.Int32"
                    End If
                    Exit Select
                Case &HC0DE1353
                    If (s = "LONG") Then
                        Return "System.Int64"
                    End If
                    Exit Select
                Case &HC48AC062
                    If (s = "USHORT") Then
                        Return "System.UInt16"
                    End If
                    Exit Select
                Case &HA59F665D
                    If (s = "CHAR") Then
                        Return "System.Char"
                    End If
                    Exit Select
                Case &HC007F499
                    If (s = "DATE") Then
                        Return "System.DateTime"
                    End If
                    Exit Select
                Case &HD4897AA8
                    If (s = "SBYTE") Then
                        Return "System.SByte"
                    End If
                    Exit Select
                Case &HDD37540A
                    If (s = "UINTEGER") Then
                        Return "System.UInt32"
                    End If
                    Exit Select
                Case &HDF980448
                    If (s = "DOUBLE") Then
                        Return "System.Double"
                    End If
                    Exit Select
                Case &HF6097378
                    If (s = "STRING") Then
                        Return "System.String"
                    End If
                    Exit Select
            End Select
            Return Nothing
        End Function

        Public Shared Function TypeName( Expression As Object) As String
            Dim str2 As String
            If (Expression Is Nothing) Then
                Return "Nothing"
            End If
            Dim typ As Type = Expression.GetType
            If (typ.IsCOMObject AndAlso (String.CompareOrdinal(typ.Name, "__ComObject") = 0)) Then
                str2 = Information.TypeNameOfCOMObject(Expression, True)
            Else
                str2 = Utils.VBFriendlyNameOfType(typ, False)
            End If
            Return str2
        End Function

        Public Shared Function VbTypeName( SystemName As String) As String
            SystemName = Strings.Trim(SystemName).ToUpperInvariant
            If (Strings.Left(SystemName, 7) = "SYSTEM.") Then
                SystemName = Strings.Mid(SystemName, 8)
            End If
            Dim s As String = SystemName

            ' <PrivateImplementationDetails>
            Select Case ComputeStringHash(s)
                Case &HBECAA04
                    If (s = "INT64") Then
                        Return "Long"
                    End If
                    Exit Select
                Case &HD75DF9F
                    If (s = "BYTE") Then
                        Return "Byte"
                    End If
                    Exit Select
                Case &H43E9AB2
                    If (s = "UINT16") Then
                        Return "UShort"
                    End If
                    Exit Select
                Case &H44FE3D3
                    If (s = "UINT64") Then
                        Return "ULong"
                    End If
                    Exit Select
                Case &HFFDF971
                    If (s = "INT16") Then
                        Return "Short"
                    End If
                    Exit Select
                Case &H19FA1E69
                    If (s = "SINGLE") Then
                        Return "Single"
                    End If
                    Exit Select
                Case &H1B2E0F7A
                    If (s = "OBJECT") Then
                        Return "Object"
                    End If
                    Exit Select
                Case &H48AF9A2C
                    If (s = "DECIMAL") Then
                        Return "Decimal"
                    End If
                    Exit Select
                Case &H8843E7AC
                    If (s = "UINT32") Then
                        Return "UInteger"
                    End If
                    Exit Select
                Case &H9357C1D0
                    If (s = "DATETIME") Then
                        Return "Date"
                    End If
                    Exit Select
                Case &H83F89FDF
                    If (s = "INT32") Then
                        Return "Integer"
                    End If
                    Exit Select
                Case &H880B7E9F
                    If (s = "BOOLEAN") Then
                        Return "Boolean"
                    End If
                    Exit Select
                Case &HA59F665D
                    If (s = "CHAR") Then
                        Return "Char"
                    End If
                    Exit Select
                Case &HD4897AA8
                    If (s = "SBYTE") Then
                        Return "SByte"
                    End If
                    Exit Select
                Case &HDF980448
                    If (s = "DOUBLE") Then
                        Return "Double"
                    End If
                    Exit Select
                Case &HF6097378
                    If (s = "STRING") Then
                        Return "String"
                    End If
                    Exit Select
            End Select
            Return Nothing
        End Function

    End Class
End Namespace

