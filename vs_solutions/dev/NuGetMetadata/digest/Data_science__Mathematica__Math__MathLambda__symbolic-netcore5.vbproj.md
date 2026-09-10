# Data_science/Mathematica/Math/MathLambda/symbolic-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.Lambda
- AssemblyName  : Microsoft.VisualBasic.Math.Lambda
- TargetFramework: net10.0
- Source files  : 16
- Existing Title: Symbolic Math Engine: Calculus, Algebra and MathML Compiler
- Existing Desc : Immutable expression-tree computer algebra for sciBASIC#: derivatives, integrals, limits, Taylor expansion, polynomial factorisation, simplification, Boolean algebra and a MathML to LINQ expression compiler.
- Existing Tags : scibasic;symbolic-math;computer-algebra;calculus;mathml;polynomial

## Namespaces
- Symbolic  [files: 14]

## Public types
- Class MathMLCompiler (MathMLCompiler.vb) - mathML -> lambda -> linq expression -> compile VB lambda
- Module BooleanAlgebra (Symbolic\BooleanAlgebra.vb)
- Module Derivative (Symbolic\Derivative.vb)
- Module ExpressionExtensions (Symbolic\ExpressionExtensions.vb) - Shared helpers that operate on the immutable <see cref="Expression"/> tree. All transformations are functional: they return a new node and never mutate the input tree.
- Module Integration (Symbolic\Integration.vb)
- Module Limit (Symbolic\Limit.vb)
- Module MakeSimplify (Symbolic\MakeSimplify.vb)
- Module PolyExpansion (Symbolic\PolyExpansion.vb) - Expand products and powers of sums into distributed form.
- Class UnivariatePoly (Symbolic\Polynomial.vb) - A univariate polynomial a0 + a1*x + ... + an*x^n with numeric coefficients.
- Module Polynomial (Symbolic\Polynomial.vb)
- Module Rationalizer (Symbolic\Rationalize.vb) - Rationalise the denominators of an expression: eliminate square roots and the imaginary unit from every denominator by conjugate multiplication.
- Module Substitute (Symbolic\Substitute.vb)
- Module Symbolic (Symbolic\Symbolic.vb) - symbolic computation engine
- Structure TaylorResult (Symbolic\Taylor.vb) - The polynomial part and the Lagrange remainder of a Taylor expansion.
- Module Taylor (Symbolic\Taylor.vb)
- Class UnaryExpression (Symbolic\UnaryExpression.vb)
- Class UnifySymbol (Symbolic\UnifySymbol.vb) - a unify symbol model a * (x ^ n)
- Class SymbolIndex (SymbolIndex.vb) - the parameter symbol index of the lambda function

## Notable public members
- Public Shared Function CreateLambda(lambda As MLLambda) As LambdaExpression
- Public Function TruthTable(vars As String(), f As Func(Of Boolean(), Boolean)) As Integer()
- Public Function QuineMcCluskey(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As String()
- Public Function QMCSimplifySOP(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
- Public Function QMCSimplifyPOS(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
- Public Function GetDerivative(exp As Expression, Optional wrt As String = Nothing) As Expression
- Public Function Differentiate(expr As Expression, x$) As Expression
- Public Function DerivativeN(expr As Expression, x$, n As Integer) As Expression
- Public Function PartialDerivative(expr As Expression, x$) As Expression
- Public Function Jacobian(funcs As Expression(), vars As String()) As Expression(,)
- Public Function Hessian(f As Expression, vars As String()) As Expression(,)
- Public Function ImplicitDerivative(F As Expression, dependentVar$, independentVar$) As Expression
- Public Function Clone(expr As Expression) As Expression
- Public Function ExprEquals(a As Expression, b As Expression) As Boolean
- Public Function GetSymbols(expr As Expression) As String()
- Public Function IsConstant(expr As Expression) As Boolean
- Public Function DependsOn(expr As Expression, var$) As Boolean
- Public Function MakeLiteral(x As Double) As Literal
- Public Function Add(a As Expression, b As Expression) As Expression
- Public Function Subt(a As Expression, b As Expression) As Expression
- Public Function Mul(a As Expression, b As Expression) As Expression
- Public Function Div(a As Expression, b As Expression) As Expression
- Public Function Pow(a As Expression, b As Expression) As Expression
- Public Function Negate(a As Expression) As Expression
- Public Function Reciprocal(a As Expression) As Expression
- Public Function FlattenSum(expr As Expression) As List(Of Expression)
- Public Function FlattenProduct(expr As Expression) As List(Of Expression)
- Public Sub SplitCoefficient(term As Expression, ByRef coeff As Double, ByRef body As Expression)
- Public Function NumericValue(expr As Expression) As Double?
- Public Function Rewrite(expr As Expression) As Expression
- Protected Overridable Function RewriteLiteral(x As Literal) As Expression
- Protected Overridable Function RewriteSymbol(x As SymbolExpression) As Expression
- Protected Overridable Function RewriteBinary(x As BinaryExpression) As Expression
- Protected Overridable Function RewriteFunction(name$, args As Expression()) As Expression
- Protected Overridable Function RewriteUnary(op$, v As Expression) As Expression
- Protected Overridable Function RewriteNot(v As Expression) As Expression
- Protected Overridable Function RewriteLogical(x As LogicalLiteral) As Expression
- Protected Overridable Function RewriteFactorial(v As Expression) As Expression
- Public Function Integrate(expr As String, var$) As Expression
- Public Function Integrate(expr As Expression, var$) As Expression
- Public Function DefiniteIntegral(expr As String, var$, lower As Double, upper As Double) As Double
- Public Function DefiniteIntegral(expr As Expression, var$, lower As Double, upper As Double) As Double
- Public Function Limit(expr As String, var$, target As String) As Expression
- Public Function Limit(expr As Expression, var$, target As Expression) As Expression
- Friend Function simplifyExpr(raw As Expression) As Expression
- Friend Function simplifyRaw(raw As Expression) As Expression
- Public Function Expands(expression As Expression) As Expression
- Public ReadOnly Property Degree As Integer
- Friend Function IsUnivariatePolynomial(expr As Expression, var$) As Boolean
- Public Function Factor(expr As Expression, Optional var As String = Nothing) As Expression
- Public Function Factor(expr As Expression, vars As String()) As Expression
- Public Function PolynomialMultiply(a As Expression, b As Expression, Optional var As String = Nothing) As Expression
- Public Function PolynomialDivide(dividend As Expression, divisor As Expression, Optional var As String = Nothing) As Expression
- Public Function PolynomialRemainder(dividend As Expression, divisor As Expression, Optional var As String = Nothing) As Expression
- Public Function PolynomialGCD(a As Expression, b As Expression, Optional var As String = Nothing) As Expression
- Friend Function Rationalize(expr As Expression) As Expression
- Public Function Substitute(expr As Expression, oldSymbol$, replacement As Expression) As Expression
- Public Function Substitute(expr As Expression, oldSymbol$, value As Double) As Expression
- Public Function Substitute(expr As Expression, mapping As Dictionary(Of String, Expression)) As Expression
- Public Function Simplify(expr As String) As Expression
- ... and 55 more

## Imports
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math.Scripting.MathExpression
- Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
- Microsoft.VisualBasic.MIME.application.xml
- Microsoft.VisualBasic.Scripting.Runtime
- ML = Microsoft.VisualBasic.MIME.application.xml.MathML.BinaryExpression
- MLLambda = Microsoft.VisualBasic.MIME.application.xml.MathML.LambdaExpression
- MLSymbol = Microsoft.VisualBasic.MIME.application.xml.MathML.SymbolExpression
- Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine
- System.Linq.Expressions
- System.Reflection
- System.Runtime.CompilerServices

## File tree
- MathMLCompiler.vb
- Symbolic\BooleanAlgebra.vb
- Symbolic\Derivative.vb
- Symbolic\ExpressionExtensions.vb
- Symbolic\Integration.vb
- Symbolic\Limit.vb
- Symbolic\MakeSimplify.vb
- Symbolic\PolyExpansion.vb
- Symbolic\Polynomial.vb
- Symbolic\Rationalize.vb
- Symbolic\Substitute.vb
- Symbolic\Symbolic.vb
- Symbolic\Taylor.vb
- Symbolic\UnaryExpression.vb
- Symbolic\UnifySymbol.vb
- SymbolIndex.vb

