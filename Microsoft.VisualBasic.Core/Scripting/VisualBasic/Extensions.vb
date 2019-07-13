#Region "Microsoft.VisualBasic::831cd1364b87f86a7f5eeb3b6068327e, Microsoft.VisualBasic.Core\Scripting\VisualBasic\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) AsVBIdentifier
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Scripting.SymbolBuilder.VBLanguage

    <HideModuleName> Public Module Extensions

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
    End Module
End Namespace
