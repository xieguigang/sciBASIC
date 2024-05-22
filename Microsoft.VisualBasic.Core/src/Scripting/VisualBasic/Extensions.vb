#Region "Microsoft.VisualBasic::ad9e9b967f360b9d54e24c1554d24870, Microsoft.VisualBasic.Core\src\Scripting\VisualBasic\Extensions.vb"

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

    '   Total Lines: 44
    '    Code Lines: 22 (50.00%)
    ' Comment Lines: 17 (38.64%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 5 (11.36%)
    '     File Size: 1.76 KB


    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) AsVBIdentifier, IsValidVBSymbolName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports r = System.Text.RegularExpressions.Regex

Namespace Scripting.SymbolBuilder.VBLanguage

    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' 这个拓展函数将字典之中的字符串主键处理为符合VB的对象命名规则的字符串
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVBIdentifier(Of T)(table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Return table.ToDictionary(Function(map) AsVBIdentifier(map.Key), Function(map) map.Value)
        End Function

        ''' <summary>
        ''' Normalize the input text token as a valid VisualBasic identifier
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVBIdentifier(key As String) As String
            Return key.NormalizePathString(alphabetOnly:=True).Replace(" ", "_")
        End Function

        ''' <summary>
        ''' the given string is a valid VB identifier symbol?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsValidVBSymbolName(name As String) As Boolean
            Static symbolPattern As New r(Patterns.Identifer, RegexICSng)
            Return symbolPattern.Match(name).Value = name
        End Function
    End Module
End Namespace
