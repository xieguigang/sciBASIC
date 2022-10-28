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

Namespace Drawing2D.HeatMap.hqx
    Public MustInherit Class Hqx
        Private Const Ymask As Integer = &H00FF0000
        Private Const Umask As Integer = &H0000FF00
        Private Const Vmask As Integer = &H000000FF

        ''' <summary>
        ''' Compares two ARGB colors according to the provided Y, U, V and A thresholds. </summary>
        ''' <paramname="c1"> an ARGB color </param>
        ''' <paramname="c2"> a second ARGB color </param>
        ''' <paramname="trY"> the Y (luminance) threshold </param>
        ''' <paramname="trU"> the U (chrominance) threshold </param>
        ''' <paramname="trV"> the V (chrominance) threshold </param>
        ''' <paramname="trA"> the A (transparency) threshold </param>
        ''' <returns> true if colors differ more than the thresholds permit, false otherwise </returns>
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: 'final' parameters are not available in .NET:
        'ORIGINAL LINE: protected static boolean diff(final int c1, final int c2, final int trY, final int trU, final int trV, final int trA)
        Protected Friend Shared Function diff(ByVal c1 As Integer, ByVal c2 As Integer, ByVal [trY] As Integer, ByVal trU As Integer, ByVal trV As Integer, ByVal trA As Integer) As Boolean
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final int YUV1 = RgbYuv.getYuv(c1);
            Dim YUV1 As Integer = hqx.RgbYuv.getYuv(c1)
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final int YUV2 = RgbYuv.getYuv(c2);
            Dim YUV2 As Integer = hqx.RgbYuv.getYuv(c2)

            Return Math.Abs((YUV1 And hqx.Hqx.Ymask) - (YUV2 And hqx.Hqx.Ymask)) > [trY] OrElse Math.Abs((YUV1 And hqx.Hqx.Umask) - (YUV2 And hqx.Hqx.Umask)) > trU OrElse Math.Abs((YUV1 And hqx.Hqx.Vmask) - (YUV2 And hqx.Hqx.Vmask)) > trV OrElse Math.Abs((c1 >> 24) - (c2 >> 24)) > trA
        End Function

    End Class

End Namespace
