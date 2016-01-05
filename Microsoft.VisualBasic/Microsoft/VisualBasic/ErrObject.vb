Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security

Namespace Microsoft.VisualBasic
    Public NotInheritable Class ErrObject
        ' Methods
        Friend Sub New()
            Me.Clear
        End Sub

        <SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Friend Sub CaptureException(ex As Exception)
            RuntimeHelpers.PrepareConstrainedRegions()

            Try
            Finally
                If (Not ex Is Me.m_curException) Then
                    If Me.m_ClearOnCapture Then
                        Me.Clear()
                    Else
                        Me.m_ClearOnCapture = True
                    End If
                    Me.m_curException = ex
                End If
            End Try
        End Sub

        <SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Friend Sub CaptureException(ex As Exception, lErl As Integer)
            RuntimeHelpers.PrepareConstrainedRegions()

            Try
            Finally
                Me.CaptureException(ex)
                Me.m_curErl = lErl
            End Try
        End Sub

        <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Public Sub Clear()
            RuntimeHelpers.PrepareConstrainedRegions()

            Try
            Finally
                Me.m_curException = Nothing
                Me.m_curNumber = 0
                Me.m_curSource = ""
                Me.m_curDescription = ""
                Me.m_curHelpFile = ""
                Me.m_curHelpContext = 0
                Me.m_curErl = 0
                Me.m_NumberIsSet = False
                Me.m_SourceIsSet = False
                Me.m_DescriptionIsSet = False
                Me.m_HelpFileIsSet = False
                Me.m_HelpContextIsSet = False
                Me.m_ClearOnCapture = True
            End Try
        End Sub

        Friend Function CreateException(Number As Integer, Description As String) As Exception
            Me.Clear()
            Me.Number = Number
            If (Number = 0) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Me.m_ClearOnCapture = False
            Return Me.MapNumberToException(Me.m_curNumber, Description)
        End Function

        Private Function FilterDefaultMessage(Msg As String) As String
            If (Not Me.m_curException Is Nothing) Then
                Dim number As Integer = Me.Number
                If ((Msg Is Nothing) OrElse (Msg.Length = 0)) Then
                    Msg = Utils.GetResourceString(("ID" & Conversions.ToString(number)))
                    Return Msg
                End If
                If (String.CompareOrdinal("Exception from HRESULT: 0x", 0, Msg, 0, Math.Min(Msg.Length, &H1A)) = 0) Then
                    Dim resourceString As String = Utils.GetResourceString(("ID" & Conversions.ToString(Me.m_curNumber)), False)
                    If (Not resourceString Is Nothing) Then
                        Msg = resourceString
                    End If
                End If
            End If
            Return Msg
        End Function

        Public Function GetException() As Exception
            Return Me.m_curException
        End Function

        Private Function MakeHelpLink(HelpFile As String, HelpContext As Integer) As String
            Return (HelpFile & "#" & Conversions.ToString(HelpContext))
        End Function

        Friend Function MapErrorNumber(Number As Integer) As Integer
            If (Number > &HFFFF) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (Number < 0) Then
                If ((Number And &H1FFF0000) = &HA0000) Then
                    Return (Number And &HFFFF)
                End If
                Dim num2 As Integer = Number
                If (num2 <= -2147286960) Then
                    If (num2 <= -2147312566) Then
                        Select Case num2
                            Case -2147467263
                                Return &H1BD
                            Case -2147467262
                                Return 430
                            Case -2147467261
                                Return -2147467261
                            Case -2147467260
                                Return &H11F
                            Case -2147352575
                                Return &H1B6
                            Case -2147352574, -2147352567, -2147352560
                                Return Number
                            Case -2147352573
                                Return &H1B6
                            Case -2147352572
                                Return &H1C0
                            Case -2147352571
                                Return 13
                            Case -2147352570
                                Return &H1B6
                            Case -2147352569
                                Return &H1BE
                            Case -2147352568
                                Return &H1CA
                            Case -2147352566
                                Return 6
                            Case -2147352565
                                Return 9
                            Case -2147352564
                                Return &H1BF
                            Case -2147352563
                                Return 10
                            Case -2147352562
                                Return 450
                            Case -2147352561
                                Return &H1C1
                            Case -2147352559
                                Return &H1C3
                            Case -2147352558
                                Return 11
                            Case -2147319786
                                Return &H8016
                            Case -2147319785
                                Return &H1CD
                            Case -2147319784
                                Return &H8018
                            Case -2147319783
                                Return &H8019
                            Case -2147319782, -2147319781, -2147319778, -2147319777, -2147319776, -2147319775, -2147319774, -2147319773, -2147319772, -2147319771, -2147319770
                                Return Number
                            Case -2147319780
                                Return &H801C
                            Case -2147319779
                                Return &H801D
                            Case -2147319769
                                Return &H8027
                            Case -2147319768
                                Return &H8028
                            Case -2147319767
                                Return &H8029
                            Case -2147319766
                                Return &H802A
                            Case -2147319765
                                Return &H802B
                            Case -2147319764
                                Return &H802C
                            Case -2147319763
                                Return &H802D
                            Case -2147319762
                                Return &H802E
                            Case -2147319761
                                Return &H1C5
                            Case -2147316576
                                Return 13
                            Case -2147316575
                                Return 9
                            Case -2147316574
                                Return &H39
                            Case -2147316573
                                Return &H142
                            Case -2147312566
                                Return &H30
                            Case -2147317571
                                Return &H88BD
                            Case -2147317563
                                Return &H88C5
                        End Select
                        Return Number
                    End If
                    Select Case num2
                        Case -2147287039
                            Return &H8006
                        Case -2147287038
                            Return &H35
                        Case -2147287037
                            Return &H4C
                        Case -2147287036
                            Return &H43
                        Case -2147287035
                            Return 70
                        Case -2147287034
                            Return &H8004
                        Case -2147287033
                            Return Number
                        Case -2147287032
                            Return 7
                        Case -2147312508
                            Return &H9C84
                        Case -2147312509
                            Return &H9C83
                        Case -2147287015
                            Return &H8003
                        Case -2147287014, -2147287013, -2147287012, -2147287009
                            Return Number
                        Case -2147287011
                            Return &H8005
                        Case -2147287010
                            Return &H8004
                        Case -2147287008
                            Return &H4B
                        Case -2147287007
                            Return 70
                        Case -2147286960
                            Return &H3A
                        Case -2147287022
                            Return &H43
                        Case -2147287021
                            Return 70
                    End Select
                    Return Number
                End If
                If (num2 <= -2147221014) Then
                    Select Case num2
                        Case -2147286789
                            Return &H8018
                        Case -2147286788
                            Return &H35
                        Case -2147286787
                            Return &H8018
                        Case -2147286786
                            Return &H8000
                        Case -2147286785
                            Return Number
                        Case -2147286784
                            Return 70
                        Case -2147286783
                            Return 70
                        Case -2147286782
                            Return &H8005
                        Case -2147286781
                            Return &H39
                        Case -2147286780
                            Return &H8019
                        Case -2147286779
                            Return &H8019
                        Case -2147286778
                            Return &H8015
                        Case -2147286777
                            Return &H8019
                        Case -2147286776
                            Return &H8019
                        Case -2147221230
                            Return &H1AD
                        Case -2147286928
                            Return &H3D
                        Case -2147221018
                            Return &H1B0
                        Case -2147221014
                            Return &H1B0
                        Case -2147221164
                            Return &H1AD
                        Case -2147221021
                            Return &H1AD
                    End Select
                    Return Number
                End If
                If (num2 <= -2147024891) Then
                    Select Case num2
                        Case -2147220994
                            Return &H1AD
                        Case -2147024891
                            Return 70
                        Case -2147221005
                            Return &H1AD
                        Case -2147221003
                            Return &H1AD
                    End Select
                    Return Number
                End If
                Select Case num2
                    Case -2147024882
                        Return 7
                    Case -2147024809
                        Return 5
                    Case -2147023174
                        Return &H1CE
                    Case -2146959355
                        Return &H1AD
                End Select
            End If
            Return Number
        End Function

        Private Function MapExceptionToNumber(e As Exception) As Integer
            Dim type As Type = e.GetType
            If (type Is GetType(IndexOutOfRangeException)) Then
                Return 9
            End If
            If (type Is GetType(RankException)) Then
                Return 9
            End If
            If (type Is GetType(DivideByZeroException)) Then
                Return 11
            End If
            If (type Is GetType(OverflowException)) Then
                Return 6
            End If
            If (type Is GetType(NotFiniteNumberException)) Then
                If (DirectCast(e, NotFiniteNumberException).OffendingNumber = 0) Then
                    Return 11
                End If
                Return 6
            End If
            If (type Is GetType(NullReferenceException)) Then
                Return &H5B
            End If
            If TypeOf e Is AccessViolationException Then
                Return -2147467261
            End If
            If (type Is GetType(InvalidCastException)) Then
                Return 13
            End If
            If (type Is GetType(NotSupportedException)) Then
                Return 13
            End If
            If (type Is GetType(COMException)) Then
                Return Utils.MapHRESULT(DirectCast(e, COMException).ErrorCode)
            End If
            If (type Is GetType(SEHException)) Then
                Return &H63
            End If
            If (type Is GetType(DllNotFoundException)) Then
                Return &H35
            End If
            If (type Is GetType(EntryPointNotFoundException)) Then
                Return &H1C5
            End If
            If (type Is GetType(TypeLoadException)) Then
                Return &H1AD
            End If
            If (type Is GetType(OutOfMemoryException)) Then
                Return 7
            End If
            If (type Is GetType(FormatException)) Then
                Return 13
            End If
            If (type Is GetType(DirectoryNotFoundException)) Then
                Return &H4C
            End If
            If (type Is GetType(IOException)) Then
                Return &H39
            End If
            If (type Is GetType(FileNotFoundException)) Then
                Return &H35
            End If
            If TypeOf e Is MissingMemberException Then
                Return &H1B6
            End If
            If TypeOf e Is InvalidOleVariantTypeException Then
                Return &H1CA
            End If
            Return 5
        End Function

        Private Function MapNumberToException(Number As Integer, Description As String) As Exception
            Dim vBDefinedError As Boolean = False
            Return ExceptionUtils.BuildException(Number, Description, vBDefinedError)
        End Function

        Private Sub ParseHelpLink(HelpLink As String)
            If ((HelpLink Is Nothing) OrElse (HelpLink.Length = 0)) Then
                If Not Me.m_HelpContextIsSet Then
                    Me.HelpContext = 0
                End If
                If Not Me.m_HelpFileIsSet Then
                    Me.HelpFile = ""
                End If
            Else
                Dim length As Integer = Strings.m_InvariantCompareInfo.IndexOf(HelpLink, "#", CompareOptions.Ordinal)
                If (length <> -1) Then
                    If Not Me.m_HelpContextIsSet Then
                        If (length < HelpLink.Length) Then
                            Me.HelpContext = Conversions.ToInteger(HelpLink.Substring((length + 1)))
                        Else
                            Me.HelpContext = 0
                        End If
                    End If
                    If Not Me.m_HelpFileIsSet Then
                        Me.HelpFile = HelpLink.Substring(0, length)
                    End If
                Else
                    If Not Me.m_HelpContextIsSet Then
                        Me.HelpContext = 0
                    End If
                    If Not Me.m_HelpFileIsSet Then
                        Me.HelpFile = HelpLink
                    End If
                End If
            End If
        End Sub

        <MethodImpl(MethodImplOptions.NoInlining)>
        Public Sub Raise(Number As Integer, Optional Source As Object = Nothing, Optional Description As Object = Nothing, Optional HelpFile As Object = Nothing, Optional HelpContext As Object = Nothing)
            If (Number = 0) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Me.Number = Number
            If (Not Source Is Nothing) Then
                Me.Source = Conversions.ToString(Source)
            Else
                Dim vBHost As IVbHost = HostServices.VBHost
                If (vBHost Is Nothing) Then
                    Dim fullName As String = Assembly.GetCallingAssembly.FullName
                    Dim num As Integer = Strings.InStr(fullName, ",", CompareMethod.Binary)
                    If (num < 1) Then
                        Me.Source = fullName
                    Else
                        Me.Source = Strings.Left(fullName, (num - 1))
                    End If
                Else
                    Me.Source = vBHost.GetWindowTitle
                End If
            End If
            If (Not HelpFile Is Nothing) Then
                Me.HelpFile = Conversions.ToString(HelpFile)
            End If
            If (Not HelpContext Is Nothing) Then
                Me.HelpContext = Conversions.ToInteger(HelpContext)
            End If
            If (Not Description Is Nothing) Then
                Me.Description = Conversions.ToString(Description)
            ElseIf Not Me.m_DescriptionIsSet Then
                Me.Description = Utils.GetResourceString(DirectCast(Me.m_curNumber, vbErrors))
            End If
            Dim exception1 As Exception = Me.MapNumberToException(Me.m_curNumber, Me.m_curDescription)
            exception1.Source = Me.m_curSource
            exception1.HelpLink = Me.MakeHelpLink(Me.m_curHelpFile, Me.m_curHelpContext)
            Me.m_ClearOnCapture = False
            Throw exception1
        End Sub

        Friend Sub SetUnmappedError(Number As Integer)
            Me.Clear()
            Me.Number = Number
            Me.m_ClearOnCapture = False
        End Sub


        ' Properties
        Public Property Description As String
            Get
                If Me.m_DescriptionIsSet Then
                    Return Me.m_curDescription
                End If
                If (Not Me.m_curException Is Nothing) Then
                    Me.Description = Me.FilterDefaultMessage(Me.m_curException.Message)
                    Return Me.m_curDescription
                End If
                Return ""
            End Get
            Set(Value As String)
                Me.m_curDescription = Value
                Me.m_DescriptionIsSet = True
            End Set
        End Property

        Public ReadOnly Property Erl As Integer
            Get
                Return Me.m_curErl
            End Get
        End Property

        Public Property HelpContext As Integer
            Get
                If Me.m_HelpContextIsSet Then
                    Return Me.m_curHelpContext
                End If
                If (Not Me.m_curException Is Nothing) Then
                    Me.ParseHelpLink(Me.m_curException.HelpLink)
                    Return Me.m_curHelpContext
                End If
                Return 0
            End Get
            Set(Value As Integer)
                Me.m_curHelpContext = Value
                Me.m_HelpContextIsSet = True
            End Set
        End Property

        Public Property HelpFile As String
            Get
                If Me.m_HelpFileIsSet Then
                    Return Me.m_curHelpFile
                End If
                If (Not Me.m_curException Is Nothing) Then
                    Me.ParseHelpLink(Me.m_curException.HelpLink)
                    Return Me.m_curHelpFile
                End If
                Return ""
            End Get
            Set(Value As String)
                Me.m_curHelpFile = Value
                Me.m_HelpFileIsSet = True
            End Set
        End Property

        Public ReadOnly Property LastDllError As Integer
            <SecurityCritical>
            Get
                Return Marshal.GetLastWin32Error
            End Get
        End Property

        Public Property Number As Integer
            Get
                If Me.m_NumberIsSet Then
                    Return Me.m_curNumber
                End If
                If (Not Me.m_curException Is Nothing) Then
                    Me.Number = Me.MapExceptionToNumber(Me.m_curException)
                    Return Me.m_curNumber
                End If
                Return 0
            End Get
            Set(Value As Integer)
                Me.m_curNumber = Me.MapErrorNumber(Value)
                Me.m_NumberIsSet = True
            End Set
        End Property

        Public Property Source As String
            Get
                If Me.m_SourceIsSet Then
                    Return Me.m_curSource
                End If
                If (Not Me.m_curException Is Nothing) Then
                    Me.Source = Me.m_curException.Source
                    Return Me.m_curSource
                End If
                Return ""
            End Get
            Set(Value As String)
                Me.m_curSource = Value
                Me.m_SourceIsSet = True
            End Set
        End Property


        ' Fields
        Private m_ClearOnCapture As Boolean
        Private m_curDescription As String
        Private m_curErl As Integer
        Private m_curException As Exception
        Private m_curHelpContext As Integer
        Private m_curHelpFile As String
        Private m_curNumber As Integer
        Private m_curSource As String
        Private m_DescriptionIsSet As Boolean
        Private m_HelpContextIsSet As Boolean
        Private m_HelpFileIsSet As Boolean
        Private m_NumberIsSet As Boolean
        Private m_SourceIsSet As Boolean
    End Class
End Namespace

