// Generated from IR
import '../use_cases/add_order_use_case.dart';

class COrderErrorMessage {
  final AddOrderUseCase _useCase;

  COrderErrorMessage({required AddOrderUseCase useCase}) : _useCase = useCase;

  void onClose() { _useCase.handleClose(); }
  void onCorrect() { _useCase.handleCorrect(); }
  void onDeleteOrder() { _useCase.handleDeleteOrder(); }
}
