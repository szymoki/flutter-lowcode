using FlutterCodeGenerator;

// args[0] = folder z plikami .txt (RSL-bis), args[1] = folder wyjściowy (aplikacja Flutter)
var inputFolder = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "examples");
var outputFolder = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "flutter_web");

var result = Pipeline.Run(inputFolder, outputFolder, Console.Out);
foreach (var w in result.Warnings)
    Console.Error.WriteLine("Ostrzeżenie: " + w);
foreach (var e in result.Errors)
    Console.Error.WriteLine("Błąd: " + e);
return result.Success ? 0 : 1;
