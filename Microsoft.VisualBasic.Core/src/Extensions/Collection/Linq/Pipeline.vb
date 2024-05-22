#Region "Microsoft.VisualBasic::5cbadd7b88a66bbf48adbb098c5e4da6, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\Pipeline.vb"

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

    '   Total Lines: 42
    '    Code Lines: 23 (54.76%)
    ' Comment Lines: 14 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.90%)
    '     File Size: 1.43 KB


    '     Module PipelineExtensions
    ' 
    '         Function: DoCall, PipeOf
    ' 
    '         Sub: DoCall
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    <HideModuleName> Public Module PipelineExtensions

        ''' <summary>
        ''' Delegate pipeline function
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="Tout"></typeparam>
        ''' <param name="input"></param>
        ''' <param name="apply"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function DoCall(Of T, Tout)(input As T, apply As Func(Of T, Tout)) As Tout
            Return apply(input)
        End Function

        ''' <summary>
        ''' Delegate pipeline function
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="input"></param>
        ''' <param name="apply"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Sub DoCall(Of T)(input As T, apply As Action(Of T))
            Call apply(input)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function PipeOf(Of T, Rest)(input As T, task As Action(Of T, Rest)) As Action(Of Rest)
            Return Sub(a) task(input, a)
        End Function
    End Module
End Namespace
