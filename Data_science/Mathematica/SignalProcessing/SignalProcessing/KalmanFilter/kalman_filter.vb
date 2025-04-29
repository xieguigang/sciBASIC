#Region "Microsoft.VisualBasic::10ceff52983a32e76288540a4b668f97, Data_science\Mathematica\SignalProcessing\SignalProcessing\KalmanFilter\kalman_filter.vb"

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

    '   Total Lines: 53
    '    Code Lines: 20 (37.74%)
    ' Comment Lines: 28 (52.83%)
    '    - Xml Docs: 21.43%
    ' 
    '   Blank Lines: 5 (9.43%)
    '     File Size: 1.80 KB


    '     Class kalman1_state
    ' 
    ' 
    ' 
    '     Class kalman2_state
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KalmanFilter

    ' 
    '  FileName : kalman_filter.c
    '  Author   : xiahouzuoxin @163.com
    '  Version  : v1.0
    '  Date     : 2014/9/24 20:36:51
    '  Brief    : 
    '  
    '  Copyright (C) MICL,USTB
    ' 

    ' 
    '  FileName : kalman_filter.h
    '  Author   : xiahouzuoxin @163.com
    '  Version  : v1.0
    '  Date     : 2014/9/24 20:37:01
    '  Brief    : 
    '  
    '  Copyright (C) MICL,USTB
    ' 

    ' 
    '  NOTES: n Dimension means the state is n dimension, 
    '  measurement always 1 dimension 
    ' 

    ''' <summary>
    ''' Represents the state of a 1-dimensional Kalman filter
    ''' </summary>
    Public Class kalman1_state
        Public x As Single ' state
        Public A As Single ' x(n)=A*x(n-1)+u(n),u(n)~N(0,q)
        Public H As Single ' z(n)=H*x(n)+w(n),w(n)~N(0,r)
        Public q As Single ' process(predict) noise convariance
        Public r As Single ' measure noise convariance
        Public p As Single ' estimated error convariance
        Public gain As Single
    End Class

    ''' <summary>
    ''' Represents the state of a 2-dimensional Kalman filter
    ''' </summary>
    Public Class kalman2_state
        Public x As Single() = New Single(1) {} ' state: [0]-angle [1]-diffrence of angle, 2x1
        Public A As Single()() = {New Single(1) {}, New Single(1) {}}
        Public H As Single() = New Single(1) {} ' Z(n)=H*X(n)+W(n),W(n)~N(0,r), 1x2
        Public q As Single() = New Single(1) {} ' process(predict) noise convariance,2x1 [q0,0; 0,q1]
        Public r As Single ' measure noise convariance
        Public p As Single()() = {New Single(1) {}, New Single(1) {}}
        Public gain As Single() = New Single(1) {} ' 2x1
    End Class
End Namespace
