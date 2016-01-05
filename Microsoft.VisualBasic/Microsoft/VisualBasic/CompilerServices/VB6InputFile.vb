Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Security
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class VB6InputFile
        Inherits VB6File
        ' Methods
        Public Sub New(FileName As String, share As OpenShare)
            MyBase.New(FileName, OpenAccess.Read, share, -1)
        End Sub

        Friend Overrides Function CanInput() As Boolean
            Return True
        End Function

        Friend Overrides Function EOF() As Boolean
            Return MyBase.m_eof
        End Function

        Public Overrides Function GetMode() As OpenMode
            Return OpenMode.Input
        End Function

        Friend Overrides Sub Input(ByRef Value As Boolean)
            Value = BooleanType.FromObject(Me.ParseInputString(Me.InputStr))
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
            Value = DateType.FromObject(Me.ParseInputString(Me.InputStr))
        End Sub

        Friend Overrides Sub Input(ByRef Value As Decimal)
            Value = DecimalType.FromObject(Me.InputNum(VariantType.Decimal), Utils.GetInvariantCultureInfo.NumberFormat)
        End Sub

        Friend Overrides Sub Input(ByRef Value As Double)
            Value = DoubleType.FromObject(Me.InputNum(VariantType.Double), Utils.GetInvariantCultureInfo.NumberFormat)
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

        <SecurityCritical> _
        Friend Overrides Sub Input(ByRef obj As Object)
            Dim num As Integer = MyBase.SkipWhiteSpaceEOF
            Select Case num
                Case &H22
                    Dim numRef As Long
                    num = MyBase.m_sr.Read
                    numRef = CLng(AddressOf Me.m_position) = (numRef + 1)
                    obj = MyBase.ReadInField(1)
                    MyBase.SkipTrailingWhiteSpace
                    Return
                Case &H23
                    obj = Me.ParseInputString(Me.InputStr)
                    Return
            End Select
            Dim str As String = MyBase.ReadInField(3)
            obj = Conversion.ParseInputField(str, VariantType.Empty)
            MyBase.SkipTrailingWhiteSpace
        End Sub

        Friend Overrides Sub Input(ByRef Value As Single)
            Value = SingleType.FromObject(Me.InputNum(VariantType.Single), Utils.GetInvariantCultureInfo.NumberFormat)
        End Sub

        Friend Overrides Sub Input(ByRef Value As String)
            Value = Me.InputStr
        End Sub

        Friend Overrides Function LOC() As Long
            Return ((MyBase.m_position + &H7F) / &H80)
        End Function

        Friend Overrides Sub OpenFile()
            Try 
                MyBase.m_file = New FileStream(MyBase.m_sFullPath, FileMode.Open, DirectCast(MyBase.m_access, FileAccess), DirectCast(MyBase.m_share, FileShare))
            Catch exception As FileNotFoundException
                Throw ExceptionUtils.VbMakeException(exception, &H35)
            Catch exception2 As SecurityException
                Throw ExceptionUtils.VbMakeException(&H35)
            Catch exception3 As DirectoryNotFoundException
                Throw ExceptionUtils.VbMakeException(exception3, &H4C)
            Catch exception4 As IOException
                Throw ExceptionUtils.VbMakeException(exception4, &H4B)
            Catch exception5 As StackOverflowException
                Throw exception5
            Catch exception6 As OutOfMemoryException
                Throw exception6
            Catch exception7 As ThreadAbortException
                Throw exception7
            Catch exception8 As Exception
                Throw ExceptionUtils.VbMakeException(exception8, &H4C)
            End Try
            MyBase.m_Encoding = Utils.GetFileIOEncoding
            MyBase.m_sr = New StreamReader(MyBase.m_file, MyBase.m_Encoding, False, &H80)
            MyBase.m_eof = (MyBase.m_file.Length = 0)
        End Sub

        Friend Function ParseInputString(ByRef sInput As String) As Object
            Dim obj2 As Object = sInput
            If ((sInput.Chars(0) = "#"c) AndAlso (sInput.Length <> 1)) Then
                sInput = sInput.Substring(1, (sInput.Length - 2))
                If (sInput = "NULL") Then
                    Return DBNull.Value
                End If
                If (sInput = "TRUE") Then
                    Return True
                End If
                If (sInput = "FALSE") Then
                    Return False
                End If
                If (Strings.Left(sInput, 6) = "ERROR ") Then
                    Dim num As Integer
                    If (sInput.Length > 6) Then
                        num = IntegerType.FromString(Strings.Mid(sInput, 7))
                    End If
                    Return num
                End If
                Try 
                    obj2 = DateTime.Parse(Utils.ToHalfwidthNumbers(sInput, Utils.GetCultureInfo))
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception4 As Exception
                End Try
            End If
            Return obj2
        End Function

        Public Function ReadLine() As String
            Dim numRef As Long
            Dim s As String = MyBase.m_sr.ReadLine
            numRef = CLng(AddressOf Me.m_position) = (numRef + (MyBase.m_Encoding.GetByteCount(s) + 2))
            Return Nothing
        End Function

    End Class
End Namespace

