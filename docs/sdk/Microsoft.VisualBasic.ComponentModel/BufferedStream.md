# BufferedStream
_namespace: [Microsoft.VisualBasic.ComponentModel](./index.md)_

Buffered large text dataset reader



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.BufferedStream.#ctor(System.String,System.Text.Encoding,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|@``P:System.Text.Encoding.Default``, if null|


#### BufferProvider
```csharp
Microsoft.VisualBasic.ComponentModel.BufferedStream.BufferProvider
```
当@``P:Microsoft.VisualBasic.ComponentModel.BufferedStream.EndRead``之后，这个函数将不会返回任何值

#### Reset
```csharp
Microsoft.VisualBasic.ComponentModel.BufferedStream.Reset
```
Reset the stream buffer reader to its initial state.


### Properties

#### EndRead
End of buffer read?
#### FileName
The File location of this text file.
