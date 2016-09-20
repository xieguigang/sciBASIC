#Region "Microsoft.VisualBasic::c8ba880bd6a9f0f680d1469fe0d4758f, ..\visualbasic_App\Microsoft.VisualBasic.NETProtocol\InternetTime\Constant.vb"

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

Namespace InternetTime

    ''' <summary>
    ''' Leap indicator field values
    ''' </summary>
    Public Enum LeapIndicator
        ''' <summary>
        ''' 0 - No warning
        ''' </summary>
        NoWarning       '0 - No warning
        ''' <summary>
        ''' 1 - Last minute has 61 seconds
        ''' </summary>
        LastMinute61    '1 - Last minute has 61 seconds
        ''' <summary>
        ''' 2 - Last minute has 59 seconds
        ''' </summary>
        LastMinute59    '2 - Last minute has 59 seconds
        ''' <summary>
        ''' 3 - Alarm condition (clock not synchronized)
        ''' </summary>
        Alarm           '3 - Alarm condition (clock not synchronized)
    End Enum

    ''' <summary>
    ''' Mode field values
    ''' </summary>
    Public Enum Mode
        ''' <summary>
        ''' 1 - Symmetric active
        ''' </summary>
        SymmetricActive     '1 - Symmetric active
        ''' <summary>
        ''' 2 - Symmetric pasive
        ''' </summary>
        SymmetricPassive    '2 - Symmetric pasive
        ''' <summary>
        ''' 3 - Client
        ''' </summary>
        Client              '3 - Client
        ''' <summary>
        ''' 4 - Server
        ''' </summary>
        Server              '4 - Server
        ''' <summary>
        ''' 5 - Broadcast
        ''' </summary>
        Broadcast           '5 - Broadcast
        ''' <summary>
        ''' 0, 6, 7 - Reserved
        ''' </summary>
        Unknown             '0, 6, 7 - Reserved
    End Enum

    ''' <summary>
    ''' Stratum field values
    ''' </summary>
    Public Enum Stratum
        ''' <summary>
        ''' 0 - unspecified or unavailable
        ''' </summary>
        Unspecified         '0 - unspecified or unavailable
        ''' <summary>
        ''' 1 - primary reference (e.g. radio-clock)
        ''' </summary>
        PrimaryReference    '1 - primary reference (e.g. radio-clock)
        ''' <summary>
        ''' 2-15 - secondary reference (via NTP or SNTP)
        ''' </summary>
        SecondaryReference  '2-15 - secondary reference (via NTP or SNTP)
        ''' <summary>
        ''' 16-255 - reserved
        ''' </summary>
        Reserved            '16-255 - reserved
    End Enum
End Namespace
