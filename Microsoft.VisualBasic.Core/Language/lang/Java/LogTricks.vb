#Region "Microsoft.VisualBasic::9b5bb278e9cc7e9e483c1e377eaae6ea, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\lang\Java\LogTricks.vb"

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

Imports sys = System.Math

Namespace Language.Java

    '
    ' * LogTricks.java
    ' *
    ' * Copyright (c) 2002-2015 Alexei Drummond, Andrew Rambaut and Marc Suchard
    ' *
    ' * This file is part of BEAST.
    ' * See the NOTICE file distributed with this work for additional
    ' * information regarding copyright ownership and licensing.
    ' *
    ' * BEAST is free software; you can redistribute it and/or modify
    ' * it under the terms of the GNU Lesser General Public License as
    ' * published by the Free Software Foundation; either version 2
    ' * of the License, or (at your option) any later version.
    ' *
    ' *  BEAST is distributed in the hope that it will be useful,
    ' *  but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' *  GNU Lesser General Public License for more details.
    ' *
    ' * You should have received a copy of the GNU Lesser General Public
    ' * License along with BEAST; if not, write to the
    ' * Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
    ' * Boston, MA  02110-1301  USA
    ' 

    ''' <summary>
    ''' @author Marc Suchard
    ''' </summary>
    Public Module LogTricks

        Public ReadOnly maxFloat As Double = Double.MaxValue '3.40282347E+38;
        Public ReadOnly logLimit As Double = -maxFloat / 100
        Public ReadOnly logZero As Double = -maxFloat

        Public Const NATS As Double = 400 '40;

        Public Function logSumNoCheck(x As Double, y As Double) As Double
            Dim temp As Double = y - x
            If sys.Abs(temp) > NATS Then
                Return If(x > y, x, y)
            Else
                Return x + JavaMath.log1p(sys.Exp(temp))
            End If
        End Function

        Public Function logSum(x As Double()) As Double
            Dim sum As Double = x(0)
            Dim len As Integer = x.Length
            For i As Integer = 1 To len - 1
                sum = logSumNoCheck(sum, x(i))
            Next i
            Return sum
        End Function

        Public Function logSum(x As Double, y As Double) As Double
            Dim temp As Double = y - x
            If temp > NATS OrElse x < logLimit Then Return y
            If temp < -NATS OrElse y < logLimit Then Return x
            If temp < 0 Then Return x + JavaMath.log1p(sys.Exp(temp))
            Return y + JavaMath.log1p(sys.Exp(-temp))
        End Function

        Public Sub logInc(x As Double?, y As Double)
            Dim temp As Double = y - x
            If temp > NATS OrElse x < logLimit Then
                x = y
            ElseIf temp < -NATS OrElse y < logLimit Then

            Else
                x += JavaMath.log1p(sys.Exp(temp))
            End If
        End Sub

        Public Function logDiff(x As Double, y As Double) As Double
            Debug.Assert(x > y)
            Dim temp As Double = y - x
            If temp < -NATS OrElse y < logLimit Then Return x
            Return x + JavaMath.log1p(-sys.Exp(temp))
        End Function
    End Module
End Namespace
