# Cube
_namespace: [Microsoft.VisualBasic.Imaging.Drawing3D](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Cube.#ctor(System.Int32,System.Drawing.Color[])
```


|Parameter Name|Remarks|
|--------------|-------|
|d%|正方形的边的长度|
|colors|-|


#### GetEnumerator
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Cube.GetEnumerator
```

> 
>  ```
>  0 - {0, 1, 2, 3} 0,1,2,3 = 0,1,2,3
>  1 - {1, 5, 6, 2}   1,2   = 5,6
>  2 - {5, 4, 7, 6}   1,2   = 4,7
>  3 - {4, 0, 3, 7}
>  4 - {0, 4, 5, 1}
>  5 - {3, 2, 6, 7}
>  ```
>  


### Properties

#### _faces
Create an array representing the 6 faces of a cube. Each face is composed by indices to the vertex array
 above.
