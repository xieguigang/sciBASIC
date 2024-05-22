#Region "Microsoft.VisualBasic::20be5e5db69eaa44a3037b9727dc487a, Microsoft.VisualBasic.Core\src\Language\Language\Java\Reflection.vb"

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

    '   Total Lines: 31
    '    Code Lines: 18 (58.06%)
    ' Comment Lines: 9 (29.03%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 4 (12.90%)
    '     File Size: 986 B


    '     Module Reflection
    ' 
    '         Function: GetConstructor, NewInstance
    ' 
    ' 
    ' /********************************************************************************/

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
