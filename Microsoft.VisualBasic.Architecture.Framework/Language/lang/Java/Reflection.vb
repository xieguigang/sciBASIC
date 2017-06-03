#Region "Microsoft.VisualBasic::20be5e5db69eaa44a3037b9727dc487a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\lang\Java\Reflection.vb"

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

Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Language.Java

    Public Module Reflection

        ''' <summary>
        ''' Gets the default class constructor
        ''' 
        ''' ```vbnet
        ''' Dim o As New &lt;<paramref name="type"/>>()
        ''' ```
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetConstructor(type As Type) As ConstructorInfo
            Return type.GetConstructor(
                BindingFlags.Public Or BindingFlags.Instance,
                Nothing, {}, Nothing)
        End Function

        <Extension>
        Public Function NewInstance(ctr As ConstructorInfo) As Object
            Return ctr.Invoke(BindingFlags.Public Or BindingFlags.Instance, Nothing, {}, CultureInfo.CurrentCulture)
        End Function
    End Module
End Namespace
