﻿#Region "Microsoft.VisualBasic::9df0cafebad54944ebd07fbb43cd6231, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/SignalProcessing/SignalProcessing//NDtw/Preprocessing/NonePreprocessor.vb"

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

    '   Total Lines: 28
    '    Code Lines: 12
    ' Comment Lines: 11
    '   Blank Lines: 5
    '     File Size: 740 B


    '     Class NonePreprocessor
    ' 
    '         Function: Preprocess, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' signal data processor that do nothing
    ''' 
    ''' f(x) = x
    ''' </summary>
    Public Class NonePreprocessor : Inherits IPreprocessor

        ''' <summary>
        ''' do nothing at here
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Preprocess(data As Double()) As Double()
            Return data
        End Function

        Public Overrides Function ToString() As String
            Return "None"
        End Function
    End Class
End Namespace

