Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices

    <EditorBrowsable(EditorBrowsableState.Never)>
    Friend Class VB6BinaryFile
        Inherits VB6RandomFile

        ' Methods
        Public Sub New(FileName As String, access As OpenAccess, share As OpenShare)
            MyBase.New(FileName, access, share, -1)
        End Sub

        Friend Overrides Function CanInput() As Boolean
            Return True
        End Function

        Friend Overrides Function CanWrite() As Boolean
            Return True
        End Function

        Friend Overrides Sub [Get](ByRef Value As String, Optional RecordNumber As Long = 0, Optional StringIsFixedLength As Boolean = False)
            Dim byteCount As Integer
            MyBase.ValidateReadable()

            If (Value Is Nothing) Then
                byteCount = 0
            Else
                byteCount = MyBase.m_Encoding.GetByteCount(Value)
            End If
            Value = Me.GetFixedLengthString(RecordNumber, byteCount)
        End Sub

        Public Overrides Function GetMode() As OpenMode
            Return OpenMode.Binary
        End Function

        Friend Overrides Sub Input(ByRef Value As Boolean)
            Value = BooleanType.FromString(Me.InputStr)
        End Sub

        Friend Overrides Sub Input(ByRef Value As Byte)
            Value = ByteType.FromObject(Me.InputNum(VariantType.Byte))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Char)
            Dim str As String = Me.InputStr
            If (str.Length > 0) Then
                Value = str.Chars(0)
            Else
                Value = ChrW(0)
            End If
        End Sub

        Friend Overrides Sub Input(ByRef Value As DateTime)
            Value = DateType.FromString(Me.InputStr, Utils.GetCultureInfo)
        End Sub

        Friend Overrides Sub Input(ByRef Value As Decimal)
            Value = DecimalType.FromObject(Me.InputNum(VariantType.Decimal))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Double)
            Value = DoubleType.FromObject(Me.InputNum(VariantType.Double))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Short)
            Value = ShortType.FromObject(Me.InputNum(VariantType.Short))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Integer)
            Value = IntegerType.FromObject(Me.InputNum(VariantType.Integer))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Long)
            Value = LongType.FromObject(Me.InputNum(VariantType.Long))
        End Sub

        <SecurityCritical>
        Friend Overrides Sub Input(ByRef Value As Object)
            Value = Me.InputStr
        End Sub

        Friend Overrides Sub Input(ByRef Value As Single)
            Value = SingleType.FromObject(Me.InputNum(VariantType.Single))
        End Sub

        Friend Overrides Sub Input(ByRef Value As String)
            Value = Me.InputStr
        End Sub

        Protected Overrides Function InputStr() As String
            Dim str As String
            If ((MyBase.m_access <> OpenAccess.ReadWrite) AndAlso (MyBase.m_access <> OpenAccess.Read)) Then
                Throw New NullReferenceException(New NullReferenceException.Message, New IOException(Utils.GetResourceString("FileOpenedNoRead")))
            End If
            If (MyBase.SkipWhiteSpaceEOF = &H22) Then
                Dim numRef As Long
                MyBase.m_sr.Read
                numRef = CLng(AddressOf Me.m_position) = (numRef + 1)
                str = MyBase.ReadInField(1)
            Else
                str = MyBase.ReadInField(2)
            End If
            MyBase.SkipTrailingWhiteSpace
            Return str
        End Function

        Friend Overrides Function LOC() As Long
            Return MyBase.m_position
        End Function

        Friend Overrides Sub Lock(lStart As Long, lEnd As Long)
            Dim lRecordLen As Long
            If (lStart > lEnd) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (MyBase.m_lRecordLen = -1) Then
                lRecordLen = 1
            Else
                lRecordLen = MyBase.m_lRecordLen
            End If
            Dim position As Long = ((lStart - 1) * lRecordLen)
            Dim length As Long = (((lEnd - lStart) + 1) * lRecordLen)
            MyBase.m_file.Lock(position, length)
        End Sub

        Friend Overrides Sub Put(Value As String, Optional RecordNumber As Long = 0, Optional StringIsFixedLength As Boolean = False)
            MyBase.ValidateWriteable()
            MyBase.PutString(RecordNumber, Value)
        End Sub

        Friend Overrides Function Seek() As Long
            Return (MyBase.m_position + 1)
        End Function

        Friend Overrides Sub Seek(BaseOnePosition As Long)
            If (BaseOnePosition <= 0) Then
                Throw ExceptionUtils.VbMakeException(&H3F)
            End If
            Dim num As Long = (BaseOnePosition - 1)
            MyBase.m_file.Position = num
            MyBase.m_position = num
            If (Not MyBase.m_sr Is Nothing) Then
                MyBase.m_sr.DiscardBufferedData
            End If
        End Sub

        Friend Overrides Sub Unlock(lStart As Long, lEnd As Long)
            Dim lRecordLen As Long
            If (lStart > lEnd) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (MyBase.m_lRecordLen = -1) Then
                lRecordLen = 1
            Else
                lRecordLen = MyBase.m_lRecordLen
            End If
            Dim position As Long = ((lStart - 1) * lRecordLen)
            Dim length As Long = (((lEnd - lStart) + 1) * lRecordLen)
            MyBase.m_file.Unlock(position, length)
        End Sub

    End Class
End Namespace

