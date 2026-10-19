// Generated from IR
import '../use_cases/find_order_use_case.dart';

class COrderWindow {
  final FindOrderUseCase _findOrderUseCase;

  COrderWindow({required FindOrderUseCase findOrderUseCase}) : _findOrderUseCase = findOrderUseCase;

  void onClose() { _findOrderUseCase.handleCloseOrderWindow(); }
  void onCancel() { _findOrderUseCase.handleCancel(); }
}
