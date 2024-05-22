#Region "Microsoft.VisualBasic::760f40fe4f1ed74133d713b3ee081bf1, Microsoft.VisualBasic.Core\src\Scripting\ScriptBuilder.vb"

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

    '   Total Lines: 155
    '    Code Lines: 81 (52.26%)
    ' Comment Lines: 54 (34.84%)
    '    - Xml Docs: 92.59%
    ' 
    '   Blank Lines: 20 (12.90%)
    '     File Size: 5.74 KB


    '     Class ScriptBuilder
    ' 
    '         Properties: Preview, script
    ' 
    '         Constructor: (+6 Overloads) Sub New
    '         Function: AppendLine, Replace, (+2 Overloads) Save, ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text

Namespace Scripting.SymbolBuilder

    ''' <summary>
    ''' 对<see cref="StringBuilder"/>对象的拓展，添加了操作符凭借字符串，从而能够让生成代码的操作更加的方便
    ''' </summary>
    Public Class ScriptBuilder : Implements ISaveHandle

        Public ReadOnly Property script As StringBuilder

        ''' <summary>
        ''' The variable in target script text should be in format like: ``{$name}``
        ''' </summary>
        ''' <param name="name"></param>
        Default Public WriteOnly Property Assign(name As String) As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                Dim key As String = $"{{${name}}}"
                ' do generate key for string replacement and then
                ' do string replacement
                Call script.Replace(key, Value)
            End Set
        End Property

        ''' <summary>
        ''' Equals to <see cref="StringBuilder.ToString()"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Preview As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return script.ToString
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(sb As StringBuilder)
            script = sb
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(capacity As Integer)
            script = New StringBuilder(capacity)
        End Sub

        Sub New(lines As IEnumerable(Of String))
            script = New StringBuilder(lines.JoinBy(vbCrLf))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New()
            Call Me.New(capacity:=1024)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(script$)
            Call Me.New(New StringBuilder(script))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(html As XElement)
            Call Me.New(html.ToString)
        End Sub

        ''' <summary>
        ''' <see cref="StringBuilder.Replace(String, String)"/>
        ''' </summary>
        ''' <param name="key$"></param>
        ''' <param name="value$"></param>
        ''' <returns></returns>
        Public Function Replace(key$, value$) As ScriptBuilder
            Call script.Replace(key, value)
            Return Me
        End Function

        ''' <summary>
        ''' Display the string text in the <see cref="StringBuilder"/> object.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return script.ToString
        End Function

        ''' <summary>
        ''' Appends a copy of the specified string followed by the default line terminator
        ''' to the end of the current <see cref="ScriptBuilder"/> object.
        ''' </summary>
        ''' <param name="line$">The string to append.</param>
        ''' <returns>A reference to this instance after the append operation has completed.</returns>
        Public Function AppendLine(Optional line$ = "") As ScriptBuilder
            Call script.AppendLine(line)
            Return Me
        End Function

        ''' <summary>
        ''' <see cref="StringBuilder.ToString()"/>
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(sb As ScriptBuilder) As String
            Return sb.script.ToString
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(script As String) As ScriptBuilder
            Return New ScriptBuilder(script)
        End Operator

        ''' <summary>
        ''' <see cref="StringBuilder.Append"/>
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Operator &(sb As ScriptBuilder, s As String) As ScriptBuilder
            Call sb.script.Append(s)
            Return sb
        End Operator

        ''' <summary>
        ''' <see cref="StringBuilder.AppendLine"/>
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Operator +(sb As ScriptBuilder, s As String) As ScriptBuilder
            Call sb.script.AppendLine(s)
            Return sb
        End Operator

        ''' <summary>
        ''' save value from <see cref="ToString()"/>
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Return script.ToString.SaveTo(path, encoding)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return script.ToString.SaveTo(path, encoding.CodePage)
        End Function
    End Class
End Namespace
