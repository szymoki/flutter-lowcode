// Generated from IR
import '../use_cases/start_use_case.dart';

class CMainMenu {
  final StartUseCase _useCase;

  CMainMenu({required StartUseCase useCase}) : _useCase = useCase;

  void onFindOrder() { _useCase.handleFindOrder(); }
}
