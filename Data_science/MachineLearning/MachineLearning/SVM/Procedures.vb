#Region "Microsoft.VisualBasic::fbe24c26cfb881053f219aa21956ba93, Data_science\MachineLearning\MachineLearning\SVM\Procedures.vb"

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

'     Module Procedures
' 
'         Properties: IsVerbose
' 
'         Function: sigmoid_predict, svm_check_parameter, svm_check_probability_model, svm_get_nr_class, svm_get_nr_sv
'                   svm_get_svm_type, svm_get_svr_probability, svm_predict, svm_predict_probability, svm_predict_values
'                   svm_svr_probability, svm_train, svm_train_one
' 
'         Sub: info, multiclass_probability, setRandomSeed, sigmoid_train, solve_c_svc
'              solve_epsilon_svr, solve_nu_svc, solve_nu_svr, solve_one_class, svm_binary_svc_probability
'              svm_cross_validation, svm_get_labels, svm_get_sv_indices, svm_group_classes
'         Class decision_function
' 
'             Properties: alpha, rho
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace SVM

    Friend Module Procedures

        Public Property IsVerbose As Boolean

        '
        ' construct and solve various formulations
        '
        Public Const LIBSVM_VERSION As Integer = 318

        Private rand As Random = randf.seeds
        Private svm_print_stdout As TextWriter = Console.Out

        Public Sub setRandomSeed(seed As Integer)
            rand = New Random(seed)
        End Sub

        Public Sub info(s As String)
            If IsVerbose Then svm_print_stdout.Write(s)
        End Sub

        Private Sub solve_c_svc(prob As Problem, param As Parameter, alpha As Double(), si As SolutionInfo, Cp As Double, Cn As Double)
            Dim l = prob.Count
            Dim minus_ones = New Double(l - 1) {}
            Dim y = New SByte(l - 1) {}
            Dim i As Integer

            For i = 0 To l - 1
                alpha(i) = 0
                minus_ones(i) = -1

                If prob.Y(i) > 0 Then
                    y(i) = +1
                Else
                    y(i) = -1
                End If
            Next

            Dim s As Solver = New Solver()
            s.Solve(l, New SVC_Q(prob, param, y), minus_ones, y, alpha, Cp, Cn, param.EPS, si, param.Shrinking)
            Dim sum_alpha As Double = 0

            For i = 0 To l - 1
                sum_alpha += alpha(i)
            Next

            If Cp = Cn Then
                Procedures.info("nu = " & sum_alpha / (Cp * prob.Count) & ASCII.LF)
            End If

            For i = 0 To l - 1
                alpha(i) *= y(i)
            Next
        End Sub

        Private Sub solve_nu_svc(prob As Problem, param As Parameter, alpha As Double(), si As SolutionInfo)
            Dim i As Integer
            Dim l = prob.Count
            Dim nu = param.Nu
            Dim y = New SByte(l - 1) {}

            For i = 0 To l - 1

                If prob.Y(i) > 0 Then
                    y(i) = +1
                Else
                    y(i) = -1
                End If
            Next

            Dim sum_pos = nu * l / 2
            Dim sum_neg = nu * l / 2

            For i = 0 To l - 1

                If y(i) = +1 Then
                    alpha(i) = stdNum.Min(1.0, sum_pos)
                    sum_pos -= alpha(i)
                Else
                    alpha(i) = stdNum.Min(1.0, sum_neg)
                    sum_neg -= alpha(i)
                End If
            Next

            Dim zeros = New Double(l - 1) {}

            For i = 0 To l - 1
                zeros(i) = 0
            Next

            Dim s As Solver_NU = New Solver_NU()
            s.Solve(l, New SVC_Q(prob, param, y), zeros, y, alpha, 1.0, 1.0, param.EPS, si, param.Shrinking)
            Dim r = si.r

            Procedures.info("C = " & 1 / r & ASCII.LF)

            For i = 0 To l - 1
                alpha(i) *= y(i) / r
            Next

            si.rho /= r
            si.obj /= r * r
            si.upper_bound_p = 1 / r
            si.upper_bound_n = 1 / r
        End Sub

        Private Sub solve_one_class(prob As Problem, param As Parameter, alpha As Double(), si As SolutionInfo)
            Dim l = prob.Count
            Dim zeros = New Double(l - 1) {}
            Dim ones = New SByte(l - 1) {}
            Dim i As Integer
            Dim n As Integer = CInt(param.Nu * prob.Count)   ' # of alpha's at upper bound

            For i = 0 To n - 1
                alpha(i) = 1
            Next

            If n < prob.Count Then alpha(n) = param.Nu * prob.Count - n

            For i = n + 1 To l - 1
                alpha(i) = 0
            Next

            For i = 0 To l - 1
                zeros(i) = 0
                ones(i) = 1
            Next

            Dim s As Solver = New Solver()
            s.Solve(l, New ONE_CLASS_Q(prob, param), zeros, ones, alpha, 1.0, 1.0, param.EPS, si, param.Shrinking)
        End Sub

        Private Sub solve_epsilon_svr(prob As Problem, param As Parameter, alpha As Double(), si As SolutionInfo)
            Dim l = prob.Count
            Dim alpha2 = New Double(2 * l - 1) {}
            Dim linear_term = New Double(2 * l - 1) {}
            Dim y = New SByte(2 * l - 1) {}
            Dim i As Integer

            For i = 0 To l - 1
                alpha2(i) = 0
                linear_term(i) = param.P - prob.Y(i)
                y(i) = 1
                alpha2(i + l) = 0
                linear_term(i + l) = param.P + prob.Y(i)
                y(i + l) = -1
            Next

            Dim s As Solver = New Solver()
            s.Solve(2 * l, New SVR_Q(prob, param), linear_term, y, alpha2, param.C, param.C, param.EPS, si, param.Shrinking)
            Dim sum_alpha As Double = 0

            For i = 0 To l - 1
                alpha(i) = alpha2(i) - alpha2(i + l)
                sum_alpha += stdNum.Abs(alpha(i))
            Next

            Procedures.info("nu = " & sum_alpha / (param.C * l) & ASCII.LF)
        End Sub

        Private Sub solve_nu_svr(prob As Problem, param As Parameter, alpha As Double(), si As SolutionInfo)
            Dim l = prob.Count
            Dim C = param.C
            Dim alpha2 = New Double(2 * l - 1) {}
            Dim linear_term = New Double(2 * l - 1) {}
            Dim y = New SByte(2 * l - 1) {}
            Dim i As Integer
            Dim sum = C * param.Nu * l / 2

            For i = 0 To l - 1
                alpha2(i + l) = stdNum.Min(sum, C)
                alpha2(i) = alpha2(i + l)
                sum -= alpha2(i)
                linear_term(i) = -prob.Y(i)
                y(i) = 1
                linear_term(i + l) = prob.Y(i)
                y(i + l) = -1
            Next

            Dim s As Solver_NU = New Solver_NU()
            s.Solve(2 * l, New SVR_Q(prob, param), linear_term, y, alpha2, C, C, param.EPS, si, param.Shrinking)
            Procedures.info("epsilon = " & -si.r & ASCII.LF)

            For i = 0 To l - 1
                alpha(i) = alpha2(i) - alpha2(i + l)
            Next
        End Sub

        ''' <summary>
        ''' decision_function
        ''' </summary>
        Private Class decision_function

            Public Property alpha As Double()
            Public Property rho As Double

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Class

        Private Function svm_train_one(prob As Problem, param As Parameter, Cp As Double, Cn As Double) As decision_function
            Dim alpha = New Double(prob.Count - 1) {}
            Dim si As New SolutionInfo()

            Select Case param.SvmType
                Case SvmType.C_SVC
                    solve_c_svc(prob, param, alpha, si, Cp, Cn)
                Case SvmType.NU_SVC
                    solve_nu_svc(prob, param, alpha, si)
                Case SvmType.ONE_CLASS
                    solve_one_class(prob, param, alpha, si)
                Case SvmType.EPSILON_SVR
                    solve_epsilon_svr(prob, param, alpha, si)
                Case SvmType.NU_SVR
                    solve_nu_svr(prob, param, alpha, si)
            End Select

            Procedures.info("obj = " & si.obj & ", rho = " & si.rho & ASCII.LF)

            ' output SVs

            Dim nSV = 0
            Dim nBSV = 0

            For i = 0 To prob.Count - 1

                If stdNum.Abs(alpha(i)) > 0 Then
                    nSV += 1

                    If prob.Y(i) > 0 Then
                        If stdNum.Abs(alpha(i)) >= si.upper_bound_p Then
                            nBSV += 1
                        End If
                    Else
                        If stdNum.Abs(alpha(i)) >= si.upper_bound_n Then
                            nBSV += 1
                        End If
                    End If
                End If
            Next

            Procedures.info("nSV = " & nSV & ", nBSV = " & nBSV & ASCII.LF)
            Dim f As New decision_function()
            f.alpha = alpha
            f.rho = si.rho
            Return f
        End Function

        ''' <summary>
        ''' Platt's binary SVM Probablistic Output: an improvement from Lin et al.
        ''' </summary>
        ''' <param name="l"></param>
        ''' <param name="dec_values"></param>
        ''' <param name="labels"></param>
        ''' <param name="probAB"></param>
        Private Sub sigmoid_train(l As Integer, dec_values As Double(), labels As ColorClass(), probAB As Double())
            Dim A, B As Double
            Dim prior1 As Double = 0, prior0 As Double = 0
            Dim i As Integer

            For i = 0 To l - 1
                If labels(i) > 0 Then
                    prior1 += 1
                Else
                    prior0 += 1
                End If
            Next

            Dim max_iter = 100  ' Maximal number of iterations
            Dim min_step = 0.0000000001    ' Minimal step taken in line search
            Dim sigma = 0.000000000001   ' For numerically strict PD of Hessian
            Dim eps = 0.00001
            Dim hiTarget = (prior1 + 1.0) / (prior1 + 2.0)
            Dim loTarget = 1 / (prior0 + 2.0)
            Dim t = New Double(l - 1) {}
            Dim fApB, p, q, h11, h22, h21, g1, g2, det, dA, dB, gd, stepsize As Double
            Dim newA, newB, newf, d1, d2 As Double
            Dim iter As Integer

            ' Initial Point and Initial Fun Value
            A = 0.0
            B = stdNum.Log((prior0 + 1.0) / (prior1 + 1.0))
            Dim fval = 0.0

            For i = 0 To l - 1

                If labels(i) > 0 Then
                    t(i) = hiTarget
                Else
                    t(i) = loTarget
                End If

                fApB = dec_values(i) * A + B

                If fApB >= 0 Then
                    fval += t(i) * fApB + stdNum.Log(1 + stdNum.Exp(-fApB))
                Else
                    fval += (t(i) - 1) * fApB + stdNum.Log(1 + stdNum.Exp(fApB))
                End If
            Next

            For iter = 0 To max_iter - 1
                ' Update Gradient and Hessian (use H' = H + sigma I)
                h11 = sigma ' numerically ensures strict PD
                h22 = sigma
                h21 = 0.0
                g1 = 0.0
                g2 = 0.0

                For i = 0 To l - 1
                    fApB = dec_values(i) * A + B

                    If fApB >= 0 Then
                        p = stdNum.Exp(-fApB) / (1.0 + stdNum.Exp(-fApB))
                        q = 1.0 / (1.0 + stdNum.Exp(-fApB))
                    Else
                        p = 1.0 / (1.0 + stdNum.Exp(fApB))
                        q = stdNum.Exp(fApB) / (1.0 + stdNum.Exp(fApB))
                    End If

                    d2 = p * q
                    h11 += dec_values(i) * dec_values(i) * d2
                    h22 += d2
                    h21 += dec_values(i) * d2
                    d1 = t(i) - p
                    g1 += dec_values(i) * d1
                    g2 += d1
                Next

                ' Stopping Criteria
                If stdNum.Abs(g1) < eps AndAlso stdNum.Abs(g2) < eps Then Exit For

                ' Finding Newton direction: -inv(H') * g
                det = h11 * h22 - h21 * h21
                dA = -(h22 * g1 - h21 * g2) / det
                dB = -(-h21 * g1 + h11 * g2) / det
                gd = g1 * dA + g2 * dB
                stepsize = 1        ' Line Search

                While stepsize >= min_step
                    newA = A + stepsize * dA
                    newB = B + stepsize * dB

                    ' New function value
                    newf = 0.0

                    For i = 0 To l - 1
                        fApB = dec_values(i) * newA + newB

                        If fApB >= 0 Then
                            newf += t(i) * fApB + stdNum.Log(1 + stdNum.Exp(-fApB))
                        Else
                            newf += (t(i) - 1) * fApB + stdNum.Log(1 + stdNum.Exp(fApB))
                        End If
                    Next
                    ' Check sufficient decrease
                    If newf < fval + 0.0001 * stepsize * gd Then
                        A = newA
                        B = newB
                        fval = newf
                        Exit While
                    Else
                        stepsize = stepsize / 2.0
                    End If
                End While

                If stepsize < min_step Then
                    Procedures.info("Line search fails in two-class probability estimates" & ASCII.LF)
                    Exit For
                End If
            Next

            If iter >= max_iter Then Procedures.info("Reaching maximal iterations in two-class probability estimates" & ASCII.LF)
            probAB(0) = A
            probAB(1) = B
        End Sub

        Private Function sigmoid_predict(decision_value As Double, A As Double, B As Double) As Double
            Dim fApB = decision_value * A + B

            If fApB >= 0 Then
                Return stdNum.Exp(-fApB) / (1.0 + stdNum.Exp(-fApB))
            Else
                Return 1.0 / (1 + stdNum.Exp(fApB))
            End If
        End Function

        ''' <summary>
        ''' Method 2 from the multiclass_prob paper by Wu, Lin, and Weng
        ''' </summary>
        ''' <param name="k"></param>
        ''' <param name="r"></param>
        ''' <param name="p"></param>
        Private Sub multiclass_probability(k As Integer, r As Double(,), p As Double())
            Dim t, j As Integer
            Dim iter = 0, max_iter = stdNum.Max(100, k)
            Dim Q = New Double(k - 1, k - 1) {}
            Dim Qp = New Double(k - 1) {}
            Dim pQp As Double, eps = 0.005 / k

            For t = 0 To k - 1
                p(t) = 1.0 / k  ' Valid if k = 1
                Q(t, t) = 0

                For j = 0 To t - 1
                    Q(t, t) += r(j, t) * r(j, t)
                    Q(t, j) = Q(j, t)
                Next

                For j = t + 1 To k - 1
                    Q(t, t) += r(j, t) * r(j, t)
                    Q(t, j) = -r(j, t) * r(t, j)
                Next
            Next

            For iter = 0 To max_iter - 1
                ' stopping condition, recalculate QP,pQP for numerical accuracy
                pQp = 0

                For t = 0 To k - 1
                    Qp(t) = 0

                    For j = 0 To k - 1
                        Qp(t) += Q(t, j) * p(j)
                    Next

                    pQp += p(t) * Qp(t)
                Next

                Dim max_error As Double = 0

                For t = 0 To k - 1
                    Dim [error] = stdNum.Abs(Qp(t) - pQp)
                    If [error] > max_error Then max_error = [error]
                Next

                If max_error < eps Then Exit For

                For t = 0 To k - 1
                    Dim diff = (-Qp(t) + pQp) / Q(t, t)
                    p(t) += diff
                    pQp = (pQp + diff * (diff * Q(t, t) + 2 * Qp(t))) / (1 + diff) / (1 + diff)

                    For j = 0 To k - 1
                        Qp(j) = (Qp(j) + diff * Q(t, j)) / (1 + diff)
                        p(j) /= 1 + diff
                    Next
                Next
            Next

            If iter >= max_iter Then Procedures.info("Exceeds max_iter in multiclass_prob" & ASCII.LF)
        End Sub

        ''' <summary>
        ''' Cross-validation decision values for probability estimates
        ''' </summary>
        ''' <param name="prob"></param>
        ''' <param name="param"></param>
        ''' <param name="Cp"></param>
        ''' <param name="Cn"></param>
        ''' <param name="probAB"></param>
        Private Sub svm_binary_svc_probability(prob As Problem, param As Parameter, Cp As Double, Cn As Double, probAB As Double())
            Dim i As Integer
            Dim nr_fold = 5
            Dim perm = New Integer(prob.Count - 1) {}
            Dim dec_values = New Double(prob.Count - 1) {}

            ' random shuffle
            For i = 0 To prob.Count - 1
                perm(i) = i
            Next

            For i = 0 To prob.Count - 1
                Dim j = i + rand.Next(prob.Count - i)

                Do
                    Dim __ = perm(i)
                    perm(i) = perm(j)
                    perm(j) = __
                Loop While False
            Next

            For i = 0 To nr_fold - 1
                Dim begin As Integer = CInt(i * prob.Count / nr_fold)
                Dim [end] As Integer = CInt((i + 1) * prob.Count / nr_fold)
                Dim j, k As Integer
                Dim subprob As New Problem()
                Dim subCount = prob.Count - ([end] - begin)
                subprob.X = New Node(subCount - 1)() {}
                subprob.Y = New ColorClass(subCount - 1) {}
                k = 0

                For j = 0 To begin - 1
                    subprob.X(k) = prob.X(perm(j))
                    subprob.Y(k) = prob.Y(perm(j))
                    k += 1
                Next

                For j = [end] To prob.Count - 1
                    subprob.X(k) = prob.X(perm(j))
                    subprob.Y(k) = prob.Y(perm(j))
                    k += 1
                Next

                Dim p_count = 0, n_count = 0

                For j = 0 To k - 1

                    If subprob.Y(j) > 0 Then
                        p_count += 1
                    Else
                        n_count += 1
                    End If
                Next

                If p_count = 0 AndAlso n_count = 0 Then
                    For j = begin To [end] - 1
                        dec_values(perm(j)) = 0
                    Next
                ElseIf p_count > 0 AndAlso n_count = 0 Then

                    For j = begin To [end] - 1
                        dec_values(perm(j)) = 1
                    Next
                ElseIf p_count = 0 AndAlso n_count > 0 Then

                    For j = begin To [end] - 1
                        dec_values(perm(j)) = -1
                    Next
                Else
                    Dim subparam As Parameter = CType(param.Clone(), Parameter)
                    subparam.Probability = False
                    subparam.C = 1.0
                    subparam.Weights(1) = Cp
                    subparam.Weights(-1) = Cn
                    Dim submodel = svm_train(subprob, subparam)

                    For j = begin To [end] - 1
                        Dim dec_value = New Double(0) {}
                        svm_predict_values(submodel, prob.X(perm(j)), dec_value)
                        dec_values(perm(j)) = dec_value(0)
                        ' ensure +1 -1 order; reason not using CV subroutine
                        dec_values(perm(j)) *= submodel.ClassLabels(0)
                    Next
                End If
            Next

            sigmoid_train(prob.Count, dec_values, prob.Y, probAB)
        End Sub

        ' Return parameter of a Laplace distribution 
        Private Function svm_svr_probability(prob As Problem, param As Parameter) As Double
            Dim i As Integer
            Dim nr_fold = 5
            Dim ymv = New Double(prob.Count - 1) {}
            Dim mae As Double = 0
            Dim newparam As Parameter = CType(param.Clone(), Parameter)
            newparam.Probability = False
            svm_cross_validation(prob, newparam, nr_fold, ymv)

            For i = 0 To prob.Count - 1
                ymv(i) = prob.Y(i) - ymv(i)
                mae += stdNum.Abs(ymv(i))
            Next

            mae /= prob.Count
            Dim std = stdNum.Sqrt(2 * mae * mae)
            Dim count = 0
            mae = 0

            For i = 0 To prob.Count - 1

                If stdNum.Abs(ymv(i)) > 5 * std Then
                    count = count + 1
                Else
                    mae += stdNum.Abs(ymv(i))
                End If
            Next

            mae /= prob.Count - count
            Procedures.info("Prob. model for test data: target value = predicted value + z," & ASCII.LF & "z: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" & mae & ASCII.LF)
            Return mae
        End Function

        ' label: label name, start: begin of each class, count: #data of classes, perm: indices to the original data
        ' perm, length l, must be allocated before calling this subroutine
        Private Sub svm_group_classes(prob As Problem, <Out> ByRef nr_class_ret As Integer, <Out> ByRef label_ret As Integer(), <Out> ByRef start_ret As Integer(), <Out> ByRef count_ret As Integer(), perm As Integer())
            Dim l = prob.Count
            Dim max_nr_class = 16
            Dim nr_class = 0
            Dim label = New Integer(max_nr_class - 1) {}
            Dim count = New Integer(max_nr_class - 1) {}
            Dim data_label = New Integer(l - 1) {}
            Dim i As Integer

            For i = 0 To l - 1
                Dim this_label As Integer = CInt(prob.Y(i))
                Dim j As Integer

                For j = 0 To nr_class - 1

                    If this_label = label(j) Then
                        count(j) += 1
                        Exit For
                    End If
                Next

                data_label(i) = j

                If j = nr_class Then
                    If nr_class = max_nr_class Then
                        max_nr_class *= 2
                        Dim new_data = New Integer(max_nr_class - 1) {}
                        Array.Copy(label, 0, new_data, 0, label.Length)
                        label = new_data
                        new_data = New Integer(max_nr_class - 1) {}
                        Array.Copy(count, 0, new_data, 0, count.Length)
                        count = new_data
                    End If

                    label(nr_class) = this_label
                    count(nr_class) = 1
                    nr_class += 1
                End If
            Next

            '
            ' Labels are ordered by their first occurrence in the training set. 
            ' However, for two-class sets with -1/+1 labels and -1 appears first, 
            ' we swap labels to ensure that internally the binary SVM has positive data corresponding to the +1 instances.
            '
            If nr_class = 2 AndAlso label(0) = -1 AndAlso label(1) = +1 Then
                Do
                    Dim __ = label(0)
                    label(0) = label(1)
                    label(1) = __
                Loop While False

                Do
                    Dim __ = count(0)
                    count(0) = count(1)
                    count(1) = __
                Loop While False

                For i = 0 To l - 1

                    If data_label(i) = 0 Then
                        data_label(i) = 1
                    Else
                        data_label(i) = 0
                    End If
                Next
            End If

            Dim start = New Integer(nr_class - 1) {}
            start(0) = 0

            For i = 1 To nr_class - 1
                start(i) = start(i - 1) + count(i - 1)
            Next

            For i = 0 To l - 1
                perm(start(data_label(i))) = i
                start(data_label(i)) += 1
            Next

            start(0) = 0

            For i = 1 To nr_class - 1
                start(i) = start(i - 1) + count(i - 1)
            Next

            nr_class_ret = nr_class
            label_ret = label
            start_ret = start
            count_ret = count
        End Sub

        '
        ' Interface functions
        '
        Public Function svm_train(prob As Problem, param As Parameter) As Model
            Dim model As New Model() With {
                .Parameter = param,
                .DimensionNames = prob.DimensionNames
            }

            If param.SvmType = SvmType.ONE_CLASS OrElse param.SvmType = SvmType.EPSILON_SVR OrElse param.SvmType = SvmType.NU_SVR Then
                ' regression or one-class-svm
                model.NumberOfClasses = 2
                model.ClassLabels = Nothing
                model.NumberOfSVPerClass = Nothing
                model.PairwiseProbabilityA = Nothing
                model.PairwiseProbabilityB = Nothing
                model.SupportVectorCoefficients = New Double(0)() {}

                If param.Probability AndAlso (param.SvmType = SvmType.EPSILON_SVR OrElse param.SvmType = SvmType.NU_SVR) Then
                    model.PairwiseProbabilityA = New Double(0) {}
                    model.PairwiseProbabilityA(0) = svm_svr_probability(prob, param)
                End If

                Dim f = svm_train_one(prob, param, 0, 0)
                model.Rho = New Double(0) {}
                model.Rho(0) = f.rho
                Dim nSV = 0
                Dim i As Integer

                For i = 0 To prob.Count - 1
                    If stdNum.Abs(f.alpha(i)) > 0 Then
                        nSV += 1
                    End If
                Next

                model.SupportVectorCount = nSV
                model.SupportVectors = New Node(nSV - 1)() {}
                model.SupportVectorCoefficients(0) = New Double(nSV - 1) {}
                model.SupportVectorIndices = New Integer(nSV - 1) {}
                Dim j = 0

                For i = 0 To prob.Count - 1

                    If stdNum.Abs(f.alpha(i)) > 0 Then
                        model.SupportVectors(j) = prob.X(i)
                        model.SupportVectorCoefficients(0)(j) = f.alpha(i)
                        model.SupportVectorIndices(j) = i + 1
                        j += 1
                    End If
                Next
            Else
                ' classification
                Dim l = prob.Count
                Dim nr_class As Integer
                Dim label As Integer() = Nothing
                Dim start As Integer() = Nothing
                Dim count As Integer() = Nothing
                Dim perm = New Integer(l - 1) {}

                ' group training data of the same class
                svm_group_classes(prob, nr_class, label, start, count, perm)
                If nr_class = 1 Then Procedures.info("WARNING: training data in only one class. See README for details." & ASCII.LF)
                Dim x = New Node(l - 1)() {}
                Dim i As Integer

                For i = 0 To l - 1
                    x(i) = prob.X(perm(i))
                Next

                ' calculate weighted C

                Dim weighted_C = New Double(nr_class - 1) {}

                For i = 0 To nr_class - 1
                    weighted_C(i) = param.C
                Next

                For i = 0 To nr_class - 1

                    If Not param.Weights.ContainsKey(label(i)) Then
                        Call $"WARNING: class label {label(i)} specified in weight is not found".Warning
                    Else
                        weighted_C(i) *= param.Weights(label(i))
                    End If
                Next

                ' train k*(k-1)/2 models

                Dim nonzero = New Boolean(l - 1) {}

                For i = 0 To l - 1
                    nonzero(i) = False
                Next

                Dim f = New decision_function(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
                Dim probA As Double() = Nothing, probB As Double() = Nothing

                If param.Probability Then
                    probA = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
                    probB = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
                End If

                Dim p As i32 = 0

                For i = 0 To nr_class - 1

                    For j = i + 1 To nr_class - 1
                        Dim sub_prob As New Problem()
                        Dim si = start(i), sj = start(j)
                        Dim ci = count(i), cj = count(j)
                        Dim sub_Count = ci + cj
                        sub_prob.X = New Node(sub_Count - 1)() {}
                        sub_prob.Y = New ColorClass(sub_Count - 1) {}
                        Dim k As Integer

                        For k = 0 To ci - 1
                            sub_prob.X(k) = x(si + k)
                            sub_prob.Y(k) = New ColorClass With {.enumInt = +1, .name = "temp", .color = "n/a"}
                        Next

                        For k = 0 To cj - 1
                            sub_prob.X(ci + k) = x(sj + k)
                            sub_prob.Y(ci + k) = New ColorClass With {.enumInt = -1, .name = "temp", .color = "n/a"}
                        Next

                        If param.Probability Then
                            Dim probAB = New Double(1) {}
                            svm_binary_svc_probability(sub_prob, param, weighted_C(i), weighted_C(j), probAB)
                            probA(p) = probAB(0)
                            probB(p) = probAB(1)
                        End If

                        f(p) = svm_train_one(sub_prob, param, weighted_C(i), weighted_C(j))

                        For k = 0 To ci - 1
                            If Not nonzero(si + k) AndAlso stdNum.Abs(f(p).alpha(k)) > 0 Then nonzero(si + k) = True
                        Next

                        For k = 0 To cj - 1
                            If Not nonzero(sj + k) AndAlso stdNum.Abs(f(p).alpha(ci + k)) > 0 Then nonzero(sj + k) = True
                        Next

                        p += 1
                    Next
                Next

                ' build output

                model.NumberOfClasses = nr_class
                model.ClassLabels = New Integer(nr_class - 1) {}

                For i = 0 To nr_class - 1
                    model.ClassLabels(i) = label(i)
                Next

                model.Rho = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}

                For i = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    model.Rho(i) = f(i).rho
                Next

                If param.Probability Then
                    model.PairwiseProbabilityA = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
                    model.PairwiseProbabilityB = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}

                    For i = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                        model.PairwiseProbabilityA(i) = probA(i)
                        model.PairwiseProbabilityB(i) = probB(i)
                    Next
                Else
                    model.PairwiseProbabilityA = Nothing
                    model.PairwiseProbabilityA = Nothing
                End If

                Dim nnz = 0
                Dim nz_count = New Integer(nr_class - 1) {}
                model.NumberOfSVPerClass = New Integer(nr_class - 1) {}

                For i = 0 To nr_class - 1
                    Dim nSV = 0

                    For j = 0 To count(i) - 1

                        If nonzero(start(i) + j) Then
                            nSV += 1
                            nnz += 1
                        End If
                    Next

                    model.NumberOfSVPerClass(i) = nSV
                    nz_count(i) = nSV
                Next

                Procedures.info("Total nSV = " & nnz & ASCII.LF)
                model.SupportVectorCount = nnz
                model.SupportVectors = New Node(nnz - 1)() {}
                model.SupportVectorIndices = New Integer(nnz - 1) {}
                p = 0

                For i = 0 To l - 1

                    If nonzero(i) Then
                        model.SupportVectors(p) = x(i)
                        model.SupportVectorIndices(++p) = perm(i) + 1
                    End If
                Next

                Dim nz_start = New Integer(nr_class - 1) {}
                nz_start(0) = 0

                For i = 1 To nr_class - 1
                    nz_start(i) = nz_start(i - 1) + nz_count(i - 1)
                Next

                model.SupportVectorCoefficients = New Double(nr_class - 1 - 1)() {}

                For i = 0 To nr_class - 1 - 1
                    model.SupportVectorCoefficients(i) = New Double(nnz - 1) {}
                Next

                p = 0

                For i = 0 To nr_class - 1

                    For j = i + 1 To nr_class - 1
                        ' classifier (i,j): coefficients with
                        ' i are in sv_coef[j-1][nz_start[i]...],
                        ' j are in sv_coef[i][nz_start[j]...]

                        Dim si = start(i)
                        Dim sj = start(j)
                        Dim ci = count(i)
                        Dim cj = count(j)
                        Dim q As i32 = nz_start(i)
                        Dim k As Integer

                        For k = 0 To ci - 1
                            If nonzero(si + k) Then model.SupportVectorCoefficients(j - 1)(++q) = f(p).alpha(k)
                        Next

                        q = nz_start(j)

                        For k = 0 To cj - 1
                            If nonzero(sj + k) Then model.SupportVectorCoefficients(i)(++q) = f(p).alpha(ci + k)
                        Next

                        p += 1
                    Next
                Next
            End If

            Return model
        End Function

        ' Stratified cross validation
        Public Sub svm_cross_validation(prob As Problem, param As Parameter, nr_fold As Integer, target As Double())
            Dim i As Integer
            Dim fold_start = New Integer(nr_fold + 1 - 1) {}
            Dim l = prob.Count
            Dim perm = New Integer(l - 1) {}

            ' stratified cv may not give leave-one-out rate
            ' Each class to l folds -> some folds may have zero elements
            If (param.SvmType = SvmType.C_SVC OrElse param.SvmType = SvmType.NU_SVC) AndAlso nr_fold < l Then
                Dim nr_class As Integer
                Dim label As Integer() = Nothing
                Dim start As Integer() = Nothing
                Dim count As Integer() = Nothing
                svm_group_classes(prob, nr_class, label, start, count, perm)


                ' random shuffle and then data grouped by fold using the array perm
                Dim fold_count = New Integer(nr_fold - 1) {}
                Dim c As Integer
                Dim index = New Integer(l - 1) {}

                For i = 0 To l - 1
                    index(i) = perm(i)
                Next

                For c = 0 To nr_class - 1

                    For i = 0 To count(c) - 1
                        Dim j = i + rand.Next(count(c) - i)

                        Do
                            Dim __ = index(start(c) + j)
                            index(start(c) + j) = index(start(c) + i)
                            index(start(c) + i) = __
                        Loop While False
                    Next
                Next

                For i = 0 To nr_fold - 1
                    fold_count(i) = 0

                    For c = 0 To nr_class - 1
                        fold_count(i) += CInt((i + 1) * count(c) / nr_fold - i * count(c) / nr_fold)
                    Next
                Next

                fold_start(0) = 0

                For i = 1 To nr_fold
                    fold_start(i) = fold_start(i - 1) + fold_count(i - 1)
                Next

                For c = 0 To nr_class - 1

                    For i = 0 To nr_fold - 1
                        Dim begin As Integer = start(c) + CInt(i * count(c) / nr_fold)
                        Dim [end] As Integer = start(c) + CInt((i + 1) * count(c) / nr_fold)

                        For j = begin To [end] - 1
                            perm(fold_start(i)) = index(j)
                            fold_start(i) += 1
                        Next
                    Next
                Next

                fold_start(0) = 0

                For i = 1 To nr_fold
                    fold_start(i) = fold_start(i - 1) + fold_count(i - 1)
                Next
            Else

                For i = 0 To l - 1
                    perm(i) = i
                Next

                For i = 0 To l - 1
                    Dim j = i + rand.Next(l - i)

                    Do
                        Dim __ = perm(i)
                        perm(i) = perm(j)
                        perm(j) = __
                    Loop While False
                Next

                For i = 0 To nr_fold
                    fold_start(i) = CInt(i * l / nr_fold)
                Next
            End If

            For i = 0 To nr_fold - 1
                Dim begin = fold_start(i)
                Dim [end] = fold_start(i + 1)
                Dim j, k As Integer
                Dim subprob As New Problem()
                Dim subCount = l - ([end] - begin)
                subprob.X = New Node(subCount - 1)() {}
                subprob.Y = New ColorClass(subCount - 1) {}
                k = 0

                For j = 0 To begin - 1
                    subprob.X(k) = prob.X(perm(j))
                    subprob.Y(k) = prob.Y(perm(j))
                    k += 1
                Next

                For j = [end] To l - 1
                    subprob.X(k) = prob.X(perm(j))
                    subprob.Y(k) = prob.Y(perm(j))
                    k += 1
                Next

                Dim submodel = svm_train(subprob, param)

                If param.Probability AndAlso (param.SvmType = SvmType.C_SVC OrElse param.SvmType = SvmType.NU_SVC) Then
                    Dim prob_estimates = New Double(svm_get_nr_class(submodel) - 1) {}

                    For j = begin To [end] - 1
                        target(perm(j)) = svm_predict_probability(submodel, prob.X(perm(j)), prob_estimates)
                    Next
                Else

                    For j = begin To [end] - 1
                        target(perm(j)) = svm_predict(submodel, prob.X(perm(j)))
                    Next
                End If
            Next
        End Sub

        Public Function svm_get_svm_type(model As Model) As SvmType
            Return model.Parameter.SvmType
        End Function

        Public Function svm_get_nr_class(model As Model) As Integer
            Return model.NumberOfClasses
        End Function

        Public Sub svm_get_labels(model As Model, label As Integer())
            If model.ClassLabels IsNot Nothing Then
                For i = 0 To model.NumberOfClasses - 1
                    label(i) = model.ClassLabels(i)
                Next
            End If
        End Sub

        Public Sub svm_get_sv_indices(model As Model, indices As Integer())
            If model.SupportVectorIndices IsNot Nothing Then
                For i = 0 To model.SupportVectorCount - 1
                    indices(i) = model.SupportVectorIndices(i)
                Next
            End If
        End Sub

        Public Function svm_get_nr_sv(model As Model) As Integer
            Return model.SupportVectorCount
        End Function

        Public Function svm_get_svr_probability(model As Model) As Double
            If (model.Parameter.SvmType = SvmType.EPSILON_SVR OrElse model.Parameter.SvmType = SvmType.NU_SVR) AndAlso model.PairwiseProbabilityA IsNot Nothing Then
                Return model.PairwiseProbabilityA(0)
            Else
                Console.Error.Write("Model doesn't contain information for SVR probability inference" & ASCII.LF)
                Return 0
            End If
        End Function

        Public Function svm_predict_values(model As Model, x As Node(), dec_values As Double()) As Double
            Dim i As Integer

            If model.Parameter.SvmType = SvmType.ONE_CLASS OrElse model.Parameter.SvmType = SvmType.EPSILON_SVR OrElse model.Parameter.SvmType = SvmType.NU_SVR Then
                Dim sv_coef = model.SupportVectorCoefficients(0)
                Dim sum As Double = 0

                For i = 0 To model.SupportVectorCount - 1
                    sum += sv_coef(i) * Kernel.KernelFunction(x, model.SupportVectors(i), model.Parameter)
                Next

                sum -= model.Rho(0)
                dec_values(0) = sum

                If model.Parameter.SvmType = SvmType.ONE_CLASS Then
                    Return If(sum > 0, 1, -1)
                Else
                    Return sum
                End If
            Else
                Dim nr_class = model.NumberOfClasses
                Dim l = model.SupportVectorCount
                Dim kvalue = New Double(l - 1) {}

                For i = 0 To l - 1
                    kvalue(i) = Kernel.KernelFunction(x, model.SupportVectors(i), model.Parameter)
                Next

                Dim start = New Integer(nr_class - 1) {}
                start(0) = 0

                For i = 1 To nr_class - 1
                    start(i) = start(i - 1) + model.NumberOfSVPerClass(i - 1)
                Next

                Dim vote = New Integer(nr_class - 1) {}

                For i = 0 To nr_class - 1
                    vote(i) = 0
                Next

                Dim p = 0

                For i = 0 To nr_class - 1

                    For j = i + 1 To nr_class - 1
                        Dim sum As Double = 0
                        Dim si = start(i)
                        Dim sj = start(j)
                        Dim ci = model.NumberOfSVPerClass(i)
                        Dim cj = model.NumberOfSVPerClass(j)
                        Dim k As Integer
                        Dim coef1 = model.SupportVectorCoefficients(j - 1)
                        Dim coef2 = model.SupportVectorCoefficients(i)

                        For k = 0 To ci - 1
                            sum += coef1(si + k) * kvalue(si + k)
                        Next

                        For k = 0 To cj - 1
                            sum += coef2(sj + k) * kvalue(sj + k)
                        Next

                        sum -= model.Rho(p)
                        dec_values(p) = sum

                        If dec_values(p) > 0 Then
                            vote(i) += 1
                        Else
                            vote(j) += 1
                        End If

                        p += 1
                    Next
                Next

                Dim vote_max_idx = 0

                For i = 1 To nr_class - 1
                    If vote(i) > vote(vote_max_idx) Then vote_max_idx = i
                Next

                Return model.ClassLabels(vote_max_idx)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 在这里使用double数值类型主要是为了兼容分类以及打分这两种工作模式
        ''' </returns>
        Public Function svm_predict(model As Model, x As Node()) As Double
            Dim nr_class = model.NumberOfClasses
            Dim dec_values As Double()

            If model.Parameter.SvmType = SvmType.ONE_CLASS OrElse model.Parameter.SvmType = SvmType.EPSILON_SVR OrElse model.Parameter.SvmType = SvmType.NU_SVR Then
                dec_values = New Double(0) {}
            Else
                dec_values = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
            End If

            Dim pred_result = svm_predict_values(model, x, dec_values)
            Return pred_result
        End Function

        Public Function svm_predict_probability(model As Model, x As Node(), prob_estimates As Double()) As Double
            If (model.Parameter.SvmType = SvmType.C_SVC OrElse model.Parameter.SvmType = SvmType.NU_SVC) AndAlso model.PairwiseProbabilityA IsNot Nothing AndAlso model.PairwiseProbabilityB IsNot Nothing Then
                Dim i As Integer
                Dim nr_class = model.NumberOfClasses
                Dim dec_values = New Double(CInt(nr_class * (nr_class - 1) / 2) - 1) {}
                svm_predict_values(model, x, dec_values)
                Dim min_prob = 0.0000001
                Dim pairwise_prob = New Double(nr_class - 1, nr_class - 1) {}
                Dim k = 0

                For i = 0 To nr_class - 1

                    For j = i + 1 To nr_class - 1
                        pairwise_prob(i, j) = stdNum.Min(stdNum.Max(sigmoid_predict(dec_values(k), model.PairwiseProbabilityA(k), model.PairwiseProbabilityB(k)), min_prob), 1 - min_prob)
                        pairwise_prob(j, i) = 1 - pairwise_prob(i, j)
                        k += 1
                    Next
                Next

                multiclass_probability(nr_class, pairwise_prob, prob_estimates)
                Dim prob_max_idx = 0

                For i = 1 To nr_class - 1
                    If prob_estimates(i) > prob_estimates(prob_max_idx) Then prob_max_idx = i
                Next

                Return model.ClassLabels(prob_max_idx)
            Else
                Return svm_predict(model, x)
            End If
        End Function

        Public Function svm_check_parameter(prob As Problem, param As Parameter) As String
            ' svm_type

            Dim svm_type = param.SvmType

            ' kernel_type, degree

            Dim kernel_type = param.KernelType
            If param.Gamma < 0 Then Return "gamma < 0"
            If param.Degree < 0 Then Return "degree of polynomial kernel < 0"

            ' cache_size,eps,C,nu,p,shrinking

            If param.CacheSize <= 0 Then Return "cache_size <= 0"
            If param.EPS <= 0 Then Return "eps <= 0"

            If svm_type = SvmType.C_SVC OrElse svm_type = SvmType.EPSILON_SVR OrElse svm_type = SvmType.NU_SVR Then
                If param.C <= 0 Then Return "C <= 0"
            End If

            If svm_type = SvmType.NU_SVC OrElse svm_type = SvmType.ONE_CLASS OrElse svm_type = SvmType.NU_SVR Then
                If param.Nu <= 0 OrElse param.Nu > 1 Then Return "nu <= 0 or nu > 1"
            End If

            If svm_type = SvmType.EPSILON_SVR Then
                If param.P < 0 Then Return "p < 0"
            End If

            If param.Probability AndAlso svm_type = SvmType.ONE_CLASS Then Return "one-class SVM probability output not supported yet"

            ' check whether nu-svc is feasible

            If svm_type = SvmType.NU_SVC Then
                Dim l = prob.Count
                Dim max_nr_class = 16
                Dim nr_class = 0
                Dim label = New Integer(max_nr_class - 1) {}
                Dim count = New Integer(max_nr_class - 1) {}
                Dim i As Integer

                For i = 0 To l - 1
                    Dim this_label As Integer = prob.Y(i)
                    Dim j As Integer

                    For j = 0 To nr_class - 1

                        If this_label = label(j) Then
                            count(j) += 1
                            Exit For
                        End If
                    Next

                    If j = nr_class Then
                        If nr_class = max_nr_class Then
                            max_nr_class *= 2
                            Dim new_data = New Integer(max_nr_class - 1) {}
                            Array.Copy(label, 0, new_data, 0, label.Length)
                            label = new_data
                            new_data = New Integer(max_nr_class - 1) {}
                            Array.Copy(count, 0, new_data, 0, count.Length)
                            count = new_data
                        End If

                        label(nr_class) = this_label
                        count(nr_class) = 1
                        nr_class += 1
                    End If
                Next

                For i = 0 To nr_class - 1
                    Dim n1 = count(i)

                    For j = i + 1 To nr_class - 1
                        Dim n2 = count(j)
                        If param.Nu * (n1 + n2) / 2 > stdNum.Min(n1, n2) Then Return "specified nu is infeasible"
                    Next
                Next
            End If

            Return Nothing
        End Function

        Public Function svm_check_probability_model(model As Model) As Integer
            If (model.Parameter.SvmType = SvmType.C_SVC OrElse model.Parameter.SvmType = SvmType.NU_SVC) AndAlso model.PairwiseProbabilityA IsNot Nothing AndAlso model.PairwiseProbabilityB IsNot Nothing OrElse (model.Parameter.SvmType = SvmType.EPSILON_SVR OrElse model.Parameter.SvmType = SvmType.NU_SVR) AndAlso model.PairwiseProbabilityA IsNot Nothing Then
                Return 1
            Else
                Return 0
            End If
        End Function
    End Module
End Namespace
