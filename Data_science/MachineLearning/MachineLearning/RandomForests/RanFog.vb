Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection

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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="demoProperties">"params.txt"</param>
    Public Sub Run(demoProperties As Dictionary(Of String, String))
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
        ''' </summary>

        ''' <summary>
        ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        '''   Load parameter file                                                           
        ''' </summary>
        ' 


        'Max number of trees to be constructed
        Dim max_tree = Integer.Parse(demoProperties("ForestSize"))
        'Number of classified Features
        Dim N_SNP = Integer.Parse(demoProperties("N_features"))
        'Name of training file; Load training file
        Dim trnFile As FileStream '(demoProperties("training"))
        'Name of testing file; Load testing file
        Dim tstFile As FileStream '(demoProperties("testing"))
        'Number of Features randomly selected at each node
        Dim m = Double.Parse(demoProperties("mtry")) 'Percentage of Features randomly selected at each node
        'Max number of branches allowed
        Dim max_branch = Integer.Parse(demoProperties("max_branch"))
        'Loss function used for discrete features
        '		String LF_d=demoProperties("LossFunction_discrete");
        'Loss function used for continuous features
        Dim LF_c As String = demoProperties("LossFunction")
        Dim false_positive_cost As Double
        Dim false_negative_cost As Double
        If demoProperties("false_positive_cost") Is Nothing Then
            false_positive_cost = 0
        Else
            false_positive_cost = Double.Parse(demoProperties("false_positive_cost"))
        End If
        If demoProperties("false_negative_cost") Is Nothing Then
            false_negative_cost = 0
        Else
            false_negative_cost = Double.Parse(demoProperties("false_negative_cost"))
        End If

        ''' <summary>
        ''' End loading parameter file
        ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        ''' </summary>

        'Set the arguments to the respective variables.
        Console.WriteLine("Number of trees to be grown: " & max_tree)
        'int N_SNP=Integer.parseInt(args[3]); //Number of classified Features
        Dim N_attributes = 0 'Number of total features
        Console.WriteLine("Number of SNPs (classified Features): " & N_SNP)
        Console.Write("RanFoG will run with Loss Function option: ")
        If Integer.Parse(LF_c) = 1 Then
            Console.WriteLine("Information Gain")
        ElseIf Integer.Parse(LF_c) = 2 Then
            Console.WriteLine("Mean Squared Error (L2 function)")
        ElseIf Integer.Parse(LF_c) = 3 Then
            Console.WriteLine("Pseudo Huber")
        ElseIf Integer.Parse(LF_c) = 4 Then
            Console.WriteLine("Personalized Cost Function for categories")
        ElseIf Integer.Parse(LF_c) = 5 Then
            Console.WriteLine("Gini Index")
        End If
        Console.WriteLine()


        ''' <summary>
        ''' Initialize counter variables </summary>
        Dim j = 0, jj = 0, k = 0, i = 0, N_tot = 0, N_tst = 0, N_oob = 0

        ' read the number of lines in the training file
        Try
            Dim inFile As StreamReader = New StreamReader(trnFile)
            Dim line As String
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, inFile.ReadLine())), Nothing)
                N_tot = N_tot + 1
                Dim st As StringTokenizer = New StringTokenizer(line, " ")
                ' b/w "" put the delimiter(,). 
                ' st.nextToken() is a String, so you can manipulate on it e.g.:
                'System.out.println("number of columns="+st.countTokens());
                If N_tot = 1 Then
                    N_attributes = st.countTokens() - 2
                End If
                If st.countTokens() <> N_attributes + 2 Then
                    Console.WriteLine("Training file with less columns than expected at line " & N_tot)
                    GC.WaitForPendingFinalizers()
                End If
            End While
            inFile.Close()
        Catch __unusedFileNotFoundException1__ As FileNotFoundException
            Console.WriteLine("Training file for training set not found. ")
        End Try
        Console.WriteLine("Number of genotypes (lines) in training set: " & N_tot)
        Console.WriteLine("Number of total Attributes detected: " & N_attributes)
        Console.WriteLine()

        ' read the number of lines in the testing file
        Try
            Dim testing As StreamReader = New StreamReader(tstFile)
            Dim line As String
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, testing.ReadLine())), Nothing)
                N_tst = N_tst + 1
                Dim st As StringTokenizer = New StringTokenizer(line, " ")
                If st.countTokens() <> N_attributes + 2 Then
                    Console.WriteLine("Testing file with less columns than expected at line " & i)
                    GC.WaitForPendingFinalizers()
                End If
            End While
            testing.Close()
        Catch __unusedFileNotFoundException1__ As FileNotFoundException
            Console.WriteLine("Testing file for training set not found. ")
        End Try
        Console.WriteLine("Number of genotypes (lines) in testing set: " & N_tst)

        ''' <summary>
        ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        ''' Declaracion de variables                                                 
        ''' </summary>

        'Variables read from files
        Dim phenotype = New Double(N_tot - 1) {}
        Dim ID = New String(N_tot - 1) {}
        Dim phenotype_tst = New Double(N_tst - 1) {}
        Dim ID_tst = New String(N_tst - 1) {}
        Dim Genotype = RectangularArray.Matrix(Of Double)(N_tot, N_attributes)
        Dim Genotype_tst = RectangularArray.Matrix(Of Double)(N_tst, N_attributes)

        'Variables involved in the trees
        Dim mean_j, minLoss, MSE_tree, MSEval_tree, node_mse, temp As Double
        Dim node, n_branch, n_tree As Integer
        Dim FeatureSel = New Integer(1) {} 'Feature selected at each node [regression feature,classified features]
        'out of bag variables
        Dim oob = New Integer(N_tot - 1) {}
        Dim MSE_oob, MSE_vi As Double, MSE_oob_ave As Double = 0
        'Predictive and estimated variables
        Dim GEBV = RectangularArray.Matrix(Of Double)(N_tot, 2) 'Predicted phenotype in training set
        Dim y_hat = New Double(N_tst - 1) {} 'Predicted phenotype in testing set
        Dim Selected = New Integer(N_attributes - 1) {} 'number of times SNPs are selected
        Dim VI = New Double(N_attributes - 1) {}

        'Random number generator
        Dim x1 As Random = New Random()
        Dim rand As Random = New Random()
        'Information gain variables
        Dim Loss As Double = 0

        'Output files
        Dim outTree As StreamWriter '("Trees.txt")
        Dim outTreeTest As StreamWriter '("Trees.test")
        Dim outSel As StreamWriter '("TimesSelected.txt")
        Dim outVI As StreamWriter '("Variable_Importance.txt")
        Dim outEGBV As StreamWriter '("EGBV.txt")
        Dim outPred As StreamWriter '("Predictions.txt")
        ''' <summary>
        ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% </summary>

        ''' <summary>
        ''' 1. Start reading files
        ''' </summary>
        'read the training set file
        i = 0
        Try
            Dim inFile As StreamReader = New StreamReader(trnFile)
            Dim line As String
            'inFile.readLine(); //Read header
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, inFile.ReadLine())), Nothing)
                i += 1
                Dim st As StringTokenizer = New StringTokenizer(line, " ")
                ' b/w "" put the delimiter(,). In case there are arbitrary spaces
                ' in addition to commas, e.g., a, b, c, add a single space after t$
                ' st.nextToken() is a String, so you can manipulate on it e.g.:
                phenotype(i - 1) = Double.Parse(st.nextToken())
                ID(i - 1) = st.nextToken()
                'st.nextToken();//***** to not read other fields!!****
                For j = 0 To N_attributes - 1 'for each SNP two dummy variables are created
                    Genotype(i - 1)(j) = Double.Parse(st.nextToken())
                Next
            End While
            inFile.Close()
        Catch __unusedFileNotFoundException1__ As FileNotFoundException
            Console.WriteLine("Training file for Features with wrong format. ")
        End Try 'end reading training file

        'read the testing set file
        i = 0
        Try
            Dim testing As StreamReader = New StreamReader(tstFile)
            Dim line As String
            'inFile.readLine(); //Read header
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, testing.ReadLine())), Nothing)
                i += 1
                Dim st As StringTokenizer = New StringTokenizer(line, " ")
                phenotype_tst(i - 1) = Double.Parse(st.nextToken())
                ID_tst(i - 1) = st.nextToken()
                'st.nextToken();//***** to not read other fields!!****
                For j = 0 To N_attributes - 1 'for each SNP
                    Genotype_tst(i - 1)(j) = Double.Parse(st.nextToken())
                Next
            End While
            testing.Close()
        Catch __unusedFileNotFoundException1__ As FileNotFoundException
            Console.WriteLine("Testing file for Features with wrong format. ")
        End Try 'end reading testing file
        ''' <summary>
        '''   1. End of reading files
        '''   
        ''' </summary>

        ''' <summary>
        ''' 2. Starts the forest 
        ''' </summary>
        n_tree = 0
        While n_tree < max_tree
            j = 0
            k = 0
            i = 0
            n_branch = 0
            N_oob = 0
            MSE_tree = 0
            MSEval_tree = 0
            MSE_oob = 0
            oob = New Integer(N_tot - 1) {}
            Dim SNP_tree As List(Of Integer?) = New List(Of Integer?)()
            Dim branch = New Branch(max_branch - 1) {}
            Dim branch_tst = New Branch(max_branch - 1) {}
            Dim branch_oob = New Branch(max_branch - 1) {}
            branch(n_branch) = New Branch()
            branch_tst(n_branch) = New Branch()
            branch_oob(n_branch) = New Branch()

            'Get bootstrapped sample from data, and store pointer-positions in the list of branch zero
            For i = 0 To N_tot - 1
                Dim u = x1.Next(N_tot)
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
        outTree.close()
        outTreeTest.close()

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
        outSel.close()
        outVI.close()
        outPred.close()
        outEGBV.close()
        Console.WriteLine("TERMINATED WITHOUT ERRORS")
        Console.WriteLine("Random Forest algorithm for regression and classification problems (Ver.Beta)")
        Console.WriteLine("by Oscar Gonzalez-Recio (2019) ")
        GC.WaitForPendingFinalizers()
    End Sub ' end main method

    Private Class CSharpImpl
        <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
        Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Class 'end program




