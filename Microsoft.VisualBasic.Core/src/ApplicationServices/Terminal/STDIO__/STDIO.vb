#Region "Microsoft.VisualBasic::842ede966f066ef8c19a778a952cc508, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\STDIO__\STDIO.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 309
    '    Code Lines: 195 (63.11%)
    ' Comment Lines: 80 (25.89%)
    '    - Xml Docs: 93.75%
    ' 
    '   Blank Lines: 34 (11.00%)
    '     File Size: 12.36 KB


    '     Module STDIO
    ' 
    '         Function: charTestEquals, MsgBox, scanf, ZeroFill
    ' 
    '         Sub: cat, fprintf, print, printf
    '         Delegate Function
    ' 
    '             Function: InputPassword, Read
    ' 
    '             Sub: Write, WriteLine
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' A standard input/output compatibility package that makes VisualBasic console
    ''' program easily running on the Linux server or mac osx operating system.
    ''' (一个用于让VisualBasic应用程序更加容易的运行于Linux服务器或者MAC系统之上的标准输入输出流的系统兼容包)
    ''' </summary>
    ''' <remarks></remarks>
    <Package("STDIO", Description:="A standard input/output compatibility package that makes VisualBasic console program easily running on the Linux server or mac osx operating system.",
                      Publisher:="xie.guigang@live.com",
                      Revision:=569,
                      Url:="http://gcmodeller.org")>
    Public Module STDIO

        ''' <summary>
        ''' A dictionary list of the escape characters.(转义字符列表)
        ''' </summary>
        ''' <remarks></remarks>
        Dim escapings As New Dictionary(Of String, String) From {
 _
            {"\o", String.Empty},
            {"\n", vbCrLf},
            {"\r", vbCr},
            {"\t", vbTab},
            {"\v", String.Empty},
            {"\b", vbBack},
            {"\f", vbFormFeed},
            {"\'", ASCII.Quot},
            {"\" & ASCII.Quot, ASCII.Quot}
        }

#Region "printf"

        ''' <summary>
        ''' Output the string to the console using a specific formation.
        ''' (按照指定的格式将字符串输出到终端窗口之上，请注意，这个函数除了将数据流
        ''' 输出到标准终端之外，还会输出到调试终端)
        ''' </summary>
        ''' <param name="s">A string to print on the console window.(输出到终端窗口之上的字符串)</param>
        ''' <param name="args">Formation parameters.(格式化参数)</param>
        ''' <remarks></remarks>
        '''
        Public Sub printf(s As String, ParamArray args As Object())
            s = sprintf(s, args)

            Console.Write(s)
            Call Trace.Write(s)
            Call Debug.Write(s)
        End Sub
#End Region

        <Extension>
        Public Sub fprintf(Destination As TextWriter, Format As String, ParamArray Parameters As Object())
            Destination.Write(sprintf(Format, Parameters))
        End Sub

        Public Sub print(s As String, Optional color As ConsoleColor = ConsoleColor.White)
            Dim cl As ConsoleColor = Console.ForegroundColor
            Console.ForegroundColor = color
            Console.Write(s)
            Console.ForegroundColor = cl
        End Sub

        ''' <summary>
        ''' Alias for the <see cref="Console.Write"/>.(不换行)
        ''' </summary>
        ''' <param name="out"></param>
        ''' <remarks></remarks>
        Public Sub cat(ParamArray out As String())
            Dim s As String = String.Join("", out)
            Call Console.Write(sprintf(s))
        End Sub

        ''' <summary>
        ''' Read the string that user input on the console to the function paramenter.
        ''' (将用户在终端窗口之上输入的数据赋值给一个字符串变量)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function scanf(ByRef s As String, Optional color As ConsoleColor = ConsoleColor.White) As String
            Dim cl As ConsoleColor = Console.ForegroundColor
            Call Console.Write(s)
            Console.ForegroundColor = color
            s = Console.ReadLine
            Console.ForegroundColor = cl
            Return s
        End Function

        Const ____ZERO As String =
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000" &
                "000000000000000000000000000000000000000000000000000000000000"

        ''' <summary>
        ''' Fill the number string with specific length of ZERO sequence to generates the fixed width string.
        ''' </summary>
        ''' <param name="sn"></param>
        ''' <param name="len"></param>
        ''' <returns></returns>
        Public Function ZeroFill(sn As String, len As Integer) As String
            If sn.Length >= len Then
                Return sn
            Else
                Dim d As Integer = len - sn.Length
                Dim zr As String = ____ZERO.Substring(0, d)
                Return zr & sn
            End If
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

            If style = MsgBoxStyle.AbortRetryIgnore Then
                Call Console.Write("Abort/Retry/Ignore?(a/r/i) [R]")
                [default] = "R"
            ElseIf style = MsgBoxStyle.OkCancel Then
                Call Console.Write("Ok/Cancel?(o/c) [O]")
                [default] = "O"
            ElseIf style = MsgBoxStyle.OkOnly Then
                Call Console.WriteLine("Press any key to continute...")
                Call Console.ReadKey()
                Return MsgBoxResult.Ok
            ElseIf style = MsgBoxStyle.RetryCancel Then
                Call Console.Write("Retry/Cancel?(r/c) [R]")
                [default] = "R"
            ElseIf style = MsgBoxStyle.YesNo Then
                Call Console.Write("Yes/No?(y/n) [Y]")
                [default] = "Y"
            ElseIf style = MsgBoxStyle.YesNoCancel Then
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

            If style = MsgBoxStyle.AbortRetryIgnore Then
                If charTestEquals(input, "A"c) Then
                    Return MsgBoxResult.Abort
                ElseIf charTestEquals(input, "R"c) Then
                    Return MsgBoxResult.Retry
                ElseIf charTestEquals(input, "I"c) Then
                    Return MsgBoxResult.Ignore
                Else
                    Return MsgBoxResult.Retry
                End If
            ElseIf style = MsgBoxStyle.OkCancel Then

                If charTestEquals(input, "O"c) Then
                    Return MsgBoxResult.Ok
                ElseIf charTestEquals(input, "C"c) Then
                    Return MsgBoxResult.Cancel
                Else
                    Return MsgBoxResult.Ok
                End If
            ElseIf style = MsgBoxStyle.OkOnly Then
                Return MsgBoxResult.Ok
            ElseIf style = MsgBoxStyle.RetryCancel Then

                If charTestEquals(input, "R"c) Then
                    Return MsgBoxResult.Retry
                ElseIf charTestEquals(input, "C"c) Then
                    Return MsgBoxResult.Cancel
                Else
                    Return MsgBoxResult.Retry
                End If
            ElseIf style = MsgBoxStyle.YesNo Then

                If charTestEquals(input, "Y"c) Then
                    Return MsgBoxResult.Yes
                ElseIf charTestEquals(input, "N"c) Then
                    Return MsgBoxResult.No
                Else
                    Return MsgBoxResult.Yes
                End If
            ElseIf style = MsgBoxStyle.YesNoCancel Then

                If charTestEquals(input, "Y"c) Then
                    Return MsgBoxResult.Yes
                ElseIf charTestEquals(input, "N"c) Then
                    Return MsgBoxResult.No
                ElseIf charTestEquals(input, "C"c) Then
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
        Private Function charTestEquals(input As String, compare As Char) As Boolean
            If String.IsNullOrEmpty(input) Then
                Return False
            Else
                Return Asc(input.First) = Asc(compare)
            End If
        End Function

        Public Delegate Function TryParseDelegate(Of T)(str$, ByRef val As T) As Boolean

        ''' <summary>
        ''' Read Method with Generics &amp; Delegate
        ''' 
        ''' In a console application there is often the need to ask (and validate) some data from users. 
        ''' For this reason I have created a function that make use of generics and delegates to 
        ''' speed up programming.
        ''' 
        ''' > http://www.codeproject.com/Tips/1108772/Read-Method-with-Generics-Delegate
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="msg"></param>
        ''' <param name="parser"></param>
        ''' <param name="_default"></param>
        ''' <returns></returns>
        Public Function Read(Of T)(msg$, parser As TryParseDelegate(Of T), Optional _default$ = Nothing) As T
            Dim line As String
            Dim value As T

            Do
                Call Console.Write(msg)

                If Not _default.StringEmpty Then
                    Call Console.Write($" <default={_default}>")
                End If

                Call Console.Write(": ")

                line = Console.ReadLine()

                If String.IsNullOrWhiteSpace(line) Then
                    line = _default?.ToString()
                End If
            Loop While Not parser(line, value)

            Return value
        End Function

        ''' <summary>
        ''' Writes the text representation of the specified object to the standard output
        ''' stream.
        ''' </summary>
        ''' <param name="o">The value to write, or null.</param>
        Public Sub Write(o As Object)
            If VBDebugger.ForceSTDError Then
                Call Console.Error.Write(o)
            Else
                Call Console.Write(o)
            End If
        End Sub

        ''' <summary>
        ''' Writes the text representation of the specified object, followed by the current
        ''' line terminator, to the standard output stream.
        ''' </summary>
        ''' <param name="o">The value to write.</param>
        Public Sub WriteLine(Optional o As Object = Nothing)
            If VBDebugger.ForceSTDError Then
                Call Console.Error.WriteLine(o)
            Else
                Call Console.WriteLine(o)
            End If
        End Sub

        Public Function InputPassword(Optional prompt$ = "input your password", Optional maxLength% = 20) As String
            Dim pass$ = Nothing
            Call Console.Write(prompt & ": ")
            Call New ConsolePasswordInput().PasswordInput(pass, maxLength)
            Return pass
        End Function
    End Module
End Namespace
