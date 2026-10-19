// Generated from IR
import '../use_cases/add_order_use_case.dart';
import '../use_cases/delete_order_use_case.dart';

class COrderErrorMessage {
  final AddOrderUseCase _addOrderUseCase;
  final DeleteOrderUseCase _deleteOrderUseCase;

  COrderErrorMessage({required AddOrderUseCase addOrderUseCase, required DeleteOrderUseCase deleteOrderUseCase}) : _addOrderUseCase = addOrderUseCase, _deleteOrderUseCase = deleteOrderUseCase;

  void onClose() { _addOrderUseCase.handleClose(); }
  void onCorrect() { _addOrderUseCase.handleCorrect(); }
  void onDeleteOrder() { _deleteOrderUseCase.handleDeleteOrder(); }
}
