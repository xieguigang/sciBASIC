#Region "Microsoft.VisualBasic::bd2f9068b0a8fd7a0b42615034d55cae, Microsoft.VisualBasic.Core\src\Language\Linq\Assert\VectorAssert.vb"

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

    '   Total Lines: 72
    '    Code Lines: 45
    ' Comment Lines: 14
    '   Blank Lines: 13
    '     File Size: 2.63 KB


    '     Class VectorAssert
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class AssertAll
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Operators: <>, =
    ' 
    '     Class AssertAny
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace Language

    Public Class VectorAssert(Of T)

        Protected ReadOnly vector As Vector(Of T)
        Protected ReadOnly assert As BinaryAssert(Of Object)

        Default Public ReadOnly Property RunAssert(i%, obj As T) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return assert(vector(index:=i), obj)
            End Get
        End Property

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Me.vector = vector
            Me.assert = assert
        End Sub
    End Class

    Public Class AssertAll(Of T) : Inherits VectorAssert(Of T)

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Call MyBase.New(vector, assert)
        End Sub

        ''' <summary>
        ''' Does the elements in this vector all equals to a specific value <paramref name="x"/>?
        ''' </summary>
        ''' <param name="assert"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(assert As AssertAll(Of T), x As T) As Boolean
            Return assert.vector.Sequence.All(Function(i) assert(i, x))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(assert As AssertAll(Of T), x As T) As Boolean
            Return Not assert = x
        End Operator
    End Class

    Public Class AssertAny(Of T) : Inherits VectorAssert(Of T)

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Call MyBase.New(vector, assert)
        End Sub

        ''' <summary>
        ''' Does the elements in this vector all equals to a specific value <paramref name="x"/>?
        ''' </summary>
        ''' <param name="assert"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(assert As AssertAny(Of T), x As T) As Boolean
            Return assert.vector.Sequence.Any(Function(i) assert(i, x))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(assert As AssertAny(Of T), x As T) As Boolean
            Return Not assert = x
        End Operator
    End Class
End Namespace
