Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\ProtoToken.vb
' /********************************************************************************/

Namespace Schema

    ''' <summary>
    ''' .proto 词法单元的类型。
    ''' </summary>
    Friend Enum ProtoTokenType

        ''' <summary>
        ''' 标识符（可能包含点号，例如 <c>google.protobuf.Timestamp</c>）。
        ''' </summary>
        Identifier

        ''' <summary>
        ''' 数值字面量（十进制 / 十六进制 / 浮点）。
        ''' </summary>
        Number

        ''' <summary>
        ''' 已解除转义的字符串字面量。
        ''' </summary>
        StringLiteral

        ''' <summary>
        ''' 单字符符号，例如 <c>{ } [ ] = ; , &lt; &gt;</c>。
        ''' </summary>
        Symbol

        ''' <summary>
        ''' 输入结束。
        ''' </summary>
        EndOfFile

    End Enum

    ''' <summary>
    ''' .proto 词法单元。
    ''' </summary>
    Friend Class ProtoToken

        Public ReadOnly Property Type As ProtoTokenType

        Public ReadOnly Property Text As String

        Public ReadOnly Property Line As Integer

        Public ReadOnly Property Column As Integer

        Public Sub New(type As ProtoTokenType, text As String, line As Integer, column As Integer)
            Me.Type = type
            Me.Text = If(text, "")
            Me.Line = line
            Me.Column = column
        End Sub

        Public Function IsSymbol(symbol As String) As Boolean
            Return Type = ProtoTokenType.Symbol AndAlso String.Equals(Text, symbol, StringComparison.Ordinal)
        End Function

        Public Function IsKeyword(keyword As String) As Boolean
            Return Type = ProtoTokenType.Identifier AndAlso String.Equals(Text, keyword, StringComparison.Ordinal)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Type}('{Text}') @{Line}:{Column}"
        End Function

    End Class

    ''' <summary>
    ''' .proto 解析失败时抛出的异常，携带源文件名与行列号。
    ''' </summary>
    Public Class ProtoParseException : Inherits FormatException

        ''' <summary>
        ''' 源文件名；从字符串解析时为 Nothing。
        ''' </summary>
        Public ReadOnly Property SourceFile As String

        ''' <summary>
        ''' 出错行号（从 1 开始）。
        ''' </summary>
        Public ReadOnly Property Line As Integer

        ''' <summary>
        ''' 出错列号（从 1 开始）。
        ''' </summary>
        Public ReadOnly Property Column As Integer

        Public Sub New(message As String, sourceFile As String, line As Integer, column As Integer)
            MyBase.New($"{If(sourceFile, "<memory>")}({line},{column}): {message}")
            Me.SourceFile = sourceFile
            Me.Line = line
            Me.Column = column
        End Sub

    End Class

End Namespace
