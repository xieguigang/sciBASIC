#Region "Microsoft.VisualBasic::794b4edb08ff634d8c164504a01c6ff0, Microsoft.VisualBasic.Core\src\Text\Xml\Models\ValueTuples\Extensions.vb"

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

    '   Total Lines: 19
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 829 B


    '     Module Extensions
    ' 
    '         Function: ToDictionary, ToProperties
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Text.Xml.Models

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToDictionary(Of T)(properties As IEnumerable(Of [Property]), parser As Func(Of String, T)) As Dictionary(Of String, T)
            Return properties.ToDictionary(Function(p) p.name, Function(p) parser(p.value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToProperties(Of T)(table As Dictionary(Of String, T), toString As Func(Of T, String)) As IEnumerable(Of [Property])
            Return table.Select(Function(p) New [Property] With {.name = p.Key, .value = toString(p.Value)})
        End Function
    End Module
End Namespace
