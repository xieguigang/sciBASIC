#Region "Microsoft.VisualBasic::c9390d210add93121a7350d5f7778b24, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\ComponentModel\Vector.vb"

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

Imports System.Text
Imports Microsoft.VisualBasic.Extensions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel

    ''' <summary>
    ''' 用于表示一个对象实体的属性值的一个向量
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Vector : Inherits EntityBase(Of Double)
        Implements IEnumerable(Of Double)

        Public Overrides Function ToString() As String
            If Properties.Length > 20 Then
                Return MyBase.ToString
            Else
                Return Properties.GetJson
            End If
        End Function

        ''' <summary>
        ''' 生成一个指定长度的随机数序列
        ''' </summary>
        ''' <param name="Length"></param>
        ''' <param name="Upper"></param>
        ''' <param name="Lower"></param>
        ''' <returns></returns>
        Public Shared Function Randomize(Length As UInteger, Optional Upper As Double = 1, Optional Lower As Double = 0) As Vector
            Dim LQuery As Double() = LinqAPI.Exec(Of Double) <=
                From irun As Integer
                In Length.Sequence
                Select RandomDouble() * Upper + Lower '

            Return New Vector With {
                .Properties = LQuery.ToArray
            }
        End Function

        Public Shared Widening Operator CType(x As Double()) As Vector
            Return New Vector With {
                .Properties = x
            }
        End Operator

        Public Shared Narrowing Operator CType(x As Vector) As Double()
            Return x.Properties
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of Double) Implements IEnumerable(Of Double).GetEnumerator
            For i As Integer = 0 To Properties.Length - 1
                Yield Properties(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
