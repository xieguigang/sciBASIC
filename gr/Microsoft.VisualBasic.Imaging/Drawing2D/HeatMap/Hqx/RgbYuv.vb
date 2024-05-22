#Region "Microsoft.VisualBasic::4541d1bbd1fd5c69507ad49892fd6da8, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Hqx\RgbYuv.vb"

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

    '   Total Lines: 93
    '    Code Lines: 38 (40.86%)
    ' Comment Lines: 42 (45.16%)
    '    - Xml Docs: 35.71%
    ' 
    '   Blank Lines: 13 (13.98%)
    '     File Size: 3.27 KB


    '     Module RgbYuv
    ' 
    '         Properties: IsAllocated
    ' 
    '         Function: getYuv
    ' 
    '         Sub: hqxInit, Release
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
Option Strict On

Imports System.Runtime.CompilerServices

Namespace Drawing2D.HeatMap.hqx

    Module RgbYuv

        Private Const rgbMask As UInteger = &HFFFFFF

        ''' <summary>
        ''' this array is readonly to public
        ''' </summary>
        Dim RGBtoYUV As Integer()
        Dim allocated As Boolean = False

        Public ReadOnly Property IsAllocated As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return allocated
            End Get
        End Property

        ''' <summary>
        ''' Returns the 24bit YUV equivalent of the provided 24bit RGB color.<b>Any alpha component is dropped</b>
        ''' </summary>
        ''' <param name="rgb"> a 24bit rgb color </param>
        ''' <returns> the corresponding 24bit YUV color </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function getYuv(rgb As UInteger) As Integer
            Return RGBtoYUV(CInt(rgb And rgbMask))
        End Function

        ''' <summary>
        ''' Calculates the lookup table. <b>MUST</b> be called (only once) before doing anything else
        ''' </summary>
        Public Sub hqxInit()
            Dim r, g, b, y, u, v As Integer

            RGBtoYUV = New Integer(16777215) {}

            For c As Integer = &H1000000 - 1 To 0 Step -1
                ' Initalize RGB to YUV lookup table 
                r = (c And &HFF0000) >> 16
                g = (c And &HFF00) >> 8
                b = c And &HFF
                y = CInt(+0.299R * r + 0.587R * g + 0.114R * b)
                u = CInt(-0.169R * r - 0.331R * g + 0.5R * b) + 128
                v = CInt(+0.5R * r - 0.419R * g - 0.081R * b) + 128
                RGBtoYUV(c) = y << 16 Or u << 8 Or v
            Next

            allocated = True
        End Sub

        ''' <summary>
        ''' Releases the reference to the lookup table.
        ''' <para>The table has to be calculated again for the next lookup.</para>
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Release()
            Erase RGBtoYUV
            allocated = False
        End Sub
    End Module

End Namespace
