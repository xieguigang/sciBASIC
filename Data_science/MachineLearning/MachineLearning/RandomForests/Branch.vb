#Region "Microsoft.VisualBasic::c5922e15b937867f5b478d5785ba5e08, Data_science\MachineLearning\MachineLearning\RandomForests\Branch.vb"

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

    '   Total Lines: 85
    '    Code Lines: 52 (61.18%)
    ' Comment Lines: 25 (29.41%)
    '    - Xml Docs: 92.00%
    ' 
    '   Blank Lines: 8 (9.41%)
    '     File Size: 2.91 KB


    '     Class Branch
    ' 
    '         Function: getClass, getMean, getMissClass, getMSE
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace RandomForests

    Public Class Branch

        Friend mean, mean_snp As Double
        Friend class_val As Integer
        ''' <summary>
        ''' 'F' for final branch
        ''' </summary>
        Friend status As String = " "
        Friend Feature, Child1, Child2, Parent As Integer
        Friend list As New List(Of Integer)()

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMean(phen As Double()) As Double

            Dim i = 0
            mean = 0.0R
            For i = 0 To list.Count - 1
                mean = mean + phen(list(i))
            Next
            mean = mean / list.Count
            Return mean
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getClass(phen As Double()) As Integer
            Dim i = 0
            Dim temp = New Integer(2) {}
            For i = 0 To list.Count - 1
                temp(phen(list(i))) += 1
            Next
            If temp(0) > temp(1) And temp(0) > temp(2) Then
                class_val = 0
            ElseIf temp(1) > temp(2) Then
                class_val = 1
            Else
                class_val = 2
            End If
            Return class_val
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMSE(phen As Double()) As Double
            Dim i = 0
            getMean(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + (phen(list(i)) - mean) * (phen(list(i)) - mean)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMissClass(phen As Double()) As Double
            Dim i = 0
            getClass(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + stdNum.Abs(CInt(phen(list(i))) - class_val)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function
    End Class
End Namespace
