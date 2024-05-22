#Region "Microsoft.VisualBasic::e8a127a284fc1e9731a45f1ed82cfa5f, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\ExceptionData.vb"

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

    '   Total Lines: 90
    '    Code Lines: 66 (73.33%)
    ' Comment Lines: 12 (13.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (13.33%)
    '     File Size: 3.31 KB


    '     Class ExceptionData
    ' 
    '         Properties: Message, Source, StackTrace, TypeFullName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateFromObject, CreateInstance, Exception, GetCurrentStackTrace, getMessages
    '                   ParseStackTrace, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Debugging.Diagnostics

#Disable Warning BC40000 ' Type or member is obsolete
    <ClassInterface(ClassInterfaceType.AutoDual)>
    <ComVisible(True)>
    Public Class ExceptionData
#Enable Warning BC40000 ' Type or member is obsolete

        Public Property TypeFullName As String
        Public Property Message As String()
        ''' <summary>
        ''' the source expression which trigger this error
        ''' </summary>
        ''' <returns></returns>
        Public Property Source As String
        Public Property StackTrace As StackFrame()

        Sub New()
        End Sub

        ''' <summary>
        ''' Create exception object for throw exception expression
        ''' </summary>
        ''' <returns></returns>
        Public Function Exception() As Exception
            Return New Exception(Message.Select(Function(msg, i) $"{i}. {msg}").JoinBy(ASCII.LF)) With {
                .Source = StackTrace.JoinBy(ASCII.LF)
            }
        End Function

        Public Overrides Function ToString() As String
            Return TypeFullName
        End Function

        Private Shared Iterator Function getMessages(ex As Exception) As IEnumerable(Of String)
            Do While True
                Yield ex.Message

                If Not ex.InnerException Is Nothing Then
                    ex = ex.InnerException
                Else
                    Exit Do
                End If
            Loop
        End Function

        Public Shared Function CreateFromObject(ex As Exception) As ExceptionData
            Return New ExceptionData With {
                .Message = getMessages(ex).ToArray,
                .TypeFullName = ex.GetType.FullName,
                .StackTrace = ParseStackTrace(ex.StackTrace)
            }
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

        Public Shared Function ParseStackTrace(stackTrace As String) As StackFrame()
            Return stackTrace _
                .LineTokens _
                .Where(Function(s) Not s.StringEmpty) _
                .Where(Function(s)
                           Return s.Trim.StartsWith("在") OrElse s.Trim.ToLower.StartsWith("at")
                       End Function) _
                .Select(Function(s)
                            s = Mid(s, 6).Trim
                            Return StackFrame.Parser(s)
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Parsing <see cref="Environment.StackTrace"/>, gets current stack trace information.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetCurrentStackTrace() As StackFrame()
            Return ParseStackTrace(Environment.StackTrace)
        End Function
    End Class
End Namespace
