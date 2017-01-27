# MD5Hash
_namespace: [Microsoft.VisualBasic.SecurityString](./index.md)_





### Methods

#### GetFileHashString
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.GetFileHashString(System.String)
```
Get the md5 hash calculation value for a specific file.(获取文件对象的哈希值，请注意，当文件不存在或者文件的长度为零的时候，会返回空字符串)

|Parameter Name|Remarks|
|--------------|-------|
|PathUri|The file path of the target file to be calculated.|


#### GetHashCode
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.GetHashCode(System.Collections.Generic.IEnumerable{System.Byte})
```
Gets the hashcode of the input string.

#### GetMd5Hash
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.GetMd5Hash(System.String)
```
Calculate md5 hash value for the input string.

|Parameter Name|Remarks|
|--------------|-------|
|input|-|


#### SaltValue
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.SaltValue(System.String)
```
SHA256 8 bits salt value for the private key.

|Parameter Name|Remarks|
|--------------|-------|
|value|-|


#### StringToByteArray
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.StringToByteArray(System.String)
```
由于md5是大小写无关的，故而在这里都会自动的被转换为小写形式，所以调用这个函数的时候不需要在额外的转换了

|Parameter Name|Remarks|
|--------------|-------|
|hex|-|


#### ToLong
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.ToLong(System.Byte[])
```
CityHash algorithm for convert the md5 hash value as a @``T:System.Int64`` value.

|Parameter Name|Remarks|
|--------------|-------|
|bytes|
 this input value should compute from @``M:Microsoft.VisualBasic.SecurityString.Md5HashProvider.GetMd5Bytes(System.Byte[])``
 |

> 
>  http://stackoverflow.com/questions/9661227/convert-md5-to-long
>  
>  The very best solution I found (based on my needs... mix of speed and good hash function) is Google's CityHash. 
>  The input can be any byte array including an MD5 result and the output is an unsigned 64-bit long.
> 
>  CityHash has a very good but Not perfect hash distribution, And Is very fast.
> 
>  I ported CityHash from C++ To C# In half an hour. A Java port should be straightforward too.
> 
>  Just XORing the bits doesn't give as good a distribution (though admittedly that will be very fast).
> 
>  I'm not familiar enough with Java to tell you exactly how to populate a long from a byte array 
>  (there could be a good helper I'm not familiar with, or I could get some details of arithmetic 
>  in Java wrong). 
>  Essentially, though, you'll want to do something like this:
> 
>  Long a = md5[0] * 256 * md5[1] + 256 * 256 * md5[2] + 256 * 256 * 256 * md5[3];
>  Long b = md5[4] * 256 * md5[5] + 256 * 256 * md5[6] + 256 * 256 * 256 * md5[7];
>  Long result = a ^ b;
>  
>  Note I have made no attempt To deal With endianness. If you just care about a consistent hash value, 
>  though, endianness should Not matter.
>  

#### VerifyFile
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.VerifyFile(System.String,System.String)
```
校验两个文件的哈希值是否一致

|Parameter Name|Remarks|
|--------------|-------|
|query|-|
|subject|-|


#### VerifyMd5Hash
```csharp
Microsoft.VisualBasic.SecurityString.MD5Hash.VerifyMd5Hash(System.String,System.String)
```
Verify a hash against a string.

|Parameter Name|Remarks|
|--------------|-------|
|input|-|
|comparedHash|-|



