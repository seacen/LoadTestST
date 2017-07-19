# LoadTestST
#### Easily extensible load testing framework

## NuGet Package
https://www.nuget.org/packages/LoadTestST/

## Usage
1. Implement [`ILogger`](LoadTestST/ILogger.cs) interface (e.g. `Logger : ILogger`)

1. Inherit [`TestLoader<TTestResultData>`](LoadTestST/TestLoader.cs) abstract class to create your own concrete one. (e.g. `FacebookAPITestLoader : TestLoader<string>`)

1. Inherit [`LoadTest<TResultData>`](LoadTestST/LoadTest.cs) abstract class to create various load-test cases. (e.g. `GetFriendListLoadTest : LoadTest<string>`)

1. Do load testing:
``` C#
var testLoader = new FacebookAPITestLoader(new Logger());

var loads = 1000;
testLoader.Run<GetFriendListLoadTest>(loads);
```
