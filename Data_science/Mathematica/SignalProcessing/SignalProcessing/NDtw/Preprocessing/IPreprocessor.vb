#Region "Microsoft.VisualBasic::28afe06872bb1a08c0b996f3203f7be4, Data_science\Mathematica\SignalProcessing\SignalProcessing\NDtw\Preprocessing\IPreprocessor.vb"

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

    '   Total Lines: 44
    '    Code Lines: 29 (65.91%)
    ' Comment Lines: 5 (11.36%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.43 KB


    '     Class IPreprocessor
    ' 
    '         Function: Centralization, None, Normalization, Standardization
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' signal processor for scale the signal intensity value
    ''' </summary>
    Public MustInherit Class IPreprocessor

        ''' <summary>
        ''' apply of the data processor function
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property [Function](data As Double()) As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Preprocess(data)
            End Get
        End Property

        Public MustOverride Function Preprocess(data As Double()) As Double()
        Public MustOverride Overrides Function ToString() As String

        <DebuggerStepThrough>
        Public Shared Function Centralization() As CentralizationPreprocessor
            Return New CentralizationPreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function None() As NonePreprocessor
            Return New NonePreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function Normalization() As NormalizationPreprocessor
            Return New NormalizationPreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function Standardization() As StandardizationPreprocessor
            Return New StandardizationPreprocessor
        End Function

    End Class

End Namespace
