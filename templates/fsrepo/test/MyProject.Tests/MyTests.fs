module MyProject.Tests.MyTests

open Expecto

[<Tests>]
let tests =
    testCase "math works" <| fun() ->
        Expect.equal (1 + 1) 2 "math should be working correctly"
