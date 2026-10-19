// Generated from IR
import '../use_cases/find_order_use_case.dart';

class CMainMenu {
  final FindOrderUseCase _findOrderUseCase;

  CMainMenu({required FindOrderUseCase findOrderUseCase}) : _findOrderUseCase = findOrderUseCase;

  void onFindOrder() { _findOrderUseCase.handleFindOrder(); }
}
