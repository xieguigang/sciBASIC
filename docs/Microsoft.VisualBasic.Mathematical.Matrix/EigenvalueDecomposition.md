# EigenvalueDecomposition
_namespace: [Microsoft.VisualBasic.Mathematical.Matrix](./index.md)_

Eigenvalues and eigenvectors of a real matrix. 
 If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
 diagonal and the eigenvector matrix V is orthogonal.
 I.e. A = V.Multiply(D.Multiply(V.Transpose())) and 
 V.Multiply(V.Transpose()) equals the identity matrix.
 If A is not symmetric, then the eigenvalue matrix D is block diagonal
 with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
 lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda]. The
 columns of V represent the eigenvectors in the sense that A*V = V*D,
 i.e. A.Multiply(V) equals V.Multiply(D). The matrix V may be badly
 conditioned, or even singular, so the validity of the equation
 A = V*D*Inverse(V) depends upon V.cond().



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.EigenvalueDecomposition.#ctor(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Check for symmetry, then construct the eigenvalue decomposition, returns Structure to access D and V.

|Parameter Name|Remarks|
|--------------|-------|
|Arg|Square matrix|


#### GetV
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.EigenvalueDecomposition.GetV
```
Return the eigenvector matrix

_returns:      V
 _


### Properties

#### D
Return the block diagonal eigenvalue matrix
#### e
Arrays for internal storage of eigenvalues.
 @serial internal storage of eigenvalues.
#### H
Array for internal storage of nonsymmetric Hessenberg form.
 @serial internal storage of nonsymmetric Hessenberg form.
#### ImagEigenvalues
Return the imaginary parts of the eigenvalues
#### issymmetric
Symmetry flag.
 @serial internal symmetry flag.
#### m_d
Arrays for internal storage of eigenvalues.
 @serial internal storage of eigenvalues.
#### n
Row and column dimension (square matrix).
 @serial matrix dimension.
#### ort
Working storage for nonsymmetric algorithm.
 @serial working storage for nonsymmetric algorithm.
#### RealEigenvalues
Return the real parts of the eigenvalues
#### V
Array for internal storage of eigenvectors.
 @serial internal storage of eigenvectors.
