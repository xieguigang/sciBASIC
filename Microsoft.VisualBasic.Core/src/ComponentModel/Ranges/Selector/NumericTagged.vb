#Region "Microsoft.VisualBasic::9972dfc3f040568740d2c80ffbaff018, sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Selector\NumericTagged.vb"

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

    '   Total Lines: 65
    '    Code Lines: 44
    ' Comment Lines: 10
    '   Blank Lines: 11
    '     File Size: 2.32 KB


    '     Structure NumericTagged
    ' 
    '         Properties: IValueOf_Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) CompareTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Namespace ComponentModel.Ranges

    ''' <summary>
    ''' Almost equals to <see cref="DoubleTagged(Of T)"/>, but this object is a structure type. 
    ''' (作用几乎等同于<see cref="DoubleTagged(Of T)"/>，只不过这个是Structure类型，开销会小一些)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NumericTagged(Of T) : Implements IComparable(Of Double), IComparable, IComparable(Of NumericTagged(Of T))
        Implements Value(Of T).IValueOf

        Dim tag#
        Dim value As T

        Private Property IValueOf_Value As T Implements Value(Of T).IValueOf.Value

        Sub New(tag#, value As T)
            Me.tag = tag
            Me.value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{tag} {value.GetJson}"
        End Function

        ''' <summary>
        ''' 比较的是<see cref="Tag"/>属性
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        Public Function CompareTo(other As Double) As Integer Implements IComparable(Of Double).CompareTo
            Dim d = tag - other

            If d = 0R Then
                Return 0
            Else
                Return stdNum.Sign(d)
            End If
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            Else
                Dim type = obj.GetType

                If type = GetType(Double) Then
                    Return CompareTo(CDbl(obj))
                ElseIf type = GetType(NumericTagged(Of T)) Then
                    Return CompareTo(DirectCast(obj, NumericTagged(Of T)).tag)
                Else
                    Return 1
                End If
            End If
        End Function

        Public Function CompareTo(other As NumericTagged(Of T)) As Integer Implements IComparable(Of NumericTagged(Of T)).CompareTo
            Return CompareTo(other.tag)
        End Function
    End Structure
End Namespace
