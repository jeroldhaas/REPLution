#r "../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
open Microsoft.FSharp.Compiler.Interactive.Shell



open System
open System.IO
open System.Text

// Intialize output and input streams
let sbOut = new StringBuilder()
let sbErr = new StringBuilder()
let inStream = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

// Build command line arguments & start FSI session
let argv = [| "C:\\fsi.exe" |]
let allArgs = Array.append argv [|"--noninteractive"|]

let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)  

/// Evaluate expression & return the result
let evalExpression text =
  match fsiSession.EvalExpression(text) with
  | Some value -> Console.WriteLine ( sprintf "%A" value.ReflectionValue)
  | None -> Console.WriteLine  "Got no result!"


/// Evaluate interaction & ignore the result
let evalInteraction text = 
  fsiSession.EvalInteraction(text)


evalExpression "42+1"
evalInteraction "printfn \"bye\""

/// Evaluate script & ignore the result
let evalScript scriptPath = 
  fsiSession.EvalScript(scriptPath)

File.WriteAllText("sample.fsx", "let twenty = 10 + 10")
evalScript "sample.fsx"





let collectionTest() = 

    for i in 1 .. 200 do
        let defaultArgs = [|"fsi.exe";"--noninteractive";"--nologo";"--gui-"|]
        use inStream = new StringReader("")
        use outStream = new StringWriter()
        use errStream = new StringWriter()

        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
        use session = FsiEvaluationSession.Create(fsiConfig, defaultArgs, inStream, outStream, errStream, collectible=true)
        
        session.EvalInteraction (sprintf "type D = { v : int }")
        let v = session.EvalExpression (sprintf "{ v = 42 * %d }" i)
        printfn "iteration %d, result = %A" i v.Value.ReflectionValue

collectionTest() // <-- run the test like this


    