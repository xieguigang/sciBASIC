#Region "Microsoft.VisualBasic::621eabfe57bd52ef167a5dc0ece568d4, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\StringVector.vb"

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

    '   Total Lines: 97
    '    Code Lines: 63 (64.95%)
    ' Comment Lines: 17 (17.53%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 17 (17.53%)
    '     File Size: 3.83 KB


    '     Class StringVector
    ' 
    '         Properties: Len
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: InStr, IsPattern, ToString
    '         Operators: <>, =, (+2 Overloads) Like
    ' 
    '     Module StringVectorHelpers
    ' 
    '         Function: ToLower
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports base = Microsoft.VisualBasic.Strings

Namespace Language.Vectorization

    Public Class StringVector : Inherits Vector(Of String)

        ''' <summary>
        ''' Returns the length of each strings
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property Len As IEnumerable(Of Integer)
            Get
                Return Strings.Len(Me)
            End Get
        End Property

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return buffer.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(buffer As IEnumerable(Of String))
            Me.buffer = buffer.SafeQuery.ToArray
        End Sub

        Public Shared Widening Operator CType(list As List(Of String)) As StringVector
            Return New StringVector(list)
        End Operator

        Public Shared Widening Operator CType(array As String()) As StringVector
            Return New StringVector(array)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(s1 As StringVector, s2$) As StringVector
            Return New StringVector(s1.Select(Function(s) s & s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(s1 As StringVector, s2$) As BooleanVector
            Return Not s1 = s2
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(s1 As StringVector, s2$) As BooleanVector
            Return New BooleanVector(s1.Select(Function(s) s = s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function InStr(string1 As StringVector, string2$, Optional method As CompareMethod = CompareMethod.Binary) As Vector(Of Integer)
            Return string1.Select(Function(str) base.InStr(str, string2, method)).ToVector
        End Function

        ''' <summary>
        ''' 批量执行判断目标字符串集合中的每一个字符串元素都是符合目标匹配模式的
        ''' </summary>
        ''' <param name="strings"></param>
        ''' <param name="pattern$"></param>
        ''' <param name="opt"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsPattern(strings As StringVector, pattern$, Optional opt As RegexOptions = RegexICSng) As BooleanVector
            Return strings.Select(Function(s) s.IsPattern(pattern, opt)).ToVector
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression">a,b,c,d,e</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(expression As String) As StringVector
            Return New StringVector(base.Split(expression, ","))
        End Operator

        Public Shared Operator Like(strings As StringVector, pattern As Index(Of String)) As BooleanVector
            Return New BooleanVector(strings.Select(Function(str) str Like pattern))
        End Operator
    End Class

    Public Module StringVectorHelpers

        <Extension>
        Public Function ToLower(str As IEnumerable(Of String)) As IEnumerable(Of String)
            Return str.SafeQuery.Select(AddressOf base.LCase)
        End Function
    End Module
End Namespace
