#Region "Microsoft.VisualBasic::df5acd57da01f9dfc0f1fefdfc405ef0, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Scripting\Runtime\CType\CastStringVector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Scripting.Runtime

    Public Module CastStringVector

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(values As Dictionary(Of String, Double)) As Dictionary(Of String, String)
            Return values.ToDictionary(
                Function(x) x.Key,
                Function(x) CStr(x.Value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(values As Dictionary(Of String, String)) As Dictionary(Of String, Double)
            Return values.ToDictionary(
                Function(x) x.Key,
                Function(x) x.Value.ParseNumeric)
        End Function

        <Extension> Public Function AsType(Of T)(source As IEnumerable(Of String)) As IEnumerable(Of T)
            Dim type As Type = GetType(T)
            Dim [ctype] = InputHandler.CasterString(type)
            Dim result = source.Select(Function(x) DirectCast([ctype](x), T))
            Return result
        End Function

        <Extension>
        Public Function AsDouble(source As IEnumerable(Of String)) As Double()
            Return source.AsType(Of Double).ToArray
        End Function

        <Extension>
        Public Function AsNumeric(source As IEnumerable(Of String)) As Double()
            Return source _
                .Select(Function(s) s.ParseNumeric) _
                .ToArray
        End Function

        <Extension>
        Public Function AsDouble(singles As IEnumerable(Of Single)) As Double()
            Return singles _
                .SafeQuery _
                .Select(Function(s) CDbl(s)) _
                .ToArray
        End Function

        <Extension>
        Public Function AsSingle(source As IEnumerable(Of String)) As Single()
            Return source.AsType(Of Single).ToArray
        End Function

        <Extension>
        Public Function AsBoolean(source As IEnumerable(Of String)) As Boolean()
            Return source.AsType(Of Boolean).ToArray
        End Function

        <Extension>
        Public Function AsInteger(source As IEnumerable(Of String)) As Integer()
            Return source.AsType(Of Integer).ToArray
        End Function

        <Extension>
        Public Function AsColor(source As IEnumerable(Of String)) As Color()
            Return source.AsType(Of Color).ToArray
        End Function
    End Module
End Namespace
