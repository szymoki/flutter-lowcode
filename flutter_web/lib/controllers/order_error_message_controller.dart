// Generated from IR
import '../use_cases/add_order_use_case.dart';

class COrderErrorMessage {
  final AddOrderUseCase _addOrderUseCase;

  COrderErrorMessage({required AddOrderUseCase addOrderUseCase}) : _addOrderUseCase = addOrderUseCase;

  void onClose() { _addOrderUseCase.handleClose(); }
  void onCorrect() { _addOrderUseCase.handleCorrect(); }
}
