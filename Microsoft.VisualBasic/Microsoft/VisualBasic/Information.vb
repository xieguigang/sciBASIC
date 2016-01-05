Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Threading

Namespace Microsoft.VisualBasic
    <StandardModule>
    Public NotInheritable Class Information
        ' Methods
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Shared Function Erl() As Integer
            Return ProjectData.GetProjectData.m_Err.Erl
        End Function

        Public Shared Function Err() As ErrObject
            Dim projectData As ProjectData = ProjectData.GetProjectData
            If (projectData.m_Err Is Nothing) Then
                projectData.m_Err = New ErrObject
            End If
            Return projectData.m_Err
        End Function

        Public Shared Function IsArray(VarName As Object) As Boolean
            If (VarName Is Nothing) Then
                Return False
            End If
            Return TypeOf VarName Is Array
        End Function

        Public Shared Function IsDate(Expression As Object) As Boolean
            Dim time As DateTime
            If (Expression Is Nothing) Then
                Return False
            End If
            If TypeOf Expression Is DateTime Then
                Return True
            End If
            Dim str As String = TryCast(Expression, String)
            Return ((Not str Is Nothing) AndAlso Conversions.TryParseDate(str, time))
        End Function

        Public Shared Function IsDBNull(Expression As Object) As Boolean
            If (Expression Is Nothing) Then
                Return False
            End If
            Return TypeOf Expression Is DBNull
        End Function

        Public Shared Function IsError(Expression As Object) As Boolean
            If (Expression Is Nothing) Then
                Return False
            End If
            Return TypeOf Expression Is Exception
        End Function

        Public Shared Function IsNothing(Expression As Object) As Boolean
            Return (Expression Is Nothing)
        End Function

        Public Shared Function IsNumeric(Expression As Object) As Boolean
            Dim convertible As IConvertible = TryCast(Expression, IConvertible)
            If (convertible Is Nothing) Then
                Dim chArray As Char() = TryCast(Expression, Char())
                If (chArray Is Nothing) Then
                    Return False
                End If
                Expression = New String(chArray)
            End If
            Dim typeCode As TypeCode = convertible.GetTypeCode
            Select Case typeCode
                Case TypeCode.String, TypeCode.Char
                    Dim num As Double
                    Dim str As String = convertible.ToString(Nothing)
                    Try
                        Dim num2 As Long
                        If Utils.IsHexOrOctValue(str, num2) Then
                            Return True
                        End If
                    Catch exception As StackOverflowException
                        Throw exception
                    Catch exception2 As OutOfMemoryException
                        Throw exception2
                    Catch exception3 As ThreadAbortException
                        Throw exception3
                    Catch exception6 As Exception
                        Return False
                    End Try
                    Return DoubleType.TryParse(str, num)
            End Select
            Return Information.IsOldNumericTypeCode(typeCode)
        End Function

        Friend Shared Function IsOldNumericTypeCode(TypCode As TypeCode) As Boolean
            Select Case TypCode
                Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
            End Select
            Return False
        End Function

        Public Shared Function IsReference(Expression As Object) As Boolean
            Return Not TypeOf Expression Is ValueType
        End Function

        Public Shared Function LBound(Array As Array, Optional Rank As Integer = 1) As Integer
            If (Array Is Nothing) Then
                Dim textArray1 As String() = New String() {"Array"}
                Throw ExceptionUtils.VbMakeException(New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1)), 9)
            End If
            If ((Rank >= 1) AndAlso (Rank <= Array.Rank)) Then
                Return Array.GetLowerBound((Rank - 1))
            End If
            Dim args As String() = New String() {"Rank"}
            Throw New RankException(Utils.GetResourceString("Argument_InvalidRank1", args))
        End Function

        <SecuritySafeCritical>
        Friend Shared Function LegacyTypeNameOfCOMObject(VarName As Object, bThrowException As Boolean) As String
            Dim num As Integer
            Dim str As String = "__ComObject"
            Try
                Call New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand()
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception4 As Exception
                If bThrowException Then
                    Throw exception4
                End If
                GoTo Label_0067
            End Try
            Dim pTypeInfo As ITypeInfo = Nothing
            Dim pBstrName As String = Nothing
            Dim pBstrDocString As String = Nothing
            Dim pBstrHelpFile As String = Nothing
            Dim dispatch As IDispatch = TryCast(VarName, IDispatch)
            If (((Not dispatch Is Nothing) AndAlso (dispatch.GetTypeInfo(0, &H409, pTypeInfo) >= 0)) AndAlso (pTypeInfo.GetDocumentation(-1, pBstrName, pBstrDocString, num, pBstrHelpFile) >= 0)) Then
                str = pBstrName
            End If
Label_0067:
            If (str.Chars(0) = "_"c) Then
                str = str.Substring(1)
            End If
            Return str
        End Function

        Friend Shared Function OldVBFriendlyNameOfTypeName(typename As String) As String
            Dim sRank As String = Nothing
            Dim num As Integer = (typename.Length - 1)
            If (typename.Chars(num) = "]"c) Then
                Dim index As Integer = typename.IndexOf("["c)
                If ((index + 1) = num) Then
                    sRank = "()"
                Else
                    sRank = typename.Substring(index, ((num - index) + 1)).Replace("["c, "("c).Replace("]"c, ")"c)
                End If
                typename = typename.Substring(0, index)
            End If
            Dim str3 As String = Information.OldVbTypeName(typename)
            If (str3 Is Nothing) Then
                str3 = typename
            End If
            If (sRank Is Nothing) Then
                Return str3
            End If
            Return (str3 & Utils.AdjustArraySuffix(sRank))
        End Function

        Friend Shared Function OldVbTypeName(UrtName As String) As String
            UrtName = Strings.Trim(UrtName).ToUpperInvariant
            If (Strings.Left(UrtName, 7) = "SYSTEM.") Then
                UrtName = Strings.Mid(UrtName, 8)
            End If
            Dim s As String = UrtName

            ' <PrivateImplementationDetails>
            Select Case ComputeStringHash(s)
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
                Case &HFFDF971
                    If (s = "INT16") Then
                        Return "Short"
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
                Case &H9357C1D0
                    If (s = "DATETIME") Then
                        Return "Date"
                    End If
                    Exit Select
                Case &HA59F665D
                    If (s = "CHAR") Then
                        Return "Char"
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

        Public Shared Function QBColor(Color As Integer) As Integer
            If ((Color And &HFFF0) <> 0) Then
                Dim args As String() = New String() {"Color"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return Information.QBColorTable(Color)
        End Function

        Public Shared Function RGB(Red As Integer, Green As Integer, Blue As Integer) As Integer
            If ((Red And -2147483648) <> 0) Then
                Dim args As String() = New String() {"Red"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If ((Green And -2147483648) <> 0) Then
                Dim textArray2 As String() = New String() {"Green"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            If ((Blue And -2147483648) <> 0) Then
                Dim textArray3 As String() = New String() {"Blue"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            If (Red > &HFF) Then
                Red = &HFF
            End If
            If (Green > &HFF) Then
                Green = &HFF
            End If
            If (Blue > &HFF) Then
                Blue = &HFF
            End If
            Return (((Blue * &H10000) + (Green * &H100)) + Red)
        End Function

        Public Shared Function SystemTypeName(VbName As String) As String
            Dim s As String = Strings.Trim(VbName).ToUpperInvariant

            ' <PrivateImplementationDetails>
            Select Case ComputeStringHash(s)
                Case &H21EC44B5
                    If (s = "SHORT") Then
                        Return "System.Int16"
                    End If
                    Exit Select
                Case &H48AF9A2C
                    If (s = "DECIMAL") Then
                        Return "System.Decimal"
                    End If
                    Exit Select
                Case &H880B7E9F
                    If (s = "BOOLEAN") Then
                        Return "System.Boolean"
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
                Case &H1B2E0F7A
                    If (s = "OBJECT") Then
                        Return "System.Object"
                    End If
                    Exit Select
                Case &H8D1D3425
                    If (s = "INTEGER") Then
                        Return "System.Int32"
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
                Case &HC0DE1353
                    If (s = "LONG") Then
                        Return "System.Int64"
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

        Public Shared Function TypeName(VarName As Object) As String
            Dim name As String
            Dim flag As Boolean
            If (VarName Is Nothing) Then
                Return "Nothing"
            End If
            Dim elementType As Type = VarName.GetType
            If elementType.IsArray Then
                flag = True
                elementType = elementType.GetElementType
            End If
            If elementType.IsEnum Then
                name = elementType.Name
            Else
                Select Case Type.GetTypeCode(elementType)
                    Case TypeCode.DBNull
                        name = "DBNull"
                        GoTo Label_0138
                    Case TypeCode.Boolean
                        name = "Boolean"
                        GoTo Label_0138
                    Case TypeCode.Char
                        name = "Char"
                        GoTo Label_0138
                    Case TypeCode.Byte
                        name = "Byte"
                        GoTo Label_0138
                    Case TypeCode.Int16
                        name = "Short"
                        GoTo Label_0138
                    Case TypeCode.Int32
                        name = "Integer"
                        GoTo Label_0138
                    Case TypeCode.Int64
                        name = "Long"
                        GoTo Label_0138
                    Case TypeCode.Single
                        name = "Single"
                        GoTo Label_0138
                    Case TypeCode.Double
                        name = "Double"
                        GoTo Label_0138
                    Case TypeCode.Decimal
                        name = "Decimal"
                        GoTo Label_0138
                    Case TypeCode.DateTime
                        name = "Date"
                        GoTo Label_0138
                    Case TypeCode.String
                        name = "String"
                        GoTo Label_0138
                End Select
                name = elementType.Name
                If (elementType.IsCOMObject AndAlso (String.CompareOrdinal(name, "__ComObject") = 0)) Then
                    name = Information.LegacyTypeNameOfCOMObject(VarName, True)
                End If
            End If
            Dim index As Integer = name.IndexOf("+"c)
            If (index >= 0) Then
                name = name.Substring((index + 1))
            End If
Label_0138:
            If Not flag Then
                Return name
            End If
            Dim array As Array = DirectCast(VarName, Array)
            If (array.Rank = 1) Then
                name = (name & "[]")
            Else
                name = (name & "[" & New String(","c, (array.Rank - 1)) & "]")
            End If
            Return Information.OldVBFriendlyNameOfTypeName(name)
        End Function

        <SecuritySafeCritical>
        Friend Shared Function TypeNameOfCOMObject(VarName As Object, bThrowException As Boolean) As String
            Dim num As Integer
            Dim str As String = "__ComObject"
            Try
                Call New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand()
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception4 As Exception
                If bThrowException Then
                    Throw exception4
                End If
                GoTo Label_00A7
            End Try
            Dim pTypeInfo As ITypeInfo = Nothing
            Dim pBstrName As String = Nothing
            Dim pBstrDocString As String = Nothing
            Dim pBstrHelpFile As String = Nothing
            Dim info2 As IProvideClassInfo = TryCast(VarName, IProvideClassInfo)
            If (Not info2 Is Nothing) Then
                Try
                    pTypeInfo = info2.GetClassInfo
                    If (pTypeInfo.GetDocumentation(-1, pBstrName, pBstrDocString, num, pBstrHelpFile) >= 0) Then
                        str = pBstrName
                        GoTo Label_00A7
                    End If
                    pTypeInfo = Nothing
                Catch exception5 As StackOverflowException
                    Throw exception5
                Catch exception6 As OutOfMemoryException
                    Throw exception6
                Catch exception7 As ThreadAbortException
                    Throw exception7
                Catch exception14 As Exception
                End Try
            End If
            Dim dispatch As IDispatch = TryCast(VarName, IDispatch)
            If (((Not dispatch Is Nothing) AndAlso (dispatch.GetTypeInfo(0, &H409, pTypeInfo) >= 0)) AndAlso (pTypeInfo.GetDocumentation(-1, pBstrName, pBstrDocString, num, pBstrHelpFile) >= 0)) Then
                str = pBstrName
            End If
Label_00A7:
            If (str.Chars(0) = "_"c) Then
                str = str.Substring(1)
            End If
            Return str
        End Function

        Public Shared Function UBound(Array As Array, Optional Rank As Integer = 1) As Integer
            If (Array Is Nothing) Then
                Dim textArray1 As String() = New String() {"Array"}
                Throw ExceptionUtils.VbMakeException(New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1)), 9)
            End If
            If ((Rank >= 1) AndAlso (Rank <= Array.Rank)) Then
                Return Array.GetUpperBound((Rank - 1))
            End If
            Dim args As String() = New String() {"Rank"}
            Throw New RankException(Utils.GetResourceString("Argument_InvalidRank1", args))
        End Function

        Public Shared Function VarType(VarName As Object) As VariantType
            If (VarName Is Nothing) Then
                Return VariantType.Object
            End If
            Return Information.VarTypeFromComType(VarName.GetType)
        End Function

        Friend Shared Function VarTypeFromComType(typ As Type) As VariantType
            If (Not typ Is Nothing) Then
                If typ.IsArray Then
                    typ = typ.GetElementType
                    If typ.IsArray Then
                        Return (VariantType.Array Or VariantType.Object)
                    End If
                    Dim type2 As VariantType = Information.VarTypeFromComType(typ)
                    If ((type2 And VariantType.Array) <> VariantType.Empty) Then
                        Return (VariantType.Array Or VariantType.Object)
                    End If
                    Return (type2 Or VariantType.Array)
                End If
                If typ.IsEnum Then
                    typ = [Enum].GetUnderlyingType(typ)
                End If
                If (typ Is Nothing) Then
                    Return VariantType.Empty
                End If
                Select Case Type.GetTypeCode(typ)
                    Case TypeCode.DBNull
                        Return VariantType.Null
                    Case TypeCode.Boolean
                        Return VariantType.Boolean
                    Case TypeCode.Char
                        Return VariantType.Char
                    Case TypeCode.Byte
                        Return VariantType.Byte
                    Case TypeCode.Int16
                        Return VariantType.Short
                    Case TypeCode.Int32
                        Return VariantType.Integer
                    Case TypeCode.Int64
                        Return VariantType.Long
                    Case TypeCode.Single
                        Return VariantType.Single
                    Case TypeCode.Double
                        Return VariantType.Double
                    Case TypeCode.Decimal
                        Return VariantType.Decimal
                    Case TypeCode.DateTime
                        Return VariantType.Date
                    Case TypeCode.String
                        Return VariantType.String
                End Select
                If (((typ Is GetType(Missing)) OrElse (typ Is GetType(Exception))) OrElse typ.IsSubclassOf(GetType(Exception))) Then
                    Return VariantType.Error
                End If
                If typ.IsValueType Then
                    Return VariantType.UserDefinedType
                End If
            End If
            Return VariantType.Object
        End Function

        Public Shared Function VbTypeName(UrtName As String) As String
            Return Information.OldVbTypeName(UrtName)
        End Function


        ' Fields
        Friend Const COMObjectName As String = "__ComObject"
        Private Shared ReadOnly QBColorTable As Integer() = New Integer() {0, &H800000, &H8000, &H808000, &H80, &H800080, &H8080, &HC0C0C0, &H808080, &HFF0000, &HFF00, &HFFFF00, &HFF, &HFF00FF, &HFFFF, &HFFFFFF}
    End Class
End Namespace

