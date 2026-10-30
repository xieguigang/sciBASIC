#Region "Microsoft.VisualBasic::7a79af86e80cedb87d754cb2ad9bece7, Data_science\Mathematica\Math\ODE\ODESolvers\ODEOutput.vb"

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

    '   Total Lines: 55
    '    Code Lines: 42 (76.36%)
    ' Comment Lines: 4 (7.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (16.36%)
    '     File Size: 1.63 KB


    ' Class ODEOutput
    ' 
    '     Properties: description, ID, sum, X, xrange
    '                 Y, y0
    ' 
    '     Function: GetPointsData, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class ODEOutput : Implements INamedValue

    <XmlAttribute>
    Public Property ID As String Implements INamedValue.Key

    ''' <summary>
    ''' The sampling time points of the integration result, it contains ``n + 1`` points
    ''' which includes both boundaries of the integration range.
    ''' </summary>
    ''' <returns></returns>
    Public Property X As ODESequence

    Public Property Y As NumericVector

    <XmlText>
    Public Property description As String

    ''' <summary>
    ''' 最后一个结果值为积分的总和
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property sum As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Dim v As Double() = Nothing

            If Y IsNot Nothing Then
                v = Y.vector
            End If

            If v Is Nothing OrElse v.Length = 0 Then
                Return 0.0
            Else
                Return v(v.Length - 1)
            End If
        End Get
    End Property

    Public ReadOnly Property y0 As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Dim v As Double() = Nothing

            If Y IsNot Nothing Then
                v = Y.vector
            End If

            If v Is Nothing OrElse v.Length = 0 Then
                Return 0.0
            Else
                Return v(0)
            End If
        End Get
    End Property

    Public ReadOnly Property xrange As DoubleRange
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New DoubleRange(X.range)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return If(Y Is Nothing, "{}", Y.vector.GetJson)
    End Function

    ''' <summary>
    ''' Enumerate the ``(x, y)`` points of the integration result.
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function GetPointsData() As IEnumerable(Of PointF)
        If X Is Nothing OrElse Y Is Nothing Then
            Return
        End If

        Dim xs As Double() = X.ToArray
        Dim ys As Double() = Y.vector
        Dim n As Integer = System.Math.Min(xs.Length, ys.Length)

        For i As Integer = 0 To n - 1
            Yield New PointF(CSng(xs(i)), CSng(ys(i)))
        Next
    End Function
End Class
