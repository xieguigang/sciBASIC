Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace RandomForests

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Title:            Random Forest for classified and regression problems 
    ' Files:            RanFog.java; Branch.java; Permutator.java
    '
    ' Author:           Oscar Gonzalez-Recio 
    ' email: 			 gonzalez.oscar@inia.es
    '
    ' 					 Madrid, 2010
    '
    ''''''''''''''''''''''''''' 80 columns wide //////////////////////////////////

    Public Class RanFog

        Public Property max_tree As Integer
        Public Property N_tot As Integer
        Public Property max_branch As Integer

        ''' <summary>
        ''' Program execution starts here. 
        ''' This method construct a random forest (Breiman, 2001. Machine Learning, 45)
        ''' for classification data (should be score as 0 or 1).
        '''  Results are written to files:
        '''    "Trees.txt" stores the miss-classification rate in the training set and the oob set at each tree
        '''    "Trees.test" stores the miss-classification rate in the testing set at each tree
        '''    "Variable_Importance.txt" stores the importance variable for each feature
        '''    "TimesSelected.txt" stores the number of times each feature was selected
        ''' 
        ''' The methods required a parameter file called 'params.txt' that must be located in the same folder as RanFog
        ''' The main method loads different parameters from this file 
        ''' 
        ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        '''   Load parameter file                                                           
        ''' </summary>
        ''' <param name="demoProperties">"params.txt"</param>
        Public Sub Run(demoProperties As Dictionary(Of String, String))
            Dim n_tree As Integer = 0
            Dim j, k, i, n_branch, N_oob, MSE_tree, MSEval_tree, MSE_oob As Integer

            While n_tree < max_tree
                j = 0
                k = 0
                i = 0
                n_branch = 0
                N_oob = 0
                MSE_tree = 0
                MSEval_tree = 0
                MSE_oob = 0

                Dim oob = New Integer(N_tot - 1) {}
                Dim SNP_tree As List(Of Integer?) = New List(Of Integer?)()
                Dim branch = New Branch(max_branch - 1) {}
                Dim branch_tst = New Branch(max_branch - 1) {}
                Dim branch_oob = New Branch(max_branch - 1) {}
                branch(n_branch) = New Branch()
                branch_tst(n_branch) = New Branch()
                branch_oob(n_branch) = New Branch()

                'Get bootstrapped sample from data, and store pointer-positions in the list of branch zero
                For i = 0 To N_tot - 1
                    Dim u = randf.Next(N_tot)
                    branch(n_branch).list.Insert(i, u)
                Next
                'Get out of bag data
                For i = 0 To N_tot - 1
                    oob(branch(n_branch).list(i)) = 1
                Next
                For i = 0 To N_tot - 1
                    If oob(i) = 0 Then
                        branch_oob(n_branch).list.Add(i)
                        N_oob += 1
                    End If
                Next
                Dim pointer_oob = New Integer(N_tot - 1) {}
                For i = 0 To N_tot - 1 'Store positions of oob observations
                    If oob(i) = 0 Then
                        pointer_oob(k) = i
                        k += 1
                    End If
                Next
                k = 0
                'Store pointer-positions of testing set in the branch zero for testing set.
                For i = 0 To N_tst - 1
                    branch_tst(n_branch).list.Insert(i, i)
                Next
                'Construct the tree. Grow branches until size<5 or not better classification is achieved	
                For k = 0 To n_branch + 1 - 1
                    If branch(k).list.Count > 5 Then 'Minimum size=5
                        node = N_attributes
                        minLoss = 99999999
                        'Calculate Entropy in branch[k]
                        node_mse = 0
                        node_mse = LossFunction.getLossFunctionNode(LF_c, branch(k), phenotype, Genotype, false_positive_cost, false_negative_cost)

                        For jj = 0 To m - 1 'calculate MSE, and select that SNP minimizing MSE
                            j = rand.Next(N_attributes)
                            'if (x1.nextDouble()< m/(1.d*N_attributes) ){ //Select only sqrt(N_attributes) variables
                            Loss = LossFunction.getLossFunctionSplit(LF_c, j, branch(k), phenotype, Genotype, false_positive_cost, false_negative_cost)
                            If Loss < minLoss Then 'For non-classified attributes
                                'Calculate mean for SNP j
                                mean_j = 0
                                For i = 0 To branch(k).list.Count - 1
                                    mean_j = mean_j + Genotype(branch(k).list(i))(j) / branch(k).list.Count
                                Next
                                FeatureSel(0) = j
                                minLoss = Loss
                            End If

                            'Decide keeping the classified or the non-classified attribute
                            If node_mse <= minLoss Then
                                node = N_attributes
                            Else
                                'Select non-classified Feature, and calculate its mean
                                mean_j = 0
                                For i = 0 To branch(k).list.Count - 1
                                    mean_j = mean_j + Genotype(branch(k).list(i))(FeatureSel(0)) / branch(k).list.Count
                                Next
                                node = FeatureSel(0)
                                branch(k).Feature = FeatureSel(0)
                                branch(k).mean_snp = mean_j
                            End If
                            '}
                        Next
                        If node <> N_attributes Then 'Create a new branch, only if MSE of the previous branch is minimized
                            Selected(node) += 1
                            SNP_tree.Add(node) 'add the selected SNP to the end of the list

                            'Create right branch for the bootstrapped sample of the training set and for the testing set 
                            n_branch += 1
                            branch(k).Child1 = n_branch
                            branch(n_branch) = New Branch()
                            branch_tst(n_branch) = New Branch()
                            For i = 0 To branch(k).list.Count - 1
                                If Genotype(branch(k).list(i))(node) <= branch(k).mean_snp Then
                                    branch(n_branch).list.Add(branch(k).list(i))
                                End If
                            Next
                            For i = 0 To branch_tst(k).list.Count - 1
                                If Genotype_tst(branch_tst(k).list(i))(node) <= branch(k).mean_snp Then
                                    branch_tst(n_branch).list.Add(branch_tst(k).list(i))
                                End If
                            Next

                            'Create left branch for the bootstrapped sample of the training set and for the testing set 
                            n_branch += 1
                            branch(k).Child2 = n_branch
                            branch(n_branch) = New Branch()
                            branch_tst(n_branch) = New Branch()
                            For i = 0 To branch(k).list.Count - 1
                                If Genotype(branch(k).list(i))(node) > branch(k).mean_snp Then
                                    branch(n_branch).list.Add(branch(k).list(i))
                                End If
                            Next
                            For i = 0 To branch_tst(k).list.Count - 1
                                If Genotype_tst(branch_tst(k).list(i))(node) > branch(k).mean_snp Then
                                    branch_tst(n_branch).list.Add(branch_tst(k).list(i))
                                End If
                                'outBranch.print( branch[k].snp+" "+branch[k].child1+" "+branch[k].child2+" "+branch[k].getMean(phenotype)+" " );
                            Next 'No-SNP has reduced miss-classification of previous branch
                        Else
                            branch(k).status = "F" 'the branch is set as dead-end branch
                            'outBranchMSE.print( branch[k].list.size()+" "+branch[k].getMSE(phenotype)+" " );
                            MSE_tree = MSE_tree + branch(k).getMSE(phenotype)
                            For i = 0 To branch(k).list.Count - 1 'Accumulate classification to estimate genomic value
                                GEBV(branch(k).list(i))(0) = GEBV(branch(k).list(i))(0) + branch(k).getMean(phenotype)
                                GEBV(branch(k).list(i))(1) += 1
                            Next
                            For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean of the corresponding branch in the training bootstrapped sample
                                y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + branch(k).getMean(phenotype)
                            Next
                        End If 'decision on creating a new branch
                        'If branch size is <=5 stop growing the tree
                    Else
                        'outBranchMSE.print( branch[k].list.size()+" "+branch[k].getMSE(phenotype)+" " );
                        branch(k).status = "F" 'the branch is set as dead-end branch
                        If branch(k).list.Count <> 0 Then
                            MSE_tree = MSE_tree + branch(k).getMSE(phenotype)
                        End If
                        For i = 0 To branch(k).list.Count - 1 'Accumulate classification to estimate genomic value
                            GEBV(branch(k).list(i))(0) = GEBV(branch(k).list(i))(0) + branch(k).getMean(phenotype)
                            GEBV(branch(k).list(i))(1) += 1
                        Next
                        If branch_tst(k).list.Count <> 0 AndAlso branch(k).list.Count <> 0 Then
                            temp = branch(k).getMean(phenotype)
                            For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean of the corresponding branch in the training bootstrapped sample
                                y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + temp
                            Next 'if training branch has size=0
                        Else
                            temp = branch(0).getMean(phenotype)
                            For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean branch 0
                                y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + temp
                            Next
                        End If
                    End If 'checking branch size
                Next 'for over n_branch

                'Construct the oob-tree following nodes selected previously, and calculate miss-classification rate in the oob sample
                MSE_oob = 0
                For k = 0 To n_branch + 1 - 1
                    If branch(k).status.CompareTo("F") <> 0 Then
                        branch_oob(branch(k).Child1) = New Branch()
                        branch_oob(branch(k).Child2) = New Branch()
                        For i = 0 To branch_oob(k).list.Count - 1
                            If Genotype(branch_oob(k).list(i))(branch(k).Feature) <= branch(k).mean_snp Then
                                branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                            Else
                                branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                            End If
                        Next
                    Else
                        If branch(k).list.Count = 0 Then
                            temp = branch(0).getMean(phenotype)
                        Else
                            temp = branch(k).getMean(phenotype)
                        End If
                        MSE_oob = MSE_oob + LossFunction.getLossFunctionOOB(LF_c, branch_oob(k), phenotype, temp, false_positive_cost, false_negative_cost) 'Math.abs(phenotype[branch_oob[k].list.get(i)]-temp);
                    End If
                Next
                k = 0
                MSE_oob = MSE_oob / N_oob
                MSE_oob_ave = MSE_oob_ave + MSE_oob

                'Calculate VARIABLE IMPORTANCE using the oob set, and permutating corresponding feature observations
                Dim pointer_oob_perm = Permutator.permute(pointer_oob)
                Dim pointer_perm = New Integer(N_tot - 1) {}
                For i = 0 To N_tot - 1 'Get out-of-bag data
                    If oob(i) = 0 Then
                        pointer_perm(i) = pointer_oob_perm(k)
                        k += 1
                    End If
                Next
                k = 0
                For j = 0 To N_attributes - 1
                    MSE_vi = 0
                    For k = 0 To n_branch + 1 - 1 'Construct the oob-tree
                        If branch(k).Child1 <> 0 OrElse branch(k).Child2 <> 0 Then
                            branch_oob(branch(k).Child1) = New Branch()
                            branch_oob(branch(k).Child2) = New Branch()
                            For i = 0 To branch_oob(k).list.Count - 1
                                If branch(k).Feature = j Then
                                    If Genotype(pointer_perm(branch_oob(k).list(i)))(branch(k).Feature) <= branch(k).mean_snp Then
                                        branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                                    Else
                                        branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                                    End If
                                Else
                                    If Genotype(branch_oob(k).list(i))(branch(k).Feature) <= branch(k).mean_snp Then
                                        branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                                    Else
                                        branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                                    End If
                                End If
                            Next
                        Else
                            If branch(k).list.Count = 0 Then
                                temp = branch(0).getMean(phenotype)
                            Else
                                temp = branch(k).getMean(phenotype)
                            End If
                            MSE_vi = MSE_vi + LossFunction.getLossFunctionOOB(LF_c, branch_oob(k), phenotype, temp, false_positive_cost, false_negative_cost) 'Math.abs(phenotype[branch_oob[k].list.get(i)]-temp);
                        End If
                    Next
                    MSE_vi = MSE_vi / N_oob 'compare miss-classification rate permuting Feature j on MSE_oob
                    VI(j) = VI(j) + (MSE_vi - MSE_oob) / max_tree 'Add variable importance and average over total trees
                Next

                'Calculate miss-classification rate at this tree for the testing set
                MSEval_tree = 0
                For i = 0 To N_tst - 1
                    MSEval_tree = MSEval_tree + (phenotype_tst(i) - y_hat(i) / (n_tree + 1)) * (phenotype_tst(i) - y_hat(i) / (n_tree + 1))
                Next
                Console.WriteLine("Iteration #" & n_tree + 1 & ";MSE in testing set=" & MSEval_tree / N_tst)
                Console.WriteLine("average Loss Function in OOB=" & MSE_oob_ave / CSng(n_tree + 1) & "; N_oob=" & N_oob)
                outTree.WriteLine(MSE_oob_ave / CSng(n_tree + 1) & " " & MSE_oob)
                outTreeTest.WriteLine(MSEval_tree / N_tst)
                n_tree += 1 'go to next tree
            End While 'over n_tree
            ''' <summary>
            '''   2. Ends the forest 
            '''   
            ''' </summary>

            Console.WriteLine("Writing output files")
            outTree.Close()
            outTreeTest.Close()

            'Prepare the output files and its format

            'Write file with number of times each Feature was selected and its relative importance 
            For j = 0 To N_attributes - 1 'for each Feature
                outSel.WriteLine(j + 1 & " " & Selected(j))
                outVI.WriteLine(j + 1 & " " & (VI(j)))
            Next
            For i = 0 To N_tot - 1 'Predicted GBV in training set
                outEGBV.WriteLine(ID(i) & " " & (GEBV(i)(0) / CSng(GEBV(i)(1))))
            Next
            For i = 0 To N_tst - 1 'Predicted GBV in testing set
                outPred.WriteLine(ID_tst(i) & " " & (y_hat(i) / (n_tree + 1)))
            Next
            outSel.Close()
            outVI.Close()
            outPred.Close()
            outEGBV.Close()
            Console.WriteLine("TERMINATED WITHOUT ERRORS")
            Console.WriteLine("Random Forest algorithm for regression and classification problems (Ver.Beta)")
            Console.WriteLine("by Oscar Gonzalez-Recio (2019) ")
            GC.WaitForPendingFinalizers()
        End Sub ' end main method
    End Class 'end program
End Namespace


