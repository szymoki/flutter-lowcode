// Generated from IR
import '../use_cases/add_order_use_case.dart';

class COrderSuccessMessage {
  final AddOrderUseCase _addOrderUseCase;

  COrderSuccessMessage({required AddOrderUseCase addOrderUseCase}) : _addOrderUseCase = addOrderUseCase;

  void onOk() { _addOrderUseCase.handleOk(); }
  void onCancel() { _addOrderUseCase.handleCancel(); }
}
