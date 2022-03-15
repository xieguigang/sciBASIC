#Region "Microsoft.VisualBasic::82ecd271dd553aa30f81cc8c154dd810, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\StackFrame.vb"

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

    '   Total Lines: 91
    '    Code Lines: 62
    ' Comment Lines: 17
    '   Blank Lines: 12
    '     File Size: 2.93 KB


    '     Class StackFrame
    ' 
    '         Properties: File, Line, Method
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FromUnknownLocation, Parser, parserImpl, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Debugging.Diagnostics

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

        <DebuggerStepThrough>
        Sub New()
        End Sub

        ''' <summary>
        ''' make value copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As StackFrame)
            File = clone.File
            Line = clone.Line
            Method = New Method With {
                .Method = clone.Method.Method,
                .[Module] = clone.Method.Module,
                .[Namespace] = clone.Method.Namespace
            }
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Method} at {File}:line {Line}"
        End Function

        Public Shared Function Parser(line As String) As StackFrame
            With line.Replace("位置", "in").Replace("行号", "line")
                Return .StringSplit(" in ").DoCall(AddressOf parserImpl)
            End With
        End Function

        Private Shared Function parserImpl(t As String()) As StackFrame
            Dim method As String = t(0)
            Dim location As String = t.ElementAtOrDefault(1)
            Dim file$, lineNumber$

            If Not location.StringEmpty Then
                t = location.StringSplit("[:]line ")

                If t.Length = 1 Then
                    ' on mono environment
                    file = "Unknown"
                    lineNumber = 0
                Else
                    file = t(0)
                    lineNumber = t(1)
                End If
            Else
                file = "Unknown"
                lineNumber = 0
            End If

            Return New StackFrame With {
                .Method = New Method(method),
                .File = file.Replace("\", "/"), ' fix for BSON string storage
                .Line = lineNumber
            }
        End Function

        Public Shared Function FromUnknownLocation(stackName As String) As StackFrame
            Return New StackFrame With {
                .File = "n/a",
                .Line = "n/a",
                .Method = New Method With {
                    .Method = stackName,
                    .[Module] = "unknown",
                    .[Namespace] = "unknown"
                }
            }
        End Function
    End Class
End Namespace
