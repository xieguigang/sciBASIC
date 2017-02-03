# CholeskyDecomposition
_namespace: [Microsoft.VisualBasic.Mathematical.Matrix](./index.md)_

Cholesky Decomposition.
 For a symmetric, positive definite matrix A, the Cholesky decomposition
 is an lower triangular matrix L so that A = L*L'.
 If the matrix is not symmetric or positive definite, the constructor
 returns a partial decomposition and sets an internal flag that may
 be queried by the isSPD() method.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.CholeskyDecomposition.#ctor(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Cholesky algorithm for symmetric and positive definite matrix. returns Structure to access L and isspd flag.

|Parameter Name|Remarks|
|--------------|-------|
|Arg|  Square, symmetric matrix.
 |


#### GetL
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.CholeskyDecomposition.GetL
```
Return triangular factor.

_returns:      L
 _

#### Solve
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.CholeskyDecomposition.Solve(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Solve A*X = B

|Parameter Name|Remarks|
|--------------|-------|
|B|  A Matrix with as many rows as A and any number of columns.
 |


_returns:      X so that L*L'*X = B
 _


### Properties

#### isspd
Symmetric and positive definite flag.
 @serial is symmetric and positive definite flag.
#### L
Array for internal storage of decomposition.
 @serial internal array storage.
#### n
Row and column dimension (square matrix).
 @serial matrix dimension.
#### SPD
Is the matrix symmetric and positive definite?
