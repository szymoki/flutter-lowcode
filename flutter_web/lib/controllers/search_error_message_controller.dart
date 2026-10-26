// Generated from IR
import '../use_cases/find_order_use_case.dart';

class CSearchErrorMessage {
  final FindOrderUseCase _findOrderUseCase;

  CSearchErrorMessage({required FindOrderUseCase findOrderUseCase}) : _findOrderUseCase = findOrderUseCase;

  void onClose() { _findOrderUseCase.handleCloseSearchErrorMessage(); }
}
