Imports System

' 
'  Copyright © 2003 Maxim Stepin (maxst@hiend3d.com)
' 
'  Copyright © 2010 Cameron Zemek (grom@zeminvaders.net)
' 
'  Copyright © 2011 Tamme Schichler (tamme.schichler@googlemail.com)
' 
'  Copyright © 2012 A. Eduardo García (arcnorj@gmail.com)
' 
'  This file is part of hqx-java.
' 
'  hqx-java is free software: you can redistribute it and/or modify
'  it under the terms of the GNU Lesser General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' 
'  hqx-java is distributed in the hope that it will be useful,
'  but WITHOUT ANY WARRANTY; without even the implied warranty of
'  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
'  GNU Lesser General Public License for more details.
' 
'  You should have received a copy of the GNU Lesser General Public License
'  along with hqx-java. If not, see <http://www.gnu.org/licenses/>.
' 
Imports stdNum = System.Math

Namespace Drawing2D.HeatMap.hqx

    ''' <summary>
    ''' + wikipedia: https://en.wikipedia.org/wiki/Hqx
    ''' + [Tamschi/hqxSharp](https://github.com/Tamschi/hqxSharp): hqxSharp is a C# port of hqx, a fast, high-quality magnification filter designed for pixel art.
    ''' + [Arcnor/hqx-java](https://github.com/Arcnor/hqx-java): hqx-java is a Java port of hqx, a fast, high-quality magnification filter designed for pixel art.
    ''' </summary>
    Public MustInherit Class HqxScaling

        Private Const Ymask As Integer = &HFF0000
        Private Const Umask As Integer = &HFF00
        Private Const Vmask As Integer = &HFF

        ''' <summary>
        ''' Compares two ARGB colors according to the provided Y, U, V and A thresholds. </summary>
        ''' <param name="c1"> an ARGB color </param>
        ''' <param name="c2"> a second ARGB color </param>
        ''' <param name="trY"> the Y (luminance) threshold </param>
        ''' <param name="trU"> the U (chrominance) threshold </param>
        ''' <param name="trV"> the V (chrominance) threshold </param>
        ''' <param name="trA"> the A (transparency) threshold </param>
        ''' <returns> true if colors differ more than the thresholds permit, false otherwise </returns>
        Protected Friend Shared Function diff(c1 As Integer, c2 As Integer, [trY] As Integer, trU As Integer, trV As Integer, trA As Integer) As Boolean
            Dim YUV1 As Integer = RgbYuv.getYuv(c1)
            Dim YUV2 As Integer = RgbYuv.getYuv(c2)

            Return stdNum.Abs((YUV1 And HqxScaling.Ymask) - (YUV2 And HqxScaling.Ymask)) > [trY] OrElse stdNum.Abs((YUV1 And HqxScaling.Umask) - (YUV2 And HqxScaling.Umask)) > trU OrElse stdNum.Abs((YUV1 And HqxScaling.Vmask) - (YUV2 And HqxScaling.Vmask)) > trV OrElse stdNum.Abs((c1 >> 24) - (c2 >> 24)) > trA
        End Function

    End Class

End Namespace
