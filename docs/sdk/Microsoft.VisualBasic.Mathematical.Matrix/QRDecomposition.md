# QRDecomposition
_namespace: [Microsoft.VisualBasic.Mathematical.Matrix](./index.md)_

QR Decomposition.
 For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
 orthogonal matrix Q and an n-by-n upper triangular matrix R so that
 A = Q*R.
 
 The QR decompostion always exists, even if the matrix does not have
 full rank, so the constructor will never fail. The primary use of the
 QR decomposition is in the least squares solution of nonsquare systems
 of simultaneous linear equations. This will fail if IsFullRank()
 returns false.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.QRDecomposition.#ctor(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
QR Decomposition, computed by Householder reflections. returns Structure to access R and the Householder vectors and compute Q.

|Parameter Name|Remarks|
|--------------|-------|
|A|   Rectangular matrix
 |


#### Solve
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.QRDecomposition.Solve(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Least squares solution of A*X = B

|Parameter Name|Remarks|
|--------------|-------|
|B|   A Matrix with as many rows as A and any number of columns.
 |


_returns:      X that minimizes the two norm of Q*R*X-B.
 _


### Properties

#### FullRank
Is the matrix full rank?
#### H
Return the Householder vectors
#### m
Row and column dimensions.
 @serial column dimension.
 @serial row dimension.
#### n
Row and column dimensions.
 @serial column dimension.
 @serial row dimension.
#### Q
Generate and return the (economy-sized) orthogonal factor
#### QR
Array for internal storage of decomposition.
 @serial internal array storage.
#### R
Return the upper triangular factor
#### Rdiag
Array for internal storage of diagonal of R.
 @serial diagonal of R.
