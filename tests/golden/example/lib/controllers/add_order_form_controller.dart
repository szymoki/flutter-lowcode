// Generated from IR
import '../use_cases/add_order_use_case.dart';

class CAddOrderForm {
  final AddOrderUseCase _addOrderUseCase;

  CAddOrderForm({required AddOrderUseCase addOrderUseCase}) : _addOrderUseCase = addOrderUseCase;

  void onAdd() { _addOrderUseCase.handleAdd(); }
}
