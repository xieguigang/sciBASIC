#Region "Microsoft.VisualBasic::7eae30de9a7a30da019e1d768ce52f24, Microsoft.VisualBasic.Core\Text\IO\TextEncodings.vb"

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

    '     Module TextEncodings
    ' 
    '         Properties: DefaultEncoding, TextEncodings, UTF8, UTF8WithoutBOM
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __gbk2312_encoding, CodeArray, (+2 Overloads) CodePage, codePageTable, GetEncodings
    '                   ParseEncodingsName, TransEncoding
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports defaultEncoding = Microsoft.VisualBasic.Language.Default.Default(Of System.Text.Encoding)

Namespace Text

    ''' <summary>
    ''' 表示字符编码。若要浏览此类型的.NET Framework 源代码，请参阅 Reference Source。
    ''' </summary>
    ''' 
    <FrameworkConfig(TextEncodingEnvironmentConfigName)>
    Public Module TextEncodings

        Public ReadOnly Property UTF8WithoutBOM As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>
        ''' <see cref="Encoding.Default"/>
        ''' 
        ''' (获取操作系统的当前 ANSI 代码页的编码。这个属性可以通过``default_encoding``环境参数来设置)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DefaultEncoding As defaultEncoding = Encoding.Default
        ''' <summary>
        ''' UTF-8 without BOM.(获取 UTF-8 格式的编码。<see cref="Encoding.UTF8"/>默认是带有BOM标记的，这里使用无BOM标记的UTF8编码)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UTF8 As defaultEncoding = UTF8WithoutBOM

        ''' <summary>
        ''' 编码由枚举类型<see cref="Encodings"/>到<see cref="Encoding"/>之间的映射
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 可能在Linux服务器上面没有使用gb2312编码集合，则在这个模块之中初始化会出错
        ''' 
        ''' ```bash
        ''' locale -a
        ''' 
        ''' yum install -y mono-locale-extras
        ''' ```
        ''' </remarks>
        Public ReadOnly Property TextEncodings As IReadOnlyDictionary(Of Encodings, Encoding) = codePageTable()

        ''' <summary>
        ''' 在这个函数之中会根据当前所运行的平台对utf8编码进行一下额外的处理
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function codePageTable() As Dictionary(Of Encodings, Encoding)
            Dim utf8 As Encoding

            If App.IsMicrosoftPlatform Then
                ' 2018-3-5
                ' Windows平台上面的Excel表格只能够读取带有BOM前缀的csv文件，否则中文会出现乱码
                utf8 = Encoding.UTF8
            Else
                utf8 = UTF8WithoutBOM
            End If

            Return New Dictionary(Of Encodings, Encoding) From {
 _
                {Encodings.ASCII, Encoding.ASCII},
                {Encodings.GB2312, __gbk2312_encoding()},
                {Encodings.Unicode, Encoding.Unicode},
                {Encodings.UTF7, Encoding.UTF7},
                {Encodings.UTF32, Encoding.UTF32},
                {Encodings.UTF8, utf8},
                {Encodings.UTF8WithoutBOM, UTF8WithoutBOM},
                {Encodings.Default, Encoding.Default},
                {Encodings.UTF16, Encoding.Unicode}
            }
        End Function

        Friend Const TextEncodingEnvironmentConfigName$ = "default_encoding"

        ''' <summary>
        ''' 构造函数会自动的从命令行配置之中设置默认的编码格式
        ''' </summary>
        ''' <remarks>
        ''' ###### Linux下面提示 Encoding 936 data could not be found.
        ''' 
        ''' 处理方法
        '''
        ''' 1. 应该首先``locale -a``看有没有安装``gbk``
        ''' 2. 没安装的话需要先安装gbk编码
        ''' 3. 然后再安装``mono-locale-extras``
        '''
        ''' ```bash
        ''' locale -a
        ''' yum install -y mono-locale-extras
        ''' ```
        ''' </remarks>
        Sub New()
            ' setting default codepage from App commandline options for current App Process session.
            Dim codepage$ = App.GetVariable(TextEncodingEnvironmentConfigName)

            ' If no settings from the commandline, using default ANSI encoding
            If codepage.StringEmpty Then
                DefaultEncoding = Encoding.Default
            Else
                DefaultEncoding = Text _
                    .ParseEncodingsName(codepage, Encodings.Default) _
                    .CodePage

                Call $"*default_encoding* have been changed to {DefaultEncoding.DefaultValue.ToString}".__INFO_ECHO
            End If

            ' 如果检测到了gb2312编码被映射为了utf8编码，则提示用户为服务器安装gb2312编码
            If TextEncodings(Encodings.GB2312) Is Encoding.UTF8 Then
                Call {
                    "You can just ignore this warning, or fix this warning by enable the gb2312 encoding on your server.",
                    "For enable the gb2312 encoding, you can run commands:",
                    "",
                    "   yum install -y mono-locale-extras",
                    ""
                }.JoinBy(ASCII.LF) _
                 .Warning
            End If
        End Sub

        Const gb2312_not_enable$ = "It seems that your Linux server didn't enable the gbk2312 text encoding, sciBASIC# will using the default utf8 encoding mapping to the gb2312 encoding."

        ''' <summary>
        ''' 在linux上面如果没有安装gb2312的话，会出错，则这个函数会默认使用UTF8编码
        ''' 并给出警告信息
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the linux server didn't enable the gb2312 text encoding, then this exception happends...
        ''' 
        ''' ```bash
        ''' [ERROR] FATAL UNHANDLED EXCEPTION: System.Exception: [Path]  /home/software/cytonetwork.test/cytonetwork ---> System.Exception: [DIR]  /home/software/cytonetwork.test ---> System.TypeInitializationException: The type initializer for 'Microsoft.VisualBasic.Text.TextEncodings' threw an exception. ---> System.NotSupportedException: Encoding 936 data could not be found. Make sure you have correct international codeset assembly installed and enabled.
        ''' at System.Text.Encoding.GetEncoding (System.Int32 codepage) [0x0023f] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        ''' at System.Text.Encoding.GetEncoding (System.String name) [0x00012] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        ''' at Microsoft.VisualBasic.Text.TextEncodings..cctor () [0x00030] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        '''  --- End of inner exception stack trace ---
        ''' at Microsoft.VisualBasic.TextDoc.SaveTo (System.String text, System.String path, System.Text.Encoding encoding, System.Boolean append, System.Boolean throwEx) [0x00063] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        '''  --- End of inner exception stack trace ---
        '''  --- End of inner exception stack trace ---
        ''' at Microsoft.VisualBasic.TextDoc.SaveTo (System.String text, System.String path, System.Text.Encoding encoding, System.Boolean append, System.Boolean throwEx) [0x000a7] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        ''' at Microsoft.VisualBasic.Language.UnixBash.LinuxRunHelper.BashShell () [0x0001b] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        ''' at Microsoft.VisualBasic.CommandLine.Interpreter.__methodInvoke (System.String commandName, System.Object[] argvs, System.String[] help_argvs) [0x001c9] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        ''' at Microsoft.VisualBasic.CommandLine.Interpreter.Execute (Microsoft.VisualBasic.CommandLine.CommandLine args) [0x00024] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        ''' at Microsoft.VisualBasic.App.RunCLI (System.Type Interpreter, Microsoft.VisualBasic.CommandLine.CommandLine args, System.String caller) [0x00012] In &lt;00ade39f7ffc4ab69ceb325aefc4ee1b>:0
        ''' at cytonetwork.Program.Main () [0x0000f] In &lt;0Fa3aca1569b43dc8ca208295f3a029d>:0
        ''' ```
        ''' </remarks>
        Private Function __gbk2312_encoding() As Encoding
            Try
                Return Encoding.GetEncoding("GB2312")
            Catch ex As Exception

                Call App.LogException(ex)

                If Not App.IsMicrosoftPlatform Then
                    Call gb2312_not_enable.Warning
                End If

                Return Encoding.UTF8
            End Try
        End Function

        ''' <summary>
        ''' Get text file save <see cref="Encoding"/> instance
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function CodePage(value As Encodings) As Encoding
            If _TextEncodings.ContainsKey(value) Then
                Return _TextEncodings(value)
            Else
                Return Encoding.UTF8
            End If
        End Function

        ''' <summary>
        ''' 从字符串名称之中解析出编码格式的枚举值(名称的大小写不敏感)
        ''' </summary>
        ''' <param name="encoding">名称的大小写不敏感</param>
        ''' <param name="onFailure"></param>
        ''' <returns></returns>
        <Extension> Public Function ParseEncodingsName(encoding$, Optional onFailure As Encodings = Encodings.ASCII) As Encodings
            For Each key In TextEncodings.Keys
                If encoding.TextEquals(key.ToString) Then
                    Return key
                End If
            Next

            Return onFailure
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CodePage(encodingName$, Optional [default] As Encodings = Encodings.Default) As Encoding
            Return encodingName.ParseEncodingsName(onFailure:=[default]).CodePage
        End Function

        Public Function GetEncodings(value As Encoding) As Encodings
            Dim Name As String = value.ToString.Split("."c).Last

            Select Case Name
                Case NameOf(Encodings.ASCII) : Return Encodings.ASCII
                Case NameOf(Encodings.GB2312) : Return Encodings.GB2312
                Case NameOf(Encodings.Unicode) : Return Encodings.Unicode
                Case NameOf(Encodings.UTF32) : Return Encodings.UTF32
                Case NameOf(Encodings.UTF7) : Return Encodings.UTF7
                Case NameOf(Encodings.UTF8) : Return Encodings.UTF8
                Case NameOf(Encodings.Default) : Return Encodings.Default
                Case Else
                    Return Encodings.UTF8
            End Select
        End Function

        ''' <summary>
        ''' 有时候有些软件对文本的编码是有要求的，则可以使用这个函数进行文本编码的转换
        ''' 例如R程序默认是读取ASCII，而。NET的默认编码是UTF8，则可以使用这个函数将目标文本文件转换为ASCII编码的文本文件
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <param name="from"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TransEncoding(path$, encoding As Encodings, Optional from As Encoding = Nothing) As Boolean
            If Not path.FileExists Then
                Call "".SaveTo(path, encoding.CodePage)
            End If

            Dim tmp$ = If(from Is Nothing, IO.File.ReadAllText(path), IO.File.ReadAllText(path, from))
            Return tmp.SaveTo(path, encoding.CodePage)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CodeArray(chars As IEnumerable(Of Char)) As Integer()
            Return chars.Select(AddressOf AscW).ToArray
        End Function
    End Module
End Namespace
