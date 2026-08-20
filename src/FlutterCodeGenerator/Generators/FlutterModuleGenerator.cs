namespace FlutterCodeGenerator.Generators;

public class FlutterModuleGenerator
{
    public void Generate(string outputPath)
    {
        var libPath = Path.Combine(outputPath, "lib");
        Directory.CreateDirectory(libPath);

        WriteMain(libPath);
        WriteGeneratedWidget(Path.Combine(libPath, "generated_widget.dart"));
    }

    private void WriteMain(string libPath)
    {
        var mainContent = """
// Plik wygenerowany przez FlutterCodeGenerator (.NET)
// Ten moduł jest częścią aplikacji webowej Flutter.

""";
        File.WriteAllText(Path.Combine(libPath, "main.dart"), mainContent + """
void main() {
  // Entry point - można podłączyć pod Flutter Web
}
""");
    }

    private void WriteGeneratedWidget(string filePath)
    {
        var content = """
// Wygenerowany widget - moduł do aplikacji web
// Źródło: FlutterCodeGenerator

import 'package:flutter/material.dart';

class GeneratedWidget extends StatelessWidget {
  const GeneratedWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return const MaterialApp(
      title: 'Generated Flutter Module',
      home: Scaffold(
        body: Center(
          child: Text('Moduł wygenerowany przez .NET'),
        ),
      ),
    );
  }
}
""";
        File.WriteAllText(filePath, content);
    }
}
