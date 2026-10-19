// Generated from IR
import '../use_cases/delete_order_use_case.dart';

class CDeleteConfirmationMessage {
  final DeleteOrderUseCase _deleteOrderUseCase;

  CDeleteConfirmationMessage({required DeleteOrderUseCase deleteOrderUseCase}) : _deleteOrderUseCase = deleteOrderUseCase;

  void onDelete() { _deleteOrderUseCase.handleDelete(); }
}
