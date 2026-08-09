#Region "Microsoft.VisualBasic::58a444b6720fde3adb5d0e4fa644333c, nlp\NLP\Tokenizer\src\HuggingFace\ByteLevelAlphabet.vb"

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

    '   Total Lines: 145
    '    Code Lines: 70 (48.28%)
    ' Comment Lines: 53 (36.55%)
    '    - Xml Docs: 92.45%
    ' 
    '   Blank Lines: 22 (15.17%)
    '     File Size: 5.66 KB


    '     Module ByteLevelAlphabet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DecodeToBytes, DecodeToString, EncodeBytes, GetChar, TryGetByte
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
' the sciBASIC framework declares its own 'Encoding' symbol in the global
' namespace, so an explicit alias is required here to reference the BCL type.
Imports TextEncoding = System.Text.Encoding

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' GPT-2 的 <c>bytes_to_unicode</c> 字节-字符双向映射表。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' ByteLevel 系列的分词器（GPT-2 / RoBERTa / DeepSeek 等）并不会直接把原始字节
    ''' 写进词表，而是先把 256 个字节值一一映射到<b>可打印的 Unicode 字符</b>上，
    ''' 从而使得词表文件本身是可读的纯文本。最典型的表现就是空格被映射为 <c>Ġ</c>，
    ''' 换行被映射为 <c>Ċ</c>，而中文这类多字节字符则会呈现为 <c>å±ĭ</c> 之类的乱码形态。
    ''' </para>
    ''' <para>
    ''' 映射规则为：可打印的 ASCII 区间（<c>!</c>~<c>~</c>）以及 Latin-1 的两段可打印
    ''' 区间（<c>¡</c>~<c>¬</c>、<c>®</c>~<c>ÿ</c>）保持原值；其余 68 个不可打印字节
    ''' 依次映射到 <c>U+0100</c> 开始的私有区段。
    ''' </para>
    ''' <para>
    ''' 该映射是与 python 端输出保持一致的<b>首要决定性因素</b>，因此这里以静态只读
    ''' 查找表的形式实现，全局构造一次之后复用。
    ''' </para>
    ''' </remarks>
    Public Module ByteLevelAlphabet

        ''' <summary>
        ''' 字节值到映射字符的查找表，下标即为字节值。
        ''' </summary>
        Private ReadOnly byteToChar As Char()
        ''' <summary>
        ''' 映射字符到字节值的反查表。
        ''' </summary>
        Private ReadOnly charToByte As Dictionary(Of Char, Byte)

        Sub New()
            byteToChar = New Char(255) {}
            charToByte = New Dictionary(Of Char, Byte)(256)

            Dim assigned As Boolean() = New Boolean(255) {}
            Dim n As Integer = 0

            ' the printable ranges are mapped onto themselves
            For Each range As (from As Integer, [to] As Integer) In {
                (AscW("!"c), AscW("~"c)),
                (&HA1, &HAC),
                (&HAE, &HFF)
            }
                For b As Integer = range.from To range.to
                    byteToChar(b) = ChrW(b)
                    assigned(b) = True
                Next
            Next

            ' the remaining non printable bytes are shifted into the U+0100 area
            For b As Integer = 0 To 255
                If Not assigned(b) Then
                    byteToChar(b) = ChrW(256 + n)
                    n += 1
                End If
            Next

            For b As Integer = 0 To 255
                charToByte(byteToChar(b)) = CByte(b)
            Next
        End Sub

        ''' <summary>
        ''' 获取指定字节值所对应的映射字符。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetChar(b As Byte) As Char
            Return byteToChar(b)
        End Function

        ''' <summary>
        ''' 获取指定映射字符所对应的字节值。
        ''' </summary>
        ''' <returns>当该字符不属于映射表时返回 <see langword="False"/>。</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TryGetByte(c As Char, ByRef b As Byte) As Boolean
            Return charToByte.TryGetValue(c, b)
        End Function

        ''' <summary>
        ''' 把文本按 UTF-8 编码为字节序列，再逐字节映射为可见字符串。
        ''' </summary>
        ''' <remarks>
        ''' 这是编码方向的入口：映射之后得到的字符串才可以拿去查询词表。
        ''' </remarks>
        Public Function EncodeBytes(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return String.Empty
            End If

            Dim bytes As Byte() = TextEncoding.UTF8.GetBytes(text)
            Dim buffer As Char() = New Char(bytes.Length - 1) {}

            For i As Integer = 0 To bytes.Length - 1
                buffer(i) = byteToChar(bytes(i))
            Next

            Return New String(buffer)
        End Function

        ''' <summary>
        ''' 把映射字符串还原为原始的字节序列。
        ''' </summary>
        ''' <remarks>
        ''' 无法识别的字符（例如未被 ByteLevel 处理过的特殊 token）会按 UTF-8
        ''' 原样写回，从而保证解码过程不会因为混入特殊 token 而丢失内容。
        ''' </remarks>
        Public Function DecodeToBytes(token As String) As Byte()
            If String.IsNullOrEmpty(token) Then
                Return New Byte() {}
            End If

            Dim buffer As New List(Of Byte)(token.Length)

            For Each c As Char In token
                Dim b As Byte

                If charToByte.TryGetValue(c, b) Then
                    buffer.Add(b)
                Else
                    buffer.AddRange(TextEncoding.UTF8.GetBytes(c.ToString()))
                End If
            Next

            Return buffer.ToArray()
        End Function

        ''' <summary>
        ''' 把映射字符串还原为原始文本。
        ''' </summary>
        Public Function DecodeToString(token As String) As String
            Return TextEncoding.UTF8.GetString(DecodeToBytes(token))
        End Function

    End Module

End Namespace
