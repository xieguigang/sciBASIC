﻿#Region "Microsoft.VisualBasic::d446095173d19ee6e59605a9801baaeb, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/SignalProcessing/SignalProcessing//FFT/TransformCore.vb"

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

    '   Total Lines: 37
    '    Code Lines: 9
    ' Comment Lines: 22
    '   Blank Lines: 6
    '     File Size: 1.38 KB


    '     Class TransformCore
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2017 - presented by Kei Nakai
'
' Original project is developed and published by OpenGamma Inc.
'
' Copyright (C) 2012 - present by OpenGamma Inc. and the OpenGamma group of companies
'
' Please see distribution for license.
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'     http://www.apache.org/licenses/LICENSE-2.0
'     
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.

Namespace FFT

    ''' <summary>
    ''' Core utility class to provide general utility methods
    ''' </summary>
    Friend Class TransformCore
        Public Shared ReadOnly THREADS_BEGIN_N_1D_FFT_2THREADS As Integer = 8192

        Public Shared ReadOnly THREADS_BEGIN_N_1D_FFT_4THREADS As Integer = 65536

        Public Shared ReadOnly THREADS_BEGIN_N_1D As Integer = 32768

        Public Shared ReadOnly THREADS_BEGIN_N_2D As Integer = 65536

        Public Shared ReadOnly THREADS_BEGIN_N_3D As Integer = 65536
    End Class
End Namespace
