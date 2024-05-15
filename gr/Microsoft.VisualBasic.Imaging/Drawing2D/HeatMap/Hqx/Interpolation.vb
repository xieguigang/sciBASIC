#Region "Microsoft.VisualBasic::c464018a22be96d1849c69d195b603e7, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Hqx\Interpolation.vb"

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

    '   Total Lines: 144
    '    Code Lines: 69
    ' Comment Lines: 51
    '   Blank Lines: 24
    '     File Size: 5.64 KB


    '     Class Interpolation
    ' 
    '         Function: Mix14To1To1, Mix2To1To1, Mix2To3To3, Mix2To7To7, Mix3To1
    '                   Mix4To2To1, Mix5To3, Mix6To1To1, Mix7To1, MixColours
    '                   MixEven
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

    ''' <summary>
    ''' Helper class to interpolate colors. Nothing to see here, move along...
    ''' </summary>
    Friend NotInheritable Class Interpolation

        Const MaskAlpha As UInteger = &HFF000000
        Const MaskGreen As UInteger = &HFF00
        Const MaskRedBlue As UInteger = &HFF00FF
        Const AlphaShift As UInteger = 24

        ' return statements:
        '	 1. line: green
        '	 2. line: red and blue
        '	 3. line: alpha

        Friend Shared Function Mix3To1(c1 As UInteger, c2 As UInteger) As UInteger
            'return (c1*3+c2) >> 2;
            Return MixColours(3, 1, c1, c2)
        End Function

        Friend Shared Function Mix2To1To1(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*2+c2+c3) >> 2;
            Return MixColours(2, 1, 1, c1, c2, c3)
        End Function

        Friend Shared Function Mix7To1(c1 As UInteger, c2 As UInteger) As UInteger
            'return (c1*7+c2)/8;
            Return MixColours(7, 1, c1, c2)
        End Function

        Friend Shared Function Mix2To7To7(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*2+(c2+c3)*7)/16;
            Return MixColours(2, 7, 7, c1, c2, c3)
        End Function

        Friend Shared Function MixEven(c1 As UInteger, c2 As UInteger) As UInteger
            'return (c1+c2) >> 1;
            Return MixColours(1, 1, c1, c2)
        End Function

        Friend Shared Function Mix4To2To1(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*5+c2*2+c3)/8;
            Return MixColours(5, 2, 1, c1, c2, c3)
        End Function

        Friend Shared Function Mix6To1To1(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*6+c2+c3)/8;
            Return MixColours(6, 1, 1, c1, c2, c3)
        End Function

        Friend Shared Function Mix5To3(c1 As UInteger, c2 As UInteger) As UInteger
            'return (c1*5+c2*3)/8;
            Return MixColours(5, 3, c1, c2)
        End Function

        Friend Shared Function Mix2To3To3(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*2+(c2+c3)*3)/8;
            Return MixColours(2, 3, 3, c1, c2, c3)
        End Function

        Friend Shared Function Mix14To1To1(c1 As UInteger, c2 As UInteger, c3 As UInteger) As UInteger
            'return (c1*14+c2+c3)/16;
            Return MixColours(14, 1, 1, c1, c2, c3)
        End Function

        ''' <summary>
        ''' This method can overflow between blue and red and from red 
        ''' to nothing when the sum of all weightings is higher than 255.
        ''' It only works for weightings with a sum that Is a power of 
        ''' two, otherwise the blue value Is corrupted.
        ''' </summary>
        ''' <param name="weightingsAndColours">
        ''' weighting0, weighting1[, ...], colour0, colour1[, ...]
        ''' </param>
        ''' <returns></returns>
        Public Shared Function MixColours(ParamArray weightingsAndColours As UInteger()) As UInteger
            Dim totalPartsColour As UInteger = 0
            Dim totalPartsAlpha As UInteger = 0
            Dim totalGreen As UInteger = 0
            Dim totalRedBlue As UInteger = 0
            Dim totalAlpha As UInteger = 0
            Dim nsize As Integer = CInt(weightingsAndColours.Length / 2)

            For i As Integer = 0 To nsize - 1
                Dim weighting = weightingsAndColours(i)
                Dim colour = weightingsAndColours(nsize + i)

                If (weighting > 0) Then
                    Dim alpha = (colour >> AlphaShift) * weighting

                    totalPartsAlpha += weighting
                    If (alpha <> 0) Then
                        totalAlpha += alpha

                        totalPartsColour += weighting
                        totalGreen += (colour And MaskGreen) * weighting
                        totalRedBlue += (colour And MaskRedBlue) * weighting
                    End If
                End If
            Next

            totalAlpha /= totalPartsAlpha
            totalAlpha <<= AlphaShift

            If (totalPartsColour > 0) Then
                totalGreen /= totalPartsColour
                totalGreen = totalGreen And MaskGreen

                totalRedBlue /= totalPartsColour
                totalRedBlue = totalRedBlue And MaskRedBlue
            End If

            Return totalAlpha Or totalGreen Or totalRedBlue
        End Function
    End Class

End Namespace
