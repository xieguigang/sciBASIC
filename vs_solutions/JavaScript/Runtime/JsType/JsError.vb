#Region "Microsoft.VisualBasic::318c9bf7afc65e3c9c26cd6a72705b39, vs_solutions\JavaScript\Runtime\JsError.vb"

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

    '   Total Lines: 36
    '    Code Lines: 16 (44.44%)
    ' Comment Lines: 13 (36.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.37 KB
    ' 
    ' 
    '     Class JsError
    ' 
    '         Properties: Frames, Message, Name, Original, Stack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Capture, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' JavaScript Error object: <c>name</c> plus <c>message</c>, and — like Node — the
    ''' JS stack frozen at the moment the error was constructed.
    ''' <para>
    ''' Engine-raised runtime errors carry one of these as their payload, so script
    ''' <c>catch</c> handlers can read <c>e.message</c> / <c>e.name</c> / <c>e.stack</c>
    ''' exactly like in JavaScript; <c>String(e)</c> renders as <c>"Name: message"</c>
    ''' (unchanged), while <c>e.stack</c> adds the frames.
    ''' </para>
    ''' <para>
    ''' <see cref="Frames"/> is captured eagerly in the constructor: the stack must be
    ''' frozen at the construction site, because by the time a handler reads
    ''' <c>e.stack</c> the interpreter has already unwound.
    ''' </para>
    ''' </summary>
    Public Class JsError

        Public ReadOnly Name As String
        Public ReadOnly Message As String

        ''' <summary>
        ''' The .NET exception this error was raised from (CLR bridge calls), or Nothing
        ''' for engine-raised errors. <see cref="ClrInterop.ToHostException"/> hands it
        ''' back to host code, so a script rethrow keeps the original exception type.
        ''' </summary>
        Public ReadOnly Original As Exception

        ''' <summary>
        ''' JS stack frames frozen at construction time, innermost first
        ''' (see <see cref="JsStackTrace.Capture"/>).
        ''' </summary>
        Public ReadOnly Property Frames As List(Of JsStackFrame)

        Public Sub New(name As String, Optional message As String = "", Optional original As Exception = Nothing)
            Me.New(name, message, original, Nothing)
        End Sub

        ''' <summary>
        ''' <paramref name="frames"/> = Nothing captures the current JS stack; pass an
        ''' existing list to share the frames of a wrapping error.
        ''' </summary>
        Public Sub New(name As String, message As String, original As Exception, frames As List(Of JsStackFrame))
            Me.Name = name
            Me.Message = If(message, "")
            Me.Original = original
            Me.Frames = If(frames, JsStackTrace.Capture())
        End Sub

        ''' <summary>
        ''' Build an error that carries the JS stack of this very moment — the JS
        ''' <c>new Error(message)</c> semantics.
        ''' </summary>
        Public Shared Function Capture(name As String,
                                      Optional message As String = "",
                                      Optional original As Exception = Nothing) As JsError

            Return New JsError(name, message, original)
        End Function

        ''' <summary>
        ''' Node style stack text: <c>"Name: message"</c> followed by the frames
        ''' (<c>    at fn (file:line:col)</c>).
        ''' </summary>
        Public ReadOnly Property Stack As String
            Get
                Return JsStackTrace.FormatWithHeader(Name, Message, Frames)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Message Is Nothing OrElse Message.Length = 0 Then Return Name
            Return Name & ": " & Message
        End Function

    End Class

End Namespace
