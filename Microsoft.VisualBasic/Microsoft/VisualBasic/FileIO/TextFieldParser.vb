Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Security.Permissions
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Microsoft.VisualBasic.FileIO
    Public Class TextFieldParser
        Implements IDisposable
        ' Methods
        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(stream As Stream)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromStream(stream, Encoding.UTF8, True)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(reader As TextReader)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            If (reader Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("reader")
            End If
            Me.m_Reader = reader
            Me.ReadToBuffer()
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(path As String)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromPath(path, Encoding.UTF8, True)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(stream As Stream, defaultEncoding As Encoding)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromStream(stream, defaultEncoding, True)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(path As String, defaultEncoding As Encoding)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromPath(path, defaultEncoding, True)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(stream As Stream, defaultEncoding As Encoding, detectEncoding As Boolean)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromStream(stream, defaultEncoding, detectEncoding)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(path As String, defaultEncoding As Encoding, detectEncoding As Boolean)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.InitializeFromPath(path, defaultEncoding, detectEncoding)
        End Sub

        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub New(stream As Stream, defaultEncoding As Encoding, detectEncoding As Boolean, leaveOpen As Boolean)
            Me.m_CommentTokens = New String(0 - 1) {}
            Me.m_LineNumber = 1
            Me.m_EndOfData = False
            Me.m_ErrorLine = ""
            Me.m_ErrorLineNumber = -1
            Me.m_TextFieldType = FieldType.Delimited
            Me.m_WhitespaceCodes = New Integer() {9, 11, 12, &H20, &H85, 160, &H1680, &H2000, &H2001, &H2002, &H2003, &H2004, &H2005, &H2006, &H2007, &H2008, &H2009, &H200A, &H200B, &H2028, &H2029, &H3000, &HFEFF}
            Me.m_WhiteSpaceRegEx = New Regex("\s", RegexOptions.CultureInvariant)
            Me.m_TrimWhiteSpace = True
            Me.m_Position = 0
            Me.m_PeekPosition = 0
            Me.m_CharsRead = 0
            Me.m_NeedPropertyCheck = True
            Me.m_Buffer = New Char(&H1000 - 1) {}
            Me.m_HasFieldsEnclosedInQuotes = True
            Me.m_MaxLineSize = &H989680
            Me.m_MaxBufferSize = &H989680
            Me.m_LeaveOpen = False
            Me.m_LeaveOpen = leaveOpen
            Me.InitializeFromStream(stream, defaultEncoding, detectEncoding)
        End Sub

        Private Function ArrayHasChanged() As Boolean
            Dim num4 As Integer
            Dim lowerBound As Integer = 0
            Select Case Me.m_TextFieldType
                Case FieldType.Delimited
                    If (Me.m_Delimiters Is Nothing) Then
                        Return False
                    End If
                    lowerBound = Me.m_DelimitersCopy.GetLowerBound(0)
                    num4 = lowerBound
                    Dim upperBound As Integer = Me.m_DelimitersCopy.GetUpperBound(0)
                    Dim i As Integer = num4
                    Do While (i <= upperBound)
                        If (Me.m_Delimiters(i) <> Me.m_DelimitersCopy(i)) Then
                            Return True
                        End If
                        i += 1
                    Loop
                    Exit Select
                Case FieldType.FixedWidth
                    If (Me.m_FieldWidths Is Nothing) Then
                        Return False
                    End If
                    lowerBound = Me.m_FieldWidthsCopy.GetLowerBound(0)
                    Dim num6 As Integer = lowerBound
                    num4 = Me.m_FieldWidthsCopy.GetUpperBound(0)
                    Dim j As Integer = num6
                    Do While (j <= num4)
                        If (Me.m_FieldWidths(j) <> Me.m_FieldWidthsCopy(j)) Then
                            Return True
                        End If
                        j += 1
                    Loop
                    Exit Select
            End Select
            Return False
        End Function

        Private Function CharacterIsInDelimiter(testCharacter As Char) As Boolean
            Dim delimiters As String() = Me.m_Delimiters
            Dim i As Integer
            For i = 0 To delimiters.Length - 1
                If (delimiters(i).IndexOf(testCharacter) > -1) Then
                    Return True
                End If
            Next i
            Return False
        End Function

        Private Sub CheckCommentTokensForWhitespace(tokens As String())
            If (Not tokens Is Nothing) Then
                Dim str As String
                For Each str In tokens
                    If Me.m_WhiteSpaceRegEx.IsMatch(str) Then
                        Throw ExceptionUtils.GetArgumentExceptionWithArgName("CommentTokens", "TextFieldParser_WhitespaceInToken", New String(0 - 1) {})
                    End If
                Next
            End If
        End Sub

        Public Sub Close()
            Me.CloseReader()
        End Sub

        Private Sub CloseReader()
            Me.FinishReading()

            If (Not Me.m_Reader Is Nothing) Then
                If Not Me.m_LeaveOpen Then
                    Me.m_Reader.Close()
                End If
                Me.m_Reader = Nothing
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposing Then
                If Not Me.m_Disposed Then
                    Me.Close()
                End If
                Me.m_Disposed = True
            End If
        End Sub

        Protected Overrides Sub Finalize()
            Me.Dispose(False)
            MyBase.Finalize()
        End Sub

        Private Sub FinishReading()
            Me.m_LineNumber = -1
            Me.m_EndOfData = True
            Me.m_Buffer = Nothing
            Me.m_DelimiterRegex = Nothing
            Me.m_BeginQuotesRegex = Nothing
        End Sub

        Private Function GetEndOfLineIndex(Line As String) As Integer
            Dim length As Integer = Line.Length
            If (length <> 1) Then
                If ((Conversions.ToString(Line.Chars((length - 2))) = ChrW(13)) Or (Conversions.ToString(Line.Chars((length - 2))) = ChrW(10))) Then
                    Return (length - 2)
                End If
                If ((Conversions.ToString(Line.Chars((length - 1))) = ChrW(13)) Or (Conversions.ToString(Line.Chars((length - 1))) = ChrW(10))) Then
                    Return (length - 1)
                End If
            End If
            Return length
        End Function

        Private Function GetFixedWidthField(Line As StringInfo, Index As Integer, FieldLength As Integer) As String
            Dim str2 As String
            If (FieldLength > 0) Then
                str2 = Line.SubstringByTextElements(Index, FieldLength)
            ElseIf (Index >= Line.LengthInTextElements) Then
                str2 = String.Empty
            Else
                Dim trimChars As Char() = New Char() {ChrW(13), ChrW(10)}
                str2 = Line.SubstringByTextElements(Index).TrimEnd(trimChars)
            End If
            If Me.m_TrimWhiteSpace Then
                Return str2.Trim
            End If
            Return str2
        End Function

        Private Function IgnoreLine(line As String) As Boolean
            If (Not line Is Nothing) Then
                Dim str As String = line.Trim
                If (str.Length = 0) Then
                    Return True
                End If
                If (Not Me.m_CommentTokens Is Nothing) Then
                    Dim str2 As String
                    For Each str2 In Me.m_CommentTokens
                        If (str2 <> "") Then
                            If str.StartsWith(str2, StringComparison.Ordinal) Then
                                Return True
                            End If
                            If line.StartsWith(str2, StringComparison.Ordinal) Then
                                Return True
                            End If
                        End If
                    Next
                End If
            End If
            Return False
        End Function

        Private Function IncreaseBufferSize() As Integer
            Dim numRef As Integer
            Me.m_PeekPosition = Me.m_CharsRead
            Dim num As Integer = (Me.m_Buffer.Length + &H1000)
            If (num > Me.m_MaxBufferSize) Then
                Throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_BufferExceededMaxSize", New String(0 - 1) {})
            End If
            Dim destinationArray As Char() = New Char(((num - 1) + 1) - 1) {}
            Array.Copy(Me.m_Buffer, destinationArray, Me.m_Buffer.Length)
            Dim num2 As Integer = Me.m_Reader.Read(destinationArray, Me.m_Buffer.Length, &H1000)
            Me.m_Buffer = destinationArray
            numRef = CInt(AddressOf Me.m_CharsRead) = (numRef + num2)
            Return num2
        End Function

        Private Sub InitializeFromPath(path As String, defaultEncoding As Encoding, detectEncoding As Boolean)
            If (path = "") Then
                Throw ExceptionUtils.GetArgumentNullException("path")
            End If
            If (defaultEncoding Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("defaultEncoding")
            End If
            Dim stream As New FileStream(Me.ValidatePath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Me.m_Reader = New StreamReader(stream, defaultEncoding, detectEncoding)
            Me.ReadToBuffer()
        End Sub

        Private Sub InitializeFromStream(stream As Stream, defaultEncoding As Encoding, detectEncoding As Boolean)
            If (stream Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("stream")
            End If
            If Not stream.CanRead Then
                Dim placeHolders As String() = New String() {"stream"}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("stream", "TextFieldParser_StreamNotReadable", placeHolders)
            End If
            If (defaultEncoding Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("defaultEncoding")
            End If
            Me.m_Reader = New StreamReader(stream, defaultEncoding, detectEncoding)
            Me.ReadToBuffer()
        End Sub

        Private Function ParseDelimitedLine() As String()
            Dim line As String = Me.ReadNextDataLine
            If (line Is Nothing) Then
                Return Nothing
            End If
            Dim lineNumber As Long = (Me.m_LineNumber - 1)
            Dim startat As Integer = 0
            Dim list As New List(Of String)
            Dim endOfLineIndex As Integer = Me.GetEndOfLineIndex(line)
            Do While (startat <= endOfLineIndex)
                Dim field As String
                Dim match As Match = Nothing
                Dim success As Boolean = False
                If Me.m_HasFieldsEnclosedInQuotes Then
                    match = Me.BeginQuotesRegex.Match(line, startat)
                    success = match.Success
                End If
                If success Then
                    startat = (match.Index + match.Length)
                    Dim builder As New QuoteDelimitedFieldBuilder(Me.m_DelimiterWithEndCharsRegex, Me.m_SpaceChars)
                    builder.BuildField(line, startat)
                    If builder.MalformedLine Then
                        Dim trimChars As Char() = New Char() {ChrW(13), ChrW(10)}
                        Me.m_ErrorLine = line.TrimEnd(trimChars)
                        Me.m_ErrorLineNumber = lineNumber
                        Dim args As String() = New String() {lineNumber.ToString(CultureInfo.InvariantCulture)}
                        Throw New MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", args), lineNumber)
                    End If
                    If builder.FieldFinished Then
                        field = builder.Field
                        startat = (builder.Index + builder.DelimiterLength)
                    Else
                        Do
                            Dim length As Integer = line.Length
                            Dim str3 As String = Me.ReadNextDataLine
                            If (str3 Is Nothing) Then
                                Dim chArray2 As Char() = New Char() {ChrW(13), ChrW(10)}
                                Me.m_ErrorLine = line.TrimEnd(chArray2)
                                Me.m_ErrorLineNumber = lineNumber
                                Dim textArray2 As String() = New String() {lineNumber.ToString(CultureInfo.InvariantCulture)}
                                Throw New MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", textArray2), lineNumber)
                            End If
                            If ((line.Length + str3.Length) > Me.m_MaxLineSize) Then
                                Dim chArray3 As Char() = New Char() {ChrW(13), ChrW(10)}
                                Me.m_ErrorLine = line.TrimEnd(chArray3)
                                Me.m_ErrorLineNumber = lineNumber
                                Dim textArray3 As String() = New String() {lineNumber.ToString(CultureInfo.InvariantCulture)}
                                Throw New MalformedLineException(Utils.GetResourceString("TextFieldParser_MaxLineSizeExceeded", textArray3), lineNumber)
                            End If
                            line = (line & str3)
                            endOfLineIndex = Me.GetEndOfLineIndex(line)
                            builder.BuildField(line, length)
                            If builder.MalformedLine Then
                                Dim chArray4 As Char() = New Char() {ChrW(13), ChrW(10)}
                                Me.m_ErrorLine = line.TrimEnd(chArray4)
                                Me.m_ErrorLineNumber = lineNumber
                                Dim textArray4 As String() = New String() {lineNumber.ToString(CultureInfo.InvariantCulture)}
                                Throw New MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedDelimitedLine", textArray4), lineNumber)
                            End If
                        Loop While Not builder.FieldFinished
                        field = builder.Field
                        startat = (builder.Index + builder.DelimiterLength)
                    End If
                    If Me.m_TrimWhiteSpace Then
                        field = field.Trim
                    End If
                    list.Add(field)
                Else
                    Dim match2 As Match = Me.m_DelimiterRegex.Match(line, startat)
                    If match2.Success Then
                        field = line.Substring(startat, (match2.Index - startat))
                        If Me.m_TrimWhiteSpace Then
                            field = field.Trim
                        End If
                        list.Add(field)
                        startat = (match2.Index + match2.Length)
                    Else
                        Dim chArray5 As Char() = New Char() {ChrW(13), ChrW(10)}
                        field = line.Substring(startat).TrimEnd(chArray5)
                        If Me.m_TrimWhiteSpace Then
                            field = field.Trim
                        End If
                        list.Add(field)
                        Exit Do
                    End If
                End If
            Loop
            Return list.ToArray
        End Function

        Private Function ParseFixedWidthLine() As String()
            Dim str As String = Me.ReadNextDataLine
            If (str Is Nothing) Then
                Return Nothing
            End If
            Dim trimChars As Char() = New Char() {ChrW(13), ChrW(10)}
            Dim line As New StringInfo(str.TrimEnd(trimChars))
            Me.ValidateFixedWidthLine(line, (Me.m_LineNumber - 1))
            Dim index As Integer = 0
            Dim num1 As Integer = (Me.m_FieldWidths.Length - 1)
            Dim strArray2 As String() = New String((num1 + 1) - 1) {}
            Dim num2 As Integer = num1
            Dim i As Integer = 0
            Do While (i <= num2)
                strArray2(i) = Me.GetFixedWidthField(line, index, Me.m_FieldWidths(i))
                index = (index + Me.m_FieldWidths(i))
                i += 1
            Loop
            Return strArray2
        End Function

        Public Function PeekChars(numberOfChars As Integer) As String
            If (numberOfChars <= 0) Then
                Dim placeHolders As String() = New String() {"numberOfChars"}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("numberOfChars", "TextFieldParser_NumberOfCharsMustBePositive", placeHolders)
            End If
            If ((Me.m_Reader Is Nothing) Or (Me.m_Buffer Is Nothing)) Then
                Return Nothing
            End If
            If Me.m_EndOfData Then
                Return Nothing
            End If
            Dim str2 As String = Me.PeekNextDataLine
            If (str2 Is Nothing) Then
                Me.m_EndOfData = True
                Return Nothing
            End If
            Dim trimChars As Char() = New Char() {ChrW(13), ChrW(10)}
            str2 = str2.TrimEnd(trimChars)
            If (str2.Length < numberOfChars) Then
                Return str2
            End If
            Return New StringInfo(str2).SubstringByTextElements(0, numberOfChars)
        End Function

        Private Function PeekNextDataLine() As String
            Dim str As String
            Dim changeBuffer As ChangeBufferFunction = New ChangeBufferFunction(AddressOf Me.IncreaseBufferSize)
            Me.SlideCursorToStartOfBuffer()
            Me.m_PeekPosition = 0
            Do
                str = Me.ReadNextLine(Me.m_PeekPosition, changeBuffer)
            Loop While Me.IgnoreLine(str)
            Return str
        End Function

        Public Function ReadFields() As String()
            If Not ((Me.m_Reader Is Nothing) Or (Me.m_Buffer Is Nothing)) Then
                Me.ValidateReadyToRead()

                Select Case Me.m_TextFieldType
                    Case FieldType.Delimited
                        Return Me.ParseDelimitedLine
                    Case FieldType.FixedWidth
                        Return Me.ParseFixedWidthLine
                End Select
            End If
            Return Nothing
        End Function

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Function ReadLine() As String
            Dim numRef As Long
            If ((Me.m_Reader Is Nothing) Or (Me.m_Buffer Is Nothing)) Then
                Return Nothing
            End If
            Dim changeBuffer As ChangeBufferFunction = New ChangeBufferFunction(AddressOf Me.ReadToBuffer)
            Dim str2 As String = Me.ReadNextLine(Me.m_Position, changeBuffer)
            If (str2 Is Nothing) Then
                Me.FinishReading()
                Return Nothing
            End If
            numRef = CLng(AddressOf Me.m_LineNumber) = (numRef + 1)
            Dim trimChars As Char() = New Char() {ChrW(13), ChrW(10)}
            Return str2.TrimEnd(trimChars)
        End Function

        Private Function ReadNextDataLine() As String
            Dim str As String
            Dim changeBuffer As ChangeBufferFunction = New ChangeBufferFunction(AddressOf Me.ReadToBuffer)
            Do
                Dim numRef As Long
                str = Me.ReadNextLine(Me.m_Position, changeBuffer)
                numRef = CLng(AddressOf Me.m_LineNumber) = (numRef + 1)
            Loop While Me.IgnoreLine(str)
            If (str Is Nothing) Then
                Me.CloseReader()
            End If
            Return str
        End Function

        Private Function ReadNextLine(ByRef Cursor As Integer, ChangeBuffer As ChangeBufferFunction) As String
            If ((Cursor Is Me.m_CharsRead) AndAlso (ChangeBuffer.Invoke = 0)) Then
                Return Nothing
            End If
            Dim builder As StringBuilder = Nothing
            Do
                Dim num2 As Integer = (Me.m_CharsRead - 1)
                Dim i As Integer = Cursor
                Do While (i <= num2)
                    Dim ch As Char = Me.m_Buffer(i)
                    If ((Conversions.ToString(ch) = ChrW(13)) Or (Conversions.ToString(ch) = ChrW(10))) Then
                        If (Not builder Is Nothing) Then
                            builder.Append(Me.m_Buffer, Cursor, ((i - Cursor) + 1))
                        Else
                            builder = New StringBuilder((i + 1))
                            builder.Append(Me.m_Buffer, Cursor, ((i - Cursor) + 1))
                        End If
                        Cursor = (i + 1)
                        If (Conversions.ToString(ch) = ChrW(13)) Then
                            If (Cursor < Me.m_CharsRead) Then
                                If (Conversions.ToString(Me.m_Buffer(Cursor)) = ChrW(10)) Then
                                    Cursor += 1
                                    builder.Append(ChrW(10))
                                End If
                            ElseIf ((ChangeBuffer.Invoke > 0) AndAlso (Conversions.ToString(Me.m_Buffer(Cursor)) = ChrW(10))) Then
                                Cursor += 1
                                builder.Append(ChrW(10))
                            End If
                        End If
                        Return builder.ToString
                    End If
                    i += 1
                Loop
                Dim charCount As Integer = (Me.m_CharsRead - Cursor)
                If (builder Is Nothing) Then
                    builder = New StringBuilder((charCount + 10))
                End If
                builder.Append(Me.m_Buffer, Cursor, charCount)
            Loop While (ChangeBuffer.Invoke > 0)
            Return builder.ToString
        End Function

        Private Function ReadToBuffer() As Integer
            Me.m_Position = 0
            Dim length As Integer = Me.m_Buffer.Length
            If (length > &H1000) Then
                length = &H1000
                Me.m_Buffer = New Char(((length - 1) + 1) - 1) {}
            End If
            Me.m_CharsRead = Me.m_Reader.Read(Me.m_Buffer, 0, length)
            Return Me.m_CharsRead
        End Function

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Function ReadToEnd() As String
            If ((Me.m_Reader Is Nothing) Or (Me.m_Buffer Is Nothing)) Then
                Return Nothing
            End If
            Dim builder1 As New StringBuilder(Me.m_Buffer.Length)
            builder1.Append(Me.m_Buffer, Me.m_Position, (Me.m_CharsRead - Me.m_Position))
            builder1.Append(Me.m_Reader.ReadToEnd)
            Me.FinishReading()
            Return builder1.ToString
        End Function

        Public Sub SetDelimiters(ParamArray delimiters As String())
            Me.Delimiters = delimiters
        End Sub

        Public Sub SetFieldWidths(ParamArray fieldWidths As Integer())
            Me.FieldWidths = fieldWidths
        End Sub

        Private Function SlideCursorToStartOfBuffer() As Integer
            If (Me.m_Position > 0) Then
                Dim length As Integer = Me.m_Buffer.Length
                Dim num3 As Integer = (Me.m_CharsRead - Me.m_Position)
                Dim destinationArray As Char() = New Char(((length - 1) + 1) - 1) {}
                Array.Copy(Me.m_Buffer, Me.m_Position, destinationArray, 0, num3)
                Dim num4 As Integer = Me.m_Reader.Read(destinationArray, num3, (length - num3))
                Me.m_CharsRead = (num3 + num4)
                Me.m_Position = 0
                Me.m_Buffer = destinationArray
                Return num4
            End If
            Return 0
        End Function

        Private Sub ValidateAndEscapeDelimiters()
            If (Me.m_Delimiters Is Nothing) Then
                Dim placeHolders As String() = New String() {"Delimiters"}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimitersNothing", placeHolders)
            End If
            If (Me.m_Delimiters.Length = 0) Then
                Dim textArray2 As String() = New String() {"Delimiters"}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimitersNothing", textArray2)
            End If
            Dim builder As New StringBuilder
            Dim builder2 As New StringBuilder
            builder2.Append((Me.EndQuotePattern & "("))
            Dim num As Integer = (Me.m_Delimiters.Length - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                If (Not Me.m_Delimiters(i) Is Nothing) Then
                    If (Me.m_HasFieldsEnclosedInQuotes AndAlso (Me.m_Delimiters(i).IndexOf(""""c) > -1)) Then
                        Throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_IllegalDelimiter", New String(0 - 1) {})
                    End If
                    Dim str As String = Regex.Escape(Me.m_Delimiters(i))
                    builder.Append((str & "|"))
                    builder2.Append((str & "|"))
                End If
                i += 1
            Loop
            Me.m_SpaceChars = Me.WhitespaceCharacters
            Me.m_DelimiterRegex = New Regex(builder.ToString(0, (builder.Length - 1)), RegexOptions.CultureInvariant)
            builder.Append(ChrW(13) & "|" & ChrW(10))
            Me.m_DelimiterWithEndCharsRegex = New Regex(builder.ToString, RegexOptions.CultureInvariant)
            builder2.Append(ChrW(13) & "|" & ChrW(10) & ")|""$")
        End Sub

        Private Sub ValidateDelimiters(delimiterArray As String())
            If (Not delimiterArray Is Nothing) Then
                Dim str As String
                For Each str In delimiterArray
                    If (str = "") Then
                        Dim placeHolders As String() = New String() {"Delimiters"}
                        Throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_DelimiterNothing", placeHolders)
                    End If
                    Dim anyOf As Char() = New Char() {ChrW(13), ChrW(10)}
                    If (str.IndexOfAny(anyOf) > -1) Then
                        Throw ExceptionUtils.GetArgumentExceptionWithArgName("Delimiters", "TextFieldParser_EndCharsInDelimiter", New String(0 - 1) {})
                    End If
                Next
            End If
        End Sub

        Private Sub ValidateFieldTypeEnumValue(value As FieldType, paramName As String)
            If ((value < FieldType.Delimited) OrElse (value > FieldType.FixedWidth)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(FieldType))
            End If
        End Sub

        Private Sub ValidateFieldWidths()
            Dim numRef As Integer
            If (Me.m_FieldWidths Is Nothing) Then
                Throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_FieldWidthsNothing", New String(0 - 1) {})
            End If
            If (Me.m_FieldWidths.Length = 0) Then
                Throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_FieldWidthsNothing", New String(0 - 1) {})
            End If
            Dim index As Integer = (Me.m_FieldWidths.Length - 1)
            Me.m_LineLength = 0
            Dim num2 As Integer = (index - 1)
            Dim i As Integer = 0
            Do While (i <= num2)
                numRef = CInt(AddressOf Me.m_LineLength) = (numRef + Me.m_FieldWidths(i))
                i += 1
            Loop
            If (Me.m_FieldWidths(index) > 0) Then
                numRef = CInt(AddressOf Me.m_LineLength) = (numRef + Me.m_FieldWidths(index))
            End If
        End Sub

        Private Sub ValidateFieldWidthsOnInput(Widths As Integer())
            Dim num As Integer = ((Widths.Length - 1) - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                If (Widths(i) < 1) Then
                    Dim placeHolders As String() = New String() {"FieldWidths"}
                    Throw ExceptionUtils.GetArgumentExceptionWithArgName("FieldWidths", "TextFieldParser_FieldWidthsMustPositive", placeHolders)
                End If
                i += 1
            Loop
        End Sub

        Private Sub ValidateFixedWidthLine(Line As StringInfo, LineNumber As Long)
            If (Line.LengthInTextElements < Me.m_LineLength) Then
                Me.m_ErrorLine = Line.String
                Me.m_ErrorLineNumber = (Me.m_LineNumber - 1)
                Dim args As String() = New String() {LineNumber.ToString(CultureInfo.InvariantCulture)}
                Throw New MalformedLineException(Utils.GetResourceString("TextFieldParser_MalFormedFixedWidthLine", args), LineNumber)
            End If
        End Sub

        Private Function ValidatePath(path As String) As String
            Dim str As String = FileSystem.NormalizeFilePath(path, "path")
            If Not File.Exists(str) Then
                Dim args As String() = New String() {str}
                Throw New FileNotFoundException(Utils.GetResourceString("IO_FileNotFound_Path", args))
            End If
            Return str
        End Function

        Private Sub ValidateReadyToRead()
            If (Me.m_NeedPropertyCheck Or Me.ArrayHasChanged) Then
                Select Case Me.m_TextFieldType
                    Case FieldType.Delimited
                        Me.ValidateAndEscapeDelimiters()
                        Exit Select
                    Case FieldType.FixedWidth
                        Me.ValidateFieldWidths()
                        Exit Select
                End Select
                If (Not Me.m_CommentTokens Is Nothing) Then
                    Dim str As String
                    For Each str In Me.m_CommentTokens
                        If (((str <> "") AndAlso (Me.m_HasFieldsEnclosedInQuotes And (Me.m_TextFieldType = FieldType.Delimited))) AndAlso (String.Compare(str.Trim, """", StringComparison.Ordinal) = 0)) Then
                            Throw ExceptionUtils.GetInvalidOperationException("TextFieldParser_InvalidComment", New String(0 - 1) {})
                        End If
                    Next
                End If
                Me.m_NeedPropertyCheck = False
            End If
        End Sub


        ' Properties
        Private ReadOnly Property BeginQuotesRegex As Regex
            Get
                If (Me.m_BeginQuotesRegex Is Nothing) Then
                    Dim args As Object() = New Object() {Me.WhitespacePattern}
                    Dim pattern As String = String.Format(CultureInfo.InvariantCulture, "\G[{0}]*""", args)
                    Me.m_BeginQuotesRegex = New Regex(pattern, RegexOptions.CultureInvariant)
                End If
                Return Me.m_BeginQuotesRegex
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property CommentTokens As String()
            Get
                Return Me.m_CommentTokens
            End Get
            Set(value As String())
                Me.CheckCommentTokensForWhitespace(value)
                Me.m_CommentTokens = value
                Me.m_NeedPropertyCheck = True
            End Set
        End Property

        Public Property Delimiters As String()
            Get
                Return Me.m_Delimiters
            End Get
            Set(value As String())
                If (Not value Is Nothing) Then
                    Me.ValidateDelimiters(value)
                    Me.m_DelimitersCopy = DirectCast(value.Clone, String())
                Else
                    Me.m_DelimitersCopy = Nothing
                End If
                Me.m_Delimiters = value
                Me.m_NeedPropertyCheck = True
                Me.m_BeginQuotesRegex = Nothing
            End Set
        End Property

        Public ReadOnly Property EndOfData As Boolean
            Get
                If Me.m_EndOfData Then
                    Return Me.m_EndOfData
                End If
                If ((Me.m_Reader Is Nothing) Or (Me.m_Buffer Is Nothing)) Then
                    Me.m_EndOfData = True
                    Return True
                End If
                If (Not Me.PeekNextDataLine Is Nothing) Then
                    Return False
                End If
                Me.m_EndOfData = True
                Return True
            End Get
        End Property

        Private ReadOnly Property EndQuotePattern As String
            Get
                Dim args As Object() = New Object() {Me.WhitespacePattern}
                Return String.Format(CultureInfo.InvariantCulture, """[{0}]*", args)
            End Get
        End Property

        Public ReadOnly Property ErrorLine As String
            Get
                Return Me.m_ErrorLine
            End Get
        End Property

        Public ReadOnly Property ErrorLineNumber As Long
            Get
                Return Me.m_ErrorLineNumber
            End Get
        End Property

        Public Property FieldWidths As Integer()
            Get
                Return Me.m_FieldWidths
            End Get
            Set(value As Integer())
                If (Not value Is Nothing) Then
                    Me.ValidateFieldWidthsOnInput(value)
                    Me.m_FieldWidthsCopy = DirectCast(value.Clone, Integer())
                Else
                    Me.m_FieldWidthsCopy = Nothing
                End If
                Me.m_FieldWidths = value
                Me.m_NeedPropertyCheck = True
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property HasFieldsEnclosedInQuotes As Boolean
            Get
                Return Me.m_HasFieldsEnclosedInQuotes
            End Get
            Set(value As Boolean)
                Me.m_HasFieldsEnclosedInQuotes = value
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property LineNumber As Long
            Get
                If ((Me.m_LineNumber <> -1) AndAlso ((Me.m_Reader.Peek = -1) And (Me.m_Position = Me.m_CharsRead))) Then
                    Me.CloseReader()
                End If
                Return Me.m_LineNumber
            End Get
        End Property

        Public Property TextFieldType As FieldType
            Get
                Return Me.m_TextFieldType
            End Get
            Set(value As FieldType)
                Me.ValidateFieldTypeEnumValue(value, "value")
                Me.m_TextFieldType = value
                Me.m_NeedPropertyCheck = True
            End Set
        End Property

        Public Property TrimWhiteSpace As Boolean
            Get
                Return Me.m_TrimWhiteSpace
            End Get
            Set(value As Boolean)
                Me.m_TrimWhiteSpace = value
            End Set
        End Property

        Private ReadOnly Property WhitespaceCharacters As String
            Get
                Dim builder As New StringBuilder
                Dim whitespaceCodes As Integer() = Me.m_WhitespaceCodes
                Dim i As Integer
                For i = 0 To whitespaceCodes.Length - 1
                    Dim testCharacter As Char = Strings.ChrW(whitespaceCodes(i))
                    If Not Me.CharacterIsInDelimiter(testCharacter) Then
                        builder.Append(testCharacter)
                    End If
                Next i
                Return builder.ToString
            End Get
        End Property

        Private ReadOnly Property WhitespacePattern As String
            Get
                Dim builder As New StringBuilder
                Dim num2 As Integer
                For Each num2 In Me.m_WhitespaceCodes
                    Dim testCharacter As Char = Strings.ChrW(num2)
                    If Not Me.CharacterIsInDelimiter(testCharacter) Then
                        builder.Append(("\u" & num2.ToString("X4", CultureInfo.InvariantCulture)))
                    End If
                Next
                Return builder.ToString
            End Get
        End Property


        ' Fields
        Private Const BEGINS_WITH_QUOTE As String = "\G[{0}]*"""
        Private Const DEFAULT_BUFFER_LENGTH As Integer = &H1000
        Private Const DEFAULT_BUILDER_INCREASE As Integer = 10
        Private Const ENDING_QUOTE As String = """[{0}]*"
        Private m_BeginQuotesRegex As Regex
        Private m_Buffer As Char()
        Private m_CharsRead As Integer
        Private m_CommentTokens As String()
        Private m_DelimiterRegex As Regex
        Private m_Delimiters As String()
        Private m_DelimitersCopy As String()
        Private m_DelimiterWithEndCharsRegex As Regex
        Private m_Disposed As Boolean
        Private m_EndOfData As Boolean
        Private m_ErrorLine As String
        Private m_ErrorLineNumber As Long
        Private m_FieldWidths As Integer()
        Private m_FieldWidthsCopy As Integer()
        Private m_HasFieldsEnclosedInQuotes As Boolean
        Private m_LeaveOpen As Boolean
        Private m_LineLength As Integer
        Private m_LineNumber As Long
        Private m_MaxBufferSize As Integer
        Private m_MaxLineSize As Integer
        Private m_NeedPropertyCheck As Boolean
        Private m_PeekPosition As Integer
        Private m_Position As Integer
        Private m_Reader As TextReader
        Private m_SpaceChars As String
        Private m_TextFieldType As FieldType
        Private m_TrimWhiteSpace As Boolean
        Private m_WhitespaceCodes As Integer()
        Private m_WhiteSpaceRegEx As Regex
        Private Const REGEX_OPTIONS As RegexOptions = RegexOptions.CultureInvariant

        ' Nested Types
        Private Delegate Function ChangeBufferFunction() As Integer
    End Class
End Namespace

