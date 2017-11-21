#Region "Microsoft.VisualBasic::2491e37f1be63c2db660e5d24428b758, ..\localblast\LocalBLAST\LocalBLAST\BlastOutput\Common\Percentage.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Math

    ''' <summary>
    ''' 分数，百分比
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure Percentage

        ''' <summary>
        ''' 分子
        ''' </summary>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Numerator As Double
        ''' <summary>
        ''' 分母
        ''' </summary>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Denominator As Double

        ''' <summary>
        ''' <see cref="Numerator"></see>/<see cref="Denominator"></see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <SoapIgnore> Public ReadOnly Property Value As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Numerator / Denominator
            End Get
        End Property

        <SoapIgnore> Public ReadOnly Property FractionExpr As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return $"{Numerator}/{Denominator}"
            End Get
        End Property

        ''' <summary>
        ''' <paramref name="n"/> / <paramref name="d"/>
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="d"></param>
        Sub New(n As Double, d As Double)
            Numerator = n
            Denominator = d
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}/{1} ({2}%)", Numerator, Denominator, Value)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Text">``\d+[/]\d+ \(\d+[%]\)``</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function TryParse(Text As String) As Percentage
            If String.IsNullOrEmpty(Text) Then Return ZERO

            Dim matchs$() = Regex.Matches(Text, "\d+").ToArray
            Dim n As Double = matchs(0).RegexParseDouble
            Dim d As Double = matchs(1).RegexParseDouble

            Return New Percentage With {
                .Numerator = n,
                .Denominator = d
            }
        End Function

        Public Shared ReadOnly Property ZERO As New Percentage(0, 1)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(value As Percentage) As Double
            Return value.Value
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(value As Percentage, n As Double) As Boolean
            Return value.Value > n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(value As Percentage, n As Double) As Boolean
            Return value.Value < n
        End Operator
    End Structure
End Namespace
