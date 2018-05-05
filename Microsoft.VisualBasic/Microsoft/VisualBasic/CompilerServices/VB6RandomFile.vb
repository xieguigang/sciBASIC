Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class VB6RandomFile
        Inherits VB6File

        ' Methods
        Public Sub New(FileName As String, access As OpenAccess, share As OpenShare, lRecordLen As Integer)
            MyBase.New(FileName, access, share, lRecordLen)
        End Sub

        Friend Overrides Sub CloseFile()
            If (Not MyBase.m_sw Is Nothing) Then
                MyBase.m_sw.Flush
            End If
            MyBase.CloseTheFile
        End Sub

        Friend Overrides Function EOF() As Boolean
            MyBase.m_eof = (MyBase.m_position >= MyBase.m_file.Length)
            Return MyBase.m_eof
        End Function

        Friend Overrides Sub [Get](ByRef Value As Boolean, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetBoolean(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Byte, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetByte(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Char, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetChar(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As DateTime, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetDate(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Decimal, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetCurrency(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Double, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetDouble(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Short, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetShort(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Integer, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetInteger(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Long, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetLong(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Single, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            Value = MyBase.GetSingle(RecordNumber)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As ValueType, Optional RecordNumber As Long = 0)
            Me.ValidateReadable()
            MyBase.GetRecord(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub [Get](ByRef Value As String, Optional RecordNumber As Long = 0, Optional StringIsFixedLength As Boolean = False)
            Me.ValidateReadable()

            If StringIsFixedLength Then
                Dim byteCount As Integer
                If (Value Is Nothing) Then
                    byteCount = 0
                Else
                    byteCount = MyBase.m_Encoding.GetByteCount(Value)
                End If
                Value = Me.GetFixedLengthString(RecordNumber, byteCount)
            Else
                Value = Me.GetLengthPrefixedString(RecordNumber)
            End If
        End Sub

        Friend Overrides Sub [Get](ByRef Value As Array, Optional RecordNumber As Long = 0, Optional ArrayIsDynamic As Boolean = False, Optional StringIsFixedLength As Boolean = False)
            Me.ValidateReadable()

            If (Value Is Nothing) Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_ArrayNotInitialized"))
            End If
            Dim elementType As Type = Value.GetType.GetElementType
            Dim fixedStringLength As Integer = -1
            Dim rank As Integer = Value.Rank
            Dim firstBound As Integer = -1
            Dim secondBound As Integer = -1
            MyBase.SetRecord(RecordNumber)
            If (MyBase.m_file.Position < MyBase.m_file.Length) Then
                If (StringIsFixedLength AndAlso (elementType Is GetType(String))) Then
                    Dim obj2 As Object
                    Select Case rank
                        Case 1
                            obj2 = Value.GetValue(0)
                            Exit Select
                        Case 2
                            obj2 = Value.GetValue(0, 0)
                            Exit Select
                        Case Else
                            Throw New ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"))
                    End Select
                    If (obj2 Is Nothing) Then
                        fixedStringLength = 0
                    Else
                        fixedStringLength = CStr(obj2).Length
                    End If
                    If (fixedStringLength = 0) Then
                        Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidFixedLengthString"))
                    End If
                End If
                If ArrayIsDynamic Then
                    Value = MyBase.GetArrayDesc(elementType)
                    rank = Value.Rank
                End If
                firstBound = Value.GetUpperBound(0)
                Select Case rank
                    Case 1
                        Exit Select
                    Case 2
                        secondBound = Value.GetUpperBound(1)
                        Exit Select
                    Case Else
                        Throw New ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"))
                End Select
                If ArrayIsDynamic Then
                    MyBase.GetArrayData(Value, elementType, firstBound, secondBound, fixedStringLength)
                Else
                    MyBase.GetFixedArray(RecordNumber, Value, elementType, firstBound, secondBound, fixedStringLength)
                End If
            End If
        End Sub

        Public Overrides Function GetMode() As OpenMode
            Return OpenMode.Random
        End Function

        Friend Overrides Sub GetObject(ByRef Value As Object, Optional RecordNumber As Long = 0, Optional ContainedInVariant As Boolean = True)
            Dim type As Type = Nothing
            Dim [variant] As VT
            Me.ValidateReadable()
            MyBase.SetRecord(RecordNumber)
            If ContainedInVariant Then
                Dim numRef As Long
                [variant] = DirectCast(MyBase.m_br.ReadInt16, VT)
                numRef = CLng(AddressOf Me.m_position) = (numRef + 2)
            Else
                type = Value.GetType
                Select Case Type.GetTypeCode(type)
                    Case TypeCode.Object
                        If Not type.IsValueType Then
                            [variant] = VT.Variant
                        Else
                            [variant] = VT.Structure
                        End If
                        GoTo Label_00D8
                    Case TypeCode.Boolean
                        [variant] = VT.Boolean
                        GoTo Label_00D8
                    Case TypeCode.Char
                        [variant] = VT.Char
                        GoTo Label_00D8
                    Case TypeCode.Byte
                        [variant] = VT.Byte
                        GoTo Label_00D8
                    Case TypeCode.Int16
                        [variant] = VT.Short
                        GoTo Label_00D8
                    Case TypeCode.Int32
                        [variant] = VT.Integer
                        GoTo Label_00D8
                    Case TypeCode.Int64
                        [variant] = VT.Long
                        GoTo Label_00D8
                    Case TypeCode.Single
                        [variant] = VT.Single
                        GoTo Label_00D8
                    Case TypeCode.Double
                        [variant] = VT.Double
                        GoTo Label_00D8
                    Case TypeCode.Decimal
                        [variant] = VT.Decimal
                        GoTo Label_00D8
                    Case TypeCode.DateTime
                        [variant] = VT.Date
                        GoTo Label_00D8
                    Case TypeCode.String
                        [variant] = VT.String
                        GoTo Label_00D8
                End Select
                [variant] = VT.variant
            End If
Label_00D8:
            If (([variant] And VT.Array) <> VT.Empty) Then
                Dim arr As Array = Nothing
                Dim vtype As VT = ([variant] Xor VT.Array)
                MyBase.GetDynamicArray(arr, MyBase.ComTypeFromVT(vtype), -1)
                Value = arr
            Else
                Select Case [variant]
                    Case VT.String
                        Value = Me.GetLengthPrefixedString(0)
                        Return
                    Case VT.Short
                        Value = MyBase.GetShort(0)
                        Return
                    Case VT.Integer
                        Value = MyBase.GetInteger(0)
                        Return
                    Case VT.Long
                        Value = MyBase.GetLong(0)
                        Return
                    Case VT.Byte
                        Value = MyBase.GetByte(0)
                        Return
                    Case VT.Date
                        Value = MyBase.GetDate(0)
                        Return
                    Case VT.Double
                        Value = MyBase.GetDouble(0)
                        Return
                    Case VT.Single
                        Value = MyBase.GetSingle(0)
                        Return
                    Case VT.Currency
                        Value = MyBase.GetCurrency(0)
                        Return
                    Case VT.Decimal
                        Value = MyBase.GetDecimal(0)
                        Return
                    Case VT.Boolean
                        Value = MyBase.GetBoolean(0)
                        Return
                    Case VT.Char
                        Value = MyBase.GetChar(0)
                        Return
                    Case VT.Structure
                        Dim o As ValueType = DirectCast(Value, ValueType)
                        MyBase.GetRecord(0, o, False)
                        Value = o
                        Return
                End Select
                If (([variant] = VT.DBNull) AndAlso ContainedInVariant) Then
                    Value = DBNull.Value
                Else
                    If ([variant] = VT.DBNull) Then
                        Dim args As String() = New String() {"DBNull"}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", args)), 5)
                    End If
                    If ([variant] = VT.Empty) Then
                        Value = Nothing
                    Else
                        If ([variant] = VT.Currency) Then
                            Dim textArray2 As String() = New String() {"Currency"}
                            Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", textArray2)), 5)
                        End If
                        Dim textArray3 As String() = New String() {type.FullName}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", textArray3)), 5)
                    End If
                End If
            End If
        End Sub

        Friend Overrides Function GetStreamReader() As StreamReader
            Return New StreamReader(MyBase.m_file, MyBase.m_Encoding)
        End Function

        Friend Overrides Function LOC() As Long
            If (MyBase.m_lRecordLen = 0) Then
                Throw ExceptionUtils.VbMakeException(&H33)
            End If
            Return (((MyBase.m_position + MyBase.m_lRecordLen) - 1) / CLng(MyBase.m_lRecordLen))
        End Function

        Friend Overrides Sub Lock(lStart As Long, lEnd As Long)
            If (lStart > lEnd) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Dim position As Long = ((lStart - 1) * MyBase.m_lRecordLen)
            Dim length As Long = (((lEnd - lStart) + 1) * MyBase.m_lRecordLen)
            MyBase.m_file.Lock(position, length)
        End Sub

        Friend Overrides Sub OpenFile()
            Dim open As FileMode
            If file.Exists(MyBase.m_sFullPath) Then
                open = FileMode.Open
            ElseIf (MyBase.m_access = OpenAccess.Read) Then
                open = FileMode.OpenOrCreate
            Else
                open = FileMode.Create
            End If
            If (MyBase.m_access = OpenAccess.Default) Then
                MyBase.m_access = OpenAccess.ReadWrite
                Try
                    Me.OpenFileHelper(open, MyBase.m_access)
                    GoTo Label_0096
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception9 As Exception
                    MyBase.m_access = OpenAccess.Write
                    Try
                        Me.OpenFileHelper(open, MyBase.m_access)
                    Catch exception4 As StackOverflowException
                        Throw exception4
                    Catch exception5 As OutOfMemoryException
                        Throw exception5
                    Catch exception6 As ThreadAbortException
                        Throw exception6
                    Catch exception13 As Exception
                        MyBase.m_access = OpenAccess.Read
                        Me.OpenFileHelper(open, MyBase.m_access)
                    End Try
                    GoTo Label_0096
                End Try
            End If
            Me.OpenFileHelper(open, MyBase.m_access)
Label_0096:
            MyBase.m_Encoding = Utils.GetFileIOEncoding
            Dim file As Stream = MyBase.m_file
            If ((MyBase.m_access = OpenAccess.Write) OrElse (MyBase.m_access = OpenAccess.ReadWrite)) Then
                MyBase.m_sw = New StreamWriter(file, MyBase.m_Encoding)
                MyBase.m_sw.AutoFlush = True
                MyBase.m_bw = New BinaryWriter(file, MyBase.m_Encoding)
            End If
            If ((MyBase.m_access = OpenAccess.Read) OrElse (MyBase.m_access = OpenAccess.ReadWrite)) Then
                MyBase.m_br = New BinaryReader(file, MyBase.m_Encoding)
                If (Me.GetMode = OpenMode.Binary) Then
                    MyBase.m_sr = New StreamReader(file, MyBase.m_Encoding, False, &H80)
                End If
            End If
        End Sub

        Private Sub OpenFileHelper(fm As FileMode, fa As OpenAccess)
            Try
                MyBase.m_file = New FileStream(MyBase.m_sFullPath, fm, DirectCast(fa, FileAccess), DirectCast(MyBase.m_share, FileShare))
            Catch exception As FileNotFoundException
                Throw ExceptionUtils.VbMakeException(exception, &H35)
            Catch exception2 As DirectoryNotFoundException
                Throw ExceptionUtils.VbMakeException(exception2, &H4C)
            Catch exception3 As SecurityException
                Throw ExceptionUtils.VbMakeException(exception3, &H35)
            Catch exception4 As IOException
                Throw ExceptionUtils.VbMakeException(exception4, &H4B)
            Catch exception5 As UnauthorizedAccessException
                Throw ExceptionUtils.VbMakeException(exception5, &H4B)
            Catch exception6 As ArgumentException
                Throw ExceptionUtils.VbMakeException(exception6, &H4B)
            Catch exception7 As StackOverflowException
                Throw exception7
            Catch exception8 As OutOfMemoryException
                Throw exception8
            Catch exception9 As ThreadAbortException
                Throw exception9
            Catch exception10 As Exception
                Throw ExceptionUtils.VbMakeException(&H33)
            End Try
        End Sub

        Friend Overrides Sub Put(Value As Boolean, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutBoolean(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Byte, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutByte(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Char, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutChar(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As DateTime, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutDate(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Decimal, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutCurrency(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Double, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutDouble(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Short, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutShort(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Integer, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutInteger(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Long, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutLong(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As Single, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutSingle(RecordNumber, Value, False)
        End Sub

        Friend Overrides Sub Put(Value As ValueType, Optional RecordNumber As Long = 0)
            Me.ValidateWriteable()
            MyBase.PutRecord(RecordNumber, Value)
        End Sub

        Friend Overrides Sub Put(Value As String, Optional RecordNumber As Long = 0, Optional StringIsFixedLength As Boolean = False)
            Me.ValidateWriteable()

            If StringIsFixedLength Then
                MyBase.PutString(RecordNumber, Value)
            Else
                MyBase.PutStringWithLength(RecordNumber, Value)
            End If
        End Sub

        Friend Overrides Sub Put(Value As Array, Optional RecordNumber As Long = 0, Optional ArrayIsDynamic As Boolean = False, Optional StringIsFixedLength As Boolean = False)
            Me.ValidateWriteable()

            If (Value Is Nothing) Then
                MyBase.PutEmpty(RecordNumber)
            Else
                Dim upperBound As Integer = Value.GetUpperBound(0)
                Dim secondBound As Integer = -1
                Dim fixedStringLength As Integer = -1
                If (Value.Rank = 2) Then
                    secondBound = Value.GetUpperBound(1)
                End If
                If StringIsFixedLength Then
                    fixedStringLength = 0
                End If
                Dim elementType As Type = Value.GetType.GetElementType
                If ArrayIsDynamic Then
                    MyBase.PutDynamicArray(RecordNumber, Value, False, fixedStringLength)
                Else
                    MyBase.PutFixedArray(RecordNumber, Value, elementType, fixedStringLength, upperBound, secondBound)
                End If
            End If
        End Sub

        Friend Overrides Sub PutObject(Value As Object, Optional RecordNumber As Long = 0, Optional ContainedInVariant As Boolean = True)
            Me.ValidateWriteable()

            If (Value Is Nothing) Then
                MyBase.PutEmpty(RecordNumber)
            Else
                Dim enumType As Type = Value.GetType
                If (enumType Is Nothing) Then
                    Dim args As String() = New String() {"Empty"}
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", args)), 5)
                End If
                If enumType.IsArray Then
                    MyBase.PutDynamicArray(RecordNumber, DirectCast(Value, Array), True, -1)
                Else
                    If enumType.IsEnum Then
                        enumType = [Enum].GetUnderlyingType(enumType)
                    End If
                    Select Case Type.GetTypeCode(enumType)
                        Case TypeCode.DBNull
                            MyBase.PutShort(RecordNumber, 1, False)
                            Return
                        Case TypeCode.Boolean
                            MyBase.PutBoolean(RecordNumber, BooleanType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Char
                            MyBase.PutChar(RecordNumber, CharType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Byte
                            MyBase.PutByte(RecordNumber, ByteType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Int16
                            MyBase.PutShort(RecordNumber, ShortType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Int32
                            MyBase.PutInteger(RecordNumber, IntegerType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Int64
                            MyBase.PutLong(RecordNumber, LongType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Single
                            MyBase.PutSingle(RecordNumber, SingleType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Double
                            MyBase.PutDouble(RecordNumber, DoubleType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.Decimal
                            MyBase.PutDecimal(RecordNumber, DecimalType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.DateTime
                            MyBase.PutDate(RecordNumber, DateType.FromObject(Value), ContainedInVariant)
                            Return
                        Case TypeCode.String
                            MyBase.PutVariantString(RecordNumber, Value.ToString)
                            Return
                    End Select
                    If (enumType Is GetType(Missing)) Then
                        Dim textArray2 As String() = New String() {"Missing"}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", textArray2)), 5)
                    End If
                    If (enumType.IsValueType AndAlso Not ContainedInVariant) Then
                        MyBase.PutRecord(RecordNumber, DirectCast(Value, ValueType))
                    Else
                        If (ContainedInVariant AndAlso enumType.IsValueType) Then
                            Dim textArray3 As String() = New String() {Utils.VBFriendlyName(enumType, Value)}
                            Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_PutObjectOfValueType1", textArray3)), 5)
                        End If
                        Dim textArray4 As String() = New String() {Utils.VBFriendlyName(enumType, Value)}
                        Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", textArray4)), 5)
                    End If
                End If
            End If
        End Sub

        Friend Overrides Function Seek() As Long
            Return (Me.LOC + 1)
        End Function

        Friend Overrides Sub Seek(Position As Long)
            MyBase.SetRecord(Position)
        End Sub

        Friend Overrides Sub Unlock(lStart As Long, lEnd As Long)
            If (lStart > lEnd) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Dim position As Long = ((lStart - 1) * MyBase.m_lRecordLen)
            Dim length As Long = (((lEnd - lStart) + 1) * MyBase.m_lRecordLen)
            MyBase.m_file.Unlock(position, length)
        End Sub

        Protected Sub ValidateReadable()
            If ((MyBase.m_access <> OpenAccess.ReadWrite) AndAlso (MyBase.m_access <> OpenAccess.Read)) Then
                Throw ExceptionUtils.VbMakeExceptionEx(&H4B, Utils.GetResourceString("FileOpenedNoRead"))
            End If
        End Sub

        Protected Sub ValidateWriteable()
            If ((MyBase.m_access <> OpenAccess.ReadWrite) AndAlso (MyBase.m_access <> OpenAccess.Write)) Then
                Throw ExceptionUtils.VbMakeExceptionEx(&H4B, Utils.GetResourceString("FileOpenedNoWrite"))
            End If
        End Sub

    End Class
End Namespace

