#Region "Microsoft.VisualBasic::f285c60baed9da04e670fa7c4447dadb, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\ComponentModel\Class.vb"

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

Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel

    ''' <summary>
    ''' Object entity classification class
    ''' </summary>
    Public Class ColorClass

        ''' <summary>
        ''' Using for the data visualization.(RGB表达式, html颜色值或者名称)
        ''' </summary>
        ''' <returns></returns>
        Public Property Color As String
        ''' <summary>
        ''' <see cref="Integer"/> encoding for this class.(即枚举类型)
        ''' </summary>
        ''' <returns></returns>
        Public Property int As Integer
        ''' <summary>
        ''' Class Name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="colors$">Using the user custom colors</param>
        ''' <returns></returns>
        Public Shared Function FromEnums(Of T)(Optional colors$() = Nothing) As ColorClass()
            Dim values As T() = Enums(Of T)()
            If colors.IsNullOrEmpty OrElse colors.Length < values.Length Then
                colors$ = Imaging _
                    .ChartColors _
                    .Select(AddressOf Imaging.ToHtmlColor) _
                    .ToArray
            End If
            Dim out As ColorClass() = values _
                .SeqIterator _
                .Select(Function(v)
                            Return New ColorClass With {
                                .int = CInt(DirectCast(+v, Object)),
                                .Color = colors(v),
                                .Name = DirectCast(CObj((+v)), [Enum]).Description
                            }
                        End Function) _
                .ToArray
            Return out
        End Function

        Public Shared Operator =(a As ColorClass, b As ColorClass) As Boolean
            Return a.Color = b.Color AndAlso a.int = b.int AndAlso a.Name = b.Name
        End Operator

        Public Shared Operator <>(a As ColorClass, b As ColorClass) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace
