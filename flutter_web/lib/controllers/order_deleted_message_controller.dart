// Generated from IR
import '../use_cases/delete_order_use_case.dart';

class COrderDeletedMessage {
  final DeleteOrderUseCase _deleteOrderUseCase;

  COrderDeletedMessage({required DeleteOrderUseCase deleteOrderUseCase}) : _deleteOrderUseCase = deleteOrderUseCase;

  void onOk() { _deleteOrderUseCase.handleOk(); }
  void onCancel() { _deleteOrderUseCase.handleCancel(); }
}
