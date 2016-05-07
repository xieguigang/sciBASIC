Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Terminal

    ''' <summary>
    ''' A standard input/output compatibility package that makes VisualBasic console
    ''' program easily running on the Linux server or mac osx operating system.
    ''' (一个用于让VisualBasic应用程序更加容易的运行于Linux服务器或者MAC系统之上的标准输入输出流的系统兼容包)
    ''' </summary>
    ''' <remarks></remarks>
    <PackageNamespace("STDIO", Description:="A standard input/output compatibility package that makes VisualBasic console program easily running on the Linux server or mac osx operating system.",
                      Publisher:="xie.guigang@live.com",
                      Revision:=569,
                      Url:="http://gcmodeller.org")>
    Public Module STDIO

        ''' <summary>
        ''' A dictionary list of the method of format a string provider class object.
        ''' (标准输入输出对象的格式化方法所提供的对象的字典)
        ''' </summary>
        ''' <remarks></remarks>
        Private ReadOnly IFormater As Dictionary(Of Char, STDIO__.I_FormatProvider.IFormatProvider) =
            New Dictionary(Of Char, STDIO__.I_FormatProvider.IFormatProvider) From
            {
                {"b"c, New STDIO__.I_FormatProvider.b},
                {"d"c, New STDIO__.I_FormatProvider.d},
                {"c"c, New STDIO__.I_FormatProvider.c},
                {"e"c, New STDIO__.I_FormatProvider.e},
                {"f"c, New STDIO__.I_FormatProvider.f},
                {"g"c, New STDIO__.I_FormatProvider.g},
                {"o"c, New STDIO__.I_FormatProvider.o},
                {"s"c, New STDIO__.I_FormatProvider.s},
                {"u"c, New STDIO__.I_FormatProvider.u},
                {"x"c, New STDIO__.I_FormatProvider.x},
                {"%"c, New STDIO__.I_FormatProvider.P}}

        ''' <summary>
        ''' A dictionary list of the escape characters.(转义字符列表)
        ''' </summary>
        ''' <remarks></remarks>
        Dim Eschs As Dictionary(Of String, String) =
            New Dictionary(Of String, String) From
            {
                {"\o", String.Empty},
                {"\n", vbCrLf},
                {"\r", vbCr},
                {"\t", vbTab},
                {"\v", String.Empty},
                {"\b", vbBack},
                {"\f", vbFormFeed},
                {"\'", QUOT_CHAR},
                {"\" & QUOT_CHAR, QUOT_CHAR}}

        Public Const QUOT_CHAR As Char = Chr(34)
        Const FORMAT_CONTROL_EXPRESSION As String = "(%-?0?(\d+)?(\.\d+)?[bdcefggosux])|%%"

        Dim IORedirected As STDIO__.I_ConsoleDeviceHandle = SystemConsoleDevice.Instance

        ''' <summary>
        ''' <see cref="System.Console"></see>系统终端对象
        ''' </summary>
        ''' <remarks></remarks>
        Friend Class SystemConsoleDevice : Implements STDIO__.I_ConsoleDeviceHandle

            Public Shared ReadOnly Instance As SystemConsoleDevice = New SystemConsoleDevice

            Public Function ReadLine() As String Implements STDIO__.I_ConsoleDeviceHandle.ReadLine
                Return System.Console.ReadLine
            End Function

            Public Sub WriteLine(s As String) Implements STDIO__.I_ConsoleDeviceHandle.WriteLine
                Call System.Console.WriteLine(s)
            End Sub

            Public Sub WriteLine(s As String, ParamArray args() As String) Implements STDIO__.I_ConsoleDeviceHandle.WriteLine
                Call System.Console.WriteLine(s)
            End Sub

            Public Function Read() As Integer Implements STDIO__.I_ConsoleDeviceHandle.Read
                Return System.Console.Read
            End Function
        End Class

        Public Sub IORedirect(IO As STDIO__.I_ConsoleDeviceHandle)
            IORedirected = IO
        End Sub

        ''' <summary>
        ''' Formatting a string using some formation arguments.(使用一些指定的格式化参数来格式化一个字符串)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="Args"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Format(s As String, ParamArray args As String()) As String
            Dim formatProvider As Regex = New Regex(FORMAT_CONTROL_EXPRESSION)
            Dim matches As String() = formatProvider.Matches(s).ToArray
            Dim p As Integer = 1, p2 As Integer
            Dim sBuilder As StringBuilder = New StringBuilder(value:=s, capacity:=128)
            Dim arg As String, l As Integer
            Dim TMP As String

            For Each escToken As KeyValuePair(Of String, String) In Eschs
                Call sBuilder.Replace(escToken.Key, escToken.Value)
            Next

            If matches.Length = 0 Then   'if no formater control character was found, then return the whole originally string directly.
                Return sBuilder.ToString
            End If 'Else formatting the target input string.
            For i As Integer = 0 To matches.Length - 1  'Formatting control string replacement.
                Dim pos As Integer = InStr(sBuilder.ToString, matches(i))
                sBuilder.Remove(pos - 1, matches(i).Length)
                sBuilder.Insert(pos - 1, "<Format idx='%'/>".Replace("%", CStr(i)))
            Next

            TMP = sBuilder.ToString

            For i As Integer = 0 To matches.Length - 1
                arg = "<Format idx='%'/>".Replace("%", i)
                l = Len(arg)
                p2 = p
                p = InStr(Start:=p, String1:=TMP, String2:=arg) + l
                If p = l Then
                    p = p2 + 1
                    p = InStr(Start:=p, String1:=TMP, String2:="<Format idx='")
                    Call __replace(p + 11, sBuilder, Index:=i)
                    TMP = sBuilder.ToString
                End If
            Next

            p = 0
            For i As Integer = 0 To matches.Length - 1
                arg = matches(i)
                sBuilder.Replace("<Format idx='%'/>".Replace("%", CStr(i)), IFormater(arg.Last).ActionFormat(arg, args(p)))
                If arg.Last <> "%"c Then
                    p += 1
                End If
            Next

            Call sBuilder.Replace("\\", "\")

            Return sBuilder.ToString
        End Function

        Private Sub __replace(Start As Integer, ByRef sBuilder As StringBuilder, Index As Integer)
            For j As Integer = Start + 1 To sBuilder.Length - 1
                If sBuilder.Chars(j) = "'"c Then
                    sBuilder.Remove(Start + 1, j - 1 - Start)
                    sBuilder.Insert(Start + 1, Index)
                    Return
                End If
            Next
        End Sub

        ''' <summary>
        ''' Output the string to the console using a specific formation.(按照指定的格式将字符串输出到终端窗口之上，请注意，这个函数除了将数据流输出到标准终端之外，还会输出到调试终端)
        ''' </summary>
        ''' <param name="s">A string to print on the console window.(输出到终端窗口之上的字符串)</param>
        ''' <param name="args">Formation parameters.(格式化参数)</param>
        ''' <remarks></remarks>
        '''
        <ExportAPI("printf", Info:="Output the string to the console using a specific formation.")>
        Public Sub Printf(s As String, ParamArray args As String())
            s = Format(s, args)
            Call IORedirected.WriteLine(s)
            Call Trace.Write(s)
            Call Debug.Write(s)
        End Sub

        ''' <summary>
        ''' 不换行
        ''' </summary>
        ''' <param name="out"></param>
        ''' <remarks></remarks>
        Public Sub cat(ParamArray out As String())
            Dim s As String = STDIO.Format(String.Join("", out))
            Call Console.Write(s)
        End Sub

        ''' <summary>
        ''' Read the string that user input on the console to the function paramenter.
        ''' (将用户在终端窗口之上输入的数据赋值给一个字符串变量)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function scanf(ByRef s As String) As String
            Call Console.Write(s)
            s = IORedirected.ReadLine
            Return s
        End Function

        <ExportAPI("Pause")>
        Public Function Pause() As Integer
            Return IORedirected.Read()
        End Function

        <ExportAPI("ZeroFill")>
        Public Function ZeroFill(n As String, len As Integer) As String
            Return STDIO__.I_FormatProvider.d.ZeroFill(n, len)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="prompt"></param>
        ''' <param name="style">
        ''' Value just allow:
        ''' <see cref="MsgBoxStyle.AbortRetryIgnore"/>,
        ''' <see cref="MsgBoxStyle.OkCancel"/>,
        ''' <see cref="MsgBoxStyle.OkOnly"/>,
        ''' <see cref="MsgBoxStyle.RetryCancel"/>,
        ''' <see cref="MsgBoxStyle.YesNo"/>,
        ''' <see cref="MsgBoxStyle.YesNoCancel"/></param>
        ''' <returns></returns>
        Public Function MsgBox(prompt As String, Optional style As MsgBoxStyle = MsgBoxStyle.YesNo) As MsgBoxResult
            Dim [default] As String = ""

            Call Console.WriteLine(prompt)

            If style.HasFlag(MsgBoxStyle.AbortRetryIgnore) Then
                Call Console.Write("Abort/Retry/Ignore?(a/r/i) [R]")
                [default] = "R"
            ElseIf style.HasFlag(MsgBoxStyle.OkCancel) Then
                Call Console.Write("Ok/Cancel?(o/c) [O]")
                [default] = "O"
            ElseIf style.HasFlag(MsgBoxStyle.OkOnly) Then
                Call Console.WriteLine("Press any key to continute...")
                Call Console.ReadKey()
                Return MsgBoxResult.Ok
            ElseIf style.HasFlag(MsgBoxStyle.RetryCancel) Then
                Call Console.Write("Retry/Cancel?(r/c) [R]")
                [default] = "R"
            ElseIf style.HasFlag(MsgBoxStyle.YesNo) Then
                Call Console.Write("Yes/No?(y/n) [Y]")
                [default] = "Y"
            ElseIf style.HasFlag(MsgBoxStyle.YesNoCancel) Then
                Call Console.Write("Yes/No/Cancel?(y/n/c) [Y]")
                [default] = "Y"
            End If

            Call Console.Write("  ")

            Dim input As String = Console.ReadLine
            If String.IsNullOrEmpty(input) Then
                input = [default]
            Else
                input = input.ToUpper
            End If

            If style.HasFlag(MsgBoxStyle.AbortRetryIgnore) Then
                If __testEquals(input, "A"c) Then
                    Return MsgBoxResult.Abort
                ElseIf __testEquals(input, "R"c) Then
                    Return MsgBoxResult.Retry
                ElseIf __testEquals(input, "I"c) Then
                    Return MsgBoxResult.Ignore
                Else
                    Return MsgBoxResult.Retry
                End If
            ElseIf style.HasFlag(MsgBoxStyle.OkCancel) Then

                If __testEquals(input, "O"c) Then
                    Return MsgBoxResult.Ok
                ElseIf __testEquals(input, "C"c) Then
                    Return MsgBoxResult.Cancel
                Else
                    Return MsgBoxResult.Ok
                End If
            ElseIf style.HasFlag(MsgBoxStyle.OkOnly) Then
                Return MsgBoxResult.Ok
            ElseIf style.HasFlag(MsgBoxStyle.RetryCancel) Then

                If __testEquals(input, "R"c) Then
                    Return MsgBoxResult.Retry
                ElseIf __testEquals(input, "C"c) Then
                    Return MsgBoxResult.Cancel
                Else
                    Return MsgBoxResult.Retry
                End If
            ElseIf style.HasFlag(MsgBoxStyle.YesNo) Then

                If __testEquals(input, "Y"c) Then
                    Return MsgBoxResult.Yes
                ElseIf __testEquals(input, "N"c) Then
                    Return MsgBoxResult.No
                Else
                    Return MsgBoxResult.Yes
                End If
            ElseIf style.HasFlag(MsgBoxStyle.YesNoCancel) Then

                If __testEquals(input, "Y"c) Then
                    Return MsgBoxResult.Yes
                ElseIf __testEquals(input, "N"c) Then
                    Return MsgBoxResult.No
                ElseIf __testEquals(input, "C"c) Then
                    Return MsgBoxResult.Cancel
                Else
                    Return MsgBoxResult.Yes
                End If
            Else
                Return MsgBoxResult.Ok
            End If
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="compare">大写的</param>
        ''' <returns></returns>
        Private Function __testEquals(input As String, compare As Char) As Boolean
            If String.IsNullOrEmpty(input) Then
                Return False
            End If
            Return Asc(input.First) = Asc(compare)
        End Function
    End Module
End Namespace