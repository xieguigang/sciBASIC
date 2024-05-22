#Region "Microsoft.VisualBasic::1f660384bd7e021798b05a8b297a717e, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Unit\Convertor.vb"

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

    '   Total Lines: 21
    '    Code Lines: 9 (42.86%)
    ' Comment Lines: 7 (33.33%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 5 (23.81%)
    '     File Size: 634 B


    '     Class Convertor
    ' 
    '         Properties: UnitType
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Ranges.Unit

    Public Class Convertor : Inherits Attribute

        Public ReadOnly Property UnitType As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="TUnit">枚举类型？</typeparam>
        ''' <param name="value"></param>
        ''' <param name="toUnit"></param>
        ''' <returns></returns>
        Public Delegate Function Convertor(Of TUnit As Structure)(value As UnitValue(Of TUnit), toUnit As TUnit) As UnitValue(Of TUnit)

        Sub New(type As Type)
            UnitType = type
        End Sub
    End Class

End Namespace
