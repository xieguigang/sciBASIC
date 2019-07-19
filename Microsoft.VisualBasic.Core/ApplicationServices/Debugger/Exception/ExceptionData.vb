#Region "Microsoft.VisualBasic::679f37232d839b0731209665afe32b3b, ApplicationServices\Debugger\Exception\ExceptionData.vb"

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

    '     Class ExceptionData
    ' 
    '         Properties: Message, StackTrace, TypeFullName
    ' 
    '         Function: CreateInstance, GetCurrentStackTrace, ToString
    ' 
    '     Class StackFrame
    ' 
    '         Properties: File, Line, Method
    ' 
    '         Function: Parser, ToString
    ' 
    '     Class Method
    ' 
    '         Properties: [Module], [Namespace], Method
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ExceptionData

        Public Property TypeFullName As String
        Public Property Message As String()
        Public Property StackTrace As StackFrame()

        Public Overrides Function ToString() As String
            Return TypeFullName
        End Function

        Public Shared Function CreateInstance(messages$(), stackTrace$(), type$) As ExceptionData
            Return New ExceptionData With {
                .TypeFullName = type,
                .Message = messages,
                .StackTrace = stackTrace _
                    .Select(AddressOf StackFrame.Parser) _
                    .ToArray
            }
        End Function

        ''' <summary>
        ''' Parsing <see cref="Environment.StackTrace"/>, gets current stack trace information.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetCurrentStackTrace() As StackFrame()
            Return Environment.StackTrace _
                .LineTokens _
                .Where(Function(s) Not s.StringEmpty) _
                .Skip(3) _
                .Select(Function(s)
                            s = Mid(s, 6).Trim
                            Return StackFrame.Parser(s)
                        End Function) _
                .ToArray
        End Function
    End Class

    Public Class StackFrame

        ''' <summary>
        ''' Method call
        ''' </summary>
        ''' <returns></returns>
        Public Property Method As Method
        ''' <summary>
        ''' The file path of the source file
        ''' </summary>
        ''' <returns></returns>
        Public Property File As String
        ''' <summary>
        ''' The line number in current source <see cref="File"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Property Line As String

        Public Overrides Function ToString() As String
            Return $"{Method} at {File}:line {Line}"
        End Function

        Public Shared Function Parser(line As String) As StackFrame
            With line.Replace("位置", "at").Replace("行号", "line")
                Dim t = .StringSplit(" at ")
                Dim method = t(0)
                Dim location = t.ElementAtOrDefault(1)
                Dim file$, lineNumber$

                If Not location.StringEmpty Then
                    t = location.StringSplit("[:]line ")
                    file = t(0)
                    lineNumber = t(1)
                Else
                    file = "Unknown"
                    lineNumber = 0
                End If

                Return New StackFrame With {
                    .Method = New Method(method),
                    .File = file,
                    .Line = lineNumber
                }
            End With
        End Function
    End Class

    Public Class Method

        Public Property [Namespace] As String
        Public Property [Module] As String
        Public Property Method As String

        Sub New(s As String)
            Dim t = s.Split("."c).AsList

            Method = t(-1)
            [Module] = t(-2)
            [Namespace] = t.Take(t.Count - 2).JoinBy(".")
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[Namespace]}.{[Module]}.{Method}"
        End Function
    End Class
End Namespace
