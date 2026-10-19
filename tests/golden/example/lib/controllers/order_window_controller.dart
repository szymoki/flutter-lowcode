// Generated from IR
import '../use_cases/find_order_use_case.dart';

class COrderWindow {
  final FindOrderUseCase _useCase;

  COrderWindow({required FindOrderUseCase useCase}) : _useCase = useCase;

  void onClose() { _useCase.handleClose(); }
  void onCancel() { _useCase.handleCancel(); }
}
