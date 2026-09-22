#Region "Microsoft.VisualBasic::b89d6d4e057dd4ed128cca9c5ce636c9, Data_science\MachineLearning\MachineLearning\RandomForests\RanFog.vb"

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

    '   Total Lines: 527
    '    Code Lines: 256 (48.58%)
    ' Comment Lines: 223 (42.31%)
    '    - Xml Docs: 51.12%
    ' 
    '   Blank Lines: 48 (9.11%)
    '     File Size: 26.82 KB


    '     Class RanFog
    ' 
    '         Properties: false_negative_cost, false_positive_cost, LF_c, max_branch, max_tree
    '                     mtry, Selected, Trees, VI
    ' 
    '         Function: Predict, Run, Tree
    ' 
    '     Class Result
    ' 
    '         Properties: data, Model, MSE_oob, outGEBV
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
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

    ''' <summary>
    ''' Random Forest for classified and regression problems 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/ogrecio/RanFog
    ''' </remarks>
    Public Class RanFog : Inherits MachineLearning.Model

        ''' <summary>
        ''' [ForestSize]Max number of trees to be constructed
        ''' </summary>
        ''' <returns>The number of the decision trees in the forest, the default value is ``500``.</returns>
        Public Property max_tree As Integer = 500
        ''' <summary>
        ''' Max number of branches allowed
        ''' </summary>
        ''' <returns>The maximum number of the branch nodes of each tree, the default value is ``2000``.</returns>
        Public Property max_branch As Integer = 2000
        ''' <summary>
        ''' [mtry]
        ''' Number of Features randomly selected at each node,
        ''' Percentage of Features randomly selected at each node
        ''' </summary>
        ''' <returns>The number of the features which are randomly tested at each node, the default value is ``100``.</returns>
        Public Property mtry As Integer = 100
        ''' <summary>
        ''' [LossFunction]
        ''' Loss function used for continuous features
        ''' </summary>
        ''' <returns>
        ''' A <see cref="LF_c"/> value, the default value is 
        ''' <see cref="LF_c.Mean_Squared_Error"/>.
        ''' </returns>
        Public Property LF_c As LF_c = LF_c.Mean_Squared_Error

        ''' <summary>
        ''' The cost of a false positive (an individual is incorrectly assigned 
        ''' as ``y_hat = 1``), it is only used by the 
        ''' <see cref="LF_c.Personalized_Cost_Function_for_categories"/> loss.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> cost value, the default value is ``1``.</returns>
        Public Property false_positive_cost As Double = 1
        ''' <summary>
        ''' The cost of a false negative (an individual is incorrectly assigned 
        ''' as ``y_hat = 0``), it is only used by the 
        ''' <see cref="LF_c.Personalized_Cost_Function_for_categories"/> loss.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> cost value, the default value is ``1``.</returns>
        Public Property false_negative_cost As Double = 1

        ''' <summary>
        ''' variable importance
        ''' </summary>
        ''' <returns>
        ''' The averaged relative importance value of each feature, which is 
        ''' measured by the increase of the out-of-bag error after the feature 
        ''' value was permuted.
        ''' </returns>
        ''' <remarks>
        ''' Write file with number of times each Feature was selected and its relative importance 
        ''' </remarks>
        Public Property VI As Double()
        ''' <summary>
        ''' number of times SNPs are selected
        ''' </summary>
        ''' <returns>The number of the times which each feature was selected for splitting a branch.</returns>
        ''' <remarks>
        ''' Write file with number of times each Feature was selected and its relative importance 
        ''' </remarks>
        Public Property Selected As Integer()

        ''' <summary>
        ''' The trained tree structures of the current forest.
        ''' 
        ''' Each element is a trimmed branch array holding ``n_branch + 1`` nodes,
        ''' and the node indices stored in <see cref="Branch.Child1"/> /
        ''' <see cref="Branch.Child2"/> refer to the position inside the same array.
        ''' The array is kept after <see cref="Run"/> returns so that downstream
        ''' analysis (for example TreeSHAP based model interpretation) can walk
        ''' the actual trees instead of treating the forest as a black box.
        ''' </summary>
        ''' <returns>A list of the branch arrays, each element describes one decision tree.</returns>
        Public Property Trees As New List(Of Branch())

        Private Function Tree(n_tree As Integer, train As Data, GEBV As Double()(), ByRef MSE_oob_ave As Double) As (Double, Double)
            'Variables involved in the trees
            Dim mean_j, minLoss, MSE_tree, MSEval_tree, node_mse, temp As Double
            Dim node As Integer
            Dim j = 0
            Dim k = 0
            Dim i = 0
            ' Feature selected at each node [regression feature,classified features]
            Dim FeatureSel As Integer
            'Information gain variables
            Dim Loss As Double = 0
            Dim n_branch = 0
            Dim N_oob = 0
            Dim MSE_oob As Double = 0
            Dim MSE_vi As Double
            Dim N_tot = train.N_tot
            Dim N_attributes = train.N_attributes
            Dim oob = New Integer(N_tot - 1) {}
            Dim SNP_tree As List(Of Integer?) = New List(Of Integer?)()
            Dim branch = New Branch(max_branch - 1) {}
            ' Dim branch_tst = New Branch(max_branch - 1) {}
            Dim branch_oob = New Branch(max_branch - 1) {}
            branch(n_branch) = New Branch()
            ' branch_tst(n_branch) = New Branch()
            branch_oob(n_branch) = New Branch()

            'Get bootstrapped sample from data, and store pointer-positions in the list of branch zero
            For i = 0 To N_tot - 1
                branch(n_branch).list.Insert(i, randf.Next(N_tot))
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
            'For i = 0 To N_tst - 1
            '    branch_tst(n_branch).list.Insert(i, i)
            'Next
            'Construct the tree. Grow branches until size<5 or not better classification is achieved	
            ' NOTE: the number of branches grows while the tree is being built,
            ' therefore the loop condition must be re-evaluated on each iteration.
            k = 0
            While k <= n_branch AndAlso k < max_branch
                If branch(k).list.Count > 5 Then 'Minimum size=5
                    node = N_attributes
                    minLoss = Double.MaxValue
                    'Calculate Entropy in branch[k]
                    node_mse = 0
                    node_mse = LossFunction.getLossFunctionNode(LF_c, branch(k), train.phenotype, train.Genotype, false_positive_cost, false_negative_cost)

                    For jj = 0 To mtry - 1 'calculate MSE, and select that SNP minimizing MSE
                        j = randf.Next(N_attributes)
                        'if (x1.nextDouble()< m/(1.d*N_attributes) ){ //Select only sqrt(N_attributes) variables
                        Loss = LossFunction.getLossFunctionSplit(LF_c, j, branch(k), train.phenotype, train.Genotype, false_positive_cost, false_negative_cost)
                        If Loss < minLoss Then 'For non-classified attributes
                            'Calculate mean for SNP j
                            mean_j = 0
                            For i = 0 To branch(k).list.Count - 1
                                mean_j = mean_j + train.Genotype(branch(k).list(i))(j) / branch(k).list.Count
                            Next
                            FeatureSel = j
                            minLoss = Loss
                        End If

                        'Decide keeping the classified or the non-classified attribute
                        If node_mse <= minLoss Then
                            node = N_attributes
                        Else
                            'Select non-classified Feature, and calculate its mean
                            mean_j = 0
                            For i = 0 To branch(k).list.Count - 1
                                mean_j = mean_j + train.Genotype(branch(k).list(i))(FeatureSel) / branch(k).list.Count
                            Next
                            node = FeatureSel
                            branch(k).Feature = FeatureSel
                            branch(k).mean_snp = mean_j
                        End If
                        '}
                    Next
                    ' 只有在分支数组之中还有空间容纳两个子节点的时候才进行分裂，
                    ' 否则让当前节点保持为叶节点，避免数组越界访问
                    If node <> N_attributes AndAlso n_branch + 2 < max_branch Then 'Create a new branch, only if MSE of the previous branch is minimized
                        Selected(node) += 1
                        SNP_tree.Add(node) 'add the selected SNP to the end of the list

                        'Create right branch for the bootstrapped sample of the training set and for the testing set 
                        n_branch += 1
                        branch(k).Child1 = n_branch
                        branch(n_branch) = New Branch()
                        ' branch_tst(n_branch) = New Branch()
                        For i = 0 To branch(k).list.Count - 1
                            If train.Genotype(branch(k).list(i))(node) <= branch(k).mean_snp Then
                                branch(n_branch).list.Add(branch(k).list(i))
                            End If
                        Next
                        'For i = 0 To branch_tst(k).list.Count - 1
                        '    If tst.Genotype(branch_tst(k).list(i))(node) <= branch(k).mean_snp Then
                        '        branch_tst(n_branch).list.Add(branch_tst(k).list(i))
                        '    End If
                        'Next

                        'Create left branch for the bootstrapped sample of the training set and for the testing set 
                        n_branch += 1
                        branch(k).Child2 = n_branch
                        branch(n_branch) = New Branch()
                        ' branch_tst(n_branch) = New Branch()
                        For i = 0 To branch(k).list.Count - 1
                            If train.Genotype(branch(k).list(i))(node) > branch(k).mean_snp Then
                                branch(n_branch).list.Add(branch(k).list(i))
                            End If
                        Next
                        'For i = 0 To branch_tst(k).list.Count - 1
                        '    If tst.Genotype(branch_tst(k).list(i))(node) > branch(k).mean_snp Then
                        '        branch_tst(n_branch).list.Add(branch_tst(k).list(i))
                        '    End If
                        '    'outBranch.print( branch[k].snp+" "+branch[k].child1+" "+branch[k].child2+" "+branch[k].getMean(phenotype)+" " );
                        'Next 'No-SNP has reduced miss-classification of previous branch
                    Else
                        branch(k).status = "F" 'the branch is set as dead-end branch
                        'outBranchMSE.print( branch[k].list.size()+" "+branch[k].getMSE(phenotype)+" " );
                        MSE_tree = MSE_tree + branch(k).getMSE(train.phenotype)
                        For i = 0 To branch(k).list.Count - 1 'Accumulate classification to estimate genomic value
                            GEBV(branch(k).list(i))(0) = GEBV(branch(k).list(i))(0) + branch(k).getMean(train.phenotype)
                            GEBV(branch(k).list(i))(1) += 1
                        Next
                        'For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean of the corresponding branch in the training bootstrapped sample
                        '    y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + branch(k).getMean(train.phenotype)
                        'Next
                    End If 'decision on creating a new branch
                    'If branch size is <=5 stop growing the tree
                Else
                    'outBranchMSE.print( branch[k].list.size()+" "+branch[k].getMSE(phenotype)+" " );
                    branch(k).status = "F" 'the branch is set as dead-end branch
                    If branch(k).list.Count <> 0 Then
                        MSE_tree = MSE_tree + branch(k).getMSE(train.phenotype)
                    End If
                    For i = 0 To branch(k).list.Count - 1 'Accumulate classification to estimate genomic value
                        GEBV(branch(k).list(i))(0) = GEBV(branch(k).list(i))(0) + branch(k).getMean(train.phenotype)
                        GEBV(branch(k).list(i))(1) += 1
                    Next
                    'If branch_tst(k).list.Count <> 0 AndAlso branch(k).list.Count <> 0 Then
                    '    temp = branch(k).getMean(train.phenotype)
                    '    For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean of the corresponding branch in the training bootstrapped sample
                    '        y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + temp
                    '    Next 'if training branch has size=0
                    'Else
                    '    temp = branch(0).getMean(train.phenotype)
                    '    For i = 0 To branch_tst(k).list.Count - 1 'Predict phenotypes in the testing set as mean branch 0
                    '        y_hat(branch_tst(k).list(i)) = y_hat(branch_tst(k).list(i)) + temp
                    '    Next
                    'End If
                End If 'checking branch size
                k += 1
            End While 'for over n_branch

            'Construct the oob-tree following nodes selected previously, and calculate miss-classification rate in the oob sample
            MSE_oob = 0
            For k = 0 To n_branch + 1 - 1
                If branch(k).status.CompareTo("F") <> 0 Then
                    branch_oob(branch(k).Child1) = New Branch()
                    branch_oob(branch(k).Child2) = New Branch()
                    For i = 0 To branch_oob(k).list.Count - 1
                        If train.Genotype(branch_oob(k).list(i))(branch(k).Feature) <= branch(k).mean_snp Then
                            branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                        Else
                            branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                        End If
                    Next
                Else
                    If branch(k).list.Count = 0 Then
                        temp = branch(0).getMean(train.phenotype)
                    Else
                        temp = branch(k).getMean(train.phenotype)
                    End If
                    MSE_oob = MSE_oob + LossFunction.getLossFunctionOOB(LF_c, branch_oob(k), train.phenotype, temp, false_positive_cost, false_negative_cost) 'std.Abs(phenotype[branch_oob[k].list.get(i)]-temp);
                End If
            Next
            k = 0
            MSE_oob = MSE_oob / N_oob
            MSE_oob_ave = MSE_oob_ave + MSE_oob

            'Calculate VARIABLE IMPORTANCE using the oob set, and permutating corresponding feature observations
            Dim pointer_oob_perm = Permutator.Permute(pointer_oob)
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
                                If train.Genotype(pointer_perm(branch_oob(k).list(i)))(branch(k).Feature) <= branch(k).mean_snp Then
                                    branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                                Else
                                    branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                                End If
                            Else
                                If train.Genotype(branch_oob(k).list(i))(branch(k).Feature) <= branch(k).mean_snp Then
                                    branch_oob(branch(k).Child1).list.Add(branch_oob(k).list(i))
                                Else
                                    branch_oob(branch(k).Child2).list.Add(branch_oob(k).list(i))
                                End If
                            End If
                        Next
                    Else
                        If branch(k).list.Count = 0 Then
                            temp = branch(0).getMean(train.phenotype)
                        Else
                            temp = branch(k).getMean(train.phenotype)
                        End If
                        MSE_vi = MSE_vi + LossFunction.getLossFunctionOOB(LF_c, branch_oob(k), train.phenotype, temp, false_positive_cost, false_negative_cost) 'std.Abs(phenotype[branch_oob[k].list.get(i)]-temp);
                    End If
                Next
                MSE_vi = MSE_vi / N_oob 'compare miss-classification rate permuting Feature j on MSE_oob
                VI(j) = VI(j) + (MSE_vi - MSE_oob) / max_tree 'Add variable importance and average over total trees
            Next

            'Calculate miss-classification rate at this tree for the testing set
            MSEval_tree = 0
            'For i = 0 To N_tst - 1
            '    MSEval_tree = MSEval_tree + (tst.phenotype(i) - y_hat(i) / (n_tree + 1)) * (tst.phenotype(i) - y_hat(i) / (n_tree + 1))
            'Next
            'Console.WriteLine("Iteration #" & n_tree + 1 & ";MSE in testing set=" & MSEval_tree / N_tst)
            VBDebugger.EchoLine("average Loss Function in OOB=" & MSE_oob_ave / CSng(n_tree + 1) & "; N_oob=" & N_oob)

            ' Snapshot the trained tree structure before the local branch array
            ' goes out of scope. The cached mean phenotype of each node is used
            ' as the leaf value during prediction and TreeSHAP interpretation.
            Dim snapshot As Branch() = New Branch(n_branch) {}

            For n As Integer = 0 To n_branch
                snapshot(n) = branch(n)

                If snapshot(n).list.Count > 0 Then
                    Call snapshot(n).getMean(train.phenotype)
                End If
            Next

            Call Trees.Add(snapshot)

            Return (MSE_oob_ave / CSng(n_tree + 1), MSE_oob)
            ' outTreeTest.WriteLine(MSEval_tree / N_tst)
        End Function

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
        Public Function Run(train As Data) As Result
            ' Optional tst As Data = Nothing
            Dim n_tree As Integer = 0
            Dim i As Integer

            'If tst Is Nothing Then
            '    tst = New Data With {
            '        .attributeNames = train.attributeNames.ToArray,
            '        .Genotype = {},
            '        .ID = {},
            '        .phenotype = {}
            '    }
            'End If

            'out of bag variables
            Dim MSE_oob_ave As Double = 0
            'Predictive and estimated variables
            Dim GEBV As Double()() = RectangularArray.Matrix(Of Double)(train.N_tot, 2) 'Predicted phenotype in training set
            ' Dim y_hat = New Double(tst.N_tot - 1) {} 'Predicted phenotype in testing set
            Dim N_tot = train.N_tot
            ' Dim N_tst = tst.N_tot
            Dim N_attributes = train.N_attributes

            'Output files
            Dim outTree As New List(Of (Double, Double))
            ' Dim outTreeTest As New StreamWriter("Trees.test")


            Dim outEGBV As New List(Of Double)
            ' Dim outPred As New StreamWriter("Predictions.txt")

            VI = New Double(train.N_attributes - 1) {}
            ' number of times SNPs are selected
            Selected = New Integer(train.N_attributes - 1) {}

            ' reset the trained tree structures from a previous run
            Trees = New List(Of Branch())

            ' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

            While n_tree < max_tree
                Tree(n_tree, train, GEBV, MSE_oob_ave)
                ' go to next tree
                n_tree += 1
            End While 'over n_tree

            ' 2. Ends the forest 

            ' Console.WriteLine("Writing output files")
            'outTree.Close()
            'outTreeTest.Close()

            'Prepare the output files and its format
            ' Predicted GBV in training set
            For i = 0 To N_tot - 1
                outEGBV.Add((GEBV(i)(0) / CSng(GEBV(i)(1))))
            Next
            'For i = 0 To N_tst - 1 'Predicted GBV in testing set
            '    outPred.WriteLine(tst.ID(i) & " " & (y_hat(i) / (n_tree + 1)))
            'Next

            ' outPred.Close()
            ' outEGBV.Close()
            VBDebugger.EchoLine("TERMINATED WITHOUT ERRORS")
            VBDebugger.EchoLine("Random Forest algorithm for regression and classification problems (Ver.Beta)")
            VBDebugger.EchoLine("by Oscar Gonzalez-Recio (2019) ")

            Return New Result With {.Model = Me, .outGEBV = outEGBV.ToArray, .data = train}
        End Function

        ''' <summary>
        ''' Predict the phenotype values of the given samples by averaging the
        ''' leaf values over all of the trees in the current forest.
        ''' </summary>
        ''' <param name="Genotype">
        ''' the feature matrix, each row is one sample. The feature order must be
        ''' identical to the one used during training.
        ''' </param>
        ''' <returns>the predicted phenotype value for each sample</returns>
        Public Function Predict(Genotype As Double()()) As Double()
            Dim pred As Double() = New Double(Genotype.Length - 1) {}

            If Trees.Count = 0 Then
                Return pred
            End If

            For i As Integer = 0 To Genotype.Length - 1
                Dim sum As Double = 0

                For Each tree As Branch() In Trees
                    Dim node As Integer = 0

                    While tree(node).status <> "F"
                        If Genotype(i)(tree(node).Feature) <= tree(node).mean_snp Then
                            node = tree(node).Child1
                        Else
                            node = tree(node).Child2
                        End If
                    End While

                    sum += tree(node).mean
                Next

                pred(i) = sum / Trees.Count
            Next

            Return pred
        End Function
    End Class

    ''' <summary>
    ''' The result object of the random forest training procedure.
    ''' </summary>
    Public Class Result

        ''' <summary>
        ''' Predicted GBV in training set
        ''' </summary>
        ''' <returns>An array of the predicted phenotype value of each training sample.</returns>
        Public Property outGEBV As Double()

        ''' <summary>
        ''' The trained random forest model.
        ''' </summary>
        ''' <returns>A <see cref="RanFog"/> model object.</returns>
        Public Property Model As RanFog
        ''' <summary>
        ''' The training dataset which was used for training the <see cref="Model"/>.
        ''' </summary>
        ''' <returns>A <see cref="Data"/> object.</returns>
        Public Property data As Data
        ''' <summary>
        ''' The out-of-bag mean squared error which was evaluated at each tree.
        ''' </summary>
        ''' <returns>
        ''' An array of the tuples, in which the ``ave`` item is the averaged 
        ''' out-of-bag error and the ``MSE_oob`` item is the out-of-bag error of 
        ''' the current tree.
        ''' </returns>
        Public Property MSE_oob As (ave As Double, MSE_oob As Double)()

    End Class
End Namespace
