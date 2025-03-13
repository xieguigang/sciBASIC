#Region "Microsoft.VisualBasic::db221bb06201d8825c7a437e7af3b2e2, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\Binding.vb"

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

    '   Total Lines: 86
    '    Code Lines: 47 (54.65%)
    ' Comment Lines: 25 (29.07%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 14 (16.28%)
    '     File Size: 2.94 KB


    '     Structure Binding
    ' 
    '         Properties: IsEmpty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, Tuple, ValueTuple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel

    ''' <summary>
    ''' Functioning the same as the <see cref="KeyValuePair(Of T, K)"/>, but with more specific on the name. 
    ''' <see cref="KeyValuePair(Of T, K)"/> its name is too generic.
    ''' (作用与<see cref="KeyValuePair(Of T, K)"/>类似，只不过类型的名称更加符合绑定的描述)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="K"></typeparam>
    Public Structure Binding(Of T, K)
        Implements IsEmpty

        Dim Bind As T
        Dim Target As K

        ''' <summary>
        ''' the name of current binding, can be optional
        ''' </summary>
        Dim name As String

        ''' <summary>
        ''' If the field <see cref="Bind"/> and <see cref="Target"/> are both nothing, then this binding is empty.
        ''' (当<see cref="Bind"/>以及<see cref="Target"/>都同时为空值的时候这个参数才会为真)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Bind Is Nothing AndAlso Target Is Nothing
            End Get
        End Property

        Sub New(source As T, target As K)
            Me.Bind = source
            Me.Target = target
        End Sub

        Public Overrides Function ToString() As String
            Dim bindStr As String

            If IsEmpty Then
                bindStr = "No binding"
            Else
                bindStr = Bind.ToString & " --> " & Target.ToString
            End If

            If name.StringEmpty Then
                Return bindStr
            Else
                Return $"[{name}] {bindStr}"
            End If
        End Function

        ''' <summary>
        ''' Convert this binding to tuple
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Tuple() As Tuple(Of T, K)
            Return Me
        End Function

#If NET48_OR_GREATER Or NET8_0_OR_GREATER Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ValueTuple() As (bind As T, target As K)
            Return (Bind, Target)
        End Function

#End If

        ''' <summary>
        ''' Implicit convert this binding as the <see cref="System.Tuple(Of T, K)"/>
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(b As Binding(Of T, K)) As Tuple(Of T, K)
            Return New Tuple(Of T, K)(b.Bind, b.Target)
        End Operator
    End Structure
End Namespace
