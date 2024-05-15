#Region "Microsoft.VisualBasic::16cb6dbbf52c42ee6547288a147a6997, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\EmGaussian\Variable.vb"

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
    '    Code Lines: 44
    ' Comment Lines: 10
    '   Blank Lines: 13
    '     File Size: 2.37 KB


    '     Class Variable
    ' 
    '         Properties: center, height, offset, width
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: gaussian, Multi_gaussian, Multi_sine, sine, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace EmGaussian

    ''' <summary>
    ''' A possible signle peak
    ''' </summary>
    Public Class Variable

        ''' <summary>
        ''' height,center,width,offset
        ''' </summary>
        Friend Const argument_size As Integer = 4

        Public Property height As Double
        Public Property center As Double
        Public Property width As Double
        Public Property offset As Double

        Sub New()
        End Sub

        Sub New(center As Double, width As Double, height As Double, Optional offset As Double = 0)
            Me.center = center
            Me.width = width
            Me.height = height
            Me.offset = offset
        End Sub

        ''' <summary>
        ''' make data copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As Variable)
            height = clone.height
            center = clone.center
            width = clone.width
            offset = clone.offset
        End Sub

        Public Overrides Function ToString() As String
            Return $"gaussian(x) = {height.ToString("G3")} * exp(-((x - {center.ToString("G3")}) ^ 2) / (2 * ({width.ToString("G3")} ^ 2))) + {offset.ToString("G3")}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function sine(x As Double) As Double
            Return height * std.Sin(2 * std.PI * (x - center) / 12 + width) + offset
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function gaussian(x As Double) As Double
            Return height * std.Exp(-((x - center) ^ 2) / (2 * (width ^ 2))) + offset
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Multi_gaussian(x As Double, vars As Variable(), offset As Double) As Double
            Return offset + Aggregate c As Variable In vars Into Sum(c.gaussian(x))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Multi_sine(x As Double, vars As Variable(), offset As Double) As Double
            Return offset + Aggregate c As Variable In vars Into Sum(c.sine(x))
        End Function

    End Class
End Namespace
