// Generated from IR
import '../models/order.dart';
import '../use_cases/add_order_use_case.dart';

class CAddOrderForm {
  final AddOrderUseCase _addOrderUseCase;

  CAddOrderForm({required AddOrderUseCase addOrderUseCase}) : _addOrderUseCase = addOrderUseCase;

  void onAdd(Order order) { _addOrderUseCase.handleAdd(order); }
  void onCancel() { _addOrderUseCase.handleCancel(); }
}
