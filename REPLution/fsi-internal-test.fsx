/// # REPLution #
///
/// ...


/// ## Setup ##
///
/// ...
#r "../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
open Microsoft.FSharp.Compiler.Interactive.Shell

open System
open System.IO
open System.Text


let outBuilder = new StringBuilder()
let errBuilder = new StringBuilder()
let inReader   = new StringReader("")
let outWriter  = new StringWriter(outBuilder)
let errWriter  = new StringWriter(errBuilder)

let fsiOptions = FsiEvaluationSession.GetDefaultConfiguration()
let args       = [|
                    "fsi.exe"
                    "--noninteractive"
                    "--nologo"
                    "--gui-"
                 |]
let fsiSession = FsiEvaluationSession.Create(
                    fsiOptions,
                    args,
                    inReader,
                    outWriter,
                    errWriter,
                    collectible=true)


/// ## Tests ##
///
/// ...


/// ### UoM ###
///
/// Test to see if saved UoM's are capable of being restored
/// when session is saved and restored
let testUoM = """[<Measure>]
                type t1
                [<Measure>]
                type t2
                [<Measure>]
                type t3
                type Test<'t> =
                  class
                    new : value:int<'t> -> Test<'t>
                    member Get : int<'t>
                  end"""
fsiSession.EvalInteraction """[1..10] |> List.iter (printfn "%A") """
let result = fsiSession.EvalExpression(testUoM)
printfn "%A" result.Value


/// ### Autocomplete? ###
///
/// This is how we'll get autocomplete working
fsiSession.GetCompletions("System.Te")

fsiSession.EvalExpression("let x = [1..1000];;")
fsiSession.EvalInteraction("let x = [1..1000000000]")
/// check to see if UoM are saved
fsiSession.EvalInteraction("[<Measure>] type T; let x' = 100<T>")


/// ## Parsing Results ##
///
/// Results returned in a 3-tuple with:
/// * FSharpParseFileResults
/// * FSharpCheckFileResults
/// * FSharpCheckProjectResults
/// ---
/// parses and checks interactions
fsiSession.ParseAndCheckInteraction("let fun = \"fun!\"")
//fsiSession.InteractiveChecker("open System.Te")
fsiSession.ParseAndCheckInteraction("open System.Te")


/// Valid Input
let (parseResults, checkFileResults, checkProjectResults) =
    fsiSession.ParseAndCheckInteraction("let n = 13")
/// Error (System.Te typo)
let (parseResults', checkFileResults', checkProjectResults') =
    fsiSession.ParseAndCheckInteraction("open System.Te")


/// ## Save Command History ##
///
/// ...
let commandHistory = fsiSession.DynamicAssembly.CodeBase
let sb     = new StringBuilder(commandHistory)
let writer = new StringWriter(sb)
let file   = File.Open("""./FSI-History.fsx""", FileMode.OpenOrCreate)
// ... and so on


/// ## Save to dll ##
///
/// discussion at https://github.com/fsharp/FSharp.Compiler.Service/pull/365
/// says dynamic assembly is to be cast
/// to System.Reflection.Emit.AssemblyBuilder and is saved by "FSI-ASSEMBLY.dll"
/// (reason unknown -- perhaps the DynamicAssembly needs a property set)
let assemBuilder = fsiSession.DynamicAssembly :?> System.Reflection.Emit.AssemblyBuilder
//let saveAssemBuilder = { fsiSession.DynamicAssembly with Name="mySession.dll" }

assemBuilder.Save("""./FSI-ASSEMBLY.dll""")


/// ## Load from DLL ##
///
/// I certainly hope this works
fsiSession.DynamicAssembly.GetLoadedModules()
let assemblyName = "FSI-ASSEMBLY"
let assemblyFile = File.Open("FSI-ASSEMBLY.dll", FileMode.Open)
let assemblyFileLen = int assemblyFile.Length
let assemblyFileBytes = [||]
let readInt = assemblyFile.Read(assemblyFileBytes, 0, assemblyFileLen)
fsiSession.DynamicAssembly.LoadModule("FSI-ASSEMBLY", assemblyBytes)