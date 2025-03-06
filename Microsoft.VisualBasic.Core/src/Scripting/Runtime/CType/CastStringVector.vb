#Region "Microsoft.VisualBasic::5679059c5b234b04b449fb300f468d89, Microsoft.VisualBasic.Core\src\Scripting\Runtime\CType\CastStringVector.vb"

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

    '   Total Lines: 204
    '    Code Lines: 96 (47.06%)
    ' Comment Lines: 90 (44.12%)
    '    - Xml Docs: 96.67%
    ' 
    '   Blank Lines: 18 (8.82%)
    '     File Size: 8.34 KB


    '     Module CastStringVector
    ' 
    '         Function: AsBoolean, (+4 Overloads) AsCharacter, AsColor, AsDateTime, (+3 Overloads) AsDouble
    '                   AsGeneric, AsInteger, (+2 Overloads) AsNumeric, AsSingle, AsType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Scripting.Runtime

    Public Module CastStringVector

        ''' <summary>
        ''' Convert the numeric <see cref="Double"/> type as the <see cref="String"/> text type.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(values As Dictionary(Of String, Double)) As Dictionary(Of String, String)
            Return values.ToDictionary(Function(x) x.Key, Function(x) CStr(x.Value))
        End Function

        ''' <summary>
        ''' Convert the numeric <see cref="Object"/> type as the <see cref="String"/> text type by <see cref="InputHandler.ToString"/>.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(values As Dictionary(Of String, Object), Optional null$ = Nothing) As Dictionary(Of String, String)
            Return values.ToDictionary(Function(x) x.Key, Function(x) Scripting.ToString(x.Value, null))
        End Function

        ''' <summary>
        ''' Cats the given numeric vector as character string.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <param name="negPrefix">add prefix to the positive value for make align with the negative symbol, 
        ''' example as if this parameter is set to true:
        ''' 
        ''' ```
        '''  256.333
        ''' -111.112  
        ''' ```
        ''' 
        ''' the positive number ``256.333`` has a white space prefix for make aligned with the negative 
        ''' value ``-111.112``.
        ''' </param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(values As IEnumerable(Of Double), Optional negPrefix As Boolean = False, Optional format$ = "G4") As IEnumerable(Of String)
            Return values _
                .SafeQuery _
                .Select(Function(d)
                            If d > 0 AndAlso negPrefix Then
                                Return " " & d.ToString(format)
                            Else
                                Return d.ToString(format)
                            End If
                        End Function)
        End Function

        ''' <summary>
        ''' 使用<see cref="Scripting.ToString"/>方法将对象集合转换为字符串序列
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="values"></param>
        ''' <param name="null$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(Of T)(values As IEnumerable(Of T), Optional null$ = "") As IEnumerable(Of String)
            Return values.SafeQuery.Select(Function(o) Scripting.ToString(o, null))
        End Function

        ''' <summary>
        ''' Convert the <see cref="String"/> value as <see cref="Double"/> numeric type.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(values As Dictionary(Of String, String)) As Dictionary(Of String, Double)
            Return values.ToDictionary(Function(x) x.Key, Function(x) x.Value.ParseNumeric)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(values As Dictionary(Of String, Integer)) As Dictionary(Of String, Double)
            Return values.ToDictionary(Function(x) x.Key, Function(x) CDbl(x.Value))
        End Function

        ''' <summary>
        ''' 将字典之中的值转换为<see cref="Object"/>类型
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsGeneric(Of T)(values As Dictionary(Of String, T)) As Dictionary(Of String, Object)
            Return values.ToDictionary(Function(x) x.Key, Function(x) CObj(x.Value))
        End Function

        ''' <summary>
        ''' string array parser helper. 
        ''' </summary>
        ''' <typeparam name="T">get parser for current type from <see cref="InputHandler.CasterString"/>.</typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 批量的将一个字符串集合解析转换为目标类型<typeparamref name="T"/>的对象的集合
        ''' </remarks>
        <Extension>
        Public Function AsType(Of T)(source As IEnumerable(Of String)) As IEnumerable(Of T)
            Dim type As Type = GetType(T)
            Dim [ctype] As LoadObject = InputHandler.CasterString(type)
            Dim result = source.Select(Function(x) DirectCast([ctype](x), T))
            Return result
        End Function

        ''' <summary>
        ''' 将字符串集合转换为一个数值向量
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsDouble(source As IEnumerable(Of String)) As Double()
            Return source.AsType(Of Double).ToArray
        End Function

        ''' <summary>
        ''' cast single float to double in batch
        ''' </summary>
        ''' <param name="singles"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsDouble(singles As IEnumerable(Of Single)) As Double()
            Return singles _
                .SafeQuery _
                .Select(Function(s) CDbl(s)) _
                .ToArray
        End Function

        ''' <summary>
        ''' cast integer to double in batch
        ''' </summary>
        ''' <param name="singles"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsDouble(singles As IEnumerable(Of Integer)) As Double()
            Return singles _
                .SafeQuery _
                .Select(Function(s) CDbl(s)) _
                .ToArray
        End Function

        ''' <summary>
        ''' parse string as float number in batch mode
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsSingle(source As IEnumerable(Of String)) As Single()
            Return source.AsType(Of Single).ToArray
        End Function

        ''' <summary>
        ''' parse string as boolean in batch mode
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsBoolean(source As IEnumerable(Of String)) As Boolean()
            Return source.AsType(Of Boolean).ToArray
        End Function

        ''' <summary>
        ''' parse string as integer in batch mode
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsInteger(source As IEnumerable(Of String)) As Integer()
            Return source.AsType(Of Integer).ToArray
        End Function

        ''' <summary>
        ''' parse string as gdi+ color object in batch mode
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AsColor(source As IEnumerable(Of String)) As Color()
            Return source.AsType(Of Color).ToArray
        End Function

        <Extension>
        Public Function AsDateTime(source As IEnumerable(Of String)) As Date()
            Return source.AsType(Of Date).ToArray
        End Function
    End Module
End Namespace
