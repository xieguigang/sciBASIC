#Region "Microsoft.VisualBasic::a6660c7d33a346849cd42dd7a67bfe2b, sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Rscript\Math\Skellam.vb"

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

    '   Total Lines: 356
    '    Code Lines: 0
    ' Comment Lines: 325
    '   Blank Lines: 31
    '     File Size: 20.43 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::a2789e7bbaebd6418755dc5bbda089b1, ..\R.Bioconductor\RDotNET.Extensions.VisualBasic\RSyntax\Math\Skellam.vb"

'' Author:
'' 
''       asuka (amethyst.asuka@gcmodeller.org)
''       xieguigang (xie.guigang@live.com)
''       xie (genetics@smrucc.org)
'' 
'' Copyright (c) 2016 GPL3 Licensed
'' 
'' 
'' GNU GENERAL PUBLIC LICENSE (GPL3)
'' 
'' This program is free software: you can redistribute it and/or modify
'' it under the terms of the GNU General Public License as published by
'' the Free Software Foundation, either version 3 of the License, or
'' (at your option) any later version.
'' 
'' This program is distributed in the hope that it will be useful,
'' but WITHOUT ANY WARRANTY; without even the implied warranty of
'' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'' GNU General Public License for more details.
'' 
'' You should have received a copy of the GNU General Public License
'' along with this program. If not, see <http://www.gnu.org/licenses/>.

'#End Region

'Imports Microsoft.VisualBasic.CommandLine.Reflection
'Imports Microsoft.VisualBasic.Scripting
'Imports Microsoft.VisualBasic.Scripting.MetaData
'Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
'Imports Microsoft.VisualBasic.Math.BasicR

'Namespace SyntaxAPI.MathExtension

'    ''' <summary>
'    ''' Skellam distribution
'    ''' 
'    ''' 
'    ''' </summary>
'    ''' <remarks></remarks>
'    <[PackageNamespace]("Skellam",
'                    Description:="The Skellam Distribution",
'                    Cites:="<li>[JOH1] Johnson N L, Kotz S (1969) Discrete distributions. Houghton Mifflin/J Wiley & Sons, New York</li>
'<li>[IRW1] Irwin J O (1937) The frequency distribution of the difference between two independent variates following the same Poisson distribution. J. of the Royal Statistical Society, Series A, 100, 415-416</li>
'                    <li>[SKE1] Skellam J G (1946) The frequency distribution of the difference between two Poisson variates belonging to different populations. J. of the Royal Statistical Society, Series A, 109, 296</li>
'                    <p><br />
'Wikipedia: Skellam distribution: http://en.wikipedia.org/wiki/Skellam_distribution",
'                    Url:="http://www.statsref.com/HTML/index.html?skellam.html")>
'    Public Module Skellam

'        ''' <summary>
'        ''' Density, distribution function, quantile function and random number generation for the Skellam distribution with parameters lambda1 and lambda2. 
'        ''' </summary>
'        ''' <param name="x">vector of quantiles.</param>
'        ''' <param name="lambda1">vectors of (non-negative) means.</param>
'        ''' <param name="lambda2">vectors of (non-negative) means.</param>
'        ''' <param name="log">logical; if TRUE, probabilities p are given as log(p).</param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        ''' 
'        <ExportAPI("dskellam.sp", Info:="Density, distribution function, quantile function and random number generation for the Skellam distribution with parameters lambda1 and lambda2. ")>
'        Public Function DskellamSp(<Parameter("X", "Vector of quantiles")> x As Vector,
'                                   <Parameter("lambda1", "vectors of (non-negative) means.")> lambda1 As Vector,
'                                   <Parameter("lambda2", "vectors of (non-negative) means.")> Optional lambda2 As Vector = Nothing,
'                                   <Parameter("log", "logical; if TRUE, probabilities p are given as log(p).")> Optional log As Boolean = False) As Vector

'            If lambda2.IsNullOrEmpty Then
'                lambda2 = lambda1
'            End If

'            ' saddlepoint density (PMF) for Skellam distribution

'            Dim terms = 1
'            If Missing(x) OrElse Missing(lambda1) Then Call [Stop]("first 2 arguments are required")

'            Dim s = VectorMath.Log(0.5 * (x + Sqrt(x ^ 2 + 4 * lambda1 * lambda2)) / lambda1) '# the saddlepoint
'            Dim K = lambda1 * (Exp(s) - 1) + lambda2 * (Exp(-s) - 1) '	# CGF(s)
'            Dim K2 = lambda1 * Exp(s) + lambda2 * Exp(-s)       ' # CGF''(s)

'            Dim ret As Vector

'            If (terms < 1) Then
'                ret = Exp(K - x * s) / Sqrt(2 * sys.PI * K2)  'saddlepoint density
'            Else
'                Dim c = (1 - ((lambda1 * Exp(s) - lambda2 * Exp(-s)) / K2) ^ 2 * 5 / 3) / K2 * 0.125 + 1
'                ret = Exp(K - x * s) / Sqrt(2 * sys.PI * K2) * (1 + c) * 0.5
'            End If
'            Return ret
'        End Function

'        <ExportAPI("dskellam")>
'        Public Function dskellam(x As Vector, lambda1 As Vector, Optional lambda2 As Vector = Nothing, Optional log As Boolean = False) As Vector

'            If Missing(lambda2) Then
'                lambda2 = lambda1
'            End If

'            '# density (PMF) of Skellam distriubition (difference of Poissons)
'            If (Missing(x) OrElse Missing(lambda1)) Then Call [Stop]("first 2 arguments are required")
'            Dim lambdas As Vector = C(lambda1, lambda2)
'            Dim oops = Not ([Is].Finite(lambdas) & (lambdas >= 0))
'            If (Any(oops)) Then
'                warning("NaNs produced")
'                lambdas.SelectWhere(oops) = Vector.NAN
'                lambda1 = lambdas.Get(1, Length(lambda1))
'                lambda2 = lambdas.Get((Length(lambda1) + 1), Length(lambdas))
'            End If
'            '# make all args the same length (for subsetting)
'            Dim lens = C(Length(x), Length(lambda1), Length(lambda2))
'            Dim len = Max(lens)
'            If (len > Min(lens)) Then
'                If (All(len / lens <> len Mod lens)) Then warning("longer object length is not a multiple of shorter object length", Domain:=NA)
'                x = Rep(x, LengthOut:=len)
'                lambda1 = Rep(lambda1, LengthOut:=len)
'                lambda2 = Rep(lambda2, LengthOut:=len)
'            End If
'            '# warn of non-integer x values (since support of PMF is integers)
'            Dim nonint As BooleanVector = x <> Trunc(x)
'            If (Any(nonint)) Then
'                Dim xreal = x.SelectWhere(nonint)
'                For i As Integer = 1 To Length(xreal)
'                    warning(Paste("non-integer x =", xreal(i)))
'                Next
'            End If
'            x = Trunc(x)
'            Dim ret = Rep(Vector.NAN, LengthOut:=len)
'            ' handle a zero lambda separately (avoids division by zero & accuracy issues for large values of lambda or x)
'            ret(lambda1 = 0) = Dpois(-x(lambda1 = 0), lambda2(lambda1 = 0), log = log)
'            ret(lambda2 = 0) = Dpois(x(lambda2 = 0), lambda1(lambda2 = 0), log = log)    '# corrects bug in VGAM 0.7-9
'            ' non-zero lambdas
'            Dim nz = [Is].Finite(lambda1) & [Is].Finite(lambda2) & (lambda1 > 0) & (lambda2 > 0)
'            Dim L1 As Vector = lambda1.SelectWhere(nz)
'            Dim L2 As Vector = lambda2.SelectWhere(nz)
'            Dim sqL12 = Sqrt(L1 * L2)
'            Dim xx = x(nz)
'            If (log) Then       ' use abs(x) in besselI for improved accuracy (prior to 2.9) and S-PLUS compatibility
'                ' log(besselI(y,nu)) == y+log(besselI(y,nu,TRUE))
'                ret(nz) = VectorMath.Log(BesselI(2 * sqL12, Abs(xx), True)) + 2 * sqL12 - L1 - L2 + xx / 2 * VectorMath.Log(L1 / L2)
'            Else
'                ' besselI(y,nu); exp(y)*besselI(y,nu,TRUE)
'                ' ret[nz] <- besselI(2*sqL12, abs(xx),TRUE)*(L1/L2)^(xx/2)*exp(2*sqL12-L1-L2)
'                ret(nz) = BesselI(2 * sqL12, Abs(xx), True) * Exp(2 * sqL12 - L1 - L2 + xx / 2 * VectorMath.Log(L1 / L2))
'            End If
'            Dim chk = nz & (Not [Is].Finite(ret) Or {Not log} & (ret < 1.0E-308)) ' use saddlepoint approximation to detect possible over/underflow
'            If (Length(chk(chk)) > 0) Then
'                L1 = lambda1(chk)
'                L2 = lambda2(chk)
'                sqL12 = Sqrt(L1 * L2)
'                xx = x(chk)
'                Dim s = VectorMath.Log(0.5 * (xx + Sqrt(xx ^ 2 + 4 * L1 * L2)) / L1) '# the saddlepoint
'                Dim K = L1 * (Exp(s) - 1) + L2 * (Exp(-s) - 1)       '# CGF(s)
'                Dim K2 = L1 * Exp(s) + L2 * Exp(-s)              '# CGF''(s)
'                Dim spd = Exp(K - x * s) / Sqrt(2 * sys.PI * K2)         '# saddlepoint density
'                Dim usp = (spd > 1.0E-308) & [Is].Finite(spd)      '# don't trust the existing result
'                If (Length(usp(usp)) > 0) Then  '  # add another term to the saddlepoint approximation
'                    Dim su = s(usp)
'                    Dim K2u = K2(usp)
'                    Dim C = (1 - ((L1(usp) * Exp(su) - L2(usp) * Exp(-su)) / K2u) ^ 2 * 5 / 3) / K2u * 0.125 + 1
'                    ret(chk)(usp) = Exp(K(usp) - x(usp) * su) / Sqrt(2 * sys.PI * K2u) * (1 + C) * 0.5
'                End If
'            End If
'            Return ret
'        End Function

'        <ExportAPI("pskellamSp")>
'        Public Function pskellamSp(q As Vector, lambda1 As Vector, Optional lambda2 As Vector = NULL, <MetaData.Parameter("Lower.Tail")> Optional LowerTail As Boolean = True, <MetaData.Parameter("log.p")> Optional logp As Boolean = False) As Vector

'            If Missing(lambda2) Then
'                lambda2 = lambda1
'            End If

'            '# Luganni-Rice saddlepoint CDF with Butler's 2nd continuity correction
'            If Missing(q) OrElse Missing(lambda1) Then [Stop]("first 2 arguments are required")

'            Dim ret As Vector

'            If LowerTail Then
'                Dim xm = -floor(q) - 0.5                                   '  # continuity corrected x
'                '# distribution specific variables
'                Dim s = Log(0.5 * (xm + Sqrt(xm ^ 2 + 4 * lambda2 * lambda1)) / lambda2) '# the saddlepoint
'                Dim K = lambda2 * (Exp(s) - 1) + lambda1 * (Exp(-s) - 1)            ' # CGF(s)
'                Dim K2 = lambda2 * Exp(s) + lambda1 * Exp(-s)                   ' # CGF''(s)
'                '# code depending on distribution only through previous variables
'                Dim u2 = 2 * Sinh(0.5 * s) * Sqrt(K2)
'                Dim w2 = Sign(s) * Sqrt(2 * (s * xm - K))
'                ret = pnorm(-w2) - dnorm(w2) * (1 / w2 - 1 / u2)
'                '# avoid numeric problems near the removable discontinuity
'                Dim xe = (xm + (lambda1 - lambda2)) / Sqrt(lambda1 + lambda2)
'                Dim g1 = (lambda1 - lambda2) / (lambda1 + lambda2) ^ 1.5
'                Dim ew = Abs(xe) < 0.0001
'                ret(ew) = (pnorm(-xe) + dnorm(xe) * g1 / 6 * (1 - xe ^ 2))(ew)
'            Else
'                ret = pskellamSp(-q - 1, lambda2, lambda1)
'            End If

'            If logp Then Return VectorMath.Log(ret) Else Return ret
'        End Function

'        <ExportAPI("pskellam")>
'        Public Function pskellam(q As Vector, lambda1 As Vector, Optional lambda2 As Vector = NULL, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector

'            If Missing(lambda2) Then
'                lambda2 = lambda1
'            End If

'            ' # CDF of Skellam distriubition (difference of Poissons)
'            If (Missing(q) OrElse Missing(lambda1)) Then [Stop]("first 2 arguments are required")
'            Dim lambdas As Vector = C(lambda1, lambda2)
'            Dim oops = Not ([Is].Finite(lambdas) & (lambdas >= 0))
'            If (Any(oops)) Then
'                warning("NaNs produced")
'                lambdas.SelectWhere(oops) = Vector.NAN
'                lambda1 = lambdas.Get(1, Length(lambda1))
'                lambda2 = lambdas.Get((Length(lambda1) + 1), Length(lambdas))
'            End If
'            '# CDF is a step function, so convert to integer values without warning
'            Dim x = floor(q)
'            '  # make all args the same length (for subsetting)
'            Dim lens = C(Length(x), Length(lambda1), Length(lambda2))
'            Dim len = Max(lens)
'            If (len > Min(lens)) Then
'                If (All(len / lens <> len Mod lens)) Then warning("longer object length is not a multiple of shorter object length", Domain:=NA)
'                x = Rep(x, LengthOut:=len)
'                lambda1 = Rep(lambda1, LengthOut:=len)
'                lambda2 = Rep(lambda2, LengthOut:=len)
'            End If
'            '  # different formulas for negative & nonnegative x (zero lambda is OK)
'            Dim neg = (x < 0) & (Not [Is].NAN(lambda1)) & (Not [Is].NAN(lambda2))
'            Dim pos = (x >= 0) & (Not [Is].NAN(lambda1)) & (Not [Is].NAN(lambda2))
'            Dim ret = Rep(Vector.NAN, LengthOut:=len)
'            If (lowertail) Then
'                ret(neg) = pchisq(2 * lambda2(neg), -2 * x(neg), 2 * lambda1(neg), logp:=logp)
'                ret(pos) = pchisq(2 * lambda1(pos), 2 * (x(pos) + 1), 2 * lambda2(pos), lowertail:=False, logp:=logp)   '  # S-PLUS does not have a lower.tail argument
'            Else
'                ret(neg) = pchisq(2 * lambda2(neg), -2 * x(neg), 2 * lambda1(neg), lowertail:=False, logp:=logp)      '  # S-PLUS does not have a lower.tail argument
'                ret(pos) = pchisq(2 * lambda1(pos), 2 * (x(pos) + 1), 2 * lambda2(pos), logp:=logp)
'            End If
'            Dim chk = (neg Or pos) & (Not [Is].Finite(ret) Or {Not logp} & (ret < 1.0E-308))   ' # use saddlepoint approximation if outside the working range of pchisq
'            If (Length(chk(chk)) > 0) Then ret(chk) = pskellamSp(x(chk), lambda1(chk), lambda2(chk), lowertail, logp)
'            Return ret
'        End Function

'        <ExportAPI("rskellam")>
'        Public Function rskellam(n As Vector, lambda1 As Vector, Optional lambda2 As Vector = NULL) As Vector
'            If Missing(lambda2) Then
'                lambda2 = lambda1
'            End If

'            ' # Skellam random variables
'            If (Missing(n) Or Missing(lambda1)) Then Call [Stop]("first 2 arguments are required")
'            If (Length(n) > 1) Then n = Length(n)
'            lambda1 = Rep(lambda1, LengthOut:=n)
'            lambda2 = Rep(lambda2, LengthOut:=n)
'            Dim oops = Not ([Is].Finite(lambda1) & (lambda1 >= 0) & [Is].Finite(lambda2) & (lambda2 >= 0))
'            If (Any(oops)) Then Call warning("NaNs produced")
'            Dim ret = Rep(Vector.NAN, LengthOut:=n)
'            n = n - Sum(oops)
'            ret(Not oops) = rPois(n, lambda1(Not oops)) - rPois(n, lambda2(Not oops))
'            Return ret
'        End Function

'        <ExportAPI("qskellam")>
'        Public Function qskellam(p As Vector, lambda1 As Vector, Optional lambda2 As Vector = NULL, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector

'            If Missing(lambda2) Then
'                lambda2 = lambda1
'            End If

'            '# inverse CDF of Skellam distriubition (difference of Poissons)
'            If (Missing(p) Or Missing(lambda1)) Then Call [Stop]("first 2 arguments are required")
'            '# make all args the same length (for subsetting)
'            Dim lens = C(Length(p), Length(lambda1), Length(lambda2))
'            Dim len = Max(lens)
'            If (len > Min(lens)) Then
'                If (All(len / lens <> len Mod lens)) Then Call warning("longer object length is not a multiple of shorter object length", Domain:=NA)
'                p = Rep(p, LengthOut:=len)
'                lambda1 = Rep(lambda1, LengthOut:=len)
'                lambda2 = Rep(lambda2, LengthOut:=len)
'            End If
'            Dim ret = Rep(Vector.NAN, LengthOut:=len)
'            Dim nz As BooleanVector
'            '# handle a zero lambda separately (quicker than interpreted search)
'            If (getOption("verbose")) Then  ' # set via  options(verbose=TRUE)  # could create a new option called validate, but would would have to test for existence before value
'                nz = Rep(BooleanVector.True, LengthOut:=len)   ' # verify search by using it for Poisson too
'            Else
'                ret(lambda2 = 0) = qpois(p(lambda2 = 0), lambda1(lambda2 = 0), lowertail:=lowertail, logp:=logp)
'                ret(lambda1 = 0) = -qpois(p(lambda1 = 0), lambda2(lambda1 = 0), lowertail:=Not lowertail, logp:=logp)
'                nz = (lambda1 <> 0) & (lambda2 <> 0)
'            End If
'            '# handle boundaries correctly
'            Dim bdry = nz & ((p = 0) Or (p + 1.01 * Machine.Double.Eps >= 1))     '# match qpois in assuming that p with 2.25e-16 of 1 are actually 1
'            If (Any(bdry)) Then
'                If (lowertail) Then
'                    ret(p(bdry) = 0) = IfElse(lambda2(p(bdry) = 0) = 0, Vector.Zero, -Vector.Inf)
'                    ret(p(bdry) > 0) = IfElse(lambda1(p(bdry) > 0) = 0, Vector.Zero, Vector.Inf)
'                Else
'                    ret(p(bdry) > 0) = IfElse(lambda2(p(bdry) = 0) = 0, Vector.Zero, -Vector.Inf)
'                    ret(p(bdry) = 0) = IfElse(lambda1(p(bdry) > 0) = 0, Vector.Zero, Vector.Inf)
'                End If
'            End If
'            If Any(bdry) Or (Not getOption("verbose") & Any(Not nz)) Then
'                ' # avoid repeated susetting later
'                nz = nz & Not bdry
'                p = p(nz)
'                lambda1 = lambda1(nz)
'                lambda2 = lambda2(nz)
'            End If
'            '  # Cornish-Fisher approximations
'            Dim z = qnorm(p, lowertail:=lowertail, logp:=logp)
'            Dim mu = lambda1 - lambda2
'            Dim vr = lambda1 + lambda2
'            Dim sg = Sqrt(vr)
'            Dim c0 = mu + z * sg
'            Dim c1 = (z ^ 2 - 1) * mu / vr / 6
'            Dim c2 = -(c1 * mu - 2 * lambda1 * lambda2 * (z ^ 2 - 3) / vr) * z / 12 / vr / sg
'            '   # test and linear search (slow if p extreme or lambda1+lambda2 small)
'            Dim q0 = round(c0 + c1 + c2)
'            Dim p0 = pskellam(q0, lambda1, lambda2, lowertail:=lowertail, logp:=logp)
'            p = p * (1 - 64 * Machine.Double.Eps)     '  # match qpois in assuming that a value within 1.4e-14 of p actually equals p

'            Dim down1 As BooleanVector

'            If (lowertail) Then
'                Dim up1 = p0 < p ' # smallest x such that F(x) >= p (consider floor for greater efficiency?)
'                Dim up = p0 < p
'                Do While (Any(up1))
'                    q0(up1) = q0(up1) + 1
'                    up1(up1) = pskellam(q0(up1), lambda1(up1), lambda2(up1), lowertail:=lowertail, logp:=logp) < p(up1)
'                Loop
'                down1 = (Not up) & (p0 > p) '  # oops: \qskellam(p,lambda,0) == qpois(p,lambda)+1 usually
'                down1(down1) = (pskellam(q0(down1) - 1, lambda1(down1), lambda2(down1), lowertail:=lowertail, logp:=logp) > p(down1))
'                Do While (Any(down1))
'                    q0(down1) = q0(down1) - 1
'                    down1(down1) = (pskellam(q0(down1) - 1, lambda1(down1), lambda2(down1), lowertail:=lowertail, logp:=logp) > p(down1))
'                Loop
'            Else       '  # largest x such that F(x,lower.tail=FALSE) <= p (consider ceiling for greater efficiency?)
'                down1 = p0 > p
'                Dim down = p0 > p
'                Do While (Any(down1))
'                    q0(down1) = q0(down1) + 1
'                    down1(down1) = pskellam(q0(down1), lambda1(down1), lambda2(down1), lowertail:=lowertail, logp:=logp) > p(down1)
'                Loop
'                Dim up1 = (Not down) & (p0 < p)
'                up1(up1) = Not (pskellam(q0(up1) - 1, lambda1(up1), lambda2(up1), lowertail:=lowertail, logp:=logp) > p(up1))
'                Do While (Any(up1))
'                    q0(up1) = q0(up1) - 1
'                    up1(up1) = Not (pskellam(q0(up1) - 1, lambda1(up1), lambda2(up1), lowertail:=lowertail, logp:=logp) > p(up1))
'                Loop
'            End If
'            ret(nz) = q0
'            Return ret
'        End Function
'    End Module
'End Namespace
