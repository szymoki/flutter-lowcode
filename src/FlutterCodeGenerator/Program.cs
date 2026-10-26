using FlutterCodeGenerator;

// Kody wyjścia: 0 – sukces, 1 – błędy specyfikacji, 2 – złe użycie.
if (args.Length != 2)
{
    Console.Error.WriteLine("Użycie: FlutterCodeGenerator <folder_z_plikami_.txt> <folder_wyjściowy>");
    return 2;
}

var result = Pipeline.Run(args[0], args[1], Console.Out);
foreach (var w in result.Warnings)
    Console.Error.WriteLine("Ostrzeżenie: " + w);
foreach (var e in result.Errors)
    Console.Error.WriteLine("Błąd: " + e);
return result.Success ? 0 : 1;
