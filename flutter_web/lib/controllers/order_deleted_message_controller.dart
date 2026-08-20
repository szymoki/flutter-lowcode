// Generated from IR
import '../use_cases/delete_order_use_case.dart';

class COrderDeletedMessage {
  final DeleteOrderUseCase _useCase;

  COrderDeletedMessage({required DeleteOrderUseCase useCase}) : _useCase = useCase;

  void onOk() { _useCase.handleOk(); }
  void onCancel() { _useCase.handleCancel(); }
}
