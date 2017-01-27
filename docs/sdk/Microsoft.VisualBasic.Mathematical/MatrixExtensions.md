# MatrixExtensions
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

This class contains methods that perform mathematical operations on matrices.
 Operations currently supported are matrix multiplication and scalar multiplication.
 
 @author Jean-Francois Larcher-Pelland



### Methods

#### MatrixMult
```csharp
Microsoft.VisualBasic.Mathematical.MatrixExtensions.MatrixMult(System.Double[][],System.Double[][])
```
Multiplies matrices a and b using the brute-force algorithm.

|Parameter Name|Remarks|
|--------------|-------|
|a| The matrix on the left. |
|b| The matrix on the right. |


_returns:  The product of the two matrices. _

#### ScalarMult
```csharp
Microsoft.VisualBasic.Mathematical.MatrixExtensions.ScalarMult(System.Double[][],System.Double)
```
Performs a scalar multiplication on matrix a using scalar value b.

|Parameter Name|Remarks|
|--------------|-------|
|a| The matrix to be multiplied. |
|b| Scalar value used in the multiplication. |


_returns:  The result of the scalar multiplication. _


