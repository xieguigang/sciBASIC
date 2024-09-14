#Region "Microsoft.VisualBasic::110445d2014662bced6ed131fd33209a, Data_science\MachineLearning\MachineLearning\ComponentModel\Status0.vb"

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

    '   Total Lines: 67
    '    Code Lines: 50 (74.63%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (25.37%)
    '     File Size: 2.05 KB


    '     Class Status0
    ' 
    '         Function: GetFloats, GetVector
    ' 
    '     Class ConstantStatus0
    ' 
    '         Properties: C
    ' 
    '         Function: GetInitialValue, ToString
    ' 
    '     Class RandomStatus0
    ' 
    '         Properties: Max, Min
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetInitialValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace ComponentModel

    Public MustInherit Class Status0

        Public MustOverride Function GetInitialValue() As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetVector(dims As Integer) As Vector
            Return New Vector(GetFloats(count:=dims))
        End Function

        Public Iterator Function GetFloats(count As Integer) As IEnumerable(Of Single)
            For i As Integer = 0 To count - 1
                Yield GetInitialValue()
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(a As Status0) As Single
            Return a.GetInitialValue
        End Operator

    End Class

    Public Class ConstantStatus0 : Inherits Status0

        <XmlText>
        Public Property C As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetInitialValue() As Single
            Return C
        End Function

        Public Overrides Function ToString() As String
            Return C.ToString
        End Function
    End Class

    Public Class RandomStatus0 : Inherits Status0

        <XmlAttribute> Public Property Min As Double
        <XmlAttribute> Public Property Max As Double

        Sub New()
        End Sub

        Sub New(min As Double, max As Double)
            Me.Min = min
            Me.Max = max
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetInitialValue() As Single
            Return randf.NextDouble(Min, Max)
        End Function

        Public Overrides Function ToString() As String
            Return $"randf({Min}, {Max}) = {GetInitialValue()}"
        End Function
    End Class
End Namespace
