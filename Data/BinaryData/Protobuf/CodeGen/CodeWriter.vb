Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\CodeGen\CodeWriter.vb
' /********************************************************************************/

Imports System.Text

Namespace CodeGen

    ''' <summary>
    ''' 带缩进栈的源码文本构建器。
    ''' </summary>
    ''' <remarks>
    ''' 统一使用 <see cref="Environment.NewLine"/> 作为换行符；空行不带缩进。
    ''' </remarks>
    Public Class CodeWriter

        Private ReadOnly _builder As New StringBuilder()
        Private _indent As Integer

        ''' <summary>
        ''' 单级缩进的空格数，默认 4。
        ''' </summary>
        Public Property IndentSize As Integer = 4

        ''' <summary>
        ''' 当前缩进层级。
        ''' </summary>
        Public ReadOnly Property IndentLevel As Integer
            Get
                Return _indent
            End Get
        End Property

        ''' <summary>
        ''' 增加一级缩进。
        ''' </summary>
        Public Sub Indent()
            _indent += 1
        End Sub

        ''' <summary>
        ''' 减少一级缩进（不会低于 0）。
        ''' </summary>
        Public Sub Dedent()
            If _indent > 0 Then
                _indent -= 1
            End If
        End Sub

        ''' <summary>
        ''' 写入一行文本；传入空字符串时写入空行。
        ''' </summary>
        Public Sub WriteLine(Optional text As String = "")
            If String.IsNullOrEmpty(text) Then
                Call _builder.Append(Environment.NewLine)
            Else
                Call _builder.Append(New String(" "c, _indent * IndentSize))
                Call _builder.Append(text)
                Call _builder.Append(Environment.NewLine)
            End If
        End Sub

        ''' <summary>
        ''' 批量写入多行。
        ''' </summary>
        Public Sub WriteLines(lines As IEnumerable(Of String))
            If lines Is Nothing Then
                Return
            End If

            For Each line As String In lines
                Call WriteLine(line)
            Next
        End Sub

        ''' <summary>
        ''' 写入一个空行。
        ''' </summary>
        Public Sub BlankLine()
            Call WriteLine("")
        End Sub

        Public Overrides Function ToString() As String
            Return _builder.ToString()
        End Function

    End Class

End Namespace
