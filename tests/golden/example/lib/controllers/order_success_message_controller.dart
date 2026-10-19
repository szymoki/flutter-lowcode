// Generated from IR
import '../use_cases/add_order_use_case.dart';

class COrderSuccessMessage {
  final AddOrderUseCase _useCase;

  COrderSuccessMessage({required AddOrderUseCase useCase}) : _useCase = useCase;

  void onOk() { _useCase.handleOk(); }
  void onCancel() { _useCase.handleCancel(); }
}
