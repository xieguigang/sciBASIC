#Region "Microsoft.VisualBasic::813c516d5a093dbc9b6a6e40cb8f72be, Microsoft.VisualBasic.Core\src\Language\Language\UnixBash\Shell\Text\Grep.vb"

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

    '     Module TextAPI
    ' 
    '         Properties: [in], f, grep, i, inv
    '                     iv, n, nv, v
    ' 
    '     Enum GrepOptions
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class GrepRegx
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: __grep, __out, Clone, ToString
    '         Operators: -, *, <<, <=, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.UnixBash

    Public Module TextAPI

        ''' <summary>
        ''' grep (global search regular expression(RE) and print out the line
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property grep As New GrepRegx

        ' Public ReadOnly Property a As GrepOptions = GrepOptions.a
        ' Public ReadOnly Property c As GrepOptions = GrepOptions.c

        Public ReadOnly Property i As GrepOptions = GrepOptions.i
        Public ReadOnly Property n As GrepOptions = GrepOptions.n
        Public ReadOnly Property v As GrepOptions = GrepOptions.v
        Public ReadOnly Property [in] As GrepOptions = i Or n
        Public ReadOnly Property inv As GrepOptions = [in] Or v
        Public ReadOnly Property iv As GrepOptions = i Or v
        Public ReadOnly Property nv As GrepOptions = n Or v

        ''' <summary>
        ''' Text source of the grep operation is a file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property f As Boolean = True
    End Module

    Public Enum GrepOptions As Integer
        null = 0
        ''' <summary>
        ''' 将 binary 文件以 text 文件的方式搜寻数据
        ''' </summary>
        a = 2
        ''' <summary>
        ''' 计算找到 '搜寻字符串' 的次数
        ''' </summary>
        c = 4
        ''' <summary>
        ''' 忽略大小写的不同，所以大小写视为相同
        ''' </summary>
        i = 8
        ''' <summary>
        ''' 顺便输出行号
        ''' </summary>
        n = 16
        ''' <summary>
        ''' 反向选择，亦即显示出没有 '搜寻字符串' 内容的那一行！
        ''' </summary>
        v = 32
    End Enum

    ''' <summary>
    ''' grep -acinv * "pattern" &lt;= "filename or text" |
    ''' grep -acinv * "pattern" &lt;&lt; "filename"
    ''' </summary>
    Public Class GrepRegx : Implements ICloneable

        Dim __opt As GrepOptions
        Dim __regx As String
        Dim __isFile As Boolean = False

        Sub New()
        End Sub

        Sub New(g As GrepRegx, opt As GrepOptions)
            __regx = g.__regx
            __opt = opt
            __isFile = g.__isFile
        End Sub

        Sub New(g As GrepRegx, pattern As String)
            __regx = pattern
            __opt = g.__opt
            __isFile = g.__isFile
        End Sub

        Sub New(g As GrepRegx, f As Boolean)
            __regx = g.__regx
            __opt = g.__opt
            __isFile = f
        End Sub

        Public Overrides Function ToString() As String
            Return New With {
                __opt,
                __regx,
                __isFile
            }.GetJson
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New GrepRegx With {
                .__opt = __opt,
                .__regx = __regx
            }
        End Function

        Public Shared Operator -(grep As GrepRegx, opt As GrepOptions) As GrepRegx
            Return New GrepRegx(grep, opt)
        End Operator

        Public Shared Operator *(grep As GrepRegx, pattern As String) As GrepRegx
            Return New GrepRegx(grep, pattern)
        End Operator

        Private Iterator Function __grep(source As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim options As RegexOptions = RegexOptions.Singleline
            Dim reversed As Boolean = __opt.HasFlag(GrepOptions.v)
            Dim i As Boolean = __opt.HasFlag(GrepOptions.n)

            If __opt.HasFlag(GrepOptions.i) Then
                options = options Or RegexOptions.IgnoreCase
            End If

            For Each line As SeqValue(Of String) In source.SeqIterator
                If Regex.Match(__regx, line.value, options).Success Then
                    If Not reversed Then
                        Yield __out(line.value, i, line.i)
                    End If
                Else
                    If reversed Then
                        Yield __out(line.value, i, line.i)
                    End If
                End If
            Next
        End Function

        Private Shared Function __out(s As String, n As Boolean, i As Integer) As String
            If n Then
                Return $"{i}:{s}"
            Else
                Return s
            End If
        End Function

        Public Shared Operator <=(grep As GrepRegx, source As String) As IEnumerable(Of String)
            If grep.__isFile Then
                Return grep.__grep(source.IterateAllLines)
            Else
                Return grep.__grep(source.LineTokens)
            End If
        End Operator

        Public Shared Operator <<(grep As GrepRegx, file As Integer) As IEnumerable(Of String)
            Return grep.__grep(My.File.GetHandle(file).FileName.IterateAllLines)
        End Operator

        Public Shared Operator >=(grep As GrepRegx, source As String) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace
