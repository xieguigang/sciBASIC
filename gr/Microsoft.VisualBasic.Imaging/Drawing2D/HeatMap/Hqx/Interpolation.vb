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

        Private Const Mask4 As Integer = &HFF000000UI
        Private Const Mask2 As Integer = &HFF00
        Private Const Mask13 As Integer = &HFF00FF

        ' return statements:
        '	 1. line: green
        '	 2. line: red and blue
        '	 3. line: alpha

        Friend Shared Function Mix3To1(c1 As Integer, c2 As Integer) As Integer
            'return (c1*3+c2) >> 2;
            If c1 = c2 Then
                Return c1
            End If
            Return (c1 And Mask2) * 3 + (c2 And Mask2) >> 2 And Mask2 Or (c1 And Mask13) * 3 + (c2 And Mask13) >> 2 And Mask13 Or ((c1 And Mask4) >> 2) * 3 + ((c2 And Mask4) >> 2) And Mask4
        End Function

        Friend Shared Function Mix2To1To1(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*2+c2+c3) >> 2;
            Return (c1 And Mask2) * 2 + (c2 And Mask2) + (c3 And Mask2) >> 2 And Mask2 Or (c1 And Mask13) * 2 + (c2 And Mask13) + (c3 And Mask13) >> 2 And Mask13 Or ((c1 And Mask4) >> 2) * 2 + ((c2 And Mask4) >> 2) + ((c3 And Mask4) >> 2) And Mask4
        End Function

        Friend Shared Function Mix7To1(c1 As Integer, c2 As Integer) As Integer
            'return (c1*7+c2)/8;
            If c1 = c2 Then
                Return c1
            End If
            Return (c1 And Mask2) * 7 + (c2 And Mask2) >> 3 And Mask2 Or (c1 And Mask13) * 7 + (c2 And Mask13) >> 3 And Mask13 Or ((c1 And Mask4) >> 3) * 7 + ((c2 And Mask4) >> 3) And Mask4
        End Function

        Friend Shared Function Mix2To7To7(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*2+(c2+c3)*7)/16;
            Return (c1 And Mask2) * 2 + (c2 And Mask2) * 7 + (c3 And Mask2) * 7 >> 4 And Mask2 Or (c1 And Mask13) * 2 + (c2 And Mask13) * 7 + (c3 And Mask13) * 7 >> 4 And Mask13 Or ((c1 And Mask4) >> 4) * 2 + ((c2 And Mask4) >> 4) * 7 + ((c3 And Mask4) >> 4) * 7 And Mask4
        End Function

        Friend Shared Function MixEven(c1 As Integer, c2 As Integer) As Integer
            'return (c1+c2) >> 1;
            If c1 = c2 Then
                Return c1
            End If
            Return (c1 And Mask2) + (c2 And Mask2) >> 1 And Mask2 Or (c1 And Mask13) + (c2 And Mask13) >> 1 And Mask13 Or ((c1 And Mask4) >> 1) + ((c2 And Mask4) >> 1) And Mask4
        End Function

        Friend Shared Function Mix4To2To1(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*5+c2*2+c3)/8;
            Return (c1 And Mask2) * 5 + (c2 And Mask2) * 2 + (c3 And Mask2) >> 3 And Mask2 Or (c1 And Mask13) * 5 + (c2 And Mask13) * 2 + (c3 And Mask13) >> 3 And Mask13 Or ((c1 And Mask4) >> 3) * 5 + ((c2 And Mask4) >> 3) * 2 + ((c3 And Mask4) >> 3) And Mask4
        End Function

        Friend Shared Function Mix6To1To1(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*6+c2+c3)/8;
            Return (c1 And Mask2) * 6 + (c2 And Mask2) + (c3 And Mask2) >> 3 And Mask2 Or (c1 And Mask13) * 6 + (c2 And Mask13) + (c3 And Mask13) >> 3 And Mask13 Or ((c1 And Mask4) >> 3) * 6 + ((c2 And Mask4) >> 3) + ((c3 And Mask4) >> 3) And Mask4
        End Function

        Friend Shared Function Mix5To3(c1 As Integer, c2 As Integer) As Integer
            'return (c1*5+c2*3)/8;
            If c1 = c2 Then
                Return c1
            End If
            Return (c1 And Mask2) * 5 + (c2 And Mask2) * 3 >> 3 And Mask2 Or (c1 And Mask13) * 5 + (c2 And Mask13) * 3 >> 3 And Mask13 Or ((c1 And Mask4) >> 3) * 5 + ((c2 And Mask4) >> 3) * 3 And Mask4
        End Function

        Friend Shared Function Mix2To3To3(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*2+(c2+c3)*3)/8;
            Return (c1 And Mask2) * 2 + (c2 And Mask2) * 3 + (c3 And Mask2) * 3 >> 3 And Mask2 Or (c1 And Mask13) * 2 + (c2 And Mask13) * 3 + (c3 And Mask13) * 3 >> 3 And Mask13 Or ((c1 And Mask4) >> 3) * 2 + ((c2 And Mask4) >> 3) * 3 + ((c3 And Mask4) >> 3) * 3 And Mask4
        End Function

        Friend Shared Function Mix14To1To1(c1 As Integer, c2 As Integer, c3 As Integer) As Integer
            'return (c1*14+c2+c3)/16;
            Return (c1 And Mask2) * 14 + (c2 And Mask2) + (c3 And Mask2) >> 4 And Mask2 Or (c1 And Mask13) * 14 + (c2 And Mask13) + (c3 And Mask13) >> 4 And Mask13 Or ((c1 And Mask4) >> 4) * 14 + ((c2 And Mask4) >> 4) + ((c3 And Mask4) >> 4) And Mask4
        End Function
    End Class

End Namespace
