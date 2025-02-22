#Region "Microsoft.VisualBasic::69a1815b362191cb2ab067259b14b036, Data_science\MachineLearning\MachineLearning\RandomForests\LossFunction.vb"

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

    '   Total Lines: 385
    '    Code Lines: 321 (83.38%)
    ' Comment Lines: 50 (12.99%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 14 (3.64%)
    '     File Size: 19.90 KB


    '     Class LossFunction
    ' 
    '         Function: getLossFunctionNode, getLossFunctionOOB, getLossFunctionSplit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

Namespace RandomForests

    ''' <summary>
    ''' This class provides a method to calculate the Loss function of a given attribute.
    ''' The method implements two sort of loss functions: 
    '''  	Info Gain (type=1) for classified covariates
    '''  	MSE (type=2)for continuous covariates.
    '''  	Pseudo-Huber Loss function (type=3).
    '''  	Cost function on misclassification (type=4) for classification problems.
    '''  	Gini index (type=5).
    ''' 
    '''  More Loss functions can be added in the future.
    ''' 
    ''' </summary>

    Public Class LossFunction

        Public Shared Function getLossFunctionNode(type As LF_c,
                                                   a As Branch,
                                                   phenotype As Double(),
                                                   Genotype As Double()(),
                                                   false_positive_cost As Double,
                                                   false_negative_cost As Double) As Double
            Dim i = 0
            Dim LF_val As Double = 0, mean As Double = 0
            Dim nn = 0
            Select Case type
                Case 1, LF_c.Information_Gain  'Information gain
                    LF_val = 0
                    Dim IO As Double = 0
                    IO = 0
                    Dim nIO = New Integer(1) {}
                    For i = 0 To a.list.Count - 1
                        nIO(phenotype(a.list(i))) += 1
                    Next
                    For i = 0 To 1
                        If nIO(i) > 0 Then
                            IO = IO - nIO(i) / CSng(a.list.Count) * (std.Log(nIO(i) / CSng(a.list.Count)) / std.Log(2))
                        End If
                    Next
                    LF_val = IO
                Case 2, LF_c.Mean_Squared_Error  'L2
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate mean for SNP j
                    mean = a.getMean(phenotype)
                    'Calculate mean squared error
                    For i = 0 To a.list.Count - 1
                        LF_val = LF_val + (phenotype(a.list(i)) - mean) * (phenotype(a.list(i)) - mean)
                        nn += 1
                    Next
                    LF_val = LF_val / CSng(nn)
                Case 3, LF_c.Pseudo_Huber  'pseudo-Huber loss function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate mean for SNP j
                    mean = a.getMean(phenotype)
                    'Calculate huber loss function
                    For i = 0 To a.list.Count - 1
                        LF_val = LF_val + std.Log(std.Cosh(phenotype(a.list(i)) - mean))
                        nn += 1
                    Next
                    LF_val = LF_val / CSng(nn)
                Case 4, LF_c.Personalized_Cost_Function_for_categories  'False Positive and False Negative cost function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0

                    'This is the cost variable of incorrectly classify individuals
                    Dim cost = New Double(1) {}
                    cost(0) = false_positive_cost 'Cost of a false positive (individual incorrectly assigned y_hat=1
                    cost(1) = false_negative_cost 'Cost of a false negative (individual incorrectly assigned y_hat=0

                    'Calculate mean for SNP j
                    mean = 0
                    For i = 0 To a.list.Count - 1
                        mean = mean + phenotype(a.list(i))
                        nn += 1
                    Next
                    mean = mean / CSng(nn)
                    For i = 0 To a.list.Count - 1
                        If CInt(phenotype(a.list(i))) <> CInt(mean) Then
                            LF_val = LF_val + cost(phenotype(a.list(i)))
                        End If
                        'LF_val=LF_val/(float)nn;
                    Next
                Case 5, LF_c.Gini_Index 'Gini index
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    Dim GI As Double = 0
                    Dim nGI = New Integer(2) {} 'nIG[phenotype_group]
                    nn = 0
                    'Calculate Gini index
                    For i = 0 To a.list.Count - 1
                        nGI(phenotype(a.list(i))) += 1
                        nn += 1
                    Next
                    GI = nGI(0) * nGI(1) + nGI(0) * nGI(2) + nGI(1) * nGI(2)
                    LF_val = GI / CSng(nn * nn) 'Wrong entered number
                Case Else
                    Throw New InvalidProgramException("Error!  Illegal option number of Loss Function type!  I quit!")

            End Select 'end of switch statement
            Return LF_val
        End Function

        Public Shared Function getLossFunctionSplit(type As LF_c,
                                                    snp As Integer,
                                                    a As Branch,
                                                    phenotype As Double(),
                                                    Genotype As Double()(),
                                                    false_positive_cost As Double,
                                                    false_negative_cost As Double) As Double
            Dim i = 0
            Dim LF_val As Double = 0, mean As Double = 0
            Dim mean_right = 0.0R, mean_left = 0.0R
            Dim n_right = 0, n_left = 0
            Select Case type
                Case 1, LF_c.Information_Gain   'Information gain
                    LF_val = 0
                    Dim IO As Double = 0, Ij As Double = 0
                    Dim nIG = RectangularArray.Matrix(Of Integer)(3, 3) 'nIG[genotype_group][phenotype_group]
                    IO = 0
                    Dim nIO = New Integer(1) {}
                    For i = 0 To a.list.Count - 1
                        nIO(phenotype(a.list(i))) += 1
                    Next
                    For i = 0 To 1
                        If nIO(i) > 0 Then
                            IO = IO - nIO(i) / CSng(a.list.Count) * (std.Log(nIO(i) / CSng(a.list.Count)) / std.Log(2))
                        End If
                    Next
                    '	Calculate Information gain for SNP j
                    For i = 0 To a.list.Count - 1
                        nIG(Genotype(a.list(i))(snp))(phenotype(a.list(i))) += 1
                    Next
                    LF_val = 0 'I0;
                    For i = 0 To 2
                        Ij = 0.0R
                        If nIG(i)(0) <> 0 Then
                            Ij = Ij - nIG(i)(0) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2)) * (std.Log(nIG(i)(0) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2))) / std.Log(2))
                        End If
                        If nIG(i)(1) <> 0 Then
                            Ij = Ij - nIG(i)(1) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2)) * (std.Log(nIG(i)(1) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2))) / std.Log(2))
                        End If
                        If nIG(i)(2) <> 0 Then
                            Ij = Ij - nIG(i)(2) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2)) * (std.Log(nIG(i)(2) / CSng(nIG(i)(0) + nIG(i)(1) + nIG(i)(2))) / std.Log(2))
                        End If
                        Ij = Ij * (nIG(i)(0) + nIG(i)(1) + nIG(i)(2)) / CSng(a.list.Count)
                        LF_val = LF_val - Ij
                    Next
                    LF_val = LF_val * -1
                Case 2, LF_c.Mean_Squared_Error  'L2
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate mean for SNP j
                    mean = 0
                    For i = 0 To a.list.Count - 1
                        mean = mean + Genotype(a.list(i))(snp) / a.list.Count
                    Next
                    'Calculate mean squared error
                    mean_right = 0.0R
                    mean_left = 0.0R
                    n_right = 0
                    n_left = 0
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            mean_right = mean_right + phenotype(a.list(i))
                            n_right += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            mean_left = mean_left + phenotype(a.list(i))
                            n_left += 1
                        End If
                    Next
                    mean_right = mean_right / n_right
                    mean_left = mean_left / n_left
                    'Calculate MSE for SNP j
                    Dim nn = 0
                    Dim temp = 0.0R
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            temp = phenotype(a.list(i))
                            LF_val = LF_val + (temp - mean_right) * (temp - mean_right)
                            nn += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            temp = phenotype(a.list(i))
                            LF_val = LF_val + (temp - mean_left) * (temp - mean_left)
                            nn += 1
                        End If
                    Next
                    LF_val = LF_val / CSng(nn)
                Case 3, LF_c.Pseudo_Huber  'pseudo-Huber loss function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate mean for SNP j
                    mean = 0
                    For i = 0 To a.list.Count - 1
                        mean = mean + Genotype(a.list(i))(snp) / a.list.Count
                    Next
                    'Calculate mean squared error
                    mean_right = 0.0R
                    mean_left = 0.0R
                    n_right = 0
                    n_left = 0
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            mean_right = mean_right + phenotype(a.list(i))
                            n_right += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            mean_left = mean_left + phenotype(a.list(i))
                            n_left += 1
                        End If
                    Next
                    mean_right = mean_right / n_right
                    mean_left = mean_left / n_left
                    'Calculate MSE for SNP j
                    Dim nn = 0
                    Dim temp = 0.0R
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            temp = phenotype(a.list(i))
                            LF_val = LF_val + std.Log(std.Cosh(temp - mean_right))
                            nn += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            temp = phenotype(a.list(i))
                            LF_val = LF_val + std.Log(std.Cosh(temp - mean_left))
                            nn += 1
                        End If
                    Next
                    LF_val = LF_val / CSng(nn)
                Case 4, LF_c.Personalized_Cost_Function_for_categories  'False Positive and False Negative cost function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0

                    'This is the cost variable of incorrectly classify individuals
                    Dim cost = New Double(1) {}
                    cost(0) = false_positive_cost 'Cost of a false positive (individual incorrectly assigned y_hat=1
                    cost(1) = false_negative_cost 'Cost of a false negative (individual incorrectly assigned y_hat=0

                    'Calculate mean for SNP j
                    mean = 0
                    For i = 0 To a.list.Count - 1
                        mean = mean + Genotype(a.list(i))(snp) / a.list.Count
                    Next
                    'Calculate mean squared error
                    mean_right = 0.0R
                    mean_left = 0.0R
                    n_right = 0
                    n_left = 0
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            mean_right = mean_right + phenotype(a.list(i))
                            n_right += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            mean_left = mean_left + phenotype(a.list(i))
                            n_left += 1
                        End If
                    Next
                    mean_right = std.Round(mean_right / n_right)
                    mean_left = std.Round(mean_left / n_left)
                    'Calculate cost function for SNP j
                    Dim nn = 0
                    Dim temp = 0.0R
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            If CInt(phenotype(a.list(i))) <> CInt(mean_right) Then
                                LF_val = LF_val + cost(phenotype(a.list(i))) ' /(float)n_right;
                            End If
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            If CInt(phenotype(a.list(i))) <> CInt(mean_left) Then
                                LF_val = LF_val + cost(phenotype(a.list(i))) ' /(float)n_left;
                            End If
                        End If
                        'LF_val=0.5d*LF_val;
                    Next
                Case 5, LF_c.Gini_Index    'Gini index
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    Dim GI As Double = 0, i_left As Double = 0, i_right As Double = 0
                    Dim nGI = RectangularArray.Matrix(Of Integer)(3, 2) 'nIG[phenotype_group][child_node]
                    n_right = 0
                    n_left = 0
                    'Calculate mean for SNP j
                    mean = 0
                    For i = 0 To a.list.Count - 1
                        mean = mean + Genotype(a.list(i))(snp) / a.list.Count
                    Next
                    'Calculate Gini index
                    For i = 0 To a.list.Count - 1
                        If Genotype(a.list(i))(snp) <= mean Then
                            nGI(phenotype(a.list(i)))(0) += 1
                            n_left += 1
                        ElseIf Genotype(a.list(i))(snp) > mean Then
                            nGI(phenotype(a.list(i)))(1) += 1
                            n_right += 1
                        End If
                    Next
                    i_left = nGI(0)(0) * nGI(1)(0) + nGI(0)(0) * nGI(2)(0) + nGI(1)(0) * nGI(2)(0)
                    i_right = nGI(0)(1) * nGI(1)(1) + nGI(0)(1) * nGI(2)(1) + nGI(1)(1) * nGI(2)(1)
                    GI = 0.5R * i_left / CSng(n_left * n_left) + 0.5R * i_right / CSng(n_right * n_right)
                    LF_val = GI 'Wrong entered number
                Case Else
                    Throw New InvalidProgramException("Error!  Illegal option number of Loss Function type!  I quit!")
            End Select 'end of switch statement
            Return LF_val
        End Function

        Public Shared Function getLossFunctionOOB(type As LF_c,
                                                  a As Branch,
                                                  phenotype As Double(),
                                                  yhat As Double,
                                                  false_positive_cost As Double,
                                                  false_negative_cost As Double) As Double
            Dim i = 0
            Dim LF_val As Double = 0
            Dim nn = 0
            Select Case type
                Case 1, LF_c.Information_Gain 'Information gain
                    LF_val = 0
                    Dim IO As Double = 0
                    IO = 0
                    Dim nIO = New Integer(1) {}
                    For i = 0 To a.list.Count - 1
                        nIO(phenotype(a.list(i))) += 1
                    Next
                    For i = 0 To 1
                        If nIO(i) > 0 Then
                            IO = IO - nIO(i) / CSng(a.list.Count) * (std.Log(nIO(i) / CSng(a.list.Count)) / std.Log(2))
                        End If
                    Next
                    LF_val = IO
                Case 2, LF_c.Mean_Squared_Error 'L2
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate mean squared error
                    For i = 0 To a.list.Count - 1
                        LF_val = LF_val + (phenotype(a.list(i)) - yhat) * (phenotype(a.list(i)) - yhat)
                    Next
                Case 3, LF_c.Pseudo_Huber 'pseudo-Huber loss function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    'Calculate huber loss function
                    For i = 0 To a.list.Count - 1
                        LF_val = LF_val + std.Log(std.Cosh(phenotype(a.list(i)) - yhat))
                    Next
                Case 4, LF_c.Personalized_Cost_Function_for_categories 'False Positive and False Negative cost function
                    'read the IG for each SNPs in the sequences
                    LF_val = 0

                    'This is the cost variable of incorrectly classify individuals
                    Dim cost = New Double(1) {}
                    cost(0) = false_positive_cost 'Cost of a false positive (individual incorrectly assigned y_hat=1
                    cost(1) = false_negative_cost 'Cost of a false negative (individual incorrectly assigned y_hat=0

                    For i = 0 To a.list.Count - 1
                        If CInt(phenotype(a.list(i))) <> CInt(yhat) Then
                            LF_val = LF_val + cost(phenotype(a.list(i)))
                        End If
                    Next
                Case 5, LF_c.Gini_Index 'Gini index
                    'read the IG for each SNPs in the sequences
                    LF_val = 0
                    Dim GI As Double = 0
                    Dim nGI = New Integer(2) {} 'nIG[phenotype_group]
                    nn = 0
                    'Calculate Gini index
                    For i = 0 To a.list.Count - 1
                        nGI(phenotype(a.list(i))) += 1
                        nn += 1
                    Next
                    GI = nGI(0) * nGI(1) + nGI(0) * nGI(2) + nGI(1) * nGI(2)
                    LF_val = GI / CSng(nn * nn)
                    If nn = 0 Then
                        LF_val = 0
                    End If 'Wrong entered number

                Case Else
                    Throw New InvalidProgramException("Error!  Illegal option number of Loss Function type!  I quit!")
            End Select 'end of switch statement
            Return LF_val
        End Function
    End Class
End Namespace
