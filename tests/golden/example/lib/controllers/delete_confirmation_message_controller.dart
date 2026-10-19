// Generated from IR
import '../use_cases/delete_order_use_case.dart';

class CDeleteConfirmationMessage {
  final DeleteOrderUseCase _useCase;

  CDeleteConfirmationMessage({required DeleteOrderUseCase useCase}) : _useCase = useCase;

  void onDelete() { _useCase.handleDelete(); }
}
