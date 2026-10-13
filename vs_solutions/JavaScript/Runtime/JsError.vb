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


    '     Class JsError
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' JavaScript Error object: <c>name</c> plus <c>message</c>.
    ''' <para>
    ''' Engine-raised runtime errors carry one of these as their payload, so script
    ''' <c>catch</c> handlers can read <c>e.message</c> / <c>e.name</c> exactly like
    ''' in JavaScript; <c>String(e)</c> renders as <c>"Name: message"</c>.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsError

        Public ReadOnly Name As String
        Public ReadOnly Message As String

        ''' <summary>
        ''' The .NET exception this error was raised from (CLR bridge calls), or Nothing
        ''' for engine-raised errors. <see cref="ClrInterop.ToHostException"/> hands it
        ''' back to host code, so a script rethrow keeps the original exception type.
        ''' </summary>
        Public ReadOnly Original As Exception

        Public Sub New(name As String, Optional message As String = "", Optional original As Exception = Nothing)
            Me.Name = name
            Me.Message = If(message, "")
            Me.Original = original
        End Sub

        Public Overrides Function ToString() As String
            If Message Is Nothing OrElse Message.Length = 0 Then Return Name
            Return Name & ": " & Message
        End Function

    End Class

End Namespace
