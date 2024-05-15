#Region "Microsoft.VisualBasic::acb3f4c657693f3f83734bd80f7d14ff, Microsoft.VisualBasic.Core\src\Language\Linq\Assert\AssertEqualsExtensions.vb"

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

    '   Total Lines: 30
    '    Code Lines: 14
    ' Comment Lines: 12
    '   Blank Lines: 4
    '     File Size: 1.09 KB


    '     Module AssertEqualsExtensions
    ' 
    '         Function: All, Any
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace Language

    Public Module AssertEqualsExtensions

        ''' <summary>
        ''' Assert that all of the elements in target vector match the test conditions
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <Extension>
        Public Function All(Of T)(vector As Vector(Of T)) As AssertAll(Of T)
            Return New AssertAll(Of T)(vector, Function(x, y) x = y)
        End Function

        ''' <summary>
        ''' Assert that any of the elements in target vector match the test conditions
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Any(Of T)(vector As Vector(Of T)) As AssertAny(Of T)
            Return New AssertAny(Of T)(vector, Function(x, y) x = y)
        End Function
    End Module
End Namespace
