#Region "Microsoft.VisualBasic::a2a9f627ada43b81f3631c70fde96537, Microsoft.VisualBasic.Core\src\Language\Linq\LanguageExtensions.vb"

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

    '   Total Lines: 65
    '    Code Lines: 41
    ' Comment Lines: 17
    '   Blank Lines: 7
    '     File Size: 2.19 KB


    '     Module LanguageExtensions
    ' 
    '         Sub: (+5 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Language

    ''' <summary>
    ''' <see cref="List(Of T)"/> initizlize syntax supports
    ''' </summary>
    Public Module LanguageExtensions

        ''' <summary>
        ''' New List From syntax supports
        ''' 
        ''' ```
        ''' {Name, value, Description?}
        ''' ```
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(Of T)(list As List(Of NamedValue(Of T)), name$, value As T, Optional descript$ = Nothing)
            list += New NamedValue(Of T) With {
                .Name = name,
                .Value = value,
                .Description = descript
            }
        End Sub

        ''' <summary>
        ''' From {"1,100", "100,1000", "200,500"}
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="range$"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(list As List(Of IntRange), range$)
            list += range
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(list As List(Of DoubleRange), range$)
            list += DoubleRange.TryParse(range)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Add(list As List(Of NamedValue), name$, value$)
            list += New NamedValue With {
                .name = name,
                .text = value
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Add(list As List(Of [Property]), name$, value$, Optional comment$ = Nothing)
            list += New [Property] With {
                .name = name,
                .value = value,
                .Comment = comment
            }
        End Sub
    End Module
End Namespace
