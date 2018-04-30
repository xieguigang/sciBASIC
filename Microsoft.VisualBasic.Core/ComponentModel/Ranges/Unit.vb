#Region "Microsoft.VisualBasic::635acb9edd7eb48716751df608b68f6b, Microsoft.VisualBasic.Core\ComponentModel\Ranges\Unit.vb"

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

    '     Class Convertor
    ' 
    '         Properties: UnitType
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '     Class UnitValue
    ' 
    '         Properties: Unit
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: ^
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Ranges

    Public Class Convertor : Inherits Attribute

        Public ReadOnly Property UnitType As Type

        Public Delegate Function Convertor(Of TUnit)(value As UnitValue(Of TUnit), toUnit As TUnit) As UnitValue(Of TUnit)

        Sub New(type As Type)

        End Sub
    End Class

    Public Class UnitValue(Of TUnit) : Inherits float

        ''' <summary>
        ''' 分（d） ``10^-1``
        ''' </summary>
        Public Const d = 10 ^ -1
        ''' <summary>
        ''' 厘（c） ``10^-2``
        ''' </summary>
        Public Const c = 10 ^ -2
        ''' <summary>
        ''' 毫（m） ``10^-3``
        ''' </summary>
        Public Const m = 10 ^ -3
        ''' <summary>
        ''' 微（μ） ``10^-6``
        ''' </summary>
        Public Const u = 10 ^ -6
        ''' <summary>
        ''' 纳（n） ``10^-9``
        ''' </summary>
        Public Const n = 10 ^ -9
        ''' <summary>
        ''' 皮（p） ``10^-12``
        ''' </summary>
        Public Const p = 10 ^ -12
        ''' <summary>
        ''' 飞（f） ``10^-15``
        ''' </summary>
        Public Const f = 10 ^ -15
        ''' <summary>
        ''' 阿（a） ``10^-18``
        ''' </summary>
        Public Const a = 10 ^ -18

        Public Property Unit As TUnit

        Sub New(value#, unit As TUnit)
            Me.Value = value
            Me.Unit = unit
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{Value} ({DirectCast(CObj(Unit), [Enum]).Description})"
        End Function

        Public Overloads Shared Operator ^(value As UnitValue(Of TUnit), unit As TUnit) As UnitValue(Of TUnit)
            Throw New NotImplementedException
        End Operator
    End Class
End Namespace
