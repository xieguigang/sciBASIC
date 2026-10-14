#Region "Microsoft.VisualBasic::2ea4761394bcb5eda8e71032ae5d831a, vs_solutions\JavaScript\Runtime\JsRuntimeException.vb"

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

    '   Total Lines: 50
    '    Code Lines: 30 (60.00%)
    ' Comment Lines: 10 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (20.00%)
    '     File Size: 2.06 KB


    '     Class JsRuntimeException
    ' 
    '         Properties: Frames, FullMessage, Stack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Describe, Error_, RangeError, ReferenceError, SyntaxError
    '                   TypeError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' Error object visible to JS try/catch; <see cref="Payload"/> is the thrown value.
    ''' <para>
    ''' Engine-raised errors (TypeError / ReferenceError / SyntaxError / RangeError)
    ''' carry a <see cref="JsError"/> payload so that script handlers can read
    ''' <c>e.name</c> and <c>e.message</c>, exactly like JavaScript. Values thrown by
    ''' the script itself (<c>throw x</c>) keep whatever payload the script threw.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsRuntimeException
        Inherits Exception

        Public ReadOnly Payload As Object

        ''' <summary>
        ''' JS stack frames frozen when this exception was created, innermost first.
        ''' <para>
        ''' Named <see cref="Stack"/> rather than <c>StackTrace</c> because
        ''' <see cref="Exception.StackTrace"/> already means the CLR trace.
        ''' </para>
        ''' </summary>
        Public ReadOnly Property Frames As List(Of JsStackFrame)

        Public Sub New(payload As Object)
            Me.New(payload, Nothing)
        End Sub

        ''' <summary>
        ''' <paramref name="frames"/> = Nothing captures the current JS stack; a
        ''' <see cref="JsError"/> payload already froze its own frames and they are reused
        ''' instead of capturing twice.
        ''' </summary>
        Public Sub New(payload As Object, frames As List(Of JsStackFrame))
            MyBase.New(Describe(payload))
            Me.Payload = payload

            Dim frozen = frames

            If frozen Is Nothing Then
                Dim jsError = TryCast(payload, JsError)

                If jsError IsNot Nothing Then frozen = jsError.Frames
            End If

            If frozen Is Nothing Then frozen = JsStackTrace.Capture()

            Me.Frames = frozen
        End Sub

        ''' <summary>
        ''' Node style stack text (message <c>"Name: message"</c> plus the JS frames); an
        ''' error payload keeps the frames captured where it was constructed.
        ''' </summary>
        Public ReadOnly Property Stack As String
            Get
                Dim jsError = TryCast(Payload, JsError)

                If jsError IsNot Nothing Then Return jsError.Stack

                Return Describe(Payload) & vbLf & JsStackTrace.Format(Frames)
            End Get
        End Property

        ''' <summary>Host facing alias of <see cref="Stack"/> for uncaught error reporting.</summary>
        Public ReadOnly Property FullMessage As String
            Get
                Return Stack
            End Get
        End Property

        Private Shared Function Describe(payload As Object) As String
            If TypeOf payload Is String Then Return CStr(payload)
            If TypeOf payload Is JsError Then Return DirectCast(payload, JsError).ToString()
            Return "JS runtime error"
        End Function

        ''' <summary>Build an engine error of the given name, carrying a JsError payload.</summary>
        Public Shared Function Error_(name As String, message As String) As JsRuntimeException
            Return New JsRuntimeException(New JsError(name, message))
        End Function

        Public Shared Function TypeError(message As String) As JsRuntimeException
            Return Error_("TypeError", message)
        End Function

        Public Shared Function ReferenceError(name As String) As JsRuntimeException
            Return Error_("ReferenceError", name & " is not defined")
        End Function

        Public Shared Function SyntaxError(message As String) As JsRuntimeException
            Return Error_("SyntaxError", message)
        End Function

        Public Shared Function RangeError(message As String) As JsRuntimeException
            Return Error_("RangeError", message)
        End Function
    End Class

End Namespace
